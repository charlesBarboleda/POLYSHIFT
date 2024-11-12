using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{
    public float attackRange = 4f;
    public float attackDamage = 10f;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enemyType = EnemyType.Melee;
        animator = GetComponent<Animator>();


    }



    protected override void Attack()
    {
        if (ClosestTarget != null)
        {
            StartCoroutine(AttackAnimation());
            // Deal damage after a short delay to sync with the animation
            Invoke(nameof(DealDamage), 1.7f);

        }
    }

    private void DealDamage()
    {
        if (ClosestTarget != null)
        {
            if (Vector3.Distance(transform.position, ClosestTarget.position) <= attackRange + 1f)
            {
                ClosestTarget.GetComponent<IDamageable>().RequestTakeDamageServerRpc(attackDamage, NetworkObjectId);
                // End the attack animation
            }
            // Deal damage to the surrounding area
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Destroyables"))
                {
                    hitCollider.GetComponent<IDamageable>().TakeDamage(attackDamage, NetworkObjectId);
                }
            }
        }
    }

    IEnumerator AttackAnimation()
    {
        animator.SetBool("isAttacking", true);
        yield return new WaitForSeconds(2f);
        animator.SetBool("isAttacking", false);
    }







}



