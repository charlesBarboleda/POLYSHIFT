using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class IsometricEnemyHealthbar : MonoBehaviour
{
    [SerializeField] Image healthbarFill;
    [SerializeField] Transform enemy;
    [SerializeField] GameObject healthbarContainer;
    EnemyHealth enemyHealth;
    [SerializeField] PlayerCameraBehavior playerCameraBehavior; // Reference to the camera behavior

    // Offset for positioning the health bar above the player's head
    Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);

    void OnEnable()
    {
        playerCameraBehavior.MainCamera = Camera.main; // Set the main camera reference
    }

    void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.CurrentHealth.OnValueChanged -= OnHealthChanged;
            enemyHealth.MaxHealth.OnValueChanged -= OnHealthChanged;
        }
    }

    void Update()
    {
        if (enemy == null || enemyHealth.CurrentHealth.Value <= 0)
        {
            // If the player is null, return the health bar to the object pool


            NetworkObject networkObject = gameObject.GetComponentInParent<NetworkObject>();
            NetworkObjectPool.Instance.ReturnNetworkObject(networkObject, "IsometricEnemyHealthbar");
            networkObject.Despawn(true);

            return;
        }

        UpdateHealthbarPosition(); // Move the health bar to follow the player
        UpdateHealthbarRotation(); // Rotate the health bar to face the camera
    }

    // Update the health bar's position in world space
    void UpdateHealthbarPosition()
    {
        // Directly set the health bar's position in world space (no need for screen space conversion)
        healthbarContainer.transform.position = enemy.transform.position + healthBarOffset;
    }

    void UpdateHealthbarRotation()
    {
        // Make the health bar face the camera (LookAt ensures it always faces the player’s POV)
        healthbarContainer.transform.LookAt(playerCameraBehavior.MainCamera.transform.position);

        // Since LookAt might result in an upside-down UI, let's ensure the healthbar is always upright
        healthbarContainer.transform.Rotate(0, 180, 0); // Flip the UI to face the correct direction
    }

    // Update the health bar fill amount based on the player's health
    void OnHealthChanged(float previousValue, float newValue)
    {
        healthbarFill.fillAmount = enemyHealth.CurrentHealth.Value / enemyHealth.MaxHealth.Value;
    }

    public void SetPlayer(Transform targetTransform, EnemyHealth enemyHealth)
    {
        enemy = targetTransform;
        this.enemyHealth = enemyHealth;
        enemyHealth.CurrentHealth.OnValueChanged += OnHealthChanged;
        enemyHealth.MaxHealth.OnValueChanged += OnHealthChanged;
    }

}