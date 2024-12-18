using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using System;
using UnityEngine.UI;

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
    [SerializeField] Image bossHealthbar;
    [SerializeField] TMP_Text bossName;
    [SerializeField] GameObject bossHealthbarContainer;


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
            EventManager.Instance.OnEnemySpawned.AddListener(OnBossSpawned);
            EventManager.Instance.OnEnemyDespawned.AddListener(OnBossDespawned);
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

    public void OnBossSpawned(Enemy enemy)
    {
        if (enemy.TryGetComponent(out BossEnemyNetworkHealth bossHealth))
        {
            bossName.gameObject.SetActive(true);
            bossHealthbarContainer.SetActive(true);
            bossName.text = bossHealth.BossName;
            bossHealth.CurrentHealth.OnValueChanged += UpdateBossHealthBar;
        }
    }

    public void OnBossDespawned(Enemy enemy)
    {
        if (enemy.TryGetComponent(out BossEnemyNetworkHealth bossHealth))
        {
            bossName.gameObject.SetActive(false);
            bossHealthbarContainer.SetActive(false);
            bossHealth.CurrentHealth.OnValueChanged -= UpdateBossHealthBar;
        }
    }

    public void UpdateBossHealthBar(float currentHealth, float maxHealth)
    {
        bossHealthbar.DOFillAmount(currentHealth / maxHealth, 0.5f);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            EventManager.Instance.OnEnemySpawned.RemoveListener(OnBossSpawned);
            EventManager.Instance.OnEnemyDespawned.RemoveListener(OnBossDespawned);
            EventManager.Instance.OnPlayerDeath.RemoveListener(HandleGameOver);
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


    [ClientRpc]
    public void HealAllPlayersClientRpc()
    {
        foreach (var ally in SpawnedAllies)
        {
            var networkHealth = ally.GetComponent<PlayerNetworkHealth>();
            if (networkHealth != null)
            {
                networkHealth.HealServerRpc(networkHealth.maxHealth.Value);
            }
        }
    }


    [ClientRpc]
    public void GiveAllPlayersLevelClientRpc(int level)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerLevel = client.PlayerObject.GetComponent<PlayerNetworkLevel>();
            playerLevel.Level.Value += level;
        }
    }



    public void MainMenu()
    {
        foreach (var networkObject in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            networkObject.Despawn(true);
        }
        if (NetworkManager.Singleton.IsHost)
        {
            // Host handles both their own shutdown and notifying clients
            StartCoroutine(HostMainMenuTransition());
        }
        else
        {
            // Clients disconnect and transition locally
            StartCoroutine(ClientMainMenuTransition());
        }
    }


    private IEnumerator HostMainMenuTransition()
    {


        Debug.Log("Host: Notifying clients and shutting down server...");

        // Notify clients to return to lobby
        NotifyClientsToDisconnectClientRpc();

        // Shutdown server
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            yield return new WaitForSeconds(0.5f); // Ensure shutdown completes
            Destroy(NetworkManager.Singleton.gameObject);
            Debug.Log("Host: NetworkManager destroyed.");
        }

        // Sign out the host's authentication session
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Host: Signing out...");
            AuthenticationService.Instance.SignOut();
        }

        // Load the lobby scene
        Debug.Log("Host: Loading lobby scene...");
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }



    private IEnumerator ClientMainMenuTransition()
    {
        Debug.Log("Client: Disconnecting...");

        // Disconnect the client
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            yield return new WaitForSeconds(0.5f); // Ensure shutdown completes
            Destroy(NetworkManager.Singleton.gameObject);
            Debug.Log("Client: NetworkManager destroyed.");
        }

        // Sign out the client's authentication session
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Client: Signing out...");
            AuthenticationService.Instance.SignOut();
        }

        // Load the lobby scene
        Debug.Log("Client: Loading lobby scene...");
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }



    [ClientRpc]
    private void NotifyClientsToDisconnectClientRpc()
    {
        Debug.Log("Client: Received disconnect notification from host...");
        StartCoroutine(ClientMainMenuTransition());
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
            foreach (var networkObject in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
            {
                networkObject.Despawn(true);
            }
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
