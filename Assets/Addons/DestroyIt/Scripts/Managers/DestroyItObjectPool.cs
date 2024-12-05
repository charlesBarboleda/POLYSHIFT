using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

namespace DestroyIt
{
    [DisallowMultipleComponent]
    public class DestroyItObjectPool : NetworkBehaviour
    {
        private DestroyItObjectPool() { }

        public List<PoolEntry> prefabsToPool;
        public bool suppressWarnings;

        private GameObject[][] Pool;
        private Dictionary<int, GameObject> autoPooledObjects;
        private GameObject container;

        private static DestroyItObjectPool _instance;
        private bool isInitialized;

        public static DestroyItObjectPool Instance
        {
            get
            {
                if (_instance == null)
                    CreateInstance();

                if (!_instance.isInitialized)
                    _instance.OnNetworkSpawn();

                return _instance;
            }
        }

        private static void CreateInstance()
        {
            DestroyItObjectPool[] objectPools = FindObjectsOfType<DestroyItObjectPool>();
            if (objectPools.Length > 1)
                Debug.LogError("Multiple DestroyItObjectPool scripts found in scene. There can be only one.");
            if (objectPools.Length == 0)
                Debug.LogError("DestroyItObjectPool script not found in scene. This is required for DestroyIt to work properly.");

            _instance = objectPools[0];
        }

        public override void OnNetworkSpawn()
        {
            if (isInitialized || !IsServer) return;
            if (prefabsToPool == null) return;

            // Check if the object pool container already exists. If not, create it.
            GameObject existingContainer = GameObject.Find("DestroyIt_ObjectPool");
            container = existingContainer != null ? existingContainer : new GameObject("DestroyIt_ObjectPool");
            container.SetActive(false);

            autoPooledObjects = autoPooledObjects ?? new Dictionary<int, GameObject>();

            Pool = new GameObject[prefabsToPool.Count][];
            for (int i = 0; i < prefabsToPool.Count; i++)
            {
                PoolEntry poolEntry = prefabsToPool[i];
                Pool[i] = new GameObject[poolEntry.Count];
                for (int n = 0; n < poolEntry.Count; n++)
                {
                    if (poolEntry.Prefab == null) continue;

                    var newObj = Instantiate(poolEntry.Prefab);
                    newObj.GetComponent<NetworkObject>().Spawn(); // Always spawn on the server
                    newObj.name = poolEntry.Prefab.name;
                    PoolObject(newObj);
                }
            }
            isInitialized = true;
            CreateInstance();
        }



        public GameObject Spawn(GameObject originalPrefab, Vector3 position, Quaternion rotation, Transform parent = null, int autoPoolID = 0)
        {
            if (autoPooledObjects != null && autoPoolID != 0 && autoPooledObjects.ContainsKey(autoPoolID))
            {
                GameObject pooledObj = autoPooledObjects[autoPoolID];
                if (pooledObj != null && !pooledObj.activeInHierarchy)
                {
                    SetObjectTransform(pooledObj, position, rotation, parent);
                    pooledObj.SetActive(true);
                    return pooledObj;
                }
            }

            string origPrefabName = originalPrefab.name;

            for (int i = 0; i < prefabsToPool.Count; i++)
            {
                GameObject prefab = prefabsToPool[i].Prefab;

                if (prefab == null || prefab.name != origPrefabName) continue;

                if (Pool != null && Pool[i].Length > 0)
                {
                    for (int j = 0; j < Pool[i].Length; j++)
                    {
                        if (Pool[i][j] != null && !Pool[i][j].activeInHierarchy) // Check if object is inactive
                        {
                            GameObject pooledObj = Pool[i][j];
                            Pool[i][j] = null;
                            SetObjectTransform(pooledObj, position, rotation, parent);
                            pooledObj.SetActive(true);
                            return pooledObj;
                        }
                    }
                }

                if (Pool == null || !prefabsToPool[i].OnlyPooled)
                {
                    GameObject pooledObj = InstantiateObject(prefabsToPool[i].Prefab, position, rotation, parent);
                    pooledObj.name = prefabsToPool[i].Prefab.name;
                    pooledObj.AddTag(Tag.Pooled);
                    return pooledObj;
                }
            }

            return InstantiateObject(originalPrefab, position, rotation, parent);
        }



        public void PoolObject(GameObject obj, bool reenableChildren = false)
        {
            if (obj == null)
            {
                Debug.LogWarning("Attempted to pool a null object.");
                return;
            }

            // Handle network objects
            obj.transform.SetParent(container.transform, true);

            var networkObj = obj.GetComponent<NetworkObject>();
            if (networkObj != null && networkObj.IsSpawned)
            {
                if (IsServer)
                    networkObj.Despawn(false); // Despawn but don't destroy
            }

            // Stop all particle systems
            ParticleSystem[] particleSystems = obj.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Stop();
                ps.Clear();
                var emission = ps.emission;
                emission.enabled = true;
            }

            // Disable physics components
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // Ensure it doesn't interact with physics while pooled
            }


            // Reset and deactivate children if needed
            if (reenableChildren)
            {
                foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
                    child.gameObject.SetActive(true);
            }

            // Reparent to the pool container and deactivate
            obj.SetActive(false);

            Debug.Log($"Pooled object: {obj.name}");
        }




        private static GameObject InstantiateObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject obj = Instantiate(prefab, position, rotation);

            var networkObj = obj.GetComponent<NetworkObject>();
            if (networkObj != null && Instance.IsServer)
            {
                networkObj.Spawn();
            }

            SetObjectTransform(obj, position, rotation, parent);
            return obj;
        }


        private static void SetObjectTransform(GameObject obj, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (parent != null && parent.gameObject.activeInHierarchy)
            {
                obj.transform.SetParent(parent, true); // Set parent and retain world position
                obj.transform.localPosition = position; // Adjust local position relative to parent
            }
            else
            {
                obj.transform.SetParent(null); // Detach from any parent
                obj.transform.position = position; // Set world position
            }

            obj.transform.rotation = rotation; // Set rotation
        }

        public void AddDestructibleObjectToPool(Destructible destObj)
        {
            if (destObj == null)
            {
                Debug.LogError("Destructible object is null. Cannot add to pool.");
                return;
            }

            if (autoPooledObjects == null)
            {
                autoPooledObjects = new Dictionary<int, GameObject>();
            }

            if (autoPooledObjects.ContainsKey(destObj.GetInstanceID())) return;

            if (destObj.destroyedPrefab == null || !destObj.autoPoolDestroyedPrefab)
            {
                // Debug.LogWarning($"Destroyed prefab is null or autoPoolDestroyedPrefab is false for {destObj.name}. Skipping pooling.");
                return;
            }

            var newObj = Instantiate(destObj.destroyedPrefab);
            if (IsServer)
            {
                var networkObj = newObj.GetComponent<NetworkObject>();
                if (networkObj != null)
                    networkObj.Spawn();
                else
                    Debug.LogWarning($"NetworkObject missing on {newObj.name}. Ensure it's properly configured.");
            }

            newObj.transform.SetParent(container.transform, true);

            Destructible[] nestedDestructibles = newObj.GetComponentsInChildren<Destructible>();
            foreach (var nestedObj in nestedDestructibles)
                AddDestructibleObjectToPool(nestedObj);

            newObj.name = destObj.destroyedPrefab.name;
            newObj.SetActive(false);
            autoPooledObjects.Add(destObj.GetInstanceID(), newObj);
        }

        public GameObject SpawnFromOriginal(string prefabName)
        {
            foreach (PoolEntry entry in prefabsToPool)
            {
                if (entry.Prefab != null && entry.Prefab.name == prefabName)
                {
                    GameObject obj = Instantiate(entry.Prefab);
                    if (IsServer)
                        obj.GetComponent<NetworkObject>().Spawn();
                    obj.name = prefabName;
                    return obj;
                }
            }
            return null;
        }
    }
}
