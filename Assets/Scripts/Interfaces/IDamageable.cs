using UnityEngine;

public interface IDamageable
{
    void HealServerRpc(float healAmount);
    void TakeDamage(float damage, ulong clientId);
    void RequestTakeDamageServerRpc(float damage, ulong clientId);
    void HandleDeath(ulong networkObjectId);
}
