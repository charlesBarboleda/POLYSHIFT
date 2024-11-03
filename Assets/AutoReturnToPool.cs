using Unity.Netcode;
using UnityEngine;

public class AutoReturnToPool : MonoBehaviour
{
    public float timeToReturn = 1f;
    public string tagName;
    void OnEnable()
    {
        Invoke("ReturnToPool", timeToReturn);
    }

    void ReturnToPool()
    {
        ObjectPooler.Instance.Despawn(tagName, gameObject);
    }

}
