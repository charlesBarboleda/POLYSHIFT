using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
    void RequestTakeDamageServerRpc(float damage);
    void HandleDeath();
}
