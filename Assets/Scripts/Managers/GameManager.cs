using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public enum GameState
{
    Lobby,
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
    public GameState CurrentGameState = GameState.Lobby;
    public List<Transform> _spawnPoints = new List<Transform>();
    PlayerInfo _localPlayer;
    Dictionary<ulong, string> _playerNames = new Dictionary<ulong, string>();
    [SerializeField] TMP_InputField _playerNameInput;

    private void Awake()
    {
        Singleton();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameLevel.Value = 0;
            SetCurrentGameState(GameState.Lobby);
        }
    }

    public void SetCurrentGameState(GameState state)
    {
        CurrentGameState = state;

        switch (CurrentGameState)
        {
            case GameState.Lobby:
                // Set game-related lobby logic here
                break;
            case GameState.InLevel:
                SpawnerManager.Instance.SpawnEnemies();
                break;
            case GameState.OutLevel:
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
        Debug.Log("Spawned Enemies: " + SpawnedEnemies.Count);
        Debug.Log("Enemies to spawn: " + SpawnerManager.Instance.EnemiesToSpawn);
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

    public void SetLocalPlayer(PlayerInfo playerInfo)
    {
        _localPlayer = playerInfo;

        // Get name from input field or generate default name
        string playerName = _playerNameInput.text.Length > 0 ? _playerNameInput.text : "Player " + _localPlayer.OwnerClientId;
        _localPlayer.SetName(playerName); // Set the name through the PlayerInfo component

        // Hide input field after setting the name
        _playerNameInput.gameObject.SetActive(false);

        Debug.Log($"Set local player name to {playerName} for client {_localPlayer.OwnerClientId}");
    }

    public void SetPlayerName(NetworkObject playerObject, string name)
    {
        ulong clientId = playerObject.OwnerClientId;
        if (_playerNames.ContainsKey(clientId))
        {
            _playerNames[clientId] = name;
        }
        else
        {
            _playerNames.Add(clientId, name);
        }
        Debug.Log($"Player name set for client {clientId}: {name}");
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        SetCurrentGameState(GameState.OutLevel);
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



}
