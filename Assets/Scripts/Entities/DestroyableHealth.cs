using Unity.Netcode;
using UnityEngine;
using DestroyIt;
using Pathfinding;

public class DestroyableHealth : NetworkBehaviour, IDamageable
{
    public enum DestroyableSize { Small, Medium, Large }

    public DestroyableSize destroyableSize;
    public float MaxHealth;
    public NetworkVariable<float> Health = new NetworkVariable<float>();

    private Destructible destructible;
    void Awake()
    {
        destructible = GetComponent<Destructible>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Health.Value = MaxHealth; // Initialize health on the server
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float healAmount)
    {
        Health.Value += healAmount;
    }

    public void TakeDamage(float damage, ulong instigator)
    {
        if (!IsServer)
            return;

        Health.Value -= 1;
        destructible?.ApplyDamage(1);

        if (Health.Value <= 0)
        {
            HandleDeath(instigator);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage, ulong instigator)
    {
        TakeDamage(damage, instigator);
    }

    public void HandleDeath(ulong instigator)
    {
        if (!IsServer) return;

        Debug.Log("Destroyable handle death started");
        var networkObject = GetComponent<NetworkObject>();
        if (networkObject == null || !networkObject.IsSpawned) return;

        destructible?.Destroy();

        switch (destroyableSize)
        {
            case DestroyableSize.Small:
                SpawnEffect("SmallDebrisDestruction");
                break;
            case DestroyableSize.Medium:
                SpawnEffect("MidDebrisDestruction");
                break;
            case DestroyableSize.Large:
                SpawnEffect("LargeDebrisDestruction");
                break;
        }

        UpdateGraphs();

        if (networkObject.IsSpawned)
            networkObject.Despawn(false);
    }

    private void SpawnEffect(string effectName)
    {
        GameObject effect = ObjectPooler.Instance.Spawn(effectName, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }

    private void UpdateGraphs()
    {
        Bounds bounds = GetComponent<Collider>().bounds;
        var guo = new GraphUpdateObject(bounds) { updatePhysics = true };
        AstarPath.active.UpdateGraphs(guo);
    }
}
