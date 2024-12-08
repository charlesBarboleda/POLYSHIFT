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
            // Despawn lobby players
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        networkObject.Despawn(true);
                    }
                }
            }

            // Load the main game scene
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);

            // Subscribe to OnLoadComplete
            NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleOnLoadComplete;
        }
    }

    private void HandleOnLoadComplete(LoadSceneEventArgs args)
    {
        if (args.SceneName == "MainGame")
        {
            // Unsubscribe from the event to avoid duplicate calls
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleOnLoadComplete;

            // Spawn players in the game scene
            SpawnPlayersInGameScene();
        }
    }


    private void SpawnPlayersInGameScene()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;

            // Spawn a new player prefab in the game scene
            GameObject newPlayer = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);
            NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);

            // Assign the player's name using the MainMenuManager's dictionary
            string playerName = GetPlayerName(clientId);
            var playerInfo = newPlayer.GetComponent<PlayerInfo>();
            if (playerInfo != null)
            {
                playerInfo.SetName(playerName);
            }

            Debug.Log($"Spawned new player prefab for client {clientId} with name {playerName}");
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

    public string GetPlayerName(ulong clientId)
    {
        return _playerNames.ContainsKey(clientId) ? _playerNames[clientId] : "Player";
    }
}

