using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;

using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using NetworkEvent = Unity.Networking.Transport.NetworkEvent;
using TMPro;

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System.Collections;
using DG.Tweening;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private int _maxConnections = 4;
    [Header("Connection UI")]
    [SerializeField] TMP_Text _playerIDText;
    [SerializeField] TMP_Text _wrongCodeText;
    [SerializeField] TMP_Text _joinCodeText;
    [SerializeField] TMP_InputField _inputCodeText;

    [Header("Script References")]
    [SerializeField] TitleScreenUIManager _titleScreenUIManager;

    [Header("ETC")]
    [SerializeField] Image _loadingScreen;

    string _playerID;
    bool _clientAuthenticated = false;
    string _joinCode;

    async void Start()
    {
        await AuthenticatePlayer();

    }

    async Task AuthenticatePlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _playerID = AuthenticationService.Instance.PlayerId;
            _clientAuthenticated = true;
            _playerIDText.text = $"Player ID: {_playerID}";
        }
        catch (Exception e)
        {
            Debug.LogError($"Error authenticating player: {e.Message}");
        }
    }

    public async Task<RelayServerData> AllocateRelayServerAndGetCode(int maxConnections, string region = null)
    {
        Allocation allocation;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error allocating relay server: {e.Message}");
            throw;
        }

        try
        {
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting join code: {e.Message}");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    }

    IEnumerator ConfigureGetCodeAndJoinHost()
    {
        Debug.Log("Enabling loading screen...");
        // Handle Loading Screen
        _loadingScreen.gameObject.SetActive(true);

        Debug.Log("Fading in loading screen...");

        _loadingScreen.DOFade(1, 0.5f);

        Debug.Log("Loading screen faded in. Allocating relay server and getting join code...");


        // Handle Relay Server Allocation
        var allocateAndGetCode = AllocateRelayServerAndGetCode(_maxConnections);

        while (!allocateAndGetCode.IsCompleted)
        {
            // Wait for the allocation and join code to be retrieved
            yield return null;
        }

        // Check if the allocation and join code were successfully retrieved
        if (allocateAndGetCode.IsFaulted)
        {

            Debug.LogError("Failed to allocate relay server and get join code.");
            yield break;
        }

        var relayServerData = allocateAndGetCode.Result;


        // Handle NetworkManager setup 
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();


        // Handle 2nd half of Loading Screen
        yield return new WaitForSeconds(1f);
        _loadingScreen.DOFade(0, 0.5f).OnComplete(() =>
        {
            _loadingScreen.gameObject.SetActive(false);
        });

        _joinCodeText.gameObject.SetActive(true);
        _joinCodeText.text = _joinCode;
        _joinCodeText.GetComponent<CanvasGroup>().DOFade(1, 1f);
    }

    public async Task<RelayServerData> JoinRelayServerWithCode(string joinCode)
    {
        JoinAllocation allocation;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error joining relay server: {e.Message}");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    }

    IEnumerator ConfigureUseCodeJoinClient(string joinCode)
    {
        _loadingScreen.gameObject.SetActive(true);
        _loadingScreen.DOFade(1, 0.5f);

        var joinAllocationFromCode = JoinRelayServerWithCode(joinCode);

        while (!joinAllocationFromCode.IsCompleted)
        {
            // Wait for the allocation to be retrieved
            yield return null;
        }

        if (joinAllocationFromCode.IsFaulted)
        {
            Debug.Log("Cannot join relay due to an exception");
            _wrongCodeText.gameObject.SetActive(true);
            _wrongCodeText.GetComponent<CanvasGroup>().DOFade(1, 1f).OnComplete(() =>
            {
                _wrongCodeText.GetComponent<CanvasGroup>().DOFade(0, 1f).OnComplete(() =>
                {
                    _wrongCodeText.gameObject.SetActive(false);
                });
            });
            yield break;
        }


        _titleScreenUIManager.DisableAllUIElements();

        var relayServerData = joinAllocationFromCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);


        NetworkManager.Singleton.StartClient();

        yield return new WaitForSeconds(1f);

        _loadingScreen.DOFade(0, 0.5f).OnComplete(() =>
        {
            _loadingScreen.gameObject.SetActive(false);
        });

    }


    public void JoinHost()
    {
        if (!_clientAuthenticated)
        {
            // Authenticate the client before joining
            Debug.LogError("Client not authenticated.");
            return;
        }
        StartCoroutine(ConfigureGetCodeAndJoinHost());
        Debug.Log("Hosting lobby...");

        // Allow host to set name after spawning
        NetworkManager.Singleton.OnClientConnectedCallback += OnHostConnected;
    }

    private void OnHostConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Host connected. Setting local name...");
            MainMenuManager.Instance.SetLocalPlayerName();
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnHostConnected;
    }

    public void StartClient()
    {
        if (!_clientAuthenticated)
        {
            Debug.LogError("Client not authenticated.");
            return;
        }

        if (_inputCodeText.text.Length <= 0)
        {
            Debug.LogError("Join code is empty.");
            return;
        }

        StartCoroutine(ConfigureUseCodeJoinClient(_inputCodeText.text));

        // Allow client to set name after connecting
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Client connected. Setting local name...");
            MainMenuManager.Instance.SetLocalPlayerName();
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }


}
