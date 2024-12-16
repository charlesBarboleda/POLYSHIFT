using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class MainMenuManager : NetworkBehaviour
{
    public static MainMenuManager Instance { get; private set; }
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
            foreach (var networkObject in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
            {
                if (networkObject.GetComponent<MainMenuManager>() != null)
                {
                    continue;
                }
                networkObject.Despawn(true);
            }

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    var networkObject = playerObject.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        Debug.Log($"Despawning NetworkObject: {networkObject.name}");
                        networkObject.Despawn(true);
                    }
                }
            }

            // Wait a frame to ensure all objects are despawned
            StartCoroutine(DelayedSceneTransition());
        }
    }

    IEnumerator DelayedSceneTransition()
    {
        yield return null; // Wait for the current frame to complete
        NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
    }


    // Called to set the player's name from the input field
    public void SetLocalPlayerName()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        string playerName = (_playerNameInput != null && _playerNameInput.text.Length > 0)
            ? _playerNameInput.text
            : $"Player {clientId}";

        // Save the name in the dictionary
        if (_playerNames.ContainsKey(clientId))
        {
            _playerNames[clientId] = playerName;
        }
        else
        {
            _playerNames.Add(clientId, playerName);
        }

        Debug.Log($"SetLocalPlayerName: Client {clientId} set name to {playerName}");
    }

    // Retrieve a player's name by clientId
    public string GetPlayerName(ulong clientId)
    {
        return _playerNames.ContainsKey(clientId) ? _playerNames[clientId] : "Player";
    }
}

