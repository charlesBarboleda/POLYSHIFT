using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerManagerUI : MonoBehaviour
{
    [SerializeField] Button spawnEnemyBtn;



    void Start()
    {
        spawnEnemyBtn.onClick.AddListener(OnSpawnEnemyBtnClicked);
    }

    void OnSpawnEnemyBtnClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject enemy = NetworkObjectPool.Instance.GetNetworkObject("MeleeZombieEnemy").gameObject;
            enemy.GetComponent<NetworkObject>().Spawn();
            // enemy.GetComponent<Rigidbody>().isKinematic = false;

            // Debug log to confirm the enemy is being spawned by the server
            Debug.Log("Enemy spawned by server: " + enemy.name);
        }
        else
        {
            Debug.LogError("Trying to spawn enemy from client, but this should only happen on the server.");
        }
    }

}
