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

                // Spawn a new game player prefab for each client
                GameObject newPlayer = Instantiate(playerPrefab);
                NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
                networkObject.SpawnAsPlayerObject(clientId);

                // Assign player name from MainMenuManager
                var playerName = MainMenuManager.Instance.GetPlayerName(clientId);
                var info = newPlayer.GetComponent<PlayerInfo>();
                if (info != null)
                {
                    info.SetName(playerName);
                }
                SpawnedAllies.Add(newPlayer);
                AlivePlayers.Add(newPlayer);
            }

            GameLevel.Value = 0;
            SetCurrentGameState(GameState.OutLevel);
            EventManager.Instance.OnPlayerDeath.AddListener(HandleGameOver);

            EnableCountdownTextClientRpc();
            EnableGameLevelTextClientRpc();
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



    [ServerRpc(RequireOwnership = false)]
    public void DisconnectGameServerRpc()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
        }
    }



    void Countdown()
    {
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
            GameCountdown.Value = 30f;
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





}
