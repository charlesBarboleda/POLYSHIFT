using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    public UnityEvent<PlayerNetworkHealth> OnPlayerSpawnReference = new UnityEvent<PlayerNetworkHealth>();
    public UnityEvent<PlayerNetworkHealth> OnPlayerDeath = new UnityEvent<PlayerNetworkHealth>();
    public UnityEvent<bool> OnPerspectiveChange = new UnityEvent<bool>();
    public UnityEvent<Enemy> OnEnemySpawned = new UnityEvent<Enemy>();
    public UnityEvent<Enemy> OnEnemyDespawned = new UnityEvent<Enemy>();

    // List to keep track of all player IDs


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlayerDeathEvent(PlayerNetworkHealth player)
    {
        OnPlayerDeath.Invoke(player);
    }
    public void PlayerSpawnReferenceEvent(PlayerNetworkHealth player)
    {

        OnPlayerSpawnReference.Invoke(player);
    }

    public void PerspectiveChange(bool IsIsometric)
    {
        OnPerspectiveChange.Invoke(IsIsometric);
    }

    public void EnemySpawnedEvent(Enemy enemy)
    {
        OnEnemySpawned.Invoke(enemy);
    }

    public void EnemyDespawnedEvent(Enemy enemy)
    {
        OnEnemyDespawned.Invoke(enemy);
    }
}
