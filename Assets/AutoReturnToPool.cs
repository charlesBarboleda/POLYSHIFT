using Unity.Netcode;
using UnityEngine;

public class AutoReturnToPool : MonoBehaviour
{
    public float timeToReturn = 1f;
    public string tagName;
    public bool isNetworkedObject = false;
    void OnEnable()
    {

        Invoke("ReturnToPool", timeToReturn);


    }

    void ReturnToPool()
    {
        if (isNetworkedObject)
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null)
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
