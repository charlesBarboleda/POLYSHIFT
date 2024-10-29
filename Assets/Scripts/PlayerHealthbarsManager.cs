using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;
using Netcode.Extensions;
using System.Collections;


public class PlayerHealthbarsManager : NetworkBehaviour
{
    Dictionary<ulong, NetworkObject> playerHealthbars = new Dictionary<ulong, NetworkObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Listen for when new players join
            NetworkManager.OnClientConnectedCallback += SpawnHealthBarForPlayer;
        }
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= SpawnHealthBarForPlayer;
        }
    }

    private void SpawnHealthBarForPlayer(ulong clientId)
    {
        if (!IsServer) return;

        NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (playerObject != null)
        {
            NetworkObject healthBarInstance = NetworkObjectPool.Instance.GetNetworkObject("IsometricPlayerHealth");
            healthBarInstance.SpawnWithOwnership(clientId);
            IsometricUIManager isometricUIManager = healthBarInstance.GetComponent<IsometricUIManager>();
            playerObject.GetComponent<PlayerNetworkHealth>().SetIsometricUI(isometricUIManager);
            isometricUIManager.SetPlayer(playerObject);

            playerHealthbars[clientId] = healthBarInstance;
        }
    }
}