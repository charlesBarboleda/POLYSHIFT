using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;

public class EnemyNetworkHealth : NetworkBehaviour, IDamageable
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> HealthRegenRate = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] string enemyName;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth.Value;
        }
        EventManager.Instance.EnemySpawnedEvent(gameObject);

    }

    void Update()
    {
        if (!IsServer) return;

        if (CurrentHealth.Value < MaxHealth.Value)
        {
            CurrentHealth.Value += HealthRegenRate.Value * Time.deltaTime;
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        {
            CurrentHealth.Value -= damage;
            if (CurrentHealth.Value <= 0)
            {
                HandleDeathClientRpc();
            }
        }
    }

    public void TakeDamage(float damage, ulong attackerId)
    {
        TakeDamage(damage);
        Debug.Log("Enemy was attacked by " + attackerId);
    }

    [ClientRpc]
    public void HandleDeathClientRpc()
    {
        HandleDeath();
    }
    public void HandleDeath()
    {
        Debug.Log("Handle death");
        NetworkObjectPool.Instance.ReturnNetworkObject(gameObject.GetComponent<NetworkObject>(), enemyName);
        EventManager.Instance.EnemyDespawnedEvent(gameObject);
    }
}
