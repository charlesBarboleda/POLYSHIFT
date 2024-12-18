using UnityEngine;

public class FireDamage : MonoBehaviour
{
    [SerializeField] private int fireDamage = 50; // Damage dealt by fire
    [SerializeField] private float damageInterval = 1f; // Time between damage ticks
    private float lastDamageTime;

    // Called when particles collide with a GameObject
    void OnParticleCollision(GameObject other)
    {
        if (Time.time > lastDamageTime + damageInterval)
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
