using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class DamageRedirector : MonoBehaviour, IDamageable
{
    private Transform parent;

    private void Awake()
    {
        parent = transform.parent;
    }

    public void HealServerRpc(float healAmount)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float damage, ulong clientId)
    {
        Debug.Log($"[DamageRedirector] Redirecting damage from {gameObject.name} to parent.");

        // Redirect damage to the parent's damage handler
        DestroyableHealth handler = parent.GetComponent<DestroyableHealth>();
        if (handler != null)
        {
            handler.RequestTakeDamageServerRpc(damage, 0);
        }
        else
        {
            Debug.LogWarning($"No DamageHandler found on parent of {gameObject.name}");
        }
    }

    public void RequestTakeDamageServerRpc(float damage, ulong clientId)
    {
        TakeDamage(damage, clientId);
    }

    public void HandleDeath(ulong networkObjectId)
    {
        throw new System.NotImplementedException();
    }
}
