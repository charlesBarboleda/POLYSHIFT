using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class HomingMissile : NetworkBehaviour
{
    public GameObject Owner { get; set; }
    Transform Target { get; set; }
    public float Speed { get; set; } = 20f;
    public float Damage { get; set; } = 10f;
    public float ExplosionRadius { get; set; } = 5f;
    public float RotationSpeed = 2f; // How quickly the missile corrects its trajectory
    public float CorrectionDuration = 2f; // Time it takes to fully home in on the target

    private Vector3 initialDeviation; // Random initial direction
    Tween trajectoryCorrectionTween;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip explosionSound;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Speed = 15f;
        ExplosionRadius = 15f;
        CorrectionDuration = 0.5f;
        initialDeviation = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized * 50f;

        trajectoryCorrectionTween = transform.DOMove(Target.position, CorrectionDuration)
           .SetEase(Ease.InOutQuad) // Smooth easing for trajectory correction
           .OnUpdate(() =>
           {
               // Continuously rotate to face the target
               Vector3 direction = (Target.position - transform.position).normalized;
               transform.rotation = Quaternion.LookRotation(direction);
           })
           .OnComplete(() =>
           {
               // When trajectory correction is complete, the missile homes in directly
               Debug.Log("Missile locked on target.");
           });
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);

        // Check if the missile has reached the target
        if (Target != null && Vector3.Distance(transform.position, Target.position) <= 0.5f)
        {
            Explode();
        }
    }
    public void SetTarget(GameObject target, GameObject owner)
    {
        Owner = owner;
        Target = target.transform;
    }

    void Explode()
    {
        audioSource.PlayOneShot(explosionSound);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag("Enemy") || hitCollider.gameObject.CompareTag("Destroyables"))
            {
                hitCollider.gameObject.GetComponent<IDamageable>()?.RequestTakeDamageServerRpc(Damage, Owner.GetComponent<NetworkObject>().NetworkObjectId);
                hitCollider.gameObject.GetComponent<Enemy>()?.OnRaycastHitServerRpc(hitCollider.gameObject.transform.position, hitCollider.gameObject.transform.forward);
            }
        }
        GameObject explosion = ObjectPooler.Instance.Spawn("HomingMissileExplosion", transform.position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn();
        ObjectPooler.Instance.Despawn("HomingMissile", gameObject);
        GetComponent<NetworkObject>().Despawn(false);
    }
}
