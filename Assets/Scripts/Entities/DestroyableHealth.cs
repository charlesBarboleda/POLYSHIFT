using Unity.Netcode;
using UnityEngine;
using DestroyIt;
using Pathfinding;
using System.Collections;
using System.Linq;
using UnityEngine.UIElements;

public class DestroyableHealth : NetworkBehaviour, IDamageable
{
    public enum DestroyableSize { Small, Medium, Large }

    public DestroyableSize destroyableSize;
    public float MaxHealth;
    public NetworkVariable<float> Health = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    bool hasFlameEffectSpawned = false;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Health.Value = MaxHealth; // Initialize health on the server.

        hasFlameEffectSpawned = false;
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

        // If health is less than or equal to 50% of the max health, spawn a flame effect.
        if (Health.Value <= MaxHealth * 0.5f)
        {
            SpawnFlameEffectRpc();
        }
        if (Health.Value <= 0)
        {
            Health.SetDirty(true); // Mark the health as dirty to ensure sync.
            HandleDeath(instigator);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnFlameEffectRpc()
    {
        var flame = ObjectPooler.Instance.Spawn("Flame", transform.position, Quaternion.identity);
        flame.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        flame.transform.SetParent(transform);
        hasFlameEffectSpawned = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage, ulong instigator)
    {
        TakeDamage(damage, instigator);
    }

    public void HandleDeath(ulong instigator)
    {
        if (!IsServer) return;

        // Spawn the destruction effect based on the size
        switch (destroyableSize)
        {
            case DestroyableSize.Small:
                SpawnEffectRpc("DefaultSmallParticle", "DefaultSmallParticle", "BurstFlame", 4f);
                break;
            case DestroyableSize.Medium:
                SpawnEffectRpc("DefaultSmallParticle", "DefaultLargeParticle", "BurstFlame", 8f);
                break;
            case DestroyableSize.Large:
                SpawnEffectRpc("DefaultLargeParticle", "DefaultLargeParticle", "BurstFlame", 12f);
                break;
        }

        // Notify clients to despawn the object and handle the server-side despawn.
        NotifyClientsToDespawn();

        UpdateGraphs(); // Update pathfinding graphs.


    }

    private void NotifyClientsToDespawn()
    {
        // Notify clients to despawn the object.
        DestroyObjectLocallyRpc();

    }


    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyObjectLocallyRpc()
    {

        // Destroy all child objects
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Destroy the parent object
        Destroy(gameObject);
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnEffectRpc(string effectName, string effectName2, string effectName3, float scale)
    {
        // Spawn the destruction effect using the object pooler.
        // Set the position of the effect to the middle of the object.

        GameObject effect = ObjectPooler.Instance.Spawn(effectName, transform.position + Vector3.up, Quaternion.identity);
        effect.transform.localScale = new Vector3(scale, scale, scale);
        GameObject effect2 = ObjectPooler.Instance.Spawn(effectName2, transform.position + Vector3.up, Quaternion.identity);
        effect2.transform.localScale = new Vector3(scale, scale, scale);
        GameObject effect3 = ObjectPooler.Instance.Spawn(effectName3, transform.position, Quaternion.identity);
        effect3.transform.localScale = new Vector3(scale / 2, scale / 2, scale / 2);
    }

    private void UpdateGraphs()
    {

        // Attempt to get the parent collider bounds
        Collider parentCollider = GetComponent<Collider>();

        Bounds bounds;
        if (parentCollider != null)
        {
            bounds = parentCollider.bounds;
        }
        else
        {

            // Combine the bounds of all child colliders
            Collider[] childColliders = GetComponentsInChildren<Collider>();
            if (childColliders.Length == 0)
            {
                return; // Exit if there are no colliders to calculate bounds
            }

            bounds = childColliders[0].bounds; // Start with the first collider's bounds
            foreach (Collider childCollider in childColliders.Skip(1))
            {
                bounds.Encapsulate(childCollider.bounds); // Expand bounds to include each child collider
            }
        }


        // Update the graph with the calculated bounds
        var guo = new GraphUpdateObject(bounds) { updatePhysics = true };
        AstarPath.active.UpdateGraphs(guo);

    }


}
