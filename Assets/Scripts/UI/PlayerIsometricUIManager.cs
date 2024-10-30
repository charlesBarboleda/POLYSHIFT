using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;

public class PlayerIsometricUIManager : MonoBehaviour
{
    [SerializeField] private Image healthbarFill;
    [SerializeField] private GameObject healthbarContainer;
    private ulong clientId;
    private PlayerNetworkHealth playerHealth;

    void Update()
    {
        if (playerHealth == null)
        {
            TryAssignPlayerHealth();
            return;
        }

        healthbarContainer.transform.position = playerHealth.transform.position + new Vector3(0, 2.5f, 0);
    }

    public void SetClientPlayer(ulong clientId)
    {
        this.clientId = clientId;
        TryAssignPlayerHealth(); // Try assigning the player reference
    }

    // Subscribe to the player health change event
    private void TryAssignPlayerHealth()
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
                break;
            }
        }
    }

    private void UpdateHealthbar(float previous, float current)
    {
        if (playerHealth != null)
        {
            healthbarFill.fillAmount = playerHealth.currentHealth.Value / playerHealth.maxHealth.Value;
        }
    }
}
