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
    public float health = 1f;

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float healAmount)
    {
        health += healAmount;
    }
    public void TakeDamage(float damage, ulong instigator)
    {
        health -= 1;
        if (health <= 0)
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
