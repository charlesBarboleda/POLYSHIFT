using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using FIMSpace.FLook;
using Unity.AI.Navigation;
using Pathfinding;
using System.Collections;

public class AIKinematics : NetworkBehaviour
{
    public float MoveSpeed;

    public AIPath Agent;
    public Transform ClosestPlayer;
    FLookAnimator lookAnimator;
    Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            // Disable AI logic on clients since only the server should run this
            enabled = false;
        }
        animator = GetComponent<Animator>();
        lookAnimator = GetComponent<FLookAnimator>();
        Agent = GetComponent<AIPath>();


    }

    void Update()
    {

        if (!IsServer) return;

        lookAnimator.SetLookTarget(ClosestPlayer);
        FindClosestPossibleTarget();
        StopAndRotateTowardsTarget();

        if (ClosestPlayer != null)
        {
            Agent.destination = ClosestPlayer.position;
        }

        if (!Agent.hasPath)
        {

            Debug.LogWarning("Agent has no path!");
            transform.position = Vector3.MoveTowards(transform.position, ClosestPlayer.position, MoveSpeed * Time.deltaTime);
        }


        animator.SetBool("IsMoving", Agent.velocity.magnitude != 0);
        Agent.maxSpeed = MoveSpeed;

    }

    void TeleportIfStuck()
    {

    }

    IEnumerator CheckIfStuck()
    {
        Vector3 lastPosition = transform.position;
        yield return new WaitForSeconds(5f);
        if (Vector3.Distance(transform.position, lastPosition) < 0.5f)
        {
            transform.position = ClosestPlayer.position;
        }
    }

    void StopAndRotateTowardsTarget()
    {
        if (ClosestPlayer != null)
        {
            if (Agent.reachedDestination)
            {

                Vector3 direction = (ClosestPlayer.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                Agent.isStopped = true;
            }
            else
            {
                Agent.isStopped = false;
            }

        }
    }

    void FindClosestPossibleTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        // Iterate through all spawned allies
        foreach (GameObject unit in GameManager.Instance.SpawnedAllies)
        {
            if (unit == null) continue; // Skip null units
            float distance = Vector3.Distance(transform.position, unit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = unit.transform;
            }
        }
        ClosestPlayer = closestTarget;

    }

    public void MoveSpeedIncreaseBy(float amount)
    {
        MoveSpeed += amount;
    }
}
