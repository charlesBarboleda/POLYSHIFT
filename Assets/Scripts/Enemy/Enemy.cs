using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public enum EnemyType
{
    Melee,
    Ranged,
    Flying,
    Tank,
    Boss,
}

public abstract class Enemy : NetworkBehaviour
{
    public EnemyType enemyType;
    public Transform ClosestTarget;
    public EnemyNetworkHealth enemyHealth;
    public AIKinematics enemyMovement;
    public Rigidbody rb;
    public NetworkObject networkObject;
    public Animator animator;
    public NavMeshAgent agent;
    public float attackCooldown = 3f;
    public bool canAttack = false;
    public float experienceDrop;
    private float elapsedCooldown = 0f;
    private List<string> bloodSplatterEffects = new List<string> { "BloodSplatter1", "BloodSplatter2", "BloodSplatter3", "BloodSplatter4", "BloodSplatter5" };

    protected abstract void Attack();


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TryGetComponent(out rb);
        TryGetComponent(out enemyHealth);
        TryGetComponent(out enemyMovement);
        TryGetComponent(out networkObject);
        TryGetComponent(out agent);
        animator = GetComponentInChildren<Animator>();
        StartCoroutine(AttackGracePeriod());
        if (enemyMovement != null)
        {
            ClosestTarget = enemyMovement.ClosestPlayer;
        }
        else
        {
            Debug.LogError("Enemy Movement is null");
        }

    }
    protected virtual void FixedUpdate()
    {

        if (IsServer)
        {

            ClosestTarget = enemyMovement.ClosestPlayer;
            if (ClosestTarget != null)
            {
                agent.SetDestination(ClosestTarget.position);

                // Check if agent is close to the target
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    // Stop the agent when within range
                    agent.velocity = Vector3.zero; // Stop sliding

                    // Perform attack if cooldown has reset
                    if (elapsedCooldown <= 0 && canAttack)
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

    IEnumerator AttackGracePeriod()
    {
        canAttack = false;
        yield return new WaitForSeconds(3f);
        canAttack = true;
    }




    [ServerRpc(RequireOwnership = false)]
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
        GameObject bloodSplatter = ObjectPooler.Instance.Spawn(bloodSplatterEffects[Random.Range(0, bloodSplatterEffects.Count)], hitPoint, Quaternion.identity);
        bloodSplatter.transform.position = hitPoint;
        bloodSplatter.transform.rotation = Quaternion.LookRotation(hitNormal);

        yield return new WaitForSeconds(3f);
        // Optionally, destroy after a short time to prevent clutter
        ObjectPooler.Destroy(bloodSplatter);
    }







}
