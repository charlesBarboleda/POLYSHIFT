using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkHealth : NetworkBehaviour, IDamageable
{
    const float DefaultHealth = 100f;
    const float DefaultRegenRate = 1f;

    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(DefaultHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(DefaultHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> healthRegenRate = new NetworkVariable<float>(DefaultRegenRate, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float DamageReduction = 0;
    bool ironResolve = false;
    float ironResolveDamageReduction = 0.15f;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentHealth.Value = maxHealth.Value;
        DamageReduction = 0;
        if (IsServer)
        {
            GameManager.Instance.SpawnedAllies.Add(gameObject);
        }



    }

    void Update()
    {
        if (!IsServer) return;

        if (currentHealth.Value < maxHealth.Value)
        {
            RegenerateHealth(healthRegenRate.Value);
        }

        if (currentHealth.Value <= 0)
        {
            HandleDeath(OwnerClientId);
        }
    }

    public void PermanentHealthIncreaseBy(float healthIncrease)
    {
        maxHealth.Value += healthIncrease;
        currentHealth.Value += healthIncrease;
    }

    public void PermanentHealthRegenIncreaseBy(float regenIncrease)
    {
        healthRegenRate.Value += regenIncrease;
    }

    public void UnlockIronResolve()
    {
        ironResolve = true;
    }

    public void IncreaseIronResolveDamageReduction(float amount)
    {
        ironResolveDamageReduction += amount;
    }

    public void PermanentDamageReductionIncreaseBy(float damageReductionIncrease)
    {
        if (damageReductionIncrease > 0)
        {
            // Increase damage reduction with diminishing returns
            DamageReduction = 1 - (1 - DamageReduction) * (1 - damageReductionIncrease);
        }
        else
        {
            // Decrease damage reduction by reversing the formula
            float reductionFactor = 1 + damageReductionIncrease; // Note: damageReductionIncrease is negative here
            DamageReduction = 1 - (1 - DamageReduction) / reductionFactor;
        }
    }




    void RegenerateHealth(float regenAmount)
    {
        currentHealth.Value += regenAmount * Time.deltaTime;
    }

    public void ReduceDamageTakenBy(float damageReduction, float duration)
    {
        StartCoroutine(ReduceDamageTakenCoroutine(damageReduction, duration));
    }

    IEnumerator ReduceDamageTakenCoroutine(float damageReduction, float duration)
    {
        float originalDamageReduction = DamageReduction;

        // Apply the modification with diminishing returns, accounting for both positive and negative values.
        if (damageReduction > 0)
        {
            // Increase damage reduction
            DamageReduction = 1 - (1 - DamageReduction) * (1 - damageReduction);
        }
        else
        {
            // Decrease damage reduction by effectively reversing the formula.
            float reductionFactor = 1 + damageReduction; // Note: damageReduction is negative here
            DamageReduction = 1 - (1 - DamageReduction) / reductionFactor;
        }

        yield return new WaitForSeconds(duration);

        // Restore the original DamageReduction value after the duration.
        DamageReduction = originalDamageReduction;
    }



    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage, ulong clientId)
    {
        TakeDamage(damage, clientId);
    }

    public void TakeDamage(float damage, ulong clientId)
    {
        if (IsServer)
        {
            // Check if the player is below 30% health to apply extra damage reduction
            if (currentHealth.Value / maxHealth.Value <= 0.3f && ironResolve)
            {
                // Apply an additional damage reduction boost when health is below 30%
                float lowHealthReduction = ironResolveDamageReduction; // Example: additional 20% reduction
                DamageReduction = 1 - (1 - DamageReduction) * (1 - lowHealthReduction);
            }

            // Calculate the effective damage after reduction
            if (DamageReduction < 1)
            {
                damage = damage * (1 - DamageReduction);
            }

            currentHealth.Value -= damage;

            if (currentHealth.Value <= 0)
            {
                HandleDeath(clientId);
            }
        }
    }

    public void HandleDeath(ulong clientId)
    {
        Debug.Log("Player died");
        if (IsServer)
        {
            GameManager.Instance.SpawnedAllies.Remove(gameObject);
            gameObject.SetActive(false);

        }
    }

    public void Respawn()
    {
        currentHealth.Value = maxHealth.Value;
        gameObject.SetActive(true);
    }


}
