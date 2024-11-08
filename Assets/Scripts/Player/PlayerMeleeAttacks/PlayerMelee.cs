using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMelee : NetworkBehaviour
{
    public List<MeleeAttack> meleeAttacks = new List<MeleeAttack>();
    private MeleeAttack currentAttack;
    private bool canAttack = true;
    private Animator animator;
    private PlayerNetworkMovement playerMovement;
    private PlayerNetworkRotation playerRotation;

    public int attackIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer) return;
        base.OnNetworkSpawn();

        animator = GetComponent<Animator>();
        playerMovement = GetComponentInParent<PlayerNetworkMovement>();
        playerRotation = GetComponentInParent<PlayerNetworkRotation>();

        meleeAttacks.AddRange(new MeleeAttack[]
        {
            gameObject.AddComponent<ArcaneBarrier>(),
            gameObject.AddComponent<SingleCrescentSlash>(),
            gameObject.AddComponent<DoubleCrescentSlash>(),
            gameObject.AddComponent<ArcaneCleave>(),
            gameObject.AddComponent<DevilSlam>()
        });

        foreach (var attack in meleeAttacks) attack.Initialize(animator);
    }

    void Update()
    {
        if (IsLocalPlayer && Input.GetMouseButtonDown(1) && canAttack)
            PerformAttack(attackIndex);
    }

    private void PerformAttack(int attackIndex)
    {
        if (attackIndex >= 0 && attackIndex < meleeAttacks.Count)
        {
            currentAttack = meleeAttacks[attackIndex];
            currentAttack.ExecuteAttack();
        }
    }

    private void DealDamage(Collider collider, Vector3 origin, float damage, float knockbackForce)
    {
        var damageable = collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            SpawnSlashImpactClientRpc("MeleeSlash1Hit", collider.transform.position, Quaternion.identity);
            damageable.RequestTakeDamageServerRpc(damage, NetworkObjectId);
        }

        var rb = collider.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce((collider.transform.position - origin).normalized * knockbackForce, ForceMode.Impulse);

        var enemy = collider.GetComponent<Enemy>();
        if (enemy != null)
            enemy.OnRaycastHitServerRpc(collider.transform.position, (collider.transform.position - origin).normalized);
    }

    public void DealDamageInCone(Vector3 origin, float attackRange, float coneAngle, float damage, float knockbackForce)
    {
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRange);
        foreach (var collider in hitColliders)
        {
            Vector3 directionToTarget = (collider.transform.position - origin).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) <= coneAngle && collider.CompareTag("Enemy"))
            {
                DealDamage(collider, origin, damage, knockbackForce);
            }
        }
    }

    public void DealDamageInCircle(Vector3 origin, float attackRange, float damage, float knockbackForce)
    {
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRange);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy") || collider.CompareTag("Destroyables"))
            {
                DealDamage(collider, origin, damage, knockbackForce);
            }
        }
    }

    public void DealDamageInExpandingCircle(Vector3 origin, float initialRange, float maxRange, float damage, float knockbackForce, float duration, float tickRate)
    {
        StartCoroutine(ExpandingDamageOverTimeCoroutine(origin, initialRange, maxRange, damage, knockbackForce, duration, tickRate));
    }

    private IEnumerator ExpandingDamageOverTimeCoroutine(Vector3 origin, float initialRange, float maxRange, float damage, float knockbackForce, float duration, float tickRate)
    {
        float elapsedTime = 0f;
        float currentRange = initialRange;
        var hitCount = new Dictionary<Collider, int>();

        while (elapsedTime < duration)
        {
            currentRange = Mathf.Lerp(initialRange, maxRange, elapsedTime / duration);
            Collider[] hitColliders = Physics.OverlapSphere(origin, currentRange);

            foreach (var collider in hitColliders)
            {
                if ((collider.CompareTag("Enemy") || collider.CompareTag("Destroyables")) && (!hitCount.ContainsKey(collider) || hitCount[collider] < 3))
                {
                    DealDamage(collider, origin, damage, knockbackForce);
                    hitCount[collider] = hitCount.ContainsKey(collider) ? hitCount[collider] + 1 : 1;
                }
            }
            yield return new WaitForSeconds(tickRate);
            elapsedTime += tickRate;
        }
    }

    [ClientRpc]
    private void SpawnSlashImpactClientRpc(string impactName, Vector3 position, Quaternion rotation)
    {
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler.Instance is null. Cannot spawn effect.");
            return;
        }

        var impact = ObjectPooler.Instance.Spawn(impactName, position, rotation);
        if (impact == null) Debug.LogError("Failed to spawn effect. Check ObjectPooler configuration.");
    }

    public void DisableMovementAndRotation()
    {
        playerMovement.canMove = false;
        playerRotation.canRotate = false;
    }

    public void EnableMovementAndRotation()
    {
        playerMovement.canMove = true;
        playerRotation.canRotate = true;
    }
}
