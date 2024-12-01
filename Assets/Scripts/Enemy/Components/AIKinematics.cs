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
        InvokeRepeating("TeleportIfStuck", 5f, 5f);

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

        // Get the nearest valid node to the current position
        var nearestNodeInfo = gridGraph.GetNearest(transform.position, NNConstraint.Default);
        var nearestNodePosition = (Vector3)nearestNodeInfo.node.position;

        // Check if the node is walkable
        if (nearestNodeInfo.node.Walkable)
        {
            // Teleport the AI to the nearest walkable node position
            transform.position = nearestNodePosition;
        }
        else
        {
            Debug.LogWarning("No walkable node found nearby.");
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
