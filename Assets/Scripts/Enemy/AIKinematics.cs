using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class AIKinematics : NetworkBehaviour
{
    public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NavMeshAgent _agent;
    Transform _closestPlayer;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

    }

    void OnEnable()
    {

    }
    public override void OnNetworkSpawn()
    {
        Debug.Log("IsServer OnNetworkSpawn: " + IsServer);
        if (!IsServer)
        {
            // Disable AI logic on clients since only the server should run this
            enabled = false;
        }
    }

    void Update()
    {

        if (IsServer)
        {
            FindClosestPlayer();

            if (_closestPlayer != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(_closestPlayer.position, out hit, 1.0f, NavMesh.AllAreas))
                {

                    _agent.SetDestination(hit.position);
                }
                else
                {
                    Debug.LogWarning("Player position is NOT on the NavMesh!");
                }
            }

            if (!_agent.hasPath)
            {
                Debug.LogWarning("Agent has no path!");
            }

            if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogError("Invalid Path!");
            }
        }

        _agent.speed = MoveSpeed.Value;
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

        _closestPlayer = closestPlayer;
    }
}
