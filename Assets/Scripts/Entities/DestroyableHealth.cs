using Unity.Netcode;
using UnityEngine;
using DestroyIt;
using Pathfinding;
using System.Collections;

public class DestroyableHealth : NetworkBehaviour, IDamageable
{
    public enum DestroyableSize { Small, Medium, Large }

    public DestroyableSize destroyableSize;
    public float MaxHealth;
    public NetworkVariable<float> Health = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Destructible destructible;

    void Awake()
    {
        destructible = GetComponent<Destructible>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Health.Value = MaxHealth; // Initialize health on the server.

        destructible?.SyncHealth(Health.Value); // Sync Destructible health.

        // Register OnValueChanged to update Destructible component.
        Health.OnValueChanged += (prev, current) =>
        {
            Debug.Log($"[Client] Health changed: {prev} -> {current}");
            destructible?.SyncHealth(current);
        };
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

        // Reduce health value on the server.
        float newValue = Health.Value - 1;
        Debug.Log($"TakeDamage: {gameObject.name} health from {Health.Value} to {newValue}");

        Health.Value = newValue; // Synchronize health with the network.
        destructible?.SyncHealth(newValue); // Sync health with the destructible system.

        if (Health.Value <= 0)
        {
            Health.SetDirty(true); // Mark the health as dirty to ensure sync.
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

        // Ensure the object is marked as destroyed in the Destructible system.
        destructible?.Destroy();

        // Remove all child objects
        DestroyChildren();

        // Spawn the destruction effect based on the size
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

        UpdateGraphs(); // Update pathfinding graphs.

        // Notify clients to despawn the object and handle the server-side despawn.
        NotifyClientsToDespawn();
    }

    private void DestroyChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void NotifyClientsToDespawn()
    {
        // Notify clients to despawn the object.
        NotifyDespawnClientRpc();

        // Delay the server-side despawn to ensure synchronization.
        StartCoroutine(DelayedDespawn());
    }

    [ClientRpc]
    private void NotifyDespawnClientRpc()
    {
        Debug.Log($"[Client] Despawning object: {gameObject.name}");

        // Destroy the GameObject and its children on the client side.
        DestroyObjectLocally();
    }

    private void DestroyObjectLocally()
    {
        // Destroy all child objects
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Destroy the parent object
        Destroy(gameObject);
    }

    private IEnumerator DelayedDespawn()
    {
        yield return null; // Wait one frame for client-side synchronization.

        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            Debug.Log($"[Server] Despawning {gameObject.name}.");
            networkObject.Despawn(false);
        }
    }

    private void SpawnEffect(string effectName)
    {
        // Spawn the destruction effect using the object pooler.
        GameObject effect = ObjectPooler.Instance.Spawn(effectName, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }

    private void UpdateGraphs()
    {
        // Update the pathfinding graphs to reflect the destruction.
        Bounds bounds = GetComponent<Collider>().bounds;
        var guo = new GraphUpdateObject(bounds) { updatePhysics = true };
        AstarPath.active.UpdateGraphs(guo);
    }
}
