using UnityEngine.UI;
using UnityEngine;

public class EnemyIsometricUIManager : MonoBehaviour
{
    [SerializeField] Image enemyHealthbarFill;
    [SerializeField] GameObject enemyHealthbarContainer;

    EnemyNetworkHealth enemyNetworkHealth;

    void Update()
    {
        if (enemyNetworkHealth == null)
        {
            return;
        }

        enemyHealthbarContainer.transform.position = enemyNetworkHealth.transform.position + new Vector3(0, 2f, 0);
    }
    public void SetEnemy(GameObject enemy)
    {
        enemyNetworkHealth = enemy.GetComponent<EnemyNetworkHealth>();
        enemyNetworkHealth.CurrentHealth.OnValueChanged += UpdateHealthBar;
    }

    void UpdateHealthBar(float oldHealth, float newHealth)
    {
        enemyHealthbarFill.fillAmount = newHealth / enemyNetworkHealth.MaxHealth;
        if (newHealth <= 0)
        {
            enemyHealthbarFill.fillAmount = 1;
        }
    }
}
