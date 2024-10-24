using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Netcode.Extensions
{
    public class NetworkObjectPool : NetworkBehaviour
    {
        public static NetworkObjectPool Instance;

        [SerializeField]
        List<PoolConfigObject> PooledPrefabsList;

        private Dictionary<string, Queue<NetworkObject>> pooledObjects = new Dictionary<string, Queue<NetworkObject>>();
        private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();

        private bool m_HasInitialized = false;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public override void OnNetworkSpawn()
        {
            InitializePool();
        }

        public override void OnNetworkDespawn()
        {
            ClearPool();
        }

        public override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            base.OnDestroy();
        }

        public void OnValidate()
        {
            for (var i = 0; i < PooledPrefabsList.Count; i++)
            {
                var prefab = PooledPrefabsList[i].Prefab;
                if (prefab != null)
                {
                    Assert.IsNotNull(
                        prefab.GetComponent<NetworkObject>(),
                        $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component."
                    );
                }

                var prewarmCount = PooledPrefabsList[i].PrewarmCount;
                if (prewarmCount < 0)
                {
                    Debug.LogWarning($"{nameof(NetworkObjectPool)}: Pooled prefab at index {i.ToString()} has a negative prewarm count! Making it not negative.");
                    var thisPooledPrefab = PooledPrefabsList[i];
                    thisPooledPrefab.PrewarmCount *= -1;
                    PooledPrefabsList[i] = thisPooledPrefab;
                }
            }
        }

        /// <summary>
        /// Gets an instance of the given prefab by tag from the pool.
        /// </summary>
        /// <param name="tag">The tag of the prefab.</param>
        /// <returns></returns>
        public NetworkObject GetNetworkObject(string tag)
        {
            return GetNetworkObjectInternal(tag, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Gets an instance of the given prefab by tag from the pool.
        /// </summary>
        /// <param name="tag">The tag of the prefab.</param>
        /// <param name="position">The position to spawn the object at.</param>
        /// <param name="rotation">The rotation to spawn the object with.</param>
        /// <returns></returns>
        public NetworkObject GetNetworkObject(string tag, Vector3 position, Quaternion rotation)
        {
            return GetNetworkObjectInternal(tag, position, rotation);
        }

        /// <summary>
        /// Return an object to the pool by tag (reset objects before returning).
        /// </summary>
        public void ReturnNetworkObject(NetworkObject networkObject, string tag)
        {
            var go = networkObject.gameObject;
            go.SetActive(false);
            pooledObjects[tag].Enqueue(networkObject);
        }

        /// <summary>
        /// Adds a prefab to the list of spawnable prefabs with the given tag.
        /// </summary>
        /// <param name="prefab">The prefab to add.</param>
        /// <param name="tag">The tag associated with the prefab.</param>
        /// <param name="prewarmCount">The number of instances to prewarm in the pool.</param>
        public void AddPrefab(GameObject prefab, string tag, int prewarmCount = 0)
        {
            var networkObject = prefab.GetComponent<NetworkObject>();

            Assert.IsNotNull(networkObject, $"{nameof(prefab)} must have {nameof(networkObject)} component.");
            Assert.IsFalse(prefabLookup.ContainsKey(tag), $"Tag {tag} is already registered in the pool.");

            RegisterPrefabInternal(prefab, tag, prewarmCount);
        }

        private void RegisterPrefabInternal(GameObject prefab, string tag, int prewarmCount)
        {
            prefabLookup[tag] = prefab;
            var prefabQueue = new Queue<NetworkObject>();
            pooledObjects[tag] = prefabQueue;

            for (int i = 0; i < prewarmCount; i++)
            {
                var go = CreateInstance(prefab);
                ReturnNetworkObject(go.GetComponent<NetworkObject>(), tag);
            }

            // Register Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this, tag));
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        private NetworkObject GetNetworkObjectInternal(string tag, Vector3 position, Quaternion rotation)
        {
            if (!pooledObjects.ContainsKey(tag))
            {
                Debug.LogWarning($"No pool exists for tag: {tag}");
                return null;
            }

            var queue = pooledObjects[tag];
            NetworkObject networkObject;

            if (queue.Count > 0)
            {
                networkObject = queue.Dequeue();
            }
            else
            {
                if (!prefabLookup.ContainsKey(tag))
                {
                    Debug.LogError($"Prefab not registered for tag: {tag}");
                    return null;
                }

                networkObject = CreateInstance(prefabLookup[tag]).GetComponent<NetworkObject>();
            }

            var go = networkObject.gameObject;
            go.SetActive(true);
            go.transform.position = position;
            go.transform.rotation = rotation;

            return networkObject;
        }

        public void InitializePool()
        {
            if (m_HasInitialized) return;
            foreach (var configObject in PooledPrefabsList)
            {
                RegisterPrefabInternal(configObject.Prefab, configObject.Tag, configObject.PrewarmCount);
            }
            m_HasInitialized = true;
        }

        public void ClearPool()
        {
            foreach (var prefab in prefabLookup.Values)
            {
                NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
            }
            pooledObjects.Clear();
            prefabLookup.Clear();
        }
    }

    [Serializable]
    public struct PoolConfigObject
    {
        public GameObject Prefab;
        public string Tag;
        public int PrewarmCount;
    }

    class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        GameObject m_Prefab;
        NetworkObjectPool m_Pool;
        string m_Tag;

        public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool, string tag)
        {
            m_Prefab = prefab;
            m_Pool = pool;
            m_Tag = tag;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return m_Pool.GetNetworkObject(m_Tag, position, rotation);
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            m_Pool.ReturnNetworkObject(networkObject, m_Tag);
        }
    }
}
