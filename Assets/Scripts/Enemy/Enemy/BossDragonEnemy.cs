using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class BossDragonEnemy : Enemy
{
    public float meleeAttackRange = 6f;
    public float rangedAttackRange = 20f;
    public float rangedAttackDamage = 100f;
    public float meleeAttackDamage = 150f;
    private List<string> AttacksFirstPhase;
    private List<string> AttacksSecondPhase;
    [SerializeField] GameObject mouth;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AttacksFirstPhase = new List<string> { "BodySlam", "Bite" };
        AttacksSecondPhase = new List<string> { "FireBreath" };

    }

    public override IEnumerator Attack()
    {
        if (ClosestTarget != null)
        {
            if (enemyHealth.CurrentHealth.Value <= enemyHealth.MaxHealth / 2)
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

    public void SpawnFireBreath()
    {
        if (ClosestTarget != null &&
            Vector3.Distance(mouth.transform.position, ClosestTarget.position) <= rangedAttackRange)
        {
            var damageable = ClosestTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.RequestTakeDamageServerRpc(rangedAttackDamage, NetworkObjectId);
            }
        }
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
            Vector3.Distance(transform.position, ClosestTarget.position) <= meleeAttackRange + 1f)
        {
            var damageable = ClosestTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.RequestTakeDamageServerRpc(meleeAttackDamage, NetworkObjectId);
            }
        }

        // AoE Damage
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, meleeAttackRange);
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
