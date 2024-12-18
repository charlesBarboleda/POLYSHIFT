using UnityEngine;

public class FireDamage : MonoBehaviour
{
    private float fireDamage = 50; // Damage dealt by fire
    private float damageInterval = 0.1f; // Time between damage ticks
    private float lastDamageTime;

    // Called when particles collide with a GameObject
    void OnParticleCollision(GameObject other)
    {
        if (Time.time > lastDamageTime + damageInterval)
        {
            if (other.CompareTag("Player") || other.CompareTag("Destroyables"))
            {
                // Check if the object has a health component
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.RequestTakeDamageServerRpc(fireDamage, 0000); // Apply damage
                }
                lastDamageTime = Time.time;
            }
        }
    }

    public void SetDamage(float damage)
    {
        fireDamage = damage;
    }
}
