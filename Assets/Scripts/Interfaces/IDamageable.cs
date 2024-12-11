using UnityEngine;

public interface IDamageable
{
    void HealServerRpc(float healAmount);
    void TakeDamage(float damage, ulong networkObjectId);
    void RequestTakeDamageServerRpc(float damage, ulong networkObjectId);
    void HandleDeath(ulong networkObjectId);
}
