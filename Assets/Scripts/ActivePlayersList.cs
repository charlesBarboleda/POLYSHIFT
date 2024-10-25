using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ActivePlayersList : NetworkBehaviour
{
    public static ActivePlayersList Instance;
    List<PlayerNetworkHealth> alivePlayers = new List<PlayerNetworkHealth>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void RegisterPlayer(PlayerNetworkHealth player)
    {
        if (!alivePlayers.Contains(player))
        {
            alivePlayers.Add(player);
        }
    }

    public void UnregisterPlayer(PlayerNetworkHealth player)
    {
        if (alivePlayers.Contains(player))
        {
            alivePlayers.Remove(player);
        }
    }

    public List<PlayerNetworkHealth> GetAlivePlayers(ulong deadPlayerClientId)
    {
        // Return all alive players except the dead player
        return alivePlayers.FindAll(p => p.OwnerClientId != deadPlayerClientId && p.CurrentHealth.Value > 0);
    }

    public List<PlayerNetworkHealth> GetAlivePlayers()
    {
        return alivePlayers;
    }
}
