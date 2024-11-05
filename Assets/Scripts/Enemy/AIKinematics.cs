using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using FIMSpace.FLook;

public class AIKinematics : NetworkBehaviour
{
    public float MoveSpeed;
    public NavMeshAgent Agent;
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
        animator = GetComponentInChildren<Animator>();
        lookAnimator = GetComponent<FLookAnimator>();
        Agent = GetComponent<NavMeshAgent>();

    }

    void Update()
    {

        if (!IsServer) return;
        lookAnimator.SetLookTarget(ClosestPlayer);
        FindClosestPlayer();
        StopAndRotateTowardsTarget();

        if (ClosestPlayer != null)
        {
            Agent.SetDestination(ClosestPlayer.position);
        }

        if (!Agent.hasPath)
        {
            Debug.LogWarning("Agent has no path!");
        }


        if (Agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogError("Invalid Path!");
        }
        animator.SetFloat("Speed", Agent.velocity.magnitude);
        Agent.speed = MoveSpeed;

    }

    void StopAndRotateTowardsTarget()
    {
        if (ClosestPlayer != null)
        {
            if (Vector3.Distance(transform.position, ClosestPlayer.position) <= Agent.stoppingDistance)
            {
                Agent.velocity = Vector3.zero;
                Agent.isStopped = true;
                Vector3 direction = (ClosestPlayer.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
            else
            {
                Agent.isStopped = false;
            }

        }
    }

    void FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        // Iterate through all connected players in the game
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject playerObject = client.PlayerObject.gameObject;
            float distance = Vector3.Distance(transform.position, playerObject.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = playerObject.transform;
            }
        }

        ClosestPlayer = closestPlayer;
    }
}
