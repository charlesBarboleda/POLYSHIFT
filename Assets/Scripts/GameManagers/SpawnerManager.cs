using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnerManager : NetworkBehaviour
{
    public static SpawnerManager Instance { get; private set; }
    public List<Enemy> EnemiesToSpawn = new List<Enemy>();
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {

    }




}
