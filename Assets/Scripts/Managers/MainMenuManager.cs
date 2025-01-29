using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuManager : NetworkBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [SerializeField] private TMPro.TMP_InputField _playerNameInput; // Input field for player names
    [SerializeField] private GameObject _loadingScreen; // Fullscreen loading screen UI
    [SerializeField] private Slider _loadingSlider; // Universal progress bar

    private bool _sceneLoaded = false; // Tracks whether the scene has finished loading
    private Dictionary<ulong, string> _playerNames = new Dictionary<ulong, string>(); // Tracks player names
    private Dictionary<ulong, bool> _clientLoadStatus = new Dictionary<ulong, bool>(); // Tracks whether each client has finished loading
    private bool _isLoading = false; // Prevents reloading scenes accidentally

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

    /// <summary>
    /// Called by the host to start the game and transition to the next scene.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (!IsServer) return;

        _clientLoadStatus.Clear();
        ShowLoadingScreenClientRpc(0f); // Notify all clients to show the loading screen

        // Despawn all networked objects before transitioning
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            player.PlayerObject.GetComponent<NetworkObject>().Despawn(true);
        }

        // Register for scene event notifications
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;

        // Start loading the scene
        Debug.Log("Loading MainGame scene...");
        var status = NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"Failed to start loading scene: {status}");
        }
    }

    /// <summary>
    /// Handles scene events, such as clients finishing their loading.
    /// </summary>
    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadComplete:
                if (sceneEvent.ClientId != NetworkManager.ServerClientId) // Ignore the server
                {
                    _clientLoadStatus[sceneEvent.ClientId] = true;
                    Debug.Log($"Client {sceneEvent.ClientId} finished loading.");
                }
                break;

            case SceneEventType.LoadEventCompleted:
                Debug.Log("All clients have finished loading the scene.");
                _sceneLoaded = true;
                StartCoroutine(CompleteLoading());
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
                break;
        }
    }

    /// <summary>
    /// Smoothly fills progress bar until 100% and adds a delay before hiding the loading screen.
    /// </summary>
    private IEnumerator CompleteLoading()
    {
        float progress = _loadingSlider.value;

        // Ensure the bar smoothly reaches 100%
        while (progress < 1f)
        {
            progress += Time.deltaTime * 2f; // Smooth transition speed
            _loadingSlider.value = Mathf.Clamp01(progress);
            yield return null;
        }

        Debug.Log("Progress bar reached 100%. Waiting before hiding...");
        yield return new WaitForSeconds(1.5f); // Add a delay before fading out

        NotifyClientsSceneReadyClientRpc();
    }

    /// <summary>
    /// Simulates smooth progress to avoid sudden jumps.
    /// </summary>
    private IEnumerator SimulateProgress()
    {
        float progress = 0f;

        while (!_sceneLoaded)
        {
            progress += Random.Range(0.05f, 0.1f);
            progress = Mathf.Clamp(progress, 0f, 0.95f);

            _loadingSlider.value = progress;
            yield return new WaitForSeconds(0.1f);
        }

        _loadingSlider.value = 1f; // Ensure full completion
        yield return new WaitForSeconds(1.5f); // Small delay for smooth transition
    }
    /// <summary>
    /// Notifies all clients that the scene is ready and hides the loading screen.
    /// </summary>
    [ClientRpc]
    private void NotifyClientsSceneReadyClientRpc()
    {
        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Received scene ready notification.");

        _isLoading = true;
        _loadingSlider.value = 1f; // Force progress bar to 100%

        _loadingScreen.GetComponent<CanvasGroup>().DOFade(0, 0.25f).OnComplete(() =>
        {
            _loadingScreen.SetActive(false);
            Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Loading screen hidden.");
        });
    }

    /// <summary>
    /// Shows the loading screen on all clients with an initial progress value.
    /// </summary>
    [ClientRpc]
    private void ShowLoadingScreenClientRpc(float initialProgress)
    {
        _loadingScreen.SetActive(true);
        _loadingScreen.GetComponent<CanvasGroup>().DOFade(1, 0.25f);
        _loadingSlider.value = initialProgress;

        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Showed loading screen.");

        // Start simulating progress locally
        if (!_isLoading)
        {
            StartCoroutine(SimulateProgress());
        }
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

