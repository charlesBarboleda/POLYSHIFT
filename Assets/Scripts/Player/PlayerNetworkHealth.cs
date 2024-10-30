using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkHealth : NetworkBehaviour, IDamageable
{
    const float DefaultHealth = 100f;
    const float DefaultRegenRate = 1f;

    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(DefaultHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(DefaultHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> healthRegenRate = new NetworkVariable<float>(DefaultRegenRate, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    private void RegenerateHealth(float regenAmount)
    {
        currentHealth.Value += regenAmount * Time.deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        if (IsServer)
        {
            float newHealth = Mathf.Max(currentHealth.Value - damage, 0f);
            currentHealth.Value = newHealth;
        }
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


}
