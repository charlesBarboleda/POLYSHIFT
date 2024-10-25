using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class AIKinematics : NetworkBehaviour
{
    public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NavMeshAgent Agent;
    public Transform ClosestPlayer;

    public override void OnNetworkSpawn()
    {
        Debug.Log("IsServer OnNetworkSpawn: " + IsServer);
        if (!IsServer)
        {
            // Disable AI logic on clients since only the server should run this
            enabled = false;
        }
        Agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {

        if (IsServer)
        {
            FindClosestPlayer();

            if (ClosestPlayer != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(ClosestPlayer.position, out hit, 1.0f, NavMesh.AllAreas))
                {

                    Agent.SetDestination(hit.position);
                }
                else
                {
                    Debug.LogWarning("Player position is NOT on the NavMesh!");
                }
            }

            if (!Agent.hasPath)
            {
                Debug.LogWarning("Agent has no path!");
            }

            if (Agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogError("Invalid Path!");
            }
        }

        Agent.speed = MoveSpeed.Value;
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
