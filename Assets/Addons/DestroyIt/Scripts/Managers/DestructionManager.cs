using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;

namespace DestroyIt
{
    /// <summary>
    /// Destruction Manager (Singleton) - manages all destructible objects.
    /// Put this script on an empty game object in your scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class DestructionManager : NetworkBehaviour
    {
        [Tooltip("If true, Destructible scripts will be deactivated on start, and will activate any time they are inside a trigger collider with the ActivateDestructibles script on it.")]
        [HideInInspector] public bool autoDeactivateDestructibles;
        [Tooltip("If true, Destructible terrain object scripts will be deactivated on start, and will activate any time they are inside a trigger collider with the ActivateDestructibles script on it.")]
        [HideInInspector] public bool autoDeactivateDestructibleTerrainObjects = true;
        [Tooltip("If true, Destructible terrain tree scripts will not be activated by ActivateDestructibles scripts. Recommended to leave this true for performance, unless you need to move trees during the game or use progressive damage textures on them.")]
        [HideInInspector] public bool destructibleTreesStayDeactivated = true;
        [Tooltip("The time in seconds to automatically deactivate Destructible scripts when they are outside an ActivateDestructibles trigger area.")]
        [HideInInspector] public float deactivateAfter = 2f;

        [Tooltip("If true, Destructible objects can be damaged and destroyed. Turn this off if you want to globally deactivate Destructible objects taking damage.")]
        public bool allowDamage = true;
        [Tooltip("Maximum allowed persistent debris pieces in the scene.")]
        public int maxPersistentDebris = 400;
        [Tooltip("Maximum allowed destroyed prefabs within [withinSeconds] seconds. When this limit is reached, a particle effect will be used instead.")]
        public int destroyedPrefabLimit = 15;
        [Tooltip("Number of seconds within which no more than [destroyedPrefabLimit] destructions will be instantiated.")]
        public int withinSeconds = 4;
        [Tooltip("The default particle effect to use when an object is destroyed.")]
        public ParticleSystem defaultParticle;
        [Tooltip("If true, persistent debris is allowed to be culled even if the camera is currently rendering it.")]
        public bool removeVisibleDebris = true;
        [Tooltip("The time (in seconds) this script processes updates.")]
        public float updateFrequency = .5f;

        [HideInInspector]
        public bool useCameraDistanceLimit = true;  // If true, things beyond the specified distance from the main camera will be destroyed in a more limiting (ie, higher performance) way.
        [HideInInspector]
        public int cameraDistanceLimit = 100;       // Specified game units (usually meters) from camera, where destruction limiting will occur.
        [HideInInspector]
        public int debrisLayer = -1;
        [HideInInspector]
        public Collider[] overlapColliders; // These are the colliders overlapped by an Overlap Sphere (used for determining affected objects in a blast radius without allocating GC).

        // Private Variables
        private float _nextUpdate;
        private List<Destructible> _destroyedObjects;
        private List<Debris> _debrisPieces;
        private List<Texture2D> _detailMasks;

        // Events
        public event Action DestroyedPrefabCounterChangedEvent;
        public event Action ActiveDebrisCounterChangedEvent;

        // Properties
        public List<float> DestroyedPrefabCounter { get; private set; }

        public bool IsDestroyedPrefabLimitReached => DestroyedPrefabCounter.Count >= destroyedPrefabLimit;

        public int ActiveDebrisCount
        {
            get
            {
                int count = 0;
                foreach (Debris debris in _debrisPieces)
                {
                    if (debris.IsActive)
                        count++;
                }
                return count;
            }
        }

        // Hide the default constructor (use DestructionManager.Instance instead).
        private DestructionManager() { }

        // Private reference only this class can access
        private static DestructionManager _instance;

        // This is the public reference that other classes will use
        public static DestructionManager Instance
        {
            get
            {
                // If _instance hasn't been set yet, we grab it from the scene.
                // This will only happen the first time this reference is used.
                if (_instance == null)
                    _instance = FindObjectOfType<DestructionManager>();
                return _instance;
            }
        }

        public void Start()
        {
            // Initialize variables
            DestroyedPrefabCounter = new List<float>();
            overlapColliders = new Collider[100];
            _detailMasks = Resources.LoadAll<Texture2D>("ProgressiveDamage").ToList();
            debrisLayer = LayerMask.NameToLayer("DestroyItDebris");
            _debrisPieces = new List<Debris>();
            _destroyedObjects = new List<Destructible>();
            _nextUpdate = Time.time + updateFrequency;

            // If the default particle hasn't been assigned, try to get it from the Resources folder.
            if (defaultParticle == null)
                defaultParticle = Resources.Load<ParticleSystem>("Default_Particles/DefaultLargeParticle");

            // Checks
            Check.IsDefaultParticleAssigned();
            if (Check.LayerExists("DestroyItDebris", false) == false)
                Debug.LogWarning("DestroyItDebris layer not found. Add a layer named 'DestroyItDebris' to your project if you want debris to ignore other debris when using Cling Points.");
        }

        private void Update()
        {
            if (Time.time < _nextUpdate) return;

            // Manage Destroyed Prefab counter
            DestroyedPrefabCounter.Update(withinSeconds);

            // Manage Debris Queue
            if (_debrisPieces.Count > 0)
            {
                // Cleanup references to debris no longer in the game
                int itemsRemoved = _debrisPieces.RemoveAll(x => x == null || !x.IsActive);
                if (itemsRemoved > 0)
                    FireActiveDebrisCounterChangedEvent();
                //TODO: Debris is getting removed from the list, but not destroyed from the game. Debris parent objects should probably check their children periodically for enabled meshes.

                // Disable debris until the Max Debris limit is satisfied.
                if (ActiveDebrisCount > maxPersistentDebris)
                {
                    int overBy = ActiveDebrisCount - maxPersistentDebris;

                    foreach (Debris debris in _debrisPieces)
                    {
                        if (overBy <= 0) break;
                        if (!debris.IsActive) continue;
                        if (!removeVisibleDebris)
                        {
                            if (debris.Rigidbody.GetComponent<Renderer>() == null) continue;
                            if (debris.Rigidbody.GetComponent<Renderer>().isVisible) continue;
                        }
                        // Disable the debris.
                        debris.Disable();
                        overBy -= 1;
                    }
                }
            }

            // Manage Destroyed Objects list (ie, we're spacing out the Destroy() calls for performance)
            if (_destroyedObjects.Count > 0)
            {
                // Destroy a maximum of 5 gameobjects per update, to space it out a little.
                int nbrObjects = _destroyedObjects.Count > 5 ? 5 : _destroyedObjects.Count;
                for (int i = 0; i < nbrObjects; i++)
                {
                    // Destroy the gameobject and remove it from the list.
                    if (_destroyedObjects[i] != null && _destroyedObjects[i].gameObject != null)
                        Destroy(_destroyedObjects[i].gameObject);
                }
                _destroyedObjects.RemoveRange(0, nbrObjects);
            }

            _nextUpdate = Time.time + updateFrequency; // reset the next update time.
        }

        /// <summary>Swaps the current destructible object with a new one and applies the correct materials to the new object.</summary>
        public void ProcessDestruction<T>(Destructible oldObj, GameObject destroyedPrefab, T damageInfo)
        {
            if (oldObj == null || !oldObj.canBeDestroyed) return;

            oldObj.FireDestroyedEvent();

            // Check for any audio clips we may need to play
            if (oldObj.destroyedSound != null)
                AudioSource.PlayClipAtPoint(oldObj.destroyedSound, oldObj.transform.position);

            oldObj.ReleaseClingingDebris();

            // Remove any Joints from the destroyed object
            foreach (Joint jnt in oldObj.GetComponentsInChildren<Joint>())
                Destroy(jnt);

            // Use fallback particle effect if prefab destruction is skipped
            if (destroyedPrefab == null || IsDestroyedPrefabLimitReached)
            {
                DestroyWithParticleEffect(oldObj, oldObj.fallbackParticle, damageInfo);
                return;
            }

            // Use the object pooler to spawn the destroyed prefab on the server
            if (IsServer)
            {
                GameObject newObj = DestroyItObjectPool.Instance.Spawn(destroyedPrefab, oldObj.PositionFixedUpdate, oldObj.RotationFixedUpdate);
                newObj.GetComponent<NetworkObject>()?.Spawn();
                InstantiateDebris(newObj, oldObj, damageInfo);
            }

            Destroy(oldObj);
            _destroyedObjects.Add(oldObj);
        }


        private void DestroyWithParticleEffect<T>(Destructible oldObj, ParticleSystem customParticle, T damageInfo)
        {

            if (oldObj.useFallbackParticle)
            {
                // Use the DestructibleGroup instance ID if it exists, otherwise use the Destructible object's parent's instance ID.
                GameObject parentObj = oldObj.gameObject.GetHighestParentWithTag(Tag.DestructibleGroup) ?? oldObj.gameObject;
                int instanceId = parentObj.GetInstanceID();

                // Use the mesh center point as the starting position for the particle effect.
                var position = oldObj.MeshCenterPoint;

                // If a particle spawn point has been specified, use that instead.
                if (oldObj.centerPointOverride != Vector3.zero)
                    position = oldObj.centerPointOverride;

                // Convert the particle spawn point position to world coordinates.
                position = oldObj.transform.TransformPoint(position);

                // Spawn the particle effect on the server
                if (IsServer)
                {
                    GameObject particleObj = DestroyItObjectPool.Instance.Spawn(customParticle.gameObject, position, oldObj.transform.rotation);

                    var networkObj = particleObj.GetComponent<NetworkObject>();
                    if (networkObj != null && !networkObj.IsSpawned)
                        networkObj.Spawn();
                }
            }

            UnparentSpecifiedChildren(oldObj);

            _destroyedObjects.Add(oldObj);

            Destroy(oldObj.gameObject);

            if (damageInfo.GetType() == typeof(ImpactDamage))
                DestructibleHelper.ReapplyImpactForce(damageInfo as ImpactDamage, oldObj.VelocityReduction);
        }

        private static void UnparentSpecifiedChildren(Destructible obj)
        {
            if (obj.unparentOnDestroy == null) return;

            foreach (GameObject child in obj.unparentOnDestroy)
            {
                if (child == null)
                    continue;

                // Unparent the child object from the destructible object.
                child.transform.parent = null;

                // Initialize any DelayedRigidbody scripts on the object.
                DelayedRigidbody[] delayedRigidbodies = child.GetComponentsInChildren<DelayedRigidbody>();
                foreach (DelayedRigidbody dr in delayedRigidbodies)
                    dr.Initialize();

                // Check whether we should turn off Kinematic on child objects, so they will fall freely.
                if (obj.disableKinematicOnUparentedChildren)
                {
                    Rigidbody[] rigidbodies = child.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rbody in rigidbodies)
                        rbody.isKinematic = false;
                }

                // Turn off any animations
                Animation[] animations = child.GetComponentsInChildren<Animation>();
                foreach (Animation anim in animations)
                    anim.enabled = false;
            }
        }

        private void InstantiateDebris<T>(GameObject newObj, Destructible oldObj, T damageInfo)
        {
            DestructibleHelper.TransferMaterials(oldObj, newObj);

            if (oldObj.isTerrainTree)
                newObj.gameObject.LockHueVariation();

            if (oldObj.transform.lossyScale != Vector3.one)
                newObj.transform.localScale = oldObj.transform.lossyScale;

            if (oldObj.destroyedPrefabParent != null)
                newObj.transform.parent = oldObj.destroyedPrefabParent.transform;

            Rigidbody[] debrisRigidbodies = newObj.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody debrisRigidbody in debrisRigidbodies)
            {
                if (debrisLayer != -1)
                    debrisRigidbody.gameObject.layer = debrisLayer;

                debrisRigidbody.linearVelocity = oldObj.VelocityFixedUpdate;
                debrisRigidbody.angularVelocity = oldObj.AngularVelocityFixedUpdate;

                // Spawn debris as a networked object
                if (IsServer)
                {
                    debrisRigidbody.gameObject.GetComponent<NetworkObject>()?.Spawn();
                    _debrisPieces.Add(new Debris { Rigidbody = debrisRigidbody, GameObject = debrisRigidbody.gameObject });
                    FireActiveDebrisCounterChangedEvent();
                }
            }

            if (oldObj.CheckForClingingDebris)
                newObj.MakeDebrisCling();

            if (damageInfo.GetType() == typeof(ImpactDamage))
                DestructibleHelper.ReapplyImpactForce(damageInfo as ImpactDamage, oldObj.VelocityReduction);

            if (damageInfo.GetType() == typeof(ExplosiveDamage) || damageInfo.GetType() == typeof(ImpactDamage))
                ExplosionHelper.ApplyForcesToDebris(newObj, 1f, damageInfo);
        }



        public void SetProgressiveDamageTexture(Renderer rend, Material sourceMat, DamageLevel damageLevel)
        {
            if (sourceMat == null) return;
            if (!sourceMat.HasProperty("_DetailMask")) return;
            Texture sourceDetailMask = sourceMat.GetTexture("_DetailMask");
            if (sourceDetailMask == null) return;
            if (_detailMasks == null || _detailMasks.Count == 0) return;

            string sourceDetailMaskName = Regex.Replace(sourceDetailMask.name, "_D[0-9]*$", "");
            Texture newDetailMask = null;
            foreach (Texture2D detailMask in _detailMasks)
            {
                if (detailMask.name == $"{sourceDetailMaskName}_D{damageLevel.visibleDamageLevel}")
                {
                    newDetailMask = detailMask;
                    break;
                }
            }

            if (newDetailMask == null) return;

            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            rend.GetPropertyBlock(propBlock);
            propBlock.SetTexture("_DetailMask", newDetailMask);
            rend.SetPropertyBlock(propBlock);
        }

        public Texture2D GetDetailMask(Material sourceMat, DamageLevel damageLevel)
        {

            if (sourceMat == null) return null;
            if (!sourceMat.HasProperty("_DetailMask")) return null;
            Texture sourceDetailMask = sourceMat.GetTexture("_DetailMask");
            if (sourceDetailMask == null) return null;
            if (_detailMasks == null || _detailMasks.Count == 0) return null;

            string sourceDetailMaskName = Regex.Replace(sourceDetailMask.name, "_D[0-9]*$", "");

            foreach (Texture2D detailMask in _detailMasks)
            {
                if (detailMask.name == $"{sourceDetailMaskName}_D{damageLevel.visibleDamageLevel - 1}")
                    return detailMask;
            }

            return null;
        }

        /// <summary>Fires when the Destroyed Prefab counter changes.</summary>
        public void FireDestroyedPrefabCounterChangedEvent()
        {
            if (DestroyedPrefabCounterChangedEvent != null) // first, make sure there is at least one listener.
                DestroyedPrefabCounterChangedEvent(); // if so, trigger the event.
        }

        /// <summary>Fires when the Active Debris count changes.</summary>
        public void FireActiveDebrisCounterChangedEvent()
        {
            if (ActiveDebrisCounterChangedEvent != null) // first, make sure there is at least one listener.
                ActiveDebrisCounterChangedEvent(); // if so, trigger the event.
        }
    }
}