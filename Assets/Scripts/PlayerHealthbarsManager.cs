using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;
using Netcode.Extensions;


public class PlayerHealthbarsManager : NetworkBehaviour
{
    Dictionary<ulong, GameObject> playerHealthbars = new Dictionary<ulong, GameObject>();

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
            GameObject healthBarInstance = NetworkObjectPool.Instance.GetNetworkObject("IsometricPlayerHealth").gameObject;
            healthBarInstance.transform.SetParent(playerObject.transform, false);
            healthBarInstance.transform.localPosition = new Vector3(0, 1.5f, 0); // Adjust position above player

            playerHealthbars[clientId] = healthBarInstance;
        }
    }
}
