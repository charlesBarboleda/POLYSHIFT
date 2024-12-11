using Unity.VisualScripting;
using UnityEngine;

public class OutOfBoundsDestruction : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.RequestTakeDamageServerRpc(99999, 0);
        }
    }
}
