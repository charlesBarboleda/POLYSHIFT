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
            animator.SetTrigger("isAttacking");
        }
    }


    public void DealDamage()
    {
        if (ClosestTarget != null &&
            Vector3.Distance(transform.position, ClosestTarget.position) <= attackRange)
        {
            var damageable = ClosestTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.RequestTakeDamageServerRpc(attackDamage, NetworkObjectId);
            }
        }

        // AoE Damage
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Destroyables"))
            {
                var damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.RequestTakeDamageServerRpc(attackDamage, NetworkObjectId);
                }
            }
        }
    }

}



