using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AutoReturnToPool : MonoBehaviour
{
    public float timeToReturn = 1f; // Time before the object is returned to the pool
    public string tagName;

    void OnEnable()
    {
        Invoke(nameof(ReturnToPool), timeToReturn);
    }

    void ReturnToPool()
    {
        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            Debug.Log(gameObject.name + " is despawning");
            networkObject.Despawn(false);
        }
        else
        {
            DespawnRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DespawnRpc()
    {
        ObjectPooler.Instance.Despawn(tagName, gameObject);
    }

}
