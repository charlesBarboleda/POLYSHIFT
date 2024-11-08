using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ArcaneBarrierManager : NetworkBehaviour, IMeleeSkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; }
    public float Duration { get; set; }
    private GameObject arcaneBarrierInstance;
    private PlayerMelee playerMelee;
    private PlayerNetworkHealth playerNetworkHealth;
    private Animator animator;

    public override void OnNetworkSpawn()
    {
        Damage = 0f;
        Duration = 60f;
        KnockbackForce = 10f;
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        AttackRange = 2f;
        playerMelee = GetComponent<PlayerMelee>();
        animator = GetComponent<Animator>();
        playerNetworkHealth = GetComponent<PlayerNetworkHealth>();
    }

    // Called when the player wants to activate the Arcane Barrier ability
    public void ActivateArcaneBarrier()
    {
        if (IsOwner)
        {
            ArcaneBarrierSpawnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ArcaneBarrierSpawnServerRpc(ServerRpcParams rpcParams = default)
    {
        // Apply the buff only to the activating client
        ApplyBuffToClientRpc(rpcParams.Receive.SenderClientId, 0.5f, 60f);

        // Spawn the barrier effects on the server, which all clients will see
        if (arcaneBarrierInstance == null)
        {
            arcaneBarrierInstance = ObjectPooler.Instance.Spawn("ArcaneDome", transform.position, transform.rotation);
            arcaneBarrierInstance.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            arcaneBarrierInstance.transform.localScale = new Vector3(AttackRange / 2, AttackRange / 2, AttackRange / 2);
            arcaneBarrierInstance.GetComponent<NetworkObject>().Spawn();
            arcaneBarrierInstance.transform.SetParent(transform);

            SpawnBarrierEffects();

            // Schedule barrier destruction on all clients
            StartBarrierDespawnClientRpc(Duration);
        }
    }

    [ClientRpc]
    private void ApplyBuffToClientRpc(ulong targetClientId, float damageReduction, float duration)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            playerNetworkHealth.ReduceDamageTakenBy(damageReduction, duration);
        }
    }

    [ClientRpc]
    private void StartBarrierDespawnClientRpc(float duration)
    {
        StartCoroutine(DestroyArcaneBarrierAfterDuration(duration));
    }

    private void SpawnBarrierEffects()
    {
        GameObject arcaneEnchant = ObjectPooler.Instance.Spawn("ArcaneEnchant", transform.position, Quaternion.Euler(-90, 0, 90));
        GameObject arcaneMuzzle = ObjectPooler.Instance.Spawn("ArcaneMuzzle", transform.position, Quaternion.Euler(-90, 0, 90));

        arcaneEnchant.GetComponent<NetworkObject>().Spawn();
        arcaneMuzzle.GetComponent<NetworkObject>().Spawn();
    }

    private IEnumerator DestroyArcaneBarrierAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (arcaneBarrierInstance != null)
        {
            arcaneBarrierInstance.GetComponent<NetworkObject>().Despawn(false);
            ObjectPooler.Instance.Despawn("ArcaneDome", arcaneBarrierInstance);
            arcaneBarrierInstance = null;
        }
    }

    public void DealDamageInCircleArcaneBarrier()
    {
        playerMelee.DealDamageInCircle(transform.position, AttackRange, Damage, KnockbackForce);
    }

    private void SetAttackSpeedMultiplier(float value)
    {
        animator.SetFloat("MeleeAttackSpeedMultiplier", value);
    }
}
