using UnityEngine;
using System.Collections;

public class DoubleCrescentSlash : MeleeAttack
{

    public float stepForwardDistance = 0.5f;
    public float attackRange = 5f;
    public float coneAngle = 45f;
    public override IEnumerator ExecuteAttack()
    {
        // Apply animation restrictions
        playerMovement.canMove = false;
        playerRotation.canRotate = false;
        animator.SetTrigger("isAttacking");
        SetAnimationDuration();
        // Wait for the animation to play and then spawn the visuals
        yield return new WaitForSeconds(0.35f);
        // Spawn the effect and move the player forward slightly to match the animation
        GameObject slash1 = ObjectPooler.Instance.Spawn("MeleeSlash1", transform.position + transform.forward * 2f, transform.rotation * Quaternion.Euler(0, 0, 20));
        slash1.transform.localScale = new Vector3(attackRange / 6, attackRange / 6, attackRange / 6);
        transform.position += transform.forward * stepForwardDistance;
        // Deal damage in a cone in front of the player
        StartCoroutine(DealDamage());
        // Wait for the animation and then spawn the second slash
        yield return new WaitForSeconds(0.15f);
        GameObject slash2 = ObjectPooler.Instance.Spawn("MeleeSlash1", transform.position + transform.forward * 2f, transform.rotation * Quaternion.Euler(0, 0, -20));
        slash2.transform.localScale = new Vector3(attackRange / 6, attackRange / 6, attackRange / 6);
        transform.position += transform.forward * stepForwardDistance;
        StartCoroutine(DealDamage());
        yield return new WaitForSeconds(attackAnimDuration - 0.6f);
        ObjectPooler.Instance.Despawn("MeleeSlash1", slash1);
        playerRotation.canRotate = true;
        playerMovement.canMove = true;
        yield return new WaitForSeconds(1f);
        ObjectPooler.Instance.Despawn("MeleeSlash1", slash2);
    }

    IEnumerator DealDamage()
    {
        yield return new WaitForSeconds(0.1f);
        // Define the starting position for the cone
        Vector3 origin = transform.position;

        // Get all colliders in a sphere around the player
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRange);

        foreach (Collider collider in hitColliders)
        {
            Vector3 directionToTarget = (collider.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            // Check if the object is within the cone
            if (angleToTarget <= coneAngle)
            {
                // Apply damage to the target
                EnemyNetworkHealth enemyHealth = collider.GetComponent<EnemyNetworkHealth>(); // Assume enemies have an EnemyHealth component
                if (enemyHealth != null)
                {
                    enemyHealth.RequestTakeDamageServerRpc(damage);
                }
                Enemy enemy = collider.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.OnRaycastHitServerRpc(collider.transform.position, collider.transform.forward);
                }

            }

            // Visualize the cone
            Vector3 leftBoundary = Quaternion.Euler(0, -coneAngle, 0) * transform.forward * attackRange;
            Vector3 rightBoundary = Quaternion.Euler(0, coneAngle, 0) * transform.forward * attackRange;

            Debug.DrawLine(origin, origin + leftBoundary, Color.red, 1f);
            Debug.DrawLine(origin, origin + rightBoundary, Color.red, 1f);
            Debug.DrawLine(origin + leftBoundary, origin + rightBoundary, Color.red, 1f); // Visualize the arc
        }
    }
}
