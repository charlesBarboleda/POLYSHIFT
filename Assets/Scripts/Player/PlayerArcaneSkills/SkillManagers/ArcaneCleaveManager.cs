using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ArcaneCleaveManager : NetworkBehaviour, ISkillManager
{
    [field: SerializeField] public float Damage { get; set; } = 30f;
    [field: SerializeField] public float KnockbackForce { get; set; } = 1f;
    [field: SerializeField] public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    [field: SerializeField] public float AttackRange { get; set; } = 2f;
    public Animator animator { get; set; }
    PlayerSkills PlayerSkills;

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        PlayerSkills = GetComponent<PlayerSkills>();
        ResetSkill();
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
    }

    public void ResetSkill()
    {
        Damage = 100f;
        KnockbackForce = 10f;
        AttackSpeedMultiplier.Value = 0.5f;
        AttackRange = 3f;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void OnArcaneCleaveSpawnRpc()
    {
        for (int i = 0; i <= 12; i++)
        {
            GameObject cleave = ObjectPooler.Instance.Spawn("ArcaneCleave", transform.position, transform.rotation);
            cleave.transform.localScale = new Vector3(AttackRange / 5, AttackRange / 5, AttackRange / 5);
            // Ensure each instance has its own unique Damage value
            var cleaveCollision = cleave.GetComponent<ArcaneCleaveCollision>();
            // Reduce cleave particle speed by 50%
            ParticleSystem[] childParticleSystems = cleave.GetComponentsInChildren<ParticleSystem>();
            ParticleSystem mainParticleSystem = cleave.GetComponent<ParticleSystem>();

            var mainModule = mainParticleSystem.main;
            mainModule.simulationSpeed = 0.5f; // Adjust this value to slow down
            foreach (ParticleSystem ps in childParticleSystems)
            {
                var mainModule2 = ps.main;
                mainModule2.simulationSpeed = 0.5f; // Adjust this value to slow down
            }
            if (cleaveCollision != null)
            {
                cleaveCollision.SetDamage(Damage); // Uncomment and verify this line
            }

            cleave.transform.Rotate(0, i * 30, 0);
        }
    }
    public void DealExpandingDamage()
    {
        PlayerSkills.DealDamageInExpandingCircle(transform.position, 0, AttackRange * 4, Damage, KnockbackForce, 0.1f * AttackRange, 0.05f, 1);
    }

    void SetAttackSpeedMultiplier(float AttackSpeedMultiplier)
    {
        animator.SetFloat("AttackSpeedMultiplier", AttackSpeedMultiplier);
    }
}
