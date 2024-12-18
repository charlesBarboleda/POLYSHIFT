using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class BossDragonEnemy : Enemy
{
    public float meleeAttackRange = 6f;
    public float rangedAttackRange = 20f;
    public float rangedAttackDamage = 100f;
    public float meleeAttackDamage = 150f;
    private List<string> AttacksFirstPhase;
    private List<string> AttacksSecondPhase;
    private DragonEnemyNetworkHealth health;
    [SerializeField] GameObject mouth;
    [SerializeField] ParticleSystem fireBreathParticles;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AttacksFirstPhase = new List<string> { "BodySlam", "Bite" };
        AttacksSecondPhase = new List<string> { "FireBreath" };
        health = GetComponent<DragonEnemyNetworkHealth>();

    }

    public override IEnumerator Attack()
    {
        if (ClosestTarget != null)
        {
            if (!health.Grounded)
            {
                isAttacking = true;
                animator.SetTrigger(AttacksSecondPhase[0]);

                // Wait until the attack animation starts
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("FireBreath"));

                // Wait for the duration of the attack animation
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

                isAttacking = false;
            }
            else
            {
                isAttacking = true;
                int randomIndex = Random.Range(0, AttacksFirstPhase.Count);
                animator.SetTrigger(AttacksFirstPhase[randomIndex]);

                // Wait until the attack animation starts
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(AttacksFirstPhase[randomIndex]));

                // Wait for the duration of the attack animation
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

                isAttacking = false;
            }
        }
    }

    public void ApplyRootMotion()
    {
        animator.applyRootMotion = true;
    }

    public void DisableRootMotion()
    {
        animator.applyRootMotion = false;
    }

    [ServerRpc]
    public void SpawnFireBreathServerRpc()
    {
        SpawnFireBreathClientRpc();
    }

    [ClientRpc]
    void SpawnFireBreathClientRpc()
    {
        StartCoroutine(SpawnFireBreath());
    }

    IEnumerator SpawnFireBreath()
    {
        // Set damage and spawn it
        fireBreathParticles.gameObject.SetActive(true);
        fireBreathParticles.GetComponentInChildren<FireDamage>().SetDamage(rangedAttackDamage);
        fireBreathParticles.Play();

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        yield return new WaitForSeconds(1f);

        // Despawn after the animation ends
        fireBreathParticles.Stop();
        yield return new WaitForSeconds(1f);
        fireBreathParticles.gameObject.SetActive(false);

    }



    public void DealDamageMouth()
    {
        if (ClosestTarget != null &&
            Vector3.Distance(mouth.transform.position, ClosestTarget.position) <= meleeAttackRange + 1f)
        {
            var damageable = ClosestTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.RequestTakeDamageServerRpc(meleeAttackDamage, NetworkObjectId);
            }
        }

        // AoE Damage
        Collider[] hitColliders = Physics.OverlapSphere(mouth.transform.position, meleeAttackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Destroyables"))
            {
                var damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.RequestTakeDamageServerRpc(meleeAttackDamage, NetworkObjectId);
                }
            }
        }
    }

    public void DealDamageBodySlam()
    {
        if (ClosestTarget != null &&
            Vector3.Distance(mouth.transform.position, ClosestTarget.position) <= meleeAttackRange + 1f)
        {
            var damageable = ClosestTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.RequestTakeDamageServerRpc(meleeAttackDamage, NetworkObjectId);
            }
        }

        // AoE Damage
        Collider[] hitColliders = Physics.OverlapSphere(mouth.transform.position, meleeAttackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Destroyables"))
            {
                var damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.RequestTakeDamageServerRpc(meleeAttackDamage, NetworkObjectId);
                }
            }
        }
    }
}
