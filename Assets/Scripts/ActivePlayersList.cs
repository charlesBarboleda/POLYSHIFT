using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ActivePlayersList : NetworkBehaviour
{
    public static ActivePlayersList Instance;

    public NetworkVariable<int> playersInGame = new NetworkVariable<int>(0);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToList;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerFromList;
    }

    void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayerToList;
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayerFromList;
    }

    void AddPlayerToList(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log($"{clientId} connected...");
            playersInGame.Value++;
        }

    }

    void RemovePlayerFromList(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log($"{clientId} disconnected...");
            playersInGame.Value--;
        }
    }

}
