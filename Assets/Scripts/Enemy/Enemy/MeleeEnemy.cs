using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{
    public float attackRange = 4f;
    public float attackDamage = 10f;
    EnemyNetworkHealth health;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enemyType = EnemyType.Melee;

        if (IsServer)
        {
            health = GetComponent<EnemyNetworkHealth>();
            health.MaxHealth += GameManager.Instance.GameLevel.Value * 5;
            health.CurrentHealth.Value = health.MaxHealth;
            attackDamage += GameManager.Instance.GameLevel.Value * 5;
            health.ExperienceDrop += GameManager.Instance.GameLevel.Value;
            enemyMovement.MoveSpeed += Random.Range(GameManager.Instance.GameLevel.Value * 0.1f, GameManager.Instance.GameLevel.Value * 0.2f);
        }
    }



    public override void Attack()
    {
        if (ClosestTarget != null)
        {
            animator.SetTrigger("isAttacking");
        }
    }


    public void DealDamage()
    {
        if (ClosestTarget != null &&
            Vector3.Distance(transform.position, ClosestTarget.position) <= attackRange + 1f)
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



