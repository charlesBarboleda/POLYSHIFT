using UnityEngine;

public class DestroyableHealth : MonoBehaviour, IDamageable
{
    public float health = 1f;

    public void TakeDamage(float damage, ulong instigator)
    {
        health -= 1;
        if (health <= 0)
        {
            HandleDeath(instigator);
        }
    }

    public void RequestTakeDamageServerRpc(float damage, ulong instigator)
    {
        TakeDamage(damage, instigator);
    }

    public void HandleDeath(ulong instigator)
    {
        // Destroy the parent object
        Destroy(gameObject.transform.parent.gameObject);
    }
}
