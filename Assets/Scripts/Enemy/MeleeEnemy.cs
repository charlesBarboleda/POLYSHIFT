using UnityEngine;

public class MeleeEnemy : Enemy
{
    public float attackRange = 4f;
    public float attackDamage = 10f;
    public float attackCooldown = 3f;
    float elapsedCooldown = 0f;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enemyType = EnemyType.Melee;

    }

    public override void Update()
    {
        base.Update();
        if (IsServer)
        {
            if (elapsedCooldown > 0)
            {
                elapsedCooldown -= Time.deltaTime;
            }
            else
            {
                // Check if the enemy is close enough to attack
                if (ClosestTarget != null && Vector3.Distance(transform.position, ClosestTarget.position) <= attackRange)
                {
                    Attack();
                    elapsedCooldown = attackCooldown;
                }
            }
        }
    }

    protected override void Attack()
    {
        if (ClosestTarget != null)
        {
            Debug.Log("Enemy attacking player");
            ClosestTarget.GetComponent<PlayerNetworkHealth>().TakeDamage(attackDamage);
            Debug.Log("Dealt damage to player");
        }
    }
}
