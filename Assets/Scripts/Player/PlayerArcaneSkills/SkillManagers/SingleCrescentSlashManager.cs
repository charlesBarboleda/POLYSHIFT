using Unity.Netcode;
using UnityEngine;

public class SingleCrescentSlashManager : NetworkBehaviour, ISkillManager
{
    public float AttackRange { get; set; } = 3f;
    public float coneAngle = 90f;
    public float KnockbackForce { get; set; } = 1f;
    public float Damage { get; set; } = 10f;
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();


    public Animator animator { get; set; }
    PlayerSkills PlayerSkills;

    public override void OnNetworkSpawn()
    {
        PlayerSkills = GetComponent<PlayerSkills>();
        animator = GetComponent<Animator>();
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;

        AttackRange = 5f;
        coneAngle = 90f;
        KnockbackForce = 3f;
    }

    public void ResetSkill()
    {
        AttackRange = 5f;
        coneAngle = 90f;
        KnockbackForce = 3f;
        Damage = 20f;
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void OnSingleCrescentSlashSpawnServerRpc()
    {
        GameObject slash = ObjectPooler.Instance.Spawn("MeleeSlash1", transform.position + (transform.forward * 2f) + transform.up, transform.rotation * Quaternion.Euler(0, 0, Random.Range(-20, 20)));
        slash.transform.localScale = new Vector3(AttackRange / 5, AttackRange / 5, AttackRange / 5);

    }


    public void DealConeDamage()
    {
        PlayerSkills.DealDamageInCone(PlayerSkills.transform.position, AttackRange, coneAngle, Damage, KnockbackForce);

    }

    void SetAttackSpeedMultiplier(float value)
    {
        animator.SetFloat("AttackSpeedMultiplier", value);
    }




}
