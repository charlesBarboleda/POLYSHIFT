using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerManagerUI : MonoBehaviour
{
    [SerializeField] Button spawnEnemyBtn;
    [SerializeField] Button spawnEnemiesBtn;
    [SerializeField] List<string> enemyNames;



    void Start()
    {
        spawnEnemiesBtn.onClick.AddListener(OnSpawnEnemyBtnClicked);
        spawnEnemyBtn.onClick.AddListener(OnSpawnSingleEnemyBtnClicked);
    }

    void OnSpawnEnemyBtnClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Spawn enemies continuously in a circle around the player
            StartCoroutine(SpawnEnemies());
        }
        else
        {
            Debug.LogError("Trying to spawn enemy from client, but this should only happen on the server.");
        }
    }

    void OnSpawnSingleEnemyBtnClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject enemy = ObjectPooler.Instance.Spawn(enemyNames[Random.Range(0, enemyNames.Count)], Vector3.zero, Quaternion.identity);
            enemy.transform.position = new Vector3(0, 1, 0);
            enemy.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            Debug.LogError("Trying to spawn enemy from client, but this should only happen on the server.");
        }
    }

    IEnumerator SpawnEnemies()
    {
        int numOfEnemies = 100;
        while (numOfEnemies-- > 0)
        {
            GameObject enemy = ObjectPooler.Instance.Spawn(enemyNames[Random.Range(0, enemyNames.Count)], Vector3.zero, Quaternion.identity);
            enemy.transform.position = new Vector3(Random.Range(-100, 100), 1, Random.Range(-100, 100));
            enemy.GetComponent<NetworkObject>().Spawn();
            yield return new WaitForSeconds(2f);
        }

    }


}
