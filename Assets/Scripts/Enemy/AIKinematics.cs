using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using FIMSpace.FLook;
using Unity.AI.Navigation;
using Pathfinding;

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


        animator.SetFloat("Speed", Agent.velocity.magnitude);
        Agent.maxSpeed = MoveSpeed;

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

        // Iterate through all connected players in the game
        foreach (GameObject unit in GameManager.Instance.SpawnedAllies)
        {
            // if (unit == null) continue;
            float distance = Vector3.Distance(transform.position, unit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = unit.transform;
            }
        }
        ClosestPlayer = closestTarget;



    }
}
