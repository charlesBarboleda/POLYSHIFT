using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthbarsManager : MonoBehaviour
{
    [SerializeField] Dictionary<GameObject, GameObject> enemyHealthbars = new Dictionary<GameObject, GameObject>();
    public void OnEnemySpawned(GameObject enemy)
    {
        // Spawn health bar for the enemy
        GameObject healthBarInstance = ObjectPooler.Generate("IsometricEnemyHealth");
        EnemyIsometricUIManager isometricUIManager = healthBarInstance.GetComponent<EnemyIsometricUIManager>();

        // Add the health bar to the dictionary
        enemyHealthbars[enemy] = healthBarInstance;

        // Bind the health bar to the enemy
        isometricUIManager.SetEnemy(enemy);
    }

    public void OnEnemyDespawned(GameObject enemy)
    {
        // Despawn health bar for the enemy
        ObjectPooler.Destroy(enemyHealthbars[enemy]);
        enemyHealthbars.Remove(enemy);
    }
}
