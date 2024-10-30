using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerHealthbarsManager : NetworkBehaviour
{
    private Dictionary<ulong, GameObject> playerHealthbars = new Dictionary<ulong, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Gather all client IDs into an array
        List<ulong> allClientIdsList = new List<ulong>();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            allClientIdsList.Add(client.ClientId);
        }
        ulong[] allClientIds = allClientIdsList.ToArray(); // Convert to array

        // Call the ClientRpc to spawn health bars on each client for all players
        SpawnHealthBarsForAllClientsClientRpc(allClientIds);
    }

    [ClientRpc]
    private void SpawnHealthBarsForAllClientsClientRpc(ulong[] allClientIds)
    {
        foreach (var clientId in allClientIds)
        {
            if (!playerHealthbars.ContainsKey(clientId))
            {
                // Each client spawns a health bar for each player
                GameObject healthBarInstance = ObjectPooler.Generate("IsometricPlayerHealth");
                IsometricUIManager isometricUIManager = healthBarInstance.GetComponent<IsometricUIManager>();

                // Bind the health bar to the player
                isometricUIManager.SetClientPlayer(clientId); // Using clientId to set up player reference
                playerHealthbars[clientId] = healthBarInstance;
            }
        }
    }
}
