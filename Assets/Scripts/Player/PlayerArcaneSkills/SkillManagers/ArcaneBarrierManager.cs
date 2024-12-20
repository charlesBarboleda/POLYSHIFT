using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ArcaneBarrierManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; }
    public float Duration { get; set; }
    private GameObject arcaneBarrierInstance;

    private PlayerSkills PlayerSkills;
    private PlayerNetworkHealth playerNetworkHealth;
    public Animator animator { get; set; }
    public float DamageReduction { get; set; } = 0.5f;

    public override void OnNetworkSpawn()
    {
        Damage = 0f;
        Duration = 60f;
        KnockbackForce = 10f;
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        AttackRange = 2f;
        PlayerSkills = GetComponent<PlayerSkills>();
        animator = GetComponent<Animator>();
        playerNetworkHealth = GetComponent<PlayerNetworkHealth>();
    }

    // Called when the player wants to activate the Arcane Barrier ability
    public void ActivateArcaneBarrier()
    {

        ArcaneBarrierSpawn();

    }

    private void ArcaneBarrierSpawn()
    {
        // Spawn the barrier effects on the server, which all clients will see
        if (arcaneBarrierInstance == null)
        {
            ApplyBuff(DamageReduction, Duration);

            SpawnBarrierRpc();
            SpawnBarrierEffectsRpc();

            // Schedule barrier destruction on all clients
            StartBarrierDespawnRpc(Duration);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnBarrierRpc()
    {
        arcaneBarrierInstance = ObjectPooler.Instance.Spawn("ArcaneDome", transform.position, transform.rotation);
        arcaneBarrierInstance.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        arcaneBarrierInstance.transform.localScale = new Vector3(AttackRange / 2, AttackRange / 2, AttackRange / 2);
        arcaneBarrierInstance.transform.SetParent(transform);
    }


    private void ApplyBuff(float damageReduction, float duration)
    {
        playerNetworkHealth.ReduceDamageTakenBy(damageReduction, duration);

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnBarrierEffectsRpc()
    {
        GameObject arcaneEnchant = ObjectPooler.Instance.Spawn("ArcaneEnchant", transform.position, Quaternion.Euler(-90, 0, 90));
        GameObject arcaneMuzzle = ObjectPooler.Instance.Spawn("ArcaneMuzzle", transform.position, Quaternion.Euler(-90, 0, 90));

    }
    [Rpc(SendTo.ClientsAndHost)]
    private void StartBarrierDespawnRpc(float duration)
    {
        StartCoroutine(DestroyArcaneBarrierAfterDuration(duration));
    }

    private IEnumerator DestroyArcaneBarrierAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (arcaneBarrierInstance != null)
        {
            ObjectPooler.Instance.Despawn("ArcaneDome", arcaneBarrierInstance);
            arcaneBarrierInstance = null;
        }
    }

    public void DealDamageInCircleArcaneBarrier()
    {
        PlayerSkills.DealDamageInCircle(transform.position, AttackRange, Damage, KnockbackForce);
    }

    private void SetAttackSpeedMultiplier(float value)
    {
        animator.SetFloat("AttackSpeedMultiplier", value);
    }

    public void ResetSkill()
    {
        Damage = 0f;
        KnockbackForce = 10f;
        AttackSpeedMultiplier.Value = 1f;
        AttackRange = 2f;
        Duration = 60f;
    }

    public void OnDisable()
    {
        AttackSpeedMultiplier.OnValueChanged -= SetAttackSpeedMultiplier;

    }

}
