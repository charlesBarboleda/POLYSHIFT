﻿using Unity.Netcode;
using UnityEngine;

namespace DestroyIt
{
    public class PoolAfter : NetworkBehaviour
    {
        public float seconds;           // seconds to wait before re-pooling this game object.
        public bool reenableChildren;   // determines whether to re-enable all child objects when this object is pooled.
        public bool removeWhenPooled;   // Remove this script when the object is pooled?
        public bool resetToPrefab;      // Reset the entire object back to prefab? (This means it will destroy and recreate the object.)

        private float _timeLeft;
        private bool _isInitialized;

        public override void OnNetworkSpawn()
        {
            _timeLeft = seconds;
            _isInitialized = true;
        }

        void OnEnable()
        {
            _timeLeft = seconds;
        }

        void Update()
        {
            if (!_isInitialized) return;

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0)
            {
                if (resetToPrefab)
                {
                    GameObject objectToPool = DestroyItObjectPool.Instance.SpawnFromOriginal(this.gameObject.name);
                    if (objectToPool != null)
                        DestroyItObjectPool.Instance.PoolObject(objectToPool);

                    Destroy(this.gameObject);
                    _isInitialized = false;
                    return;
                }

                if (removeWhenPooled)
                    Destroy(this);

                DestroyItObjectPool.Instance.PoolObject(this.gameObject, reenableChildren);
            }
        }
    }
}