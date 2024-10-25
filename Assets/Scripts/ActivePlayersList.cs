using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ActivePlayersList : NetworkBehaviour
{
    public static ActivePlayersList Instance;
    [SerializeField] List<PlayerNetworkHealth> _alivePlayers = new List<PlayerNetworkHealth>();
    [SerializeField] List<PlayerNetworkHealth> _deadPlayers = new List<PlayerNetworkHealth>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void RegisterPlayer(PlayerNetworkHealth player)
    {
        if (!_alivePlayers.Contains(player))
        {
            _alivePlayers.Add(player);
        }

        if (_deadPlayers.Contains(player))
        {
            _deadPlayers.Add(player);
        }
    }

    public void UnregisterPlayer(PlayerNetworkHealth player)
    {
        if (_alivePlayers.Contains(player))
        {
            _alivePlayers.Remove(player);
        }

        if (_deadPlayers.Contains(player))
        {
            _deadPlayers.Add(player);
        }
    }

    public List<PlayerNetworkHealth> GetAlivePlayer(ulong deadPlayerClientId)
    {
        // Return all alive players except the dead player
        return _alivePlayers.FindAll(p => p.OwnerClientId != deadPlayerClientId && p.currentHealth.Value > 0);
    }

    public List<PlayerNetworkHealth> GetAlivePlayers()
    {
        return _alivePlayers;
    }
}
