using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RelentlessOnslaughtManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; }
    public float Duration { get; set; }
    private GameObject arcaneAuraInstance;
    private PlayerSkills PlayerSkills;
    public Animator animator { get; set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Damage = 0f;
        Duration = 60f;
        KnockbackForce = 10f;
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        AttackRange = 5f;
        PlayerSkills = GetComponent<PlayerSkills>();
        animator = GetComponent<Animator>();
    }

    public void ResetSkill()
    {
        Damage = 0;
        KnockbackForce = 0;
        AttackRange = 0;
        AttackSpeedMultiplier.Value = 1f;
        Duration = 60f;
    }


    // This method is called when the player wants to use the ability
    public void ActivateRelentlessOnslaught()
    {
        OnRelentlessOnslaught();
    }

    private void OnRelentlessOnslaught()
    {
        // Get the player who triggered this action based on the sender's ClientId


        // Apply buffs only to the triggering player
        ApplyBuffs();

        SpawnAuraEffectsRpc();
        SpawnArcaneAuraRpc();
        // Notify clients to start the despawn timer
        StartAuraDespawnRpc(Duration);

    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnArcaneAuraRpc()
    {
        // Ensure only one instance is spawned
        if (arcaneAuraInstance == null)
        {
            arcaneAuraInstance = ObjectPooler.Instance.Spawn("ArcaneAura", transform.position, Quaternion.Euler(-90, 0, 90));
            arcaneAuraInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            arcaneAuraInstance.transform.SetParent(transform);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void StartAuraDespawnRpc(float duration)
    {
        StartCoroutine(DisableAuraAfterDuration(duration));
    }

    private IEnumerator DisableAuraAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (arcaneAuraInstance != null)
        {
            ObjectPooler.Instance.Despawn("ArcaneAura", arcaneAuraInstance);
            arcaneAuraInstance = null; // Ensure the reference is cleared
        }
    }

    void ApplyBuffs()
    {

        PlayerSkills.IncreaseMeleeDamageBy(3f, Duration);
        PlayerSkills.IncreaseAttackSpeedBy(3f, Duration);
        PlayerSkills.ReduceCooldownsBy(0.25f, Duration);

    }



    [Rpc(SendTo.ClientsAndHost)]
    void SpawnAuraEffectsRpc()
    {
        GameObject enchant = ObjectPooler.Instance.Spawn("ArcaneEnchant", transform.position, Quaternion.Euler(-90, 0, 90));
        GameObject muzzle = ObjectPooler.Instance.Spawn("ArcaneMuzzle", transform.position, Quaternion.Euler(-90, 0, 90));
        GameObject cast = ObjectPooler.Instance.Spawn("ArcaneCast", transform.position, Quaternion.Euler(-90, 0, 90));

    }


    void SetAttackSpeedMultiplier(float newAttackSpeedMultiplier)
    {
        animator.SetFloat("AttackSpeedMultiplier", newAttackSpeedMultiplier);
    }

    public void DealDamageInCircleRelentlessOnslaught()
    {
        PlayerSkills.DealDamageInCircle(transform.position, AttackRange, Damage, KnockbackForce);
    }
}
