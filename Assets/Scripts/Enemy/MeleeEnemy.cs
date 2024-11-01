using System.Collections;
using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{
    public float attackRange = 4f;
    public float attackDamage = 10f;
    public float attackCooldown = 3f;
    private float elapsedCooldown = 0f;
    private Animator animator;
    private NavMeshAgent agent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enemyType = EnemyType.Melee;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Configure the NavMeshAgent properties (optional)
        agent.stoppingDistance = attackRange;
        agent.autoBraking = true;
    }



    public override void Update()
    {
        base.Update();

        if (IsServer)
        {
            if (ClosestTarget != null)
            {
                agent.SetDestination(ClosestTarget.position);

                // Check if agent is close to the target
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    // Stop the agent when within range
                    agent.velocity = Vector3.zero; // Stop sliding
                    agent.isStopped = true;

                    // Perform attack if cooldown has reset
                    if (elapsedCooldown <= 0)
                    {
                        Attack();
                        elapsedCooldown = attackCooldown;
                    }
                }
                else
                {
                    // Resume movement if the target is far
                    agent.isStopped = false;
                }
            }

            // Update cooldown timer
            if (elapsedCooldown > 0)
            {
                elapsedCooldown -= Time.deltaTime;
            }
        }
    }

    protected override void Attack()
    {
        if (ClosestTarget != null)
        {
            animator.SetBool("isAttacking", true);

            // Deal damage after a short delay to sync with the animation
            Invoke(nameof(DealDamage), 2f);
        }
    }

    private void DealDamage()
    {
        if (ClosestTarget != null)
        {
            Debug.Log("Enemy attacking player");
            ClosestTarget.GetComponent<PlayerNetworkHealth>().TakeDamage(attackDamage);
            Debug.Log("Dealt damage to player");

            // End the attack animation
            animator.SetBool("isAttacking", false);
        }
    }



    [ServerRpc]
    public void OnRaycastHitServerRpc(Vector3 hitPoint, Vector3 hitNormal)
    {
        SpawnBloodSplatterClientRpc(hitPoint, hitNormal);
    }

    [ClientRpc]
    private void SpawnBloodSplatterClientRpc(Vector3 hitPoint, Vector3 hitNormal)
    {
        StartCoroutine(SpawnBloodSplatterCoroutine(hitPoint, hitNormal));
    }

    IEnumerator SpawnBloodSplatterCoroutine(Vector3 hitPoint, Vector3 hitNormal)
    {
        // Instantiate the blood splatter effect locally on each client
        GameObject bloodSplatter = ObjectPooler.Generate("BloodSplatter");
        bloodSplatter.transform.position = hitPoint;
        bloodSplatter.transform.rotation = Quaternion.LookRotation(hitNormal);

        yield return new WaitForSeconds(3f);
        // Optionally, destroy after a short time to prevent clutter
        ObjectPooler.Destroy(bloodSplatter);
    }

}



