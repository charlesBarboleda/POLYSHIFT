using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

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

        enemyHealthbarContainer.transform.position = enemyNetworkHealth.transform.position + transform.up * (enemyNetworkHealth.transform.localScale.y + 1f);
    }
    public void SetEnemy(GameObject enemy)
    {
        enemyNetworkHealth = enemy.GetComponent<EnemyNetworkHealth>();
        enemyNetworkHealth.CurrentHealth.OnValueChanged += UpdateHealthBar;
    }

    void UpdateHealthBar(float oldHealth, float newHealth)
    {
        if (enemyHealthbarFill != null)
        {
            float fillAmount = enemyNetworkHealth.CurrentHealth.Value / enemyNetworkHealth.MaxHealth;
            enemyHealthbarFill.DOFillAmount(fillAmount, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
