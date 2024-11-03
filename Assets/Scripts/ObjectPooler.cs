using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolableItem
{
    [Tooltip("The name for this pool item type (for organization in the inspector).")]
    public string itemName;

    [Tooltip("Prefab of the object to pool.")]
    public GameObject prefab;

    [Tooltip("Number of objects to pre-instantiate for this pool.")]
    public int initialSize = 10;

    [Tooltip("Allow this pool to expand if more objects are needed?")]
    public bool autoExpand = true;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }
    [Tooltip("List of different poolable item types.")]
    [SerializeField]
    private List<PoolableItem> poolableItems = new List<PoolableItem>();

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, PoolableItem> itemLookup;

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
        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        itemLookup = new Dictionary<string, PoolableItem>();

        foreach (var item in poolableItems)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < item.initialSize; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(item.itemName, objectPool);
            itemLookup.Add(item.itemName, item);
        }
    }

    public GameObject Spawn(string itemName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(itemName))
        {
            Debug.LogWarning($"Pool with item name {itemName} doesn't exist.");
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[itemName];

        GameObject obj;
        if (objectPool.Count > 0)
        {
            obj = objectPool.Dequeue();
        }
        else if (itemLookup[itemName].autoExpand)
        {
            // Create a new object if the pool is empty and autoExpand is enabled
            obj = Instantiate(itemLookup[itemName].prefab);
        }
        else
        {
            Debug.LogWarning($"Pool for {itemName} has no available objects and autoExpand is disabled.");
            return null;
        }

        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public void Despawn(string itemName, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(itemName))
        {
            Debug.LogWarning($"Pool with item name {itemName} doesn't exist.");
            Destroy(obj);  // Fallback to destroy if no pool exists for this item
            return;
        }
        ResetObject(obj);
        obj.SetActive(false);
        poolDictionary[itemName].Enqueue(obj);
    }

    void ResetObject(GameObject obj)
    {
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
    }
}
