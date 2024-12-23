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

        if (IsServer)
        {
            enemyHealth.MaxHealth += GameManager.Instance.GameLevel.Value * maxHealthScalingFactor;
            enemyHealth.CurrentHealth.Value = enemyHealth.MaxHealth;
            attackDamage += GameManager.Instance.GameLevel.Value * attackDamageScalingFactor;
            enemyHealth.ExperienceDrop += GameManager.Instance.GameLevel.Value * experienceDropScalingFactor;
            enemyMovement.MoveSpeed += Random.Range(GameManager.Instance.GameLevel.Value * 0.1f, GameManager.Instance.GameLevel.Value * 0.2f);
            enemyMovement.MoveSpeed = Mathf.Clamp(enemyMovement.MoveSpeed, 0f, moveSpeedCap);
        }
    }



    public override IEnumerator Attack()
    {
        if (ClosestTarget != null)
        {
            isAttacking = true;
            animator.SetTrigger("isAttacking");

            // Wait until the attack animation starts
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));

            // Wait for the duration of the attack animation
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            isAttacking = false;
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



