using System.Collections.Generic;
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
            if (isInitialized) return;
            if (prefabsToPool == null) return;

            // Check if the object pool container already exists. If so, use it.
            GameObject existingContainer = GameObject.Find("DestroyIt_ObjectPool");
            container = existingContainer != null ? existingContainer : new GameObject("DestroyIt_ObjectPool");
            container.SetActive(false);

            autoPooledObjects = new Dictionary<int, GameObject>();

            Pool = new GameObject[prefabsToPool.Count][];
            for (int i = 0; i < prefabsToPool.Count; i++)
            {
                PoolEntry poolEntry = prefabsToPool[i];
                Pool[i] = new GameObject[poolEntry.Count];
                for (int n = 0; n < poolEntry.Count; n++)
                {
                    if (poolEntry.Prefab == null) continue;
                    var newObj = Instantiate(poolEntry.Prefab);
                    newObj.GetComponent<NetworkObject>().Spawn();
                    newObj.name = poolEntry.Prefab.name;
                    PoolObject(newObj);
                }
            }
            isInitialized = true;
            CreateInstance();
        }

        public void AddDestructibleObjectToPool(Destructible destObj)
        {
            if (autoPooledObjects.ContainsKey(destObj.GetInstanceID())) return;

            if (destObj.destroyedPrefab != null && destObj.autoPoolDestroyedPrefab)
            {
                var newObj = Instantiate(destObj.destroyedPrefab);
                newObj.GetComponent<NetworkObject>().Spawn();
                newObj.transform.SetParent(container.transform, true);

                Destructible[] destObjectsInObject = newObj.GetComponentsInChildren<Destructible>();
                for (int i = 0; i < destObjectsInObject.Length; i++)
                    AddDestructibleObjectToPool(destObjectsInObject[i]);

                newObj.name = destObj.destroyedPrefab.name;
                newObj.AddTag(Tag.Pooled);
                DestructibleHelper.TransferMaterials(destObj, newObj);

                ClingPoint[] clingPoints = newObj.GetComponentsInChildren<ClingPoint>();
                if (clingPoints.Length == 0)
                    destObj.CheckForClingingDebris = false;

                destObj.PooledRigidbodies = newObj.GetComponentsInChildren<Rigidbody>();
                destObj.PooledRigidbodyGos = new GameObject[destObj.PooledRigidbodies.Length];
                for (int i = 0; i < destObj.PooledRigidbodies.Length; i++)
                    destObj.PooledRigidbodyGos[i] = destObj.PooledRigidbodies[i].gameObject;

                newObj.SetActive(false);
                autoPooledObjects.Add(destObj.GetInstanceID(), newObj);
            }
        }

        public GameObject SpawnFromOriginal(string prefabName)
        {
            foreach (PoolEntry entry in prefabsToPool)
            {
                if (entry.Prefab != null && entry.Prefab.name == prefabName)
                {
                    GameObject obj = Instantiate(entry.Prefab);
                    obj.GetComponent<NetworkObject>().Spawn();
                    obj.name = prefabName;
                    return obj;
                }
            }
            return null;
        }

        private static GameObject InstantiateObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject obj = Instantiate(prefab, position, rotation);
            obj.GetComponent<NetworkObject>().Spawn();

            if (obj == null) return null;

            if (parent != null && parent.gameObject.activeInHierarchy)
            {
                obj.transform.SetParent(parent, true);
                obj.transform.localPosition = position;
            }
            else
            {
                obj.transform.SetParent(null);
                obj.transform.position = position;
            }

            obj.transform.rotation = rotation;

            return obj;
        }

        public GameObject Spawn(GameObject originalPrefab, Vector3 position, Quaternion rotation, Transform parent, int autoPoolID = 0)
        {
            if (autoPooledObjects != null && autoPoolID != 0 && autoPooledObjects.ContainsKey(autoPoolID))
            {
                GameObject pooledObj = autoPooledObjects[autoPoolID];
                if (pooledObj != null)
                {
                    if (parent != null && parent.gameObject.activeInHierarchy)
                    {
                        pooledObj.transform.SetParent(parent, true);
                        pooledObj.transform.localPosition = position;
                    }
                    else
                    {
                        pooledObj.transform.SetParent(null);
                        pooledObj.transform.position = position;
                    }
                    pooledObj.transform.rotation = rotation;
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
                        if (Pool[i][j] != null)
                        {
                            GameObject pooledObj = Pool[i][j];
                            Pool[i][j] = null;
                            if (parent != null && parent.gameObject.activeInHierarchy)
                            {
                                pooledObj.transform.SetParent(parent, true);
                                pooledObj.transform.localPosition = position;
                            }
                            else
                            {
                                pooledObj.transform.SetParent(null);
                                pooledObj.transform.position = position;
                            }
                            pooledObj.transform.rotation = rotation;
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

        public GameObject Spawn(GameObject originalPrefab, Vector3 position, Quaternion rotation, int autoPoolID = 0)
        {
            return Spawn(originalPrefab, position, rotation, null, autoPoolID);
        }

        public void PoolObject(GameObject obj, bool reenableChildren = false)
        {
            for (int i = 0; i < prefabsToPool.Count; i++)
            {
                if (prefabsToPool[i].Prefab == null || prefabsToPool[i].Prefab.name != obj.name) continue;

                obj.transform.SetParent(container.transform, true);
                ParticleSystem[] particleSystems = obj.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    ps.Stop();
                    ps.Clear();
                    var emission = ps.emission;
                    emission.enabled = true;
                }
                if (reenableChildren)
                {
                    foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
                        child.gameObject.SetActive(true);
                }

                obj.AddTag(Tag.Pooled);
                obj.SetActive(false);

                for (int j = 0; j < Pool[i].Length; j++)
                {
                    if (Pool[i][j] == null)
                    {
                        Pool[i][j] = obj;
                        return;
                    }
                }

                Destroy(obj);
                return;
            }
            Destroy(obj);
        }
    }
}
