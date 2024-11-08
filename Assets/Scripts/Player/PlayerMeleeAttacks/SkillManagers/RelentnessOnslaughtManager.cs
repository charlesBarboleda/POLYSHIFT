using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RelentlessOnslaughtManager : NetworkBehaviour, IMeleeSkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; }
    public float Duration { get; set; }
    private GameObject arcaneAuraInstance;
    private PlayerMelee playerMelee;
    private Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Damage = 0f;
        Duration = 15f;
        KnockbackForce = 5f;
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        AttackRange = 5f;
        playerMelee = GetComponent<PlayerMelee>();
        animator = GetComponent<Animator>();
    }


    // This method is called when the player wants to use the ability
    public void ActivateRelentlessOnslaught()
    {
        if (IsOwner)
        {
            OnRelentlessOnslaughtServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnRelentlessOnslaughtServerRpc(ServerRpcParams rpcParams = default)
    {
        // Get the player who triggered this action based on the sender's ClientId
        var clientId = rpcParams.Receive.SenderClientId;

        // Apply buffs only to the triggering player
        ApplyBuffsClientRpc(clientId);

        if (arcaneAuraInstance == null)
        {
            arcaneAuraInstance = ObjectPooler.Instance.Spawn("ArcaneAura", transform.position, Quaternion.Euler(-90, 0, 90));
            arcaneAuraInstance.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            arcaneAuraInstance.GetComponent<NetworkObject>().Spawn();
            arcaneAuraInstance.transform.SetParent(transform);

            // Notify clients to start the despawn timer
            StartAuraDespawnClientRpc(Duration);

            SpawnAuraEffects();
        }
    }

    [ClientRpc]
    private void ApplyBuffsClientRpc(ulong targetClientId)
    {
        // Only apply the buffs if this client is the target client
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            playerMelee.IncreaseMeleeDamageBy(1.5f, Duration);
            playerMelee.IncreaseAttackSpeedBy(2f, Duration);
            playerMelee.ReduceCooldownsBy(0.5f, Duration);
        }
    }

    [ClientRpc]
    private void StartAuraDespawnClientRpc(float duration)
    {
        StartCoroutine(DisableAuraAfterDuration(duration));
    }

    private void SpawnAuraEffects()
    {
        GameObject enchant = ObjectPooler.Instance.Spawn("ArcaneEnchant", transform.position, Quaternion.Euler(-90, 0, 90));
        GameObject muzzle = ObjectPooler.Instance.Spawn("ArcaneMuzzle", transform.position, Quaternion.Euler(-90, 0, 90));
        GameObject cast = ObjectPooler.Instance.Spawn("ArcaneCast", transform.position, Quaternion.Euler(-90, 0, 90));

        enchant.GetComponent<NetworkObject>().Spawn();
        muzzle.GetComponent<NetworkObject>().Spawn();
        cast.GetComponent<NetworkObject>().Spawn();
    }

    private IEnumerator DisableAuraAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (arcaneAuraInstance != null)
        {
            arcaneAuraInstance.GetComponent<NetworkObject>().Despawn(false);
            ObjectPooler.Instance.Despawn("ArcaneAura", arcaneAuraInstance);
            arcaneAuraInstance = null;
        }
    }

    void SetAttackSpeedMultiplier(float newAttackSpeedMultiplier)
    {
        animator.SetFloat("MeleeAttackSpeedMultiplier", newAttackSpeedMultiplier);
    }

    public void DealDamageInCircleRelentlessOnslaught()
    {
        playerMelee.DealDamageInCircle(transform.position, AttackRange, Damage, KnockbackForce);
    }
}
