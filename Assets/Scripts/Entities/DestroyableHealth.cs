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
        // Synchronize with Destructible
        Health.Value = newValue;
        destructible?.SyncHealth(newValue);



        if (Health.Value <= 0)
        {
            Health.SetDirty(true);
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

        Health.Value = 0;
        Health.SetDirty(true);
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

        NotifyClientsToDespawn();
    }

    private void NotifyClientsToDespawn()
    {
        // Notify all clients to despawn the object.
        NotifyDespawnClientRpc();

        // Delay despawn to ensure all clients process the destruction logic.
        StartCoroutine(DelayedDespawn());
    }

    [ClientRpc]
    private void NotifyDespawnClientRpc()
    {
        Debug.Log($"[Client] Despawning object: {gameObject.name}");
        if (IsSpawned) // Ensure the object is still active.
        {
            gameObject.SetActive(false); // Simulate despawn on the client.
        }
    }

    private IEnumerator DelayedDespawn()
    {
        yield return 0; // Allow time for client-side sync.
        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            Debug.Log($"[Server] Despawning {gameObject.name}.");
            networkObject.Despawn(false);
        }
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
