using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using DG.Tweening;
using UnityEditor.PackageManager;

public class MainMenuManager : NetworkBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [SerializeField] private TMPro.TMP_InputField _playerNameInput; // Input field for player names
    [SerializeField] private GameObject _loadingScreen; // Fullscreen loading screen UI
    [SerializeField] private Slider _loadingSlider; // Universal progress bar

    private Dictionary<ulong, string> _playerNames = new Dictionary<ulong, string>();
    // Tracks which clients have fully loaded the scene
    private Dictionary<ulong, bool> _clientLoadStatus = new Dictionary<ulong, bool>();
    // Tracks which clients have hidden their loading screens and confirmed readiness
    private HashSet<ulong> _clientsConfirmedReady = new HashSet<ulong>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.Server)]
    public void StartGameRpc()
    {
        if (!IsServer) return;

        // Reset tracking
        _clientLoadStatus.Clear();
        _clientsConfirmedReady.Clear();

        // Track all currently connected clients as "not loaded"
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            _clientLoadStatus[client.ClientId] = false;
        }

        // Show loading screen for all
        ShowLoadingScreenRpc(0f);

        // Listen for scene events (LoadComplete, etc.)
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;

        Debug.Log("[SERVER] Loading MainGame scene...");
        var status = NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"[SERVER] Failed to start loading scene: {status}");
        }
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadComplete:
                // This means one client has finished loading the scene
                Debug.Log($"[SERVER] Client {sceneEvent.ClientId} finished loading.");
                _clientLoadStatus[sceneEvent.ClientId] = true;

                // Destroy the player object for the client
                GameManager.Instance.DestroyPlayerObject(sceneEvent.ClientId);

                // Create the player object for the client
                GameManager.Instance.CreatePlayerObject(sceneEvent.ClientId);

                // Check if all clients have loaded
                if (AllClientsLoaded())
                {
                    Debug.Log("[SERVER] All clients have loaded. Instructing them to hide loading screens...");
                    var clientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClients.Keys);
                    Debug.Log("[SERVER] Sending HideLoadingScreensClientRpc to client IDs: " + string.Join(", ", clientIds));

                    // Now call the RPC
                    HideLoadingScreensClientRpc(new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams { TargetClientIds = clientIds }
                    });
                }
                break;
        }
    }

    private bool AllClientsLoaded()
    {
        // If ANY client is still false => not all loaded
        foreach (var kvp in _clientLoadStatus)
        {
            if (!kvp.Value) return false;
        }
        return true;
    }

    /// <summary>
    /// Step 1: Server calls this when everyone is loaded, telling all clients to hide screens.
    /// </summary>
    [ClientRpc]
    private void HideLoadingScreensClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] **Hiding loading screen** at server's instruction...");

        // Force progress bar to 100% for visuals
        _loadingSlider.DOValue(1, 1f).OnComplete(() =>
        {
            _loadingScreen.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
         {
             _loadingScreen.SetActive(false);
             Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Loading screen hidden. Confirming ready to server...");

             // Step 2: Now the client confirms readiness to the server
             ConfirmSceneReadyServerRpc();
         });
        });
    }

    /// <summary>
    /// Step 2: Client calls this once they've hidden their loading screen.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ConfirmSceneReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[SERVER] Client {senderClientId} has confirmed scene ready.");

        // Mark this client as ready
        _clientsConfirmedReady.Add(senderClientId);

        Debug.Log($"[SERVER] Clients confirmed ready: {_clientsConfirmedReady.Count}/{_clientLoadStatus.Count}");

        // If all clients have confirmed => start the game
        if (_clientsConfirmedReady.Count == _clientLoadStatus.Count)
        {
            Debug.Log("[SERVER] All clients confirmed readiness! Starting the game...");
            GameManager.Instance.StartGameServerRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowLoadingScreenRpc(float initialProgress)
    {
        _loadingScreen.SetActive(true);
        _loadingScreen.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        _loadingSlider.value = initialProgress;

        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Showed Loading Screen.");
    }

    // Retrieve a player's name by clientId
    public string GetPlayerName(ulong clientId)
    {
        return _playerNames.ContainsKey(clientId) ? _playerNames[clientId] : "Player";
    }

    // Set a player's name by clientId
    public void SetLocalPlayerName()
    {
        if (_playerNameInput != null)
        {
            string playerName = _playerNameInput.text;
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Player";
            }

            _playerNames[NetworkManager.Singleton.LocalClientId] = playerName;
        }
    }
}
