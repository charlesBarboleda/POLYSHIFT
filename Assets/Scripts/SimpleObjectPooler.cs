using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPooler : MonoBehaviour
{
    public static SimpleObjectPooler Instance;
    [Tooltip("The prefab of the object to be pooled.")]
    public GameObject prefab;

    [Tooltip("The initial number of objects to instantiate.")]
    public int initialPoolSize = 10;

    private Queue<GameObject> poolQueue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        // Initialize the pool
        poolQueue = new Queue<GameObject>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        // Check if we have any objects available in the pool
        if (poolQueue.Count > 0)
        {
            obj = poolQueue.Dequeue();
        }
        else
        {
            // Expand the pool if it's empty
            obj = Instantiate(prefab);
        }

        // Activate and set position/rotation
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public void Despawn(GameObject obj)
    {
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}
