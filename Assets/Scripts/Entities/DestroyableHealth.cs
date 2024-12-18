using Unity.Netcode;
using UnityEngine;
using DestroyIt;
using Pathfinding;
using System.Collections;
using System.Linq;

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
        float newValue = Health.Value - damage;
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

        Debug.Log("[Server] Destructible Destroy called");

        // Ensure the object is marked as destroyed in the Destructible system.
        destructible?.Destroy();
        Debug.Log("[Server] Destructible Destroy completed");
        Debug.Log("[Server] Spawning destruction effect");
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
        Debug.Log("[Server] Destruction effect spawned");

        Debug.Log("[Server] Updating graphs");

        UpdateGraphs(); // Update pathfinding graphs.
        Debug.Log("[Server] Graphs updated");

        Debug.Log("[Server] Notifying clients to despawn");

        // Notify clients to despawn the object and handle the server-side despawn.
        NotifyClientsToDespawn();
        Debug.Log("[Server] Clients notified to despawn");
    }

    private void NotifyClientsToDespawn()
    {
        Debug.Log("[Server] Notifying Clients to Despawn");
        // Notify clients to despawn the object.
        NotifyDespawnClientRpc();

        // Delay the server-side despawn to ensure synchronization.
        StartCoroutine(DelayedDespawn());
    }

    [ClientRpc]
    private void NotifyDespawnClientRpc()
    {
        Debug.Log($"[Client] Despawning object: {gameObject.name}");
        DestroyObjectLocally();
    }

    private void DestroyObjectLocally()
    {
        Debug.Log($"[Client] Destroying local object: {gameObject.name}");

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
        Debug.Log($"[Server] Starting DelayedDespawn for {gameObject.name}");
        yield return null; // Wait a frame for sync.
        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            Debug.Log($"[Server] Despawning {gameObject.name}");
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
        Debug.Log("[Server] Updating pathfinding graphs.");

        // Attempt to get the parent collider bounds
        Collider parentCollider = GetComponent<Collider>();

        Bounds bounds;
        if (parentCollider != null)
        {
            Debug.Log("[Server] Parent collider found, using its bounds.");
            bounds = parentCollider.bounds;
        }
        else
        {
            Debug.LogWarning("[Server] Parent collider not found. Calculating combined bounds of child colliders.");

            // Combine the bounds of all child colliders
            Collider[] childColliders = GetComponentsInChildren<Collider>();
            if (childColliders.Length == 0)
            {
                Debug.LogError("[Server] No colliders found on parent or children. Cannot update graphs.");
                return; // Exit if there are no colliders to calculate bounds
            }

            bounds = childColliders[0].bounds; // Start with the first collider's bounds
            foreach (Collider childCollider in childColliders.Skip(1))
            {
                bounds.Encapsulate(childCollider.bounds); // Expand bounds to include each child collider
            }
        }

        Debug.Log($"[Server] Calculated bounds: {bounds}");

        // Update the graph with the calculated bounds
        var guo = new GraphUpdateObject(bounds) { updatePhysics = true };
        AstarPath.active.UpdateGraphs(guo);

        Debug.Log("[Server] Pathfinding graphs updated.");
    }


}
