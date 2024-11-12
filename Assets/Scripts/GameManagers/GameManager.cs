using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public List<Enemy> SpawnedEnemies = new List<Enemy>();
    public List<GameObject> SpawnedAllies = new List<GameObject>();
    [SerializeField] TMP_InputField _playerNameInput;
    private PlayerInfo _localPlayer;

    private Dictionary<ulong, string> _playerNames = new Dictionary<ulong, string>();

    private void Awake()
    {
        Singleton();
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
}
