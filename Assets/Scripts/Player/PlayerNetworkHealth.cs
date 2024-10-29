using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkHealth : NetworkBehaviour, IDamageable
{
    const float DefaultHealth = 100f;
    const float DefaultRegenRate = 1f;

    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(DefaultHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(DefaultHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> healthRegenRate = new NetworkVariable<float>(DefaultRegenRate, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] IsometricUIManager isometricUIManager;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth.Value;
        }

    }

    private void Update()
    {
        if (!IsServer) return;

        if (currentHealth.Value < maxHealth.Value)
        {
            RegenerateHealth(healthRegenRate.Value);
        }

        if (currentHealth.Value <= 0)
        {
            HandleDeath();
        }
    }

    void OnDisable()
    {
        currentHealth.OnValueChanged -= (previous, current) => isometricUIManager.UpdateHealthbarClientRpc();

    }

    private void RegenerateHealth(float regenAmount)
    {
        currentHealth.Value += regenAmount * Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        if (IsServer)
        {
            float newHealth = Mathf.Max(currentHealth.Value - damage, 0f);
            currentHealth.Value = newHealth;
        }
    }

    public void TakeDamage(float damage, ulong attackerId)
    {
        TakeDamage(damage);
        Debug.Log($"Player was attacked by {attackerId}");
    }

    public void HandleDeath()
    {
        Debug.Log("Player died");
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        currentHealth.Value = maxHealth.Value;
        gameObject.SetActive(true);
    }

    public void SetIsometricUI(IsometricUIManager isometricUIManager)
    {
        this.isometricUIManager = isometricUIManager;
        currentHealth.OnValueChanged += (previous, current) => isometricUIManager.UpdateHealthbarClientRpc();
        Debug.Log("Set Isometric UI subscribed to health changes");
    }
}
