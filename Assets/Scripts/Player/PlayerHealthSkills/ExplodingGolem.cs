using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class ExplodingGolem : Golem
{
    GameObject explosionEffect1;
    GameObject explosionEffect2;
    GameObject lifehit;
    GameObject bloodSplatter;

    [SerializeField] GameObject healthBar;
    protected override void BuffEffect(float buffRadius)
    {
        SpawnExplosionEffectRpc();
        if (explosionEffect1 != null)
            explosionEffect1.transform.rotation = Quaternion.Euler(-90, 0, 90);
        if (explosionEffect2 != null)
            explosionEffect2.transform.rotation = Quaternion.Euler(-90, 0, 90);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, buffRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("Destroyables"))
            {
                hitCollider.GetComponent<IDamageable>().RequestTakeDamageServerRpc(MaxHealth.Value / 2, Owner.GetComponent<NetworkObject>().NetworkObjectId);

                SpawnPostHitEffectsRpc();
                lifehit.transform.position = hitCollider.transform.position + transform.up * 2f;
                bloodSplatter.transform.position = hitCollider.transform.position + transform.up * 2f;

            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnExplosionEffectRpc()
    {
        explosionEffect1 = ObjectPooler.Instance.Spawn("LifeSphereBlast", transform.position, Quaternion.identity);
        explosionEffect2 = ObjectPooler.Instance.Spawn("LifeExplosionMega", transform.position, Quaternion.identity);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnPostHitEffectsRpc()
    {
        lifehit = ObjectPooler.Instance.Spawn("LifeSlashHit", Vector3.zero, Quaternion.identity);
        bloodSplatter = ObjectPooler.Instance.Spawn($"BloodSplatter{Random.Range(1, 6)}", Vector3.zero, Quaternion.identity);
    }

    public void DealDamageInConeExplodingGolem()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, AttackRange);
        Vector3 forward = transform.forward; // Golem's forward direction

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("Destroyables"))
            {
                // Calculate the direction to the target
                Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;

                // Calculate the angle between the forward direction and the direction to the target
                float angleToTarget = Vector3.Angle(forward, directionToTarget);

                // Only apply damage if the target is within the specified cone angle
                if (angleToTarget <= 30f) // 45 degrees for example, adjust as needed for cone width
                {
                    hitCollider.GetComponent<IDamageable>().RequestTakeDamageServerRpc(Damage, Owner.GetComponent<NetworkObject>().NetworkObjectId);


                    if (IsServer)
                    {
                        GameObject lifehit = ObjectPooler.Instance.Spawn("LifeSlashHit", hitCollider.transform.position + transform.up * 2f, Quaternion.identity);
                        lifehit.GetComponent<NetworkObject>().Spawn();
                        GameObject bloodSplatter = ObjectPooler.Instance.Spawn($"BloodSplatter{Random.Range(1, 6)}", hitCollider.transform.position + transform.up * 2f, Quaternion.identity);
                        bloodSplatter.GetComponent<NetworkObject>().Spawn();
                    }
                }
            }
        }
    }


    public override void HandleDeath(ulong networkObjectId)
    {
        if (IsServer)
        {
            GameManager.Instance.SpawnedAllies.Remove(gameObject);
            StartCoroutine(DeathRoutine());

            // Call a ClientRpc to hide the health bar for all clients
            SetHealthBarVisibilityClientRpc(false);
        }
    }

    private IEnumerator DeathRoutine()
    {
        Animator.SetTrigger("IsDead");
        BuffEffect(BuffRadius);
        // Disable movement and interactions
        CanAttack = false;
        IsDead = true;
        collider.enabled = false;
        rb.isKinematic = true;
        Agent.enabled = false;
        Agent.isStopped = true;
        Agent.canMove = false;

        yield return new WaitForSeconds(2.5f);

        Animator.enabled = false;
        StartCoroutine(ReviveAfter(ReviveTime));
    }

    [ClientRpc]
    private void SetHealthBarVisibilityClientRpc(bool isVisible)
    {
        healthBar.SetActive(isVisible);
    }


    IEnumerator ReviveAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (IsServer)
        {
            SetHealthBarVisibilityClientRpc(true);  // Show the health bar on all clients
            CurrentHealth.Value = MaxHealth.Value;
            GameManager.Instance.SpawnedAllies.Add(gameObject);
            transform.position = Owner.transform.position + transform.forward * 2f;
            IsDead = false;
            CanAttack = true;
            collider.enabled = true;
            rb.isKinematic = false;
            Agent.enabled = true;
            Agent.isStopped = false;
            Agent.canMove = true;
            Animator.enabled = true;
            Animator.ResetTrigger("IsDead");
        }
    }
}
