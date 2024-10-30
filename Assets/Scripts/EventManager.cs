using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    public UnityEvent<PlayerNetworkHealth> OnPlayerSpawnReference = new UnityEvent<PlayerNetworkHealth>();
    public UnityEvent<bool> OnPerspectiveChange = new UnityEvent<bool>();
    public UnityEvent<GameObject> OnEnemySpawned = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> OnEnemyDespawned = new UnityEvent<GameObject>();

    // List to keep track of all player IDs


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlayerSpawnReferenceEvent(PlayerNetworkHealth player)
    {

        OnPlayerSpawnReference.Invoke(player);
    }

    public void PerspectiveChange(bool isIsometric)
    {
        OnPerspectiveChange.Invoke(isIsometric);
    }

    public void EnemySpawnedEvent(GameObject enemy)
    {
        OnEnemySpawned.Invoke(enemy);
    }

    public void EnemyDespawnedEvent(GameObject enemy)
    {
        OnEnemyDespawned.Invoke(enemy);
    }
}
