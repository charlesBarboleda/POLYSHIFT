using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyHealthbarsManager : MonoBehaviour
{
    [SerializeField] Dictionary<GameObject, GameObject> enemyHealthbars = new Dictionary<GameObject, GameObject>();
    public void OnEnemySpawned(Enemy enemy)
    {
        // Spawn health bar for the enemy
        GameObject healthBarInstance = ObjectPooler.Instance.Spawn("IsometricEnemyHealth", transform.position, Quaternion.identity);
        EnemyIsometricUIManager isometricUIManager = healthBarInstance.GetComponent<EnemyIsometricUIManager>();

        // Add the health bar to the dictionary
        enemyHealthbars[enemy.gameObject] = healthBarInstance;

        // Bind the health bar to the enemy
        isometricUIManager.SetEnemy(enemy.gameObject);
    }

    public void OnEnemyDespawned(Enemy enemy)
    {
        // Despawn health bar for the enemy
        enemyHealthbars[enemy.gameObject].GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn("IsometricEnemyHealth", enemyHealthbars[enemy.gameObject]);
        enemyHealthbars.Remove(enemy.gameObject);
    }
}
