using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class HS_ParticleCollisionInstance : MonoBehaviour
{
    public float Damage;
    public string effectName;
    public bool UseWorldSpacePosition = false;
    public float Offset = 0f;
    public Vector3 rotationOffset = Vector3.zero;
    public bool useOnlyRotationOffset = true;
    public bool UseFirePointRotation = false;
    public bool DestroyMainEffect = false;

    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        part = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {
            // Spawn visual effect
            var spawnPosition = collisionEvents[i].intersection + collisionEvents[i].normal * Offset;
            var instance = ObjectPooler.Instance.Spawn(effectName, spawnPosition, Quaternion.identity) as GameObject;

            // Apply damage
            if (Physics.SphereCast(spawnPosition, 2f, collisionEvents[i].normal, out RaycastHit hit, 0.1f))
            {
                var damageable = hit.transform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.RequestTakeDamageServerRpc(Damage, 0);
                }
            }

            // Adjust rotation
            if (!UseWorldSpacePosition) instance.transform.parent = transform;
            if (UseFirePointRotation)
            {
                instance.transform.LookAt(transform.position);
            }
            else if (rotationOffset != Vector3.zero && useOnlyRotationOffset)
            {
                instance.transform.rotation = Quaternion.Euler(rotationOffset);
            }
            else
            {
                instance.transform.LookAt(spawnPosition + collisionEvents[i].normal);
                instance.transform.rotation *= Quaternion.Euler(rotationOffset);
            }
        }

        // Despawn particle system if required
        if (DestroyMainEffect)
        {
            if (TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.Despawn(false);
            }

            ObjectPooler.Instance.Despawn(effectName, gameObject);
        }

        collisionEvents.Clear();
    }
}
