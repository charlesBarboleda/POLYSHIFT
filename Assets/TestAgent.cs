using UnityEngine;
using UnityEngine.AI;

public class TestAgent : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform target;

    void Update()
    {
        if (target != null)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }
}
