using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerIsometricUIManager : MonoBehaviour
{
    [SerializeField] private Image healthbarFill;
    [SerializeField] private Image experiencebarFill;
    [SerializeField] private GameObject healthbarContainer;
    [SerializeField] private TextMeshProUGUI levelText;
    private ulong clientId;
    private PlayerNetworkHealth playerHealth;
    private PlayerNetworkLevel playerLevel;

    void Update()
    {
        if (playerHealth == null)
        {
            TryAssignPlayerComponents();
            return;
        }

        healthbarContainer.transform.position = playerHealth.transform.position + new Vector3(0, 2.5f, 0);
        UpdateExperiencebar();
    }

    public void SetClientPlayer(ulong clientId)
    {
        this.clientId = clientId;
        TryAssignPlayerComponents(); // Try assigning the player reference
    }

    // Subscribe to the player health change event
    private void TryAssignPlayerComponents()
    {
        // Find the player with the given clientId
        foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            // Check if the network object is owned by the client
            if (networkObject.OwnerClientId == clientId)
            {
                // Assign the player health component
                playerHealth = networkObject.GetComponent<PlayerNetworkHealth>();
                if (playerHealth != null)
                {
                    // Subscribe to the health change event
                    playerHealth.currentHealth.OnValueChanged += UpdateHealthbar;
                    UpdateHealthbar(playerHealth.currentHealth.Value, playerHealth.currentHealth.Value); // Initial update
                }
                playerLevel = networkObject.GetComponent<PlayerNetworkLevel>();
                if (playerLevel != null)
                {
                    playerLevel.Level.OnValueChanged += UpdateLevelText;
                }

                break;
            }
        }
    }

    void UpdateExperiencebar()
    {
        if (playerLevel != null)
        {
            experiencebarFill.fillAmount = playerLevel.Experience.Value / playerLevel.NeededExperience.Value;
        }
    }

    void UpdateLevelText(int previous, int current)
    {
        levelText.text = current.ToString();
    }

    private void UpdateHealthbar(float previous, float current)
    {
        if (playerHealth != null)
        {
            healthbarFill.fillAmount = playerHealth.currentHealth.Value / playerHealth.maxHealth.Value;
        }
    }
}
