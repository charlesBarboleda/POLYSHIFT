using UnityEngine;

public class EnemyHealthbarsManager : MonoBehaviour
{
    public void OnEnemySpawned(GameObject enemy)
    {
        // Spawn health bar for the enemy
        GameObject healthBarInstance = ObjectPooler.Generate("IsometricEnemyHealth");
        EnemyIsometricUIManager isometricUIManager = healthBarInstance.GetComponent<EnemyIsometricUIManager>();

        // Bind the health bar to the enemy
        isometricUIManager.SetEnemy(enemy);
    }
}
