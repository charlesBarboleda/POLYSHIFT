using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace DestroyIt
{
    /// <summary>
    /// Particle Manager (Singleton) - manages the playing of particle effects and handles performance throttling.
    /// Call the PlayEffect() method, and this script decides whether to play the effect based on how many are currently active.
    /// </summary>
    [DisallowMultipleComponent]
    public class ParticleManager : NetworkBehaviour
    {
        public int maxDestroyedParticles = 20; // Max particles allowed within [withinSeconds].
        public int maxPerDestructible = 5;     // Max particles allowed per destructible object or group.
        public float withinSeconds = 4f;      // Remove particles from the managed list after this many seconds.
        public float updateFrequency = 0.5f;  // Time (in seconds) for updating counters.

        public static ParticleManager Instance { get; private set; }
        private ActiveParticle[] _activeParticles = Array.Empty<ActiveParticle>();

        public ActiveParticle[] ActiveParticles => _activeParticles;

        public bool IsMaxActiveParticles => ActiveParticles.Length >= maxDestroyedParticles;

        private float _nextUpdate;

        // Events
        public event Action ActiveParticlesCounterChangedEvent;

        private ParticleManager() { } // Hide constructor

        private void Start()
        {
            Instance = this;
            _nextUpdate = Time.time + updateFrequency;
        }

        private void Update()
        {
            if (Time.time < _nextUpdate || ActiveParticles.Length == 0) return;

            var currentTime = Time.time;
            var expiredIndices = ActiveParticles
                .Select((particle, index) => (particle, index))
                .Where(pair => currentTime >= pair.particle.InstantiatedTime + withinSeconds)
                .Select(pair => pair.index)
                .ToArray();

            if (expiredIndices.Length > 0)
            {
                _activeParticles = _activeParticles.RemoveAllAt(expiredIndices);
                FireActiveParticlesCounterChangedEvent();
            }


            _nextUpdate = Time.time + updateFrequency;
        }

        /// <summary>
        /// Plays a particle effect and manages its lifecycle.
        /// </summary>
        public void PlayEffect(ParticleSystem particlePrefab, Destructible destObj, Vector3 pos, Quaternion rot, int parentId)
        {
            if (particlePrefab == null)
                particlePrefab = DestructionManager.Instance.defaultParticle;

            // Check maximum particle limits
            if (IsMaxActiveParticles) return;
            if (ActiveParticles.Count(p => p.ParentId == parentId) >= maxPerDestructible) return;

            // Spawn the particle system
            var spawnedObject = DestroyItObjectPool.Instance.Spawn(particlePrefab.gameObject, pos, rot);
            if (spawnedObject == null) return;

            var networkObject = spawnedObject.GetComponent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned)
            {
                networkObject?.Spawn(); // Ensure network object is spawned
            }

            var particleSystem = spawnedObject.GetComponent<ParticleSystem>();
            if (particleSystem == null) return;

            // Track active particles
            var newActiveParticle = new ActiveParticle
            {
                GameObject = spawnedObject,
                InstantiatedTime = Time.time,
                ParentId = parentId
            };
            Array.Resize(ref _activeParticles, _activeParticles.Length + 1);
            _activeParticles[^1] = newActiveParticle;

            FireActiveParticlesCounterChangedEvent();

            // Adjust particle scale and parent if specified
            if (destObj != null)
            {
                AdjustParticleScaleAndParent(spawnedObject, destObj);
                ReplaceParticleMaterials(spawnedObject, destObj);
            }
        }

        private void AdjustParticleScaleAndParent(GameObject particleObject, Destructible destObj)
        {
            if (destObj.fallbackParticleScale != Vector3.one)
            {
                var particleSystems = particleObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    var main = ps.main;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                }

                particleObject.transform.localScale = destObj.fallbackParticleScale;

                var poolAfter = particleObject.GetComponent<PoolAfter>();
                if (poolAfter != null)
                    poolAfter.resetToPrefab = true;
            }

            if (destObj.fallbackParticleParent != null)
            {
                if (destObj.fallbackParticleParent.gameObject.activeInHierarchy)
                    particleObject.transform.SetParent(destObj.fallbackParticleParent, true);
                else
                    particleObject.transform.SetParent(null);
            }
        }

        private void ReplaceParticleMaterials(GameObject particleObject, Destructible destObj)
        {
            if (destObj.fallbackParticleMatOption == 1) return;

            var particleRenderers = particleObject.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (var renderer in particleRenderers)
            {
                if (renderer.renderMode != ParticleSystemRenderMode.Mesh) continue;

                Material newMaterial = destObj.fallbackParticleMatOption switch
                {
                    0 => destObj.GetDestroyedParticleEffectMaterial(),
                    2 => GetReplacementMaterial(renderer, destObj),
                    _ => renderer.sharedMaterial
                };

                if (newMaterial != null)
                    renderer.material = newMaterial;

                if (renderer.sharedMaterial.IsProgressiveDamageCapable())
                {
                    var detailMask = DestructionManager.Instance.GetDetailMask(
                        renderer.sharedMaterial,
                        destObj.damageLevels.Last()
                    );
                    renderer.material.SetTexture("_DetailMask", detailMask);
                }
            }
        }

        private Material GetReplacementMaterial(ParticleSystemRenderer renderer, Destructible destObj)
        {
            var mapping = destObj.replaceParticleMats.FirstOrDefault(m => m.SourceMaterial == renderer.sharedMaterial);
            return mapping?.ReplacementMaterial ?? destObj.GetDestroyedParticleEffectMaterial();
        }

        private void FireActiveParticlesCounterChangedEvent()
        {
            ActiveParticlesCounterChangedEvent?.Invoke();
        }
    }
}
