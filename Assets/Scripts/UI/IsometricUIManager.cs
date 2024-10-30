using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;

public class IsometricUIManager : MonoBehaviour
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

    private void TryAssignPlayerHealth()
    {
        foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (networkObject.OwnerClientId == clientId)
            {
                playerHealth = networkObject.GetComponent<PlayerNetworkHealth>();
                if (playerHealth != null)
                {
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
