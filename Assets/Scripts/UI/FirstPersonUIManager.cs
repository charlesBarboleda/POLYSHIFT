using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using DG.Tweening;
public class FirstPersonUIManager : NetworkBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoCountText;
    [SerializeField] private PlayerWeapon playerWeapon;

    private PlayerNetworkHealth playerHealth;

    public override void OnNetworkSpawn()
    {
        // Only proceed if this object is owned by the local client
        playerHealth = GetComponentInParent<PlayerNetworkHealth>();

        if (playerHealth != null && playerHealth.IsOwner)
        {

            // Subscribe to health changes for this player instance only
            playerHealth.currentHealth.OnValueChanged += UpdateHealthUI;
            playerHealth.maxHealth.OnValueChanged += UpdateHealthUI;


            // Initialize health UI with current values
            UpdateHealthUI(0, playerHealth.currentHealth.Value);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        // Clean up subscriptions if this component is disabled
        if (playerHealth != null)
        {
            playerHealth.currentHealth.OnValueChanged -= UpdateHealthUI;
            playerHealth.maxHealth.OnValueChanged -= UpdateHealthUI;
        }
    }


    private void UpdateHealthUI(float previousHealth, float newHealth)
    {
        if (playerHealth != null)
        {
            float targetFill = newHealth / playerHealth.maxHealth.Value;
            healthFill.DOFillAmount(targetFill, 0.5f).SetEase(Ease.OutSine);


            healthText.text = $"{Mathf.Round(newHealth)} / {playerHealth.maxHealth.Value}";
        }
    }
}
