using Unity.Netcode;
using UnityEngine;

public class PersistentGameObject : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("Awake called on " + gameObject.name);
        DontDestroyOnLoad(gameObject);
        Debug.Log("DontDestroyOnLoad called on " + gameObject.name);
    }
}
