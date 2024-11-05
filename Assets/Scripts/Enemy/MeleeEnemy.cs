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
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Configure the NavMeshAgent properties (optional)
        agent.stoppingDistance = attackRange;

    }



    protected override void Attack()
    {
        if (ClosestTarget != null)
        {
            StartCoroutine(AttackAnimation());
            // Deal damage after a short delay to sync with the animation
            Invoke(nameof(DealDamage), 2f);

        }
    }

    private void DealDamage()
    {
        if (ClosestTarget != null)
        {
            if (Vector3.Distance(transform.position, ClosestTarget.position) <= attackRange + 1.0f)
            {
                ClosestTarget.GetComponent<PlayerNetworkHealth>().TakeDamage(attackDamage, NetworkObjectId);
                // End the attack animation
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



