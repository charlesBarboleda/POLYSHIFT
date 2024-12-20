using Unity.Netcode;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 1f;
    private Vector3 TargetPosition; // The last known position of the target
    public float DespawnDistance = 0.5f; // Distance threshold for despawning
    public string ProjectileEffectName = "RedDragonProjectile"; // Name for object pooler
    public string ProjectileHitEffectName = "RedDragonProjectileHit"; // Name for object pooler
    public float Damage = 10f; // Damage dealt by the projectile

    public void Initialize(Transform target, float speed, float damage)
    {
        // Capture the target's last known position
        TargetPosition = target.position;
        Speed = speed;
        Damage = damage;

    }

    void Update()
    {
        // Calculate direction toward the target's last position
        Vector3 direction = (TargetPosition - transform.position).normalized;
        float distanceToTravel = Speed * Time.deltaTime;

        // Spherecastall continuously during movement
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1f, direction, distanceToTravel);
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Destroyables"))
            {
                OnHitObject(hit.collider);
                return;
            }

        }
        // Move the projectile forward
        transform.position += direction * distanceToTravel;

        // Check if the projectile is close to its target position
        if (Vector3.Distance(transform.position, TargetPosition) <= DespawnDistance)
        {
            DespawnRpc();
        }
    }


    private void OnHitObject(Collider collider)
    {
        var damageable = collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Debug.Log($"Projectile hit {collider.name}, applying {Damage} damage.");
            damageable.RequestTakeDamageServerRpc(Damage, 0);
        }

        // Despawn the projectile after hitting any object
        DespawnRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void DespawnRpc()
    {
        GameObject onHitEffect = ObjectPooler.Instance.Spawn(ProjectileHitEffectName, transform.position, Quaternion.identity);
        onHitEffect.transform.rotation = Quaternion.Euler(-90, 0, 90);

        Debug.Log($"Despawning projectile at {transform.position}");
        ObjectPooler.Instance.Despawn(ProjectileEffectName, gameObject);
    }
}
