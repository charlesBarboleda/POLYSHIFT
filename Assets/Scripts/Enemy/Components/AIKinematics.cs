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
        InvokeRepeating("TeleportIfStuck", 5f, 3f);

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
            transform.position = Vector3.MoveTowards(transform.position, ClosestPlayer.position, MoveSpeed * Time.deltaTime);
        }


        animator.SetBool("IsMoving", Agent.velocity.magnitude != 0);
        Agent.maxSpeed = MoveSpeed;

    }
    void RepositionToNearestValidNode()
    {
        // Ensure the A* Pathfinding system is active
        if (AstarPath.active == null)
        {
            Debug.LogError("A* Pathfinding is not active!");
            return;
        }

        // Get the grid graph
        GridGraph gridGraph = AstarPath.active.data.gridGraph;
        if (gridGraph == null)
        {
            Debug.LogError("No GridGraph found in the A* Pathfinding setup!");
            return;
        }

        // Get the current position and facing direction
        Vector3 currentPosition = transform.position;
        Vector3 forwardDirection = transform.forward;

        float closestDistance = Mathf.Infinity;
        Vector3 bestNodePosition = currentPosition;

        // Iterate through the nodes in the grid graph
        foreach (GraphNode node in gridGraph.nodes)
        {
            if (!node.Walkable) continue; // Skip non-walkable nodes

            Vector3 nodePosition = (Vector3)node.position;
            Vector3 directionToNode = (nodePosition - currentPosition).normalized;

            // Check if the node is in front of the AI
            float dotProduct = Vector3.Dot(forwardDirection, directionToNode);
            if (dotProduct < 0.5f) continue; // Only consider nodes with a forward-facing angle (dot > 0.5)

            // Check the distance to the node
            float distance = Vector3.Distance(currentPosition, nodePosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestNodePosition = nodePosition;
            }
        }

        // Teleport the AI to the best node position found
        if (closestDistance < Mathf.Infinity)
        {
            transform.position = bestNodePosition;
            Debug.Log($"Teleported to nearest valid forward-facing node at {bestNodePosition}");
        }
        else
        {
            Debug.LogWarning("No valid forward-facing walkable node found.");
        }
    }


    void TeleportIfStuck()
    {
        StartCoroutine(CheckIfStuck());
    }

    IEnumerator CheckIfStuck()
    {
        Vector3 lastPosition = transform.position;
        yield return new WaitForSeconds(3f);
        if (Vector3.Distance(transform.position, lastPosition) < 0.5f)
        {
            RepositionToNearestValidNode();
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

    public void MoveSpeedIncreaseByPercentage(float amount)
    {
        MoveSpeed += MoveSpeed * amount;
    }
    public void MoveSpeedDecreaseByPercentage(float amount)
    {
        MoveSpeed -= MoveSpeed * amount;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        CancelInvoke();
    }
}
