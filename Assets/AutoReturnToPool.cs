using Unity.Netcode;
using UnityEngine;

public class AutoReturnToPool : NetworkBehaviour
{
    public float timeToReturn = 1f;
    public string tagName;
    public bool isNetworkedObject = false;
    public override void OnNetworkSpawn()
    {

        if (!IsServer) return;
        Debug.Log("AutoReturnToPool enabled on server");
        Invoke("ReturnToPool", timeToReturn);


    }

    void ReturnToPool()
    {
        Debug.Log("ReturnToPool called for " + gameObject.name);
        if (isNetworkedObject)
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                Debug.Log("Despawning network object: " + gameObject.name);
                networkObject.Despawn(false);
                ObjectPooler.Instance.Despawn(tagName, gameObject);

            }
        }
        else
        {
            ObjectPooler.Instance.Despawn(tagName, gameObject);
        }

    }

}
