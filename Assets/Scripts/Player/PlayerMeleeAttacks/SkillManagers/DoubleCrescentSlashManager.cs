using Unity.Netcode;
using UnityEngine;

public class DoubleCrescentSlashManager : NetworkBehaviour, IMeleeSkillManager
{
    public float stepDistance = 0.5f;
    public float AttackRange { get; set; } = 3f;
    public float coneAngle = 90f;
    public float KnockbackForce { get; set; } = 1f;
    public float Damage { get; set; } = 10f;
    public float AttackSpeedMultiplier { get; set; } = 1f;

    PlayerAudioManager audioManager;
    Animator animator;
    PlayerMelee playerMelee;
    GameObject player;


    public override void OnNetworkSpawn()
    {
        audioManager = GetComponentInParent<PlayerAudioManager>();
        animator = GetComponent<Animator>();
        animator.SetFloat("MeleeAttackSpeedMultiplier", AttackSpeedMultiplier);
        playerMelee = GetComponent<PlayerMelee>();
        player = GetComponentInParent<PlayerNetworkHealth>().gameObject;
    }


    [ServerRpc]
    public void OnDoubleCrescentSlashSpawnServerRpc()
    {
        OnDoubleCrescentSlashSpawnClientRpc();
    }
    [ClientRpc]
    void OnDoubleCrescentSlashSpawnClientRpc()
    {
        SetAttackSpeedMultiplier();
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
        playerMelee.DealDamageInCone(playerMelee.transform.position, AttackRange, coneAngle, Damage, KnockbackForce);

    }


    public void DealBigConeDamage()
    {
        playerMelee.DealDamageInCone(playerMelee.transform.position, AttackRange + 3, 100f, Damage * 2, KnockbackForce + 1);

    }

    void SetAttackSpeedMultiplier()
    {
        animator.SetFloat("MeleeAttackSpeedMultiplier", AttackSpeedMultiplier);
    }



}
