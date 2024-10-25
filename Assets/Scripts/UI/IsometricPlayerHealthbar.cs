using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;
using Netcode.Extensions;

public class IsometricPlayerHealthbar : MonoBehaviour
{
    [SerializeField] Image healthbarFill;
    [SerializeField] Transform player;
    [SerializeField] GameObject healthbarContainer;
    [SerializeField] PlayerNetworkHealth playerNetworkHealth;
    [SerializeField] PlayerCameraBehavior playerCameraBehavior; // Reference to the camera behavior

    // Offset for positioning the health bar above the player's head
    Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);

    void OnEnable()
    {
        playerNetworkHealth.currentHealth.OnValueChanged += OnHealthChanged;
        playerNetworkHealth.maxHealth.OnValueChanged += OnHealthChanged;

        playerCameraBehavior.MainCamera = Camera.main; // Set the main camera reference
    }

    void OnDisable()
    {
        playerNetworkHealth.currentHealth.OnValueChanged -= OnHealthChanged;
        playerNetworkHealth.maxHealth.OnValueChanged -= OnHealthChanged;
    }

    void Update()
    {
        if (player == null || playerNetworkHealth.currentHealth.Value <= 0)
        {
            // If the player is null, return the health bar to the object pool
            NetworkObjectPool.Instance.ReturnNetworkObject(gameObject.GetComponentInParent<NetworkObject>(), "IsometricPlayerHealthbar");
            return;
        }

        UpdateHealthbarPosition(); // Move the health bar to follow the player
        UpdateHealthbarRotation(); // Rotate the health bar to face the camera
    }

    // Update the health bar's position in world space
    void UpdateHealthbarPosition()
    {
        // Directly set the health bar's position in world space (no need for screen space conversion)
        healthbarContainer.transform.position = player.transform.position + healthBarOffset;
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
        healthbarFill.fillAmount = playerNetworkHealth.currentHealth.Value / playerNetworkHealth.maxHealth.Value;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
}
