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

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private int _maxConnections = 4;
    [Header("Connection UI")]
    [SerializeField] Button _clientBtn;
    [SerializeField] Button _hostBtn;
    [SerializeField] TMP_Text _statusText;
    [SerializeField] TMP_Text _playerIDText;
    [SerializeField] TMP_InputField _joinCodeText;

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

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();

        _joinCodeText.gameObject.SetActive(true);
        _joinCodeText.text = _joinCode;
        _statusText.text = "Joined as Host!";
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
        var joinAllocationFromCode = JoinRelayServerWithCode(joinCode);

        while (!joinAllocationFromCode.IsCompleted)
        {
            // Wait for the allocation to be retrieved
            yield return null;
        }

        if (joinAllocationFromCode.IsFaulted)
        {
            Debug.Log("Cannot join relay due to an exception");
            yield break;
        }

        var relayServerData = joinAllocationFromCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
        _statusText.text = "Joined as Client!";
    }


    public void StartClient()
    {
        if (!_clientAuthenticated)
        {
            Debug.LogError("Client not authenticated.");
            return;
        }

        if (_joinCodeText.text.Length <= 0)
        {
            Debug.LogError("Join code is empty.");
            _statusText.text = "Join code is empty.";
            return;
        }
        StartCoroutine(ConfigureUseCodeJoinClient(_joinCodeText.text));

        _clientBtn.gameObject.SetActive(false);
        _hostBtn.gameObject.SetActive(false);
        _joinCodeText.gameObject.SetActive(false);
    }

    public void JoinHost()
    {
        if (!_clientAuthenticated)
        {
            Debug.LogError("Client not authenticated.");
            return;
        }

        StartCoroutine(ConfigureGetCodeAndJoinHost());
        Debug.Log(_joinCodeText.text);

        _clientBtn.gameObject.SetActive(false);
        _hostBtn.gameObject.SetActive(false);
        _joinCodeText.gameObject.SetActive(false);


    }


}
