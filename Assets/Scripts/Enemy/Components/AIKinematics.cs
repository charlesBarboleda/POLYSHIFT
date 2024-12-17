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
    Enemy enemy;
    EnemyNetworkHealth enemyHealth;
    public bool CanMove = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            // Disable AI logic on clients since only the server should run this
            enabled = false;
        }
        CanMove = true;
        enemyHealth = GetComponent<EnemyNetworkHealth>();
        animator = GetComponent<Animator>();
        lookAnimator = GetComponent<FLookAnimator>();
        Agent = GetComponent<AIPath>();
        enemy = GetComponent<Enemy>();
        InvokeRepeating("AttackIfStuck", 0f, 3f);
        InvokeRepeating("TeleportIfStuck", 0f, 3f);

    }

    void Update()
    {

        if (!IsServer) return;

        lookAnimator.SetLookTarget(ClosestPlayer);
        FindClosestPossibleTarget();
        StopAndRotateTowardsTarget();

        if (enemyHealth.IsDead)
        {
            // Explicitly stop movement when dead
            Agent.isStopped = true;
            Agent.canMove = false;
            Agent.destination = transform.position; // Lock the agent in place
            return; // Avoid unnecessary updates when dead
        }

        if (!CanMove || enemy.isAttacking)
        {
            Agent.isStopped = true;
            Agent.canMove = false;
        }
        else if (ClosestPlayer != null)
        {

            Agent.isStopped = false;
            Agent.canMove = true;
        }


        if (ClosestPlayer != null && !enemy.isAttacking)
        {

            Agent.isStopped = false;
            Agent.canMove = true;

        }

        if (!Agent.hasPath)
        {
            if (ClosestPlayer != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, ClosestPlayer.position, MoveSpeed * Time.deltaTime);
            }

        }


        animator.SetBool("IsMoving", Agent.velocity.magnitude != 0);
        Agent.maxSpeed = MoveSpeed;
        Agent.destination = ClosestPlayer.position;
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
        }
    }


    void TeleportIfStuck()
    {
        StartCoroutine(CheckIfStuck(3f));
    }

    void AttackIfStuck()
    {
        if (enemy.isAttacking) return;
        StartCoroutine(CheckIfStuck(1f, true));
    }

    IEnumerator CheckIfStuck(float checkRate, bool isAttack = false)
    {
        Vector3 lastPosition = transform.position;
        yield return new WaitForSeconds(checkRate);
        if (enemy.isAttacking) yield break;
        if (Vector3.Distance(transform.position, lastPosition) < 0.5f)
        {
            if (isAttack)
            {
                StartCoroutine(enemy.Attack());
            }
            else
            {
                RepositionToNearestValidNode();
            }
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
        if (GameManager.Instance.SpawnedAllies.Count == 0) return;

        if (ClosestPlayer != null && !GameManager.Instance.SpawnedAllies.Contains(ClosestPlayer.gameObject))
        {
            ClosestPlayer = null;
        }

        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (GameObject unit in GameManager.Instance.SpawnedAllies)
        {
            if (!unit.activeSelf) continue; // Skip inactive units

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
