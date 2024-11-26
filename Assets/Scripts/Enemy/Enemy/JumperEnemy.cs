using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class JumperEnemy : Enemy
{
    public float jumpRange = 15f;         // Range within which the enemy leaps
    public float closeRange = 3f;        // Range for a normal "bite" attack
    public float attackDamage = 5f;     // Damage dealt by attacks
    public float jumpForce = 5f;        // Force applied during the jump
    public float leapProximityDamageRange = 2f; // Range for dealing damage mid-leap
    public float jumpCooldown = 3f;      // Cooldown between jumps
    private bool isJumping = false;      // Tracks if the enemy is currently leaping
    EnemyNetworkHealth health;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enemyType = EnemyType.Melee;
        animator = GetComponent<Animator>();

        if (IsServer)
        {
            health = GetComponent<EnemyNetworkHealth>();
            health.MaxHealth += GameManager.Instance.GameLevel.Value;
            health.CurrentHealth.Value = health.MaxHealth;
            attackDamage += GameManager.Instance.GameLevel.Value * 0.01f;

        }
    }

    protected override void Attack()
    {
        if (ClosestTarget == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, ClosestTarget.position);

        if (distanceToTarget <= closeRange)
        {
            BiteAttack();
        }
        else if (distanceToTarget <= jumpRange && !isJumping)
        {
            StartCoroutine(JumpAttack());
        }
    }

    private void BiteAttack()
    {
        // Perform a bite attack directly
        animator.SetTrigger("isAttacking");
        ClosestTarget.GetComponent<IDamageable>().RequestTakeDamageServerRpc(attackDamage, NetworkObjectId);
    }

    private IEnumerator JumpAttack()
    {
        isJumping = true;
        animator.SetTrigger("isJumping");

        // Calculate the jump direction (towards the player)
        Vector3 jumpDirection = (ClosestTarget.position - transform.position).normalized;
        jumpDirection.y = 1f; // Add upward force
        rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);

        // Continuously check for players within proximity during the jump
        StartCoroutine(CheckProximityDuringLeap());

        // Wait for the jump duration
        yield return new WaitForSeconds(animator.GetAnimatorTransitionInfo(0).duration); // Adjust based on your jump duration

        // Cooldown before allowing another jump
        yield return new WaitForSeconds(jumpCooldown);
        isJumping = false;
    }

    private IEnumerator CheckProximityDuringLeap()
    {
        while (isJumping)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, leapProximityDamageRange);
            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Player") || collider.CompareTag("Destroyables"))
                {
                    var damageable = collider.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        // Apply leap damage
                        damageable.RequestTakeDamageServerRpc(attackDamage, NetworkObjectId);
                    }
                }
            }

            // Check periodically to reduce performance cost
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stop the jumping state when the enemy lands on the ground
        if (isJumping && collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }
}
