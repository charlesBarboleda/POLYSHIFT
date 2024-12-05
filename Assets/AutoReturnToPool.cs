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
        Invoke("ReturnToPool", timeToReturn);


    }

    void ReturnToPool()
    {
        if (isNetworkedObject)
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
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
