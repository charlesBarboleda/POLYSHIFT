using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public List<Enemy> SpawnedEnemies = new List<Enemy>();
    public List<GameObject> SpawnedAllies = new List<GameObject>();
    private PlayerInfo _localPlayer;
    private Dictionary<ulong, string> _playerNames = new Dictionary<ulong, string>();
    [SerializeField] TMP_InputField _playerNameInput;
    public List<Transform> _spawnPoints = new List<Transform>();
    public int numberOfPlayers = 0;

    private void Awake()
    {
        Singleton();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
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

    void OnClientConnected(ulong clientId)
    {
        numberOfPlayers++;
        TeleportPlayerToSpawnServerRpc(clientId);
    }

    Vector3 GetSpawnPoint()
    {
        Transform spawnPoint = _spawnPoints[numberOfPlayers - 1];
        return spawnPoint.position;
    }

    [ServerRpc(RequireOwnership = false)]
    void TeleportPlayerToSpawnServerRpc(ulong clientId)
    {
        StartCoroutine(TeleportPlayerCoroutine(clientId));
    }

    IEnumerator TeleportPlayerCoroutine(ulong clientId)
    {
        yield return new WaitForSeconds(0.1f);
        TeleportPlayerToSpawnClientRpc(clientId);
    }

    [ClientRpc]
    void TeleportPlayerToSpawnClientRpc(ulong clientId)
    {
        NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient);
        var playerObject = networkClient.PlayerObject;

        if (playerObject != null)
        {
            Debug.Log(playerObject.name);
            Debug.Log(playerObject.transform.position);
            playerObject.transform.position = GetSpawnPoint();
            playerObject.transform.LookAt(Camera.main.transform);
            Debug.Log(playerObject.transform.position);
        }
    }


}
