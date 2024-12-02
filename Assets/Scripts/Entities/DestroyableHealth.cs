using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using Pathfinding;
using DestroyIt;

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

    private Destructible destructible;

    private void Awake()
    {
        destructible = GetComponent<Destructible>();
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
        destructible?.ApplyDamage(1);

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
        // Notify the Destructible script to handle destruction effects

        // Additional destruction logic
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

        // Update pathfinding graphs (if using A* Pathfinding)
        UpdateGraphs();

        // Despawn and destroy the object
        GetComponent<NetworkObject>().Despawn(true);
        destructible?.Destroy();
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
