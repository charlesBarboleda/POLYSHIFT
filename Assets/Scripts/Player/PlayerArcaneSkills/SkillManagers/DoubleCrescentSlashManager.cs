using Unity.Netcode;
using UnityEngine;

public class DoubleCrescentSlashManager : NetworkBehaviour, ISkillManager
{
    public float stepDistance = 0.5f;
    public float AttackRange { get; set; } = 3f;
    public float coneAngle = 90f;
    public float KnockbackForce { get; set; } = 1f;
    public float Damage { get; set; } = 10f;
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();

    PlayerAudioManager audioManager;
    public Animator animator { get; set; }
    PlayerSkills PlayerSkills;
    GameObject player;


    public override void OnNetworkSpawn()
    {
        audioManager = GetComponent<PlayerAudioManager>();
        animator = GetComponent<Animator>();
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        PlayerSkills = GetComponent<PlayerSkills>();
        player = GetComponent<PlayerNetworkHealth>().gameObject;

        ResetSkill();
    }

    public void ResetSkill()
    {
        AttackRange = 4f;
        coneAngle = 90f;
        KnockbackForce = 3f;
        Damage = 75f;
        stepDistance = 1f;
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void OnDoubleCrescentSlashSpawnServerRpc()
    {
        GameObject slash = ObjectPooler.Instance.Spawn("MeleeSlash1", transform.position + (transform.forward * 3f) + transform.up, transform.rotation * Quaternion.Euler(0, 0, Random.Range(-20, 20)));
        slash.transform.localScale = new Vector3(AttackRange / 2, AttackRange / 2, AttackRange / 2);
        ParticleSystem[] childParticleSystems = slash.GetComponentsInChildren<ParticleSystem>();
        ParticleSystem mainParticleSystem = slash.GetComponent<ParticleSystem>();

        var mainModule = mainParticleSystem.main;
        mainModule.simulationSpeed = 0.5f; // Adjust this value to slow down
        foreach (ParticleSystem ps in childParticleSystems)
        {
            var mainModule2 = ps.main;
            mainModule2.simulationSpeed = 0.5f; // Adjust this value to slow down
        }
    }

    public void PlayArcaneDevilSlamShoutSound()
    {
        audioManager.PlayArcaneDevilSlamShoutSound();
    }

    public void PlayMeleeSlash1Sound()
    {
        audioManager.PlayMeleeSlash1Sound();
    }

    public void frontStep()
    {
        player.transform.position += transform.forward * stepDistance;
    }

    public void frontStepLarge()
    {
        player.transform.position += transform.forward * stepDistance * 3;
    }
    public void DealSmallConeDamage()
    {
        PlayerSkills.DealDamageInCone(PlayerSkills.transform.position, AttackRange, coneAngle, Damage, KnockbackForce);

    }


    public void DealBigConeDamage()
    {
        PlayerSkills.DealDamageInCone(PlayerSkills.transform.position, AttackRange + 3, 100f, Damage * 2, KnockbackForce + 1);

    }

    void SetAttackSpeedMultiplier(float attackSpeedMultiplier)
    {
        animator.SetFloat("AttackSpeedMultiplier", attackSpeedMultiplier);
    }



}
