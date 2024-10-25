using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour, IDamageable
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(100f);
    public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(100f);
    public NetworkVariable<float> HealthRegenRate = new NetworkVariable<float>(1f);
    [SerializeField] string enemyName;
    void Start()
    {
        if (!IsServer) return;

        CurrentHealth.Value = MaxHealth.Value;
    }

    void Update()
    {
        if (!IsServer) return;

        if (CurrentHealth.Value < MaxHealth.Value)
        {
            CurrentHealth.Value += HealthRegenRate.Value * Time.deltaTime;
        }

    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        {
            CurrentHealth.Value -= damage;
            if (CurrentHealth.Value <= 0)
            {
                HandleDeath();
            }
        }
    }

    public void TakeDamage(float damage, ulong attackerId)
    {
        TakeDamage(damage);
        Debug.Log("Enemy was attacked by " + attackerId);
    }

    public void HandleDeath()
    {
        Debug.Log("Enemy died");
        NetworkObjectPool.Instance.ReturnNetworkObject(gameObject.GetComponent<NetworkObject>(), enemyName);
    }
}
