using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMelee : NetworkBehaviour
{

    public List<MeleeAttack> meleeAttacks;
    private MeleeAttack currentAttack;
    private bool canAttack = true;
    private Animator animator;
    private PlayerNetworkMovement playerMovement;
    private PlayerNetworkRotation playerRotation;

    public int attackIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer) return;
        base.OnNetworkSpawn();
        animator = GetComponent<Animator>();
        playerMovement = GetComponentInParent<PlayerNetworkMovement>();
        playerRotation = GetComponentInParent<PlayerNetworkRotation>();

        Debug.Log($"Animator assigned: {animator != null}");
        Debug.Log($"PlayerMovement assigned: {playerMovement != null}");
        Debug.Log($"PlayerRotation assigned: {playerRotation != null}");

        meleeAttacks = new List<MeleeAttack>
        {
            gameObject.AddComponent<DevilSlam>(), // Attach DevilSlam as a component
           gameObject.AddComponent<SingleCrescentSlash>(), // Attach DoubleCrescentSlash as a component
           gameObject.AddComponent<DoubleCrescentSlash>(), // Attach DoubleCrescentSlash as a component
           
            // Add more attacks here as needed
        };

        foreach (MeleeAttack attack in meleeAttacks)
        {
            attack.Initialize(animator);
        }



    }

    void Update()
    {
        if (!IsLocalPlayer) return;
        if (Input.GetMouseButtonDown(1) && canAttack)
        {
            PerformAttack(attackIndex);
        }
    }

    private void PerformAttack(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= meleeAttacks.Count) return;
        currentAttack = meleeAttacks[attackIndex];
        currentAttack.ExecuteAttack();
    }

    public void AddMeleeAttack<T>() where T : MeleeAttack
    {
        T newAttack = gameObject.AddComponent<T>(); // Add the melee attack as a component
        newAttack.Initialize(animator); // Initialize with dependencies
        meleeAttacks.Add(newAttack); // Add to the list for management

        Debug.Log($"Added new melee attack: {typeof(T).Name}");
    }

    public void RemoveMeleeAttack<T>() where T : MeleeAttack
    {
        MeleeAttack attackToRemove = meleeAttacks.Find(attack => attack is T);
        if (attackToRemove != null)
        {
            meleeAttacks.Remove(attackToRemove);
            Destroy(attackToRemove); // Destroy the component
            Debug.Log($"Removed melee attack: {typeof(T).Name}");
        }
    }

    public MeleeAttack GetMeleeAttack<T>() where T : MeleeAttack
    {
        return meleeAttacks.Find(attack => attack is T);
    }

    [ServerRpc]
    public void SpawnSlashEffectServerRpc(string slashName, Vector3 position, Quaternion rotation, float attackRange)
    {
        // Call ClientRpc to spawn the effect for all clients
        SpawnSlashEffectClientRpc(slashName, position, rotation, attackRange);
    }

    [ClientRpc]
    private void SpawnSlashEffectClientRpc(string slashName, Vector3 position, Quaternion rotation, float attackRange)
    {
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler.Instance is null. Cannot spawn effect.");
            return;
        }

        GameObject slash = ObjectPooler.Instance.Spawn(slashName, position, rotation);
        if (slash != null)
        {
            slash.transform.localScale = new Vector3(attackRange / 6, attackRange / 6, attackRange / 6);
        }
        else
        {
            Debug.LogError("Failed to spawn effect. Check ObjectPooler configuration.");
        }
    }

    [ServerRpc]
    public void SpawnSlashImpactServerRpc(string impactName, Vector3 position, Quaternion rotation)
    {
        // Call ClientRpc to spawn the effect for all clients
        SpawnSlashImpactClientRpc(impactName, position, rotation);
    }

    [ClientRpc]
    private void SpawnSlashImpactClientRpc(string impactName, Vector3 position, Quaternion rotation)
    {
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler.Instance is null. Cannot spawn effect.");
            return;
        }

        GameObject impact = ObjectPooler.Instance.Spawn(impactName, position, rotation);
        if (impact == null)
        {
            Debug.LogError("Failed to spawn effect. Check ObjectPooler configuration.");
        }
    }

    public void DealDamageInCone(Vector3 origin, float attackRange, float coneAngle, float damage, float knockbackForce)
    {
        // Implement damage logic here, similar to the original DealDamage coroutine
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRange);
        foreach (Collider collider in hitColliders)
        {
            Vector3 directionToTarget = (collider.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget <= coneAngle)
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    SpawnSlashImpactClientRpc("MeleeSlash1Hit", collider.transform.position, Quaternion.identity);
                    damageable.RequestTakeDamageServerRpc(damage, NetworkObjectId);
                }
                Rigidbody rb = collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(directionToTarget * knockbackForce, ForceMode.Impulse);
                }
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.OnRaycastHitServerRpc(collider.transform.position, directionToTarget);
                }
            }
        }
    }

    public void DealDamageInCircle(Vector3 origin, float attackRange, float damage, float knockbackForce)
    {
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRange);
        foreach (Collider collider in hitColliders)
        {
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                SpawnSlashImpactClientRpc("MeleeSlash1Hit", collider.transform.position, Quaternion.identity);
                damageable.RequestTakeDamageServerRpc(damage, NetworkObjectId);
            }
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 directionToTarget = (collider.transform.position - origin).normalized;
                rb.AddForce(directionToTarget * knockbackForce, ForceMode.Impulse);
            }
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector3 directionToTarget = (collider.transform.position - origin).normalized;
                enemy.OnRaycastHitServerRpc(collider.transform.position, directionToTarget);
            }
        }
    }

    public void DisableMovementAndRotation()
    {
        playerMovement.canMove = false;
        playerRotation.canRotate = false;
    }

    public void EnableMovementAndRotation()
    {
        playerMovement.canMove = true;
        playerRotation.canRotate = true;
    }
}
