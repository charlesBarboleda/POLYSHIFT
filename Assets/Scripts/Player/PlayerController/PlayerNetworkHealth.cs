using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkHealth : NetworkBehaviour, IDamageable
{
    const float DefaultHealth = 100f;
    const float DefaultRegenRate = 1f;

    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(DefaultHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(DefaultHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> healthRegenRate = new NetworkVariable<float>(DefaultRegenRate, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public float DamageReduction = 1;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth.Value;
            DamageReduction = 1;
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

    public void PermanentDamageReductionIncreaseBy(float damageReductionIncrease)
    {
        DamageReduction += damageReductionIncrease;
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
        DamageReduction = damageReduction;
        yield return new WaitForSeconds(duration);
        DamageReduction = 0;
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
            if (DamageReduction > 0)
            {
                damage = damage * DamageReduction;
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
