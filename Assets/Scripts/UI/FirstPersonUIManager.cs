using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class FirstPersonUIManager : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoCountText;
    [SerializeField] private PlayerWeapon playerWeapon;

    private PlayerNetworkHealth playerHealth;

    void Start()
    {
        // Only proceed if this object is owned by the local client
        playerHealth = GetComponentInParent<PlayerNetworkHealth>();

        if (playerHealth != null && playerHealth.IsOwner)
        {
            Debug.Log("Subscribing to health changes");
            // Subscribe to health changes for this player instance only
            playerHealth.currentHealth.OnValueChanged += UpdateHealthUI;
            playerHealth.maxHealth.OnValueChanged += UpdateHealthUI;
            Debug.Log("Subscribed to health changes");

            // Initialize health UI with current values
            UpdateHealthUI(0, playerHealth.currentHealth.Value);
        }
        else
        {
            Debug.Log("Setting health UI inactive");
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerWeapon != null)
        {
            UpdateAmmoUI(0, playerWeapon.currentAmmoCount);
        }
    }
    private void OnDisable()
    {
        // Clean up subscriptions if this component is disabled
        if (playerHealth != null)
        {
            playerHealth.currentHealth.OnValueChanged -= UpdateHealthUI;
            playerHealth.maxHealth.OnValueChanged -= UpdateHealthUI;
        }
    }

    private void UpdateAmmoUI(int previousAmmo, int newAmmo)
    {
        if (playerWeapon != null)
        {
            ammoCountText.text = $"{newAmmo} / {playerWeapon.maxAmmoCount}";
        }
    }

    private void UpdateHealthUI(float previousHealth, float newHealth)
    {
        if (playerHealth != null)
        {
            Debug.Log($"Updating health UI: {newHealth} / {playerHealth.maxHealth.Value}");
            healthFill.fillAmount = newHealth / playerHealth.maxHealth.Value;
            healthText.text = $"{Mathf.Round(newHealth)} / {playerHealth.maxHealth.Value}";
            Debug.Log("Updated health UI");
        }
    }
}
