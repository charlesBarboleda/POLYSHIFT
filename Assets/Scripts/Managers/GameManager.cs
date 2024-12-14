using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum GameState
{
    OutLevel,
    InLevel,
    GameOver
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public NetworkVariable<int> GameLevel = new NetworkVariable<int>(0);
    public NetworkVariable<float> GameCountdown = new NetworkVariable<float>(30f);
    public List<Enemy> SpawnedEnemies = new List<Enemy>();
    public List<GameObject> SpawnedAllies = new List<GameObject>();
    public List<GameObject> AlivePlayers = new List<GameObject>();
    public GameState CurrentGameState = GameState.OutLevel;
    public List<Transform> _spawnPoints = new List<Transform>();
    [SerializeField] GameObject playerPrefab;


    private void Awake()
    {
        Singleton();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong clientId = client.ClientId;

                // Spawn the player prefab
                GameObject newPlayer = Instantiate(playerPrefab);
                NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
                networkObject.SpawnAsPlayerObject(clientId);
                AlivePlayers.Add(newPlayer);
                SpawnedAllies.Add(newPlayer);

                // Assign the name to the player
                string playerName = MainMenuManager.Instance.GetPlayerName(clientId);
                PlayerInfo playerInfo = newPlayer.GetComponent<PlayerInfo>();
                if (playerInfo != null)
                {
                    playerInfo.SetName(playerName);
                    Debug.Log($"GameManager: Assigned name '{playerName}' to Client {clientId}");
                }
            }

            GameLevel.Value = 0;
            SetCurrentGameState(GameState.OutLevel);
            EventManager.Instance.OnPlayerDeath.AddListener(HandleGameOver);

            EnableCountdownTextClientRpc();
            EnableGameLevelTextClientRpc();
        }

    }

    private void AssignPlayerName(GameObject playerObject, ulong clientId)
    {
        string playerName = MainMenuManager.Instance.GetPlayerName(clientId);

        PlayerInfo playerInfo = playerObject.GetComponent<PlayerInfo>();
        if (playerInfo != null)
        {
            playerInfo.SetName(playerName);
            Debug.Log($"AssignPlayerName: Client {clientId}'s name set to '{playerName}'");
        }
    }


    public void SetCurrentGameState(GameState state)
    {
        CurrentGameState = state;

        switch (CurrentGameState)
        {
            case GameState.InLevel:
                // Set game-related in-level logic here
                DisableCountdownTextClientRpc();
                SpawnerManager.Instance.SpawnEnemies();
                break;
            case GameState.OutLevel:
                // Set game-related out-level logic here
                EnableCountdownTextClientRpc();
                Countdown();
                break;
            case GameState.GameOver:
                // Set game-related game over logic here
                break;
        }
    }

    public void OnEnemyDespawned(Enemy enemy)
    {
        if (SpawnedEnemies.Count == 0 && SpawnerManager.Instance.EnemiesToSpawn == 0)
        {
            SetCurrentGameState(GameState.OutLevel);
        }
    }

    private void Singleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public void MainMenu()
    {
        if (IsHost)
        {
            // Host logic: Notify clients to load MainMenu and then shut down the server
            NotifyClientsToLoadMainMenuClientRpc();
            Invoke(nameof(ShutdownServer), 1f); // Allow time for the ClientRpc to propagate
        }
        else if (IsClient)
        {
            // Client logic: Disconnect and load MainMenu locally
            DisconnectClient();
        }
    }

    // Notify all clients to load the MainMenu scene
    [ClientRpc]
    private void NotifyClientsToLoadMainMenuClientRpc()
    {
        LoadMainMenuLocally();
    }

    // Load MainMenu locally for the host and shutdown server after all clients are notified
    private void ShutdownServer()
    {
        LoadMainMenuLocally();
        NetworkManager.Singleton.Shutdown();
    }

    // Load the MainMenu scene locally
    private void LoadMainMenuLocally()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    // Logic for a client to disconnect and load the MainMenu scene locally
    private void DisconnectClient()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            LoadMainMenuLocally();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void RestartGameServerRpc()
    {
        if (IsServer)
        {
            SpawnerManager.Instance.KillAllAllies();
            SpawnerManager.Instance.KillAllEnemies();
            DestroyAllPlayersServerRpc();
            DisableGameOverUIClientRpc();
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyAllPlayersServerRpc()
    {
        if (IsServer)
        {
            // Destroys all player objects
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    var networkObject = playerObject.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        networkObject.Despawn(true);
                    }
                }
            }
        }
    }



    void Countdown()
    {
        if (IsServer)
            GameCountdown.Value = 30f;

        StartCoroutine(GameCountdownCoroutine());
    }


    IEnumerator GameCountdownCoroutine()
    {

        while (GameCountdown.Value > 0)
        {
            if (IsServer)
            {
                GameCountdown.Value -= Time.deltaTime;
            }
            yield return null;
        }

        if (IsServer)
        {
            GameLevel.Value++;
            SetCurrentGameState(GameState.InLevel);
        }
    }

    public void HandleGameOver(PlayerNetworkHealth player)
    {
        if (AlivePlayers.Count == 0)
        {
            SetCurrentGameState(GameState.GameOver);

            // Notify all clients to handle their UI
            TriggerGameOverClientRpc();
        }
    }

    [ClientRpc]
    private void TriggerGameOverClientRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerStateController = client.PlayerObject.GetComponent<PlayerStateController>();
            playerStateController.ShowGameOverUI();
        }
    }

    [ClientRpc]
    void EnableCountdownTextClientRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerUIManager = client.PlayerObject.GetComponent<PlayerUIManager>();
            playerUIManager.EnableCountdownText();
        }
    }

    [ClientRpc]
    void DisableCountdownTextClientRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerUIManager = client.PlayerObject.GetComponent<PlayerUIManager>();
            playerUIManager.DisableCountdownText();
        }
    }

    [ClientRpc]
    void EnableGameLevelTextClientRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerUIManager = client.PlayerObject.GetComponent<PlayerUIManager>();
            playerUIManager.EnableGameLevelText();
        }
    }

    [ClientRpc]
    void DisableGameOverUIClientRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerUIManager = client.PlayerObject.GetComponent<PlayerUIManager>();
            playerUIManager.DisableGameOverUI();
        }
    }





}
