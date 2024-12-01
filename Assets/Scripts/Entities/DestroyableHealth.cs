using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using Pathfinding;

public class DestroyableHealth : NetworkBehaviour, IDamageable
{
    public enum DestroyableSize
    {
        Small,
        Medium,
        Large
    }
    public DestroyableSize destroyableSize;
    public float MaxHealth;
    public VariableWithEvent<float> health = new VariableWithEvent<float>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health.Value = MaxHealth;
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float healAmount)
    {
        health.Value += healAmount;
    }
    public void TakeDamage(float damage, ulong instigator)
    {
        health.Value -= 1;
        if (health.Value <= 0)
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
        switch (destroyableSize)
        {
            case DestroyableSize.Small:
                GameObject effect = ObjectPooler.Instance.Spawn("SmallDebrisDestruction", transform.position, Quaternion.identity);
                effect.GetComponent<NetworkObject>().Spawn();
                break;
            case DestroyableSize.Medium:
                GameObject effect2 = ObjectPooler.Instance.Spawn("MidDebrisDestruction", transform.position, Quaternion.identity);
                effect2.GetComponent<NetworkObject>().Spawn();
                break;
            case DestroyableSize.Large:
                GameObject effect3 = ObjectPooler.Instance.Spawn("LargeDebrisDestruction", transform.position, Quaternion.identity);
                effect3.GetComponent<NetworkObject>().Spawn();
                break;
        }
        Bounds bounds = GetComponent<Collider>().bounds;
        var guo = new GraphUpdateObject(bounds);
        guo.updatePhysics = true;
        AstarPath.active.UpdateGraphs(guo);
        GetComponent<NetworkObject>().Despawn(true);
        Destroy(gameObject);

    }
}
