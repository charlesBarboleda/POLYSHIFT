using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuManager : NetworkBehaviour
{
    public static MainMenuManager Instance { get; private set; }
    PlayerInfo _localPlayer;
    [SerializeField] TMPro.TMP_InputField _playerNameInput;
    private Dictionary<ulong, string> _playerNames = new Dictionary<ulong, string>();

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

    [ServerRpc]
    public void StartGameServerRpc()
    {
        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        Debug.Log($"Despawning and destroying player object for client {client.ClientId}");
                        networkObject.Despawn(true);
                    }
                }
            }
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
        }
    }

    public void SetLocalPlayer(PlayerInfo playerInfo)
    {
        _localPlayer = playerInfo;

        // Get name from input field or generate a default name
        string playerName = _playerNameInput.text.Length > 0 ? _playerNameInput.text : "Player " + _localPlayer.OwnerClientId;
        _localPlayer.SetName(playerName); // Set the name through the PlayerInfo component

        // Add the player's name to the dictionary
        SetPlayerName(_localPlayer.GetComponent<NetworkObject>(), playerName);

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

    public string GetPlayerName(ulong clientId)
    {
        return _playerNames.ContainsKey(clientId) ? _playerNames[clientId] : "Player";
    }
}

