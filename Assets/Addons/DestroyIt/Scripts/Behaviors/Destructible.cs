using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable UnusedMember.Global
// ReSharper disable ForCanBeConvertedToForeach

namespace DestroyIt
{
    /// <summary>Put this script on an object you want to be destructible.</summary>
    [DisallowMultipleComponent]
    public class Destructible : NetworkBehaviour
    {
        public float TotalHitPoints
        {
            get { return _totalHitPoints; }
            set
            {
                _totalHitPoints = value;
                damageLevels.CalculateDamageLevels(_totalHitPoints);
            }
        }

        public float CurrentHitPoints
        {
            get { return _currentHitPoints; }
            set { _currentHitPoints = value; }
        }

        [SerializeField]
        [FormerlySerializedAs("totalHitPoints")]
        [HideInInspector]
        private float _totalHitPoints = 50f;

        [SerializeField]
        [FormerlySerializedAs("currentHitPoints")]
        [HideInInspector]
        private float _currentHitPoints = 50f;

        [HideInInspector] public List<DamageLevel> damageLevels;
        [HideInInspector] public GameObject destroyedPrefab;
        [HideInInspector] public GameObject destroyedPrefabParent;
        [HideInInspector] public ParticleSystem fallbackParticle;
        [HideInInspector] public int fallbackParticleMatOption;
        [HideInInspector] public List<DamageEffect> damageEffects;
        [HideInInspector] public float velocityReduction = .5f;
        [HideInInspector] public bool limitDamage = false;
        [HideInInspector] public float minDamage = 10f; // The minimum amount of damage the object can receive per hit.
        [HideInInspector] public float maxDamage = 100f; // The maximum amount of damage the object can receive per hit.
        [HideInInspector] public float minDamageTime = 0f; // The minimum amount of time (in seconds) that must pass before the object can be damaged again.
        [HideInInspector] public float ignoreCollisionsUnder = 2f;
        [HideInInspector] public List<GameObject> unparentOnDestroy;
        [HideInInspector] public bool disableKinematicOnUparentedChildren = true;
        [HideInInspector] public List<MaterialMapping> replaceMaterials;
        [HideInInspector] public List<MaterialMapping> replaceParticleMats;
        [HideInInspector] public bool canBeDestroyed = true;
        [HideInInspector] public bool canBeRepaired = true;
        [HideInInspector] public List<string> debrisToReParentByName;
        [HideInInspector] public bool debrisToReParentIsKinematic;
        [HideInInspector] public List<string> childrenToReParentByName;
        [HideInInspector] public bool isDebrisChipAway;
        [HideInInspector] public float chipAwayDebrisMass = 1f;
        [HideInInspector] public float chipAwayDebrisDrag;
        [HideInInspector] public float chipAwayDebrisAngularDrag = 0.05f;
        [HideInInspector] public bool autoPoolDestroyedPrefab = true;
        [HideInInspector] public bool useFallbackParticle = true;
        [HideInInspector] public Vector3 centerPointOverride;
        [HideInInspector] public Vector3 fallbackParticleScale = Vector3.one;
        [HideInInspector] public Transform fallbackParticleParent;
        [HideInInspector] public bool sinkWhenDestroyed;
        [HideInInspector] public bool shouldDeactivate; // If true, this script will deactivate after a set period of time (configurable on DestructionManager).
        [HideInInspector] public bool isTerrainTree; // Is this Destructible object a stand-in for a terrain tree?
        [HideInInspector] public AudioClip destroyedSound;
        [HideInInspector] public AudioClip damagedSound;
        [HideInInspector] public AudioClip repairedSound;

        // Private variables
        private bool _isDestroyed; // Tracks if the object has been destroyed.
        private DestroyableHealth _health;
        private const float InvulnerableTimer = 0.5f; // How long (in seconds) the destructible object is invulnerable after instantiation.
        private DamageLevel _currentDamageLevel;
        private bool _isInitialized;
        private float _deactivateTimer;
        private bool _firstFixedUpdate = true;
        private Rigidbody _rigidBody; // store a reference to this destructible object's rigidbody, so we don't have to use GetComponent() at runtime.
        private bool _isInvulnerable; // Determines whether the destructible object starts with a short period of invulnerability. Prevents destructible debris being immediately destroyed by the same forces that destroyed the original object.

        // Properties
        public bool UseProgressiveDamage { get; set; } = true; // Used to determine if the shader on the destructible object is
        public bool CheckForClingingDebris { get; set; } = true; // This is an added optimization used when we are auto-pooling destroyed prefabs. It allows us to avoid a GetComponentsInChildren() check for ClingPoints destruction time.
        public Rigidbody[] PooledRigidbodies { get; set; } // This is an added optimization used when we are auto-pooling destroyed prefabs. It allows us to avoid multiple GetComponentsInChildren() checks for Rigidbodies at destruction time.
        public GameObject[] PooledRigidbodyGos { get; set; } // This is an added optimization used when we are auto-pooling destroyed prefabs. It allows us to avoid multiple GetComponentsInChildren() checks for the GameObjects on Rigidibodies at destruction time.
        public float VelocityReduction => Mathf.Abs(velocityReduction - 1f) /* invert the velocity reduction value (so it makes sense in the UI) */;
        public Quaternion RotationFixedUpdate { get; private set; }
        public Vector3 PositionFixedUpdate { get; private set; }
        public Vector3 VelocityFixedUpdate { get; private set; }
        public Vector3 AngularVelocityFixedUpdate { get; private set; }
        public float LastRepairedAmount { get; private set; }
        public float LastDamagedAmount { get; private set; }
        public float LastDamagedTime { get; private set; }
        public bool IsDestroyed => _isDestroyed; // Expose the destruction state to external logic.

        public Vector3 MeshCenterPoint { get; private set; }

        // Events
        public event Action DamagedEvent;
        public event Action DestroyedEvent;
        public event Action RepairedEvent;

        public override void OnNetworkSpawn()
        {
            _health = GetComponent<DestroyableHealth>();
            if (_health != null)
            {
                TotalHitPoints = _health.MaxHealth;
                CurrentHitPoints = TotalHitPoints;
            }

            if (!IsServer) return; // Ensure initialization happens only on the server

            _isDestroyed = false; // Reset destruction state


            CheckForClingingDebris = true;

            if (damageLevels == null || damageLevels.Count == 0)
                damageLevels = DestructibleHelper.DefaultDamageLevels();
            damageLevels.CalculateDamageLevels(TotalHitPoints);

            _rigidBody = GetComponent<Rigidbody>();

            if (useFallbackParticle)
            {
                MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
                MeshCenterPoint = gameObject.GetMeshCenterPoint(meshRenderers);

                if (gameObject.IsAnyMeshPartOfStaticBatch(meshRenderers) && centerPointOverride == Vector3.zero)
                    Debug.LogWarning($"[{gameObject.name}] may not have fallback particles spawn as expected.");
            }

            PlayDamageEffects();

            _isInvulnerable = true;
            Invoke(nameof(RemoveInvulnerability), InvulnerableTimer);

            if (autoPoolDestroyedPrefab)
                DestroyItObjectPool.Instance.AddDestructibleObjectToPool(this);

            _isInitialized = true;
        }

        public void RemoveInvulnerability()
        {
            _isInvulnerable = false;
        }

        public void FixedUpdate()
        {
            if (!_isInitialized) return;
            DestructionManager destructionManager = DestructionManager.Instance;
            if (destructionManager == null) return;

            // Use the fixed update position/rotation for placement of the destroyed prefab.
            PositionFixedUpdate = transform.position;
            RotationFixedUpdate = transform.rotation;
            if (_rigidBody != null)
            {
                VelocityFixedUpdate = _rigidBody.linearVelocity;
                AngularVelocityFixedUpdate = _rigidBody.angularVelocity;
            }

            SetDamageLevel();
            PlayDamageEffects();

            // Check if this script should be auto-deactivated, as configured on the DestructionManager
            if (destructionManager.autoDeactivateDestructibles && !isTerrainTree && shouldDeactivate)
                UpdateDeactivation(destructionManager.deactivateAfter);
            else if (destructionManager.autoDeactivateDestructibleTerrainObjects && isTerrainTree && shouldDeactivate)
                UpdateDeactivation(destructionManager.deactivateAfter);

            if (IsDestroyed)
                destructionManager.ProcessDestruction(this, destroyedPrefab, new ExplosiveDamage());

            // If this is the first fixed update frame and autoDeativateDestructibles is true, start this component deactivated.
            if (_firstFixedUpdate)
                this.SetActiveOrInactive(destructionManager);

            _firstFixedUpdate = false;
        }

        private void UpdateDeactivation(float deactivateAfter)
        {
            if (_deactivateTimer > deactivateAfter)
            {
                _deactivateTimer = 0f;
                shouldDeactivate = false;
                enabled = false;
            }
            else
                _deactivateTimer += Time.fixedDeltaTime;
        }

        /// <summary>Applies a generic amount of damage, with no specific impact or explosive force.</summary>
        public void ApplyDamage(float amount)
        {
            if (!IsServer) return; // Only the server processes damage

            if (IsDestroyed || _isInvulnerable || !DestructionManager.Instance.allowDamage) return;

            if (limitDamage)
            {
                if (LastDamagedTime > 0f && minDamageTime > 0f && Time.time < LastDamagedTime + minDamageTime) return;
                if (maxDamage >= 0 && amount > maxDamage) amount = maxDamage;
                if (minDamage >= 0 && minDamage <= maxDamage && amount < minDamage) amount = minDamage;
                if (amount <= 0) return;
            }

            LastDamagedAmount = amount;
            LastDamagedTime = Time.time;
            FireDamagedEvent();

            Debug.Log($"Applying damage: {amount} to {gameObject.name}. Current HP: {CurrentHitPoints - amount}");

            if (damagedSound != null)
                AudioSource.PlayClipAtPoint(damagedSound, transform.position);

            CurrentHitPoints -= amount;
            if (CurrentHitPoints <= 0)
            {
                Debug.Log($"Object {gameObject.name} destroyed!");
                CurrentHitPoints = 0;

                if (!_isDestroyed) // Ensure destruction logic is only triggered once
                {
                    Debug.Log("Executing Destroy method");
                    Destroy();
                    Debug.Log("Destroy method executed");
                }
            }
        }


        public void ApplyDamage(Damage damage)
        {
            if (IsDestroyed || _isInvulnerable || !DestructionManager.Instance.allowDamage) return; // don't try to apply damage to an already-destroyed or invulnerable object, or if damaging object is not allowed.

            // Adjust the damage based on Min/Max/Time thresholds.
            if (limitDamage)
            {
                if (LastDamagedTime > 0f && minDamageTime > 0f && Time.time < LastDamagedTime + minDamageTime)
                    return;

                if (maxDamage >= 0 && damage.DamageAmount > maxDamage)
                    damage.DamageAmount = maxDamage;

                if (minDamage >= 0 && minDamage <= maxDamage && damage.DamageAmount < minDamage)
                    damage.DamageAmount = minDamage;

                if (damage.DamageAmount <= 0) return;
            }

            LastDamagedAmount = damage.DamageAmount;
            LastDamagedTime = Time.time;
            FireDamagedEvent();

            // Check for any audio clip we may need to play
            if (damagedSound != null)
                AudioSource.PlayClipAtPoint(damagedSound, transform.position);

            CurrentHitPoints -= damage.DamageAmount;
            if (CurrentHitPoints > 0) return;
            if (CurrentHitPoints < 0) CurrentHitPoints = 0;

            PlayDamageEffects();


            if (IsDestroyed)
                DestructionManager.Instance.ProcessDestruction(this, destroyedPrefab, damage);

        }

        public void RepairDamage(float amount)
        {
            if (IsDestroyed || !canBeRepaired) return; // object cannot be repaired if it is either already destroyed or not repairable.

            LastRepairedAmount = amount;

            CurrentHitPoints += amount;
            if (CurrentHitPoints > TotalHitPoints) // object cannot be over-repaired beyond its total hit points.
                CurrentHitPoints = TotalHitPoints;

            PlayDamageEffects();
            FireRepairedEvent();

            // Check for any audio clip we may need to play
            if (repairedSound != null)
                AudioSource.PlayClipAtPoint(repairedSound, transform.position);
        }

        public void Destroy()
        {
            if (!IsServer || _isDestroyed || _isInvulnerable)
            {
                Debug.Log($"Destroy skipped for {gameObject.name}. IsServer: {IsServer}, _isDestroyed: {_isDestroyed}, _isInvulnerable: {_isInvulnerable}");
                return;
            }
            _isDestroyed = true; // Mark the object as destroyed

            LastDamagedAmount = CurrentHitPoints;
            LastDamagedTime = Time.time;

            Debug.Log("Firing DamagedEvent");
            FireDamagedEvent();

            CurrentHitPoints = 0; // Ensure health is zeroed out

            Debug.Log("Cleaning up effects");
            CleanupEffects(); // Cleanup active effects like flames
            Debug.Log("Effects cleaned up");

            Debug.Log("Playing final damage effects");
            PlayDamageEffects();
            Debug.Log("Final damage effects played");

            Debug.Log("Triggering DestructionManager.ProcessDestruction");
            DestructionManager.Instance.ProcessDestruction(this, destroyedPrefab, CurrentHitPoints);
            Debug.Log($"DestructionManager.ProcessDestruction completed for {gameObject.name}");
        }



        private void CleanupEffects()
        {
            foreach (DamageEffect effect in damageEffects)
            {
                if (effect.GameObject != null)
                {
                    Debug.Log($"Cleaning up effect: {effect.GameObject.name}");

                    foreach (var ps in effect.ParticleSystems)
                    {
                        Debug.Log($"Stopping ParticleSystem on {effect.GameObject.name}");
                        ps.Stop();
                        ps.Clear();
                    }

                    var networkObject = effect.GameObject.GetComponent<NetworkObject>();
                    if (networkObject != null && networkObject.IsSpawned)
                    {
                        Debug.Log($"Despawning NetworkObject: {effect.GameObject.name}");
                        if (IsServer)
                            networkObject.Despawn();
                    }
                    else
                    {
                        Debug.Log($"Destroying non-networked object: {effect.GameObject.name}");
                        Destroy(effect.GameObject);
                    }

                    effect.GameObject.SetActive(false); // Ensure inactive
                    effect.GameObject = null; // Clear reference
                }
            }
        }







        /// <summary>Advances the damage state, applies damage-level materials as needed, and plays particle effects.</summary>
        private void SetDamageLevel()
        {
            DamageLevel damageLevel = damageLevels?.GetDamageLevel(CurrentHitPoints);
            if (damageLevel == null) return;
            if (_currentDamageLevel != null && damageLevel == _currentDamageLevel) return;

            _currentDamageLevel = damageLevel;
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer rend in renderers)
            {
                Destructible parentDestructible = rend.GetComponentInParent<Destructible>(); // child Destructible objects should not be affected by damage on their parents.
                if (parentDestructible != this) continue;
                bool isAcceptableRenderer = rend is MeshRenderer || rend is SkinnedMeshRenderer;
                if (isAcceptableRenderer && !rend.gameObject.HasTag(Tag.ClingingDebris) && rend.gameObject.layer != DestructionManager.Instance.debrisLayer)
                {
                    for (int j = 0; j < rend.sharedMaterials.Length; j++)
                        DestructionManager.Instance.SetProgressiveDamageTexture(rend, rend.sharedMaterials[j], _currentDamageLevel);
                }
            }

            PlayDamageEffects();
        }

        /// <summary>Gets the material to use for the fallback particle effect when this Destructible object is destroyed.</summary>
        public Material GetDestroyedParticleEffectMaterial()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer rend in renderers)
            {
                Destructible parentDestructible = rend.GetComponentInParent<Destructible>(); // only get the material for the parent object to use for a particle effect
                if (parentDestructible != this) continue;
                bool isAcceptableRenderer = rend is MeshRenderer || rend is SkinnedMeshRenderer;
                if (isAcceptableRenderer)
                    return rend.sharedMaterial;
            }

            return null; // could not find an acceptable material to use for particle effects
        }

        private void PlayDamageEffects()
        {
            if (!IsServer || _isDestroyed) return; // Skip if destroyed or not server

            if (damageEffects == null || damageEffects.Count == 0) return;

            int currentDamageLevelIndex = damageLevels.IndexOf(_currentDamageLevel ?? new DamageLevel());

            foreach (DamageEffect effect in damageEffects)
            {
                if (effect == null || effect.Prefab == null) continue;

                Quaternion rotation = transform.rotation * Quaternion.Euler(effect.Rotation);

                // Only trigger effects when damage levels match or when object is destroyed
                if (_currentDamageLevel != null && effect.TriggeredAt < damageLevels.Count)
                {
                    if (currentDamageLevelIndex >= effect.TriggeredAt && !effect.HasStarted)
                    {
                        SpawnEffect(effect, rotation);
                        effect.HasStarted = true;
                    }
                    else if (currentDamageLevelIndex < effect.TriggeredAt && effect.HasStarted)
                    {
                        DisableEffect(effect);
                    }
                }

                // Handle effects triggered at destruction
                if (effect.TriggeredAt == damageLevels.Count && IsDestroyed && !effect.HasStarted)
                {
                    Debug.Log($"Triggering destruction effect for {effect.Prefab.name}");
                    SpawnEffect(effect, rotation);
                    effect.HasStarted = true;
                }
                else if (IsDestroyed && effect.HasStarted)
                {
                    Debug.Log($"Effect {effect.Prefab.name} already started. Skipping.");
                }
            }
        }





        // NOTE: OnCollisionEnter will only fire if a rigidbody is attached to this object!
        public void OnCollisionEnter(Collision collision)
        {
            if (DestructionManager.Instance == null) return;
            if (!isActiveAndEnabled) return;

            this.ProcessDestructibleCollision(collision, GetComponent<Rigidbody>());

            if (collision.contacts.Length <= 0) return;

            Destructible destructibleObj = collision.contacts[0].otherCollider.gameObject.GetComponentInParent<Destructible>();
            if (destructibleObj != null && collision.contacts[0].otherCollider.attachedRigidbody == null)
                destructibleObj.ProcessDestructibleCollision(collision, GetComponent<Rigidbody>());
        }
        private void SpawnEffect(DamageEffect effect, Quaternion rotation)
        {
            if (effect.GameObject != null) return; // Skip if the effect is already active

            effect.GameObject = DestroyItObjectPool.Instance.Spawn(
                effect.Prefab,
                Vector3.zero, // Spawn at origin for now
                Quaternion.identity, // Default rotation
                transform); // Parent it to the object

            if (effect.GameObject != null)
            {
                var networkObject = effect.GameObject.GetComponent<NetworkObject>();
                if (networkObject != null && !networkObject.IsSpawned)
                {
                    if (IsServer)
                    {
                        Debug.Log($"Spawning NetworkObject: {effect.GameObject.name}");
                        networkObject.Spawn(); // Server-side spawning
                    }
                }

                effect.GameObject.transform.localPosition = effect.Offset; // Set local position
                effect.GameObject.transform.localRotation = Quaternion.Euler(effect.Rotation); // Set local rotation
                effect.ParticleSystems = effect.GameObject.GetComponentsInChildren<ParticleSystem>();

                if (effect.Scale != Vector3.one)
                {
                    foreach (ParticleSystem ps in effect.ParticleSystems)
                    {
                        var main = ps.main;
                        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                    }

                    effect.GameObject.transform.localScale = effect.Scale;
                }
            }
        }



        private void DisableEffect(DamageEffect effect)
        {
            if (effect.GameObject == null) return;

            foreach (var ps in effect.ParticleSystems)
            {
                var emission = ps.emission;
                emission.enabled = false; // Disable emission
                ps.Stop();
                ps.Clear();
            }

            var networkObject = effect.GameObject.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                if (IsServer)
                    networkObject.Despawn(false); // Server-side despawn
            }
            else
            {
                Destroy(effect.GameObject); // Destroy non-networked objects
            }

            effect.GameObject.SetActive(false); // Ensure it is inactive
            effect.GameObject = null; // Clear reference
            effect.HasStarted = false;
        }


        // NOTE: OnDrawGizmos will only fire if Gizmos are turned on in the Unity Editor!
        public void OnDrawGizmos()
        {
            damageEffects.DrawGizmos(transform);
            centerPointOverride.DrawGizmos(transform);
        }

        public void FireDestroyedEvent()
        {
            DestroyedEvent?.Invoke(); // If there is at least one listener, trigger the event.
        }

        public void FireRepairedEvent()
        {
            RepairedEvent?.Invoke(); // If there is at least one listener, trigger the event.
        }

        public void FireDamagedEvent()
        {
            DamagedEvent?.Invoke(); // If there is at least one listener, trigger the event.
        }


        public void SyncHealth(float amount)
        {
            CurrentHitPoints = amount;
        }
    }
}