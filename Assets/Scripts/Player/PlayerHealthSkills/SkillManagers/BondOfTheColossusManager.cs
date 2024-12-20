using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BondOfTheColossusManager : NetworkBehaviour, ISkillManager
{

    public float AttackRange { get; set; } = 3f;
    public float KnockbackForce { get; set; } = 1f;
    public float Damage { get; set; } = 10f;
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    GameObject auraRingInstance;
    GameObject auraSpinningInstance;
    GameObject auraRing2Instance;
    public Animator animator { get; set; }
    PlayerNetworkHealth playerNetworkHealth;
    PlayerNetworkMovement playerNetworkMovement;
    PlayerWeapon playerWeapon;
    PlayerSkills playerSkills;
    GolemManager golemManager;

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        golemManager = GetComponent<GolemManager>();
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        playerNetworkHealth = GetComponent<PlayerNetworkHealth>();
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerSkills = GetComponent<PlayerSkills>();
        playerWeapon = GetComponent<PlayerWeapon>();

        AttackRange = 20f;
        KnockbackForce = 25f;
        Damage = 0f;

    }

    public void ResetSkill()
    {
        AttackRange = 20f;
        KnockbackForce = 25f;
        Damage = 0f;

    }

    void Update()
    {
        if (auraRingInstance != null)
        {
            auraRingInstance.transform.position = transform.position;
        }
        if (auraSpinningInstance != null)
        {
            auraSpinningInstance.transform.position = transform.position;
        }
        if (auraRing2Instance != null)
        {
            auraRing2Instance.transform.position = transform.position;
        }
    }

    void SetAttackSpeedMultiplier(float value)
    {
        animator.SetFloat("AttackSpeedMultiplier", value);
    }

    [ServerRpc]
    public void FiftyBuffServerRpc()
    {
        StartCoroutine(ApplyFiftyBuff());
    }

    [ServerRpc]
    public void SpawnVisualEffectsServerRpc()
    {
        if (auraRingInstance == null || auraSpinningInstance == null || auraRing2Instance == null)
        {

            GameObject earthExplosion = ObjectPooler.Instance.Spawn("EarthSphereBlast", transform.position, Quaternion.identity);
            earthExplosion.GetComponent<NetworkObject>().Spawn();

            GameObject fireExplosion = ObjectPooler.Instance.Spawn("FireSphereBlast", transform.position, Quaternion.identity);
            fireExplosion.GetComponent<NetworkObject>().Spawn();

            GameObject waterExplosion = ObjectPooler.Instance.Spawn("WaterSphereBlast", transform.position, Quaternion.identity);
            waterExplosion.GetComponent<NetworkObject>().Spawn();

            auraRingInstance = ObjectPooler.Instance.Spawn("FireAuraRing", transform.position, Quaternion.identity);
            auraRingInstance.transform.localRotation = Quaternion.Euler(-90, 0, 90);
            auraRingInstance.GetComponent<NetworkObject>().Spawn();

            auraSpinningInstance = ObjectPooler.Instance.Spawn("FireAuraSpinning", transform.position, Quaternion.identity);
            auraSpinningInstance.transform.localRotation = Quaternion.Euler(-90, 0, 90);
            auraSpinningInstance.GetComponent<NetworkObject>().Spawn();

            auraRing2Instance = ObjectPooler.Instance.Spawn("FireAuraRingOuter", transform.position, Quaternion.identity);
            auraRing2Instance.transform.localRotation = Quaternion.Euler(-90, 0, 90);
            auraRing2Instance.GetComponent<NetworkObject>().Spawn();

            StartEffectDespawnClientRpc(30f);
        }
    }

    [ClientRpc]
    private void StartEffectDespawnClientRpc(float duration)
    {
        StartCoroutine(DestroyVisualEffectsAfterDuration(duration));
    }

    private IEnumerator DestroyVisualEffectsAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (auraRingInstance != null)
        {
            auraRingInstance.GetComponent<NetworkObject>().Despawn(false);
            ObjectPooler.Instance.Despawn("FireAuraRing", auraRingInstance);
            auraRingInstance = null;
        }

        if (auraSpinningInstance != null)
        {
            auraSpinningInstance.GetComponent<NetworkObject>().Despawn(false);
            ObjectPooler.Instance.Despawn("FireAuraSpinning", auraSpinningInstance);
            auraSpinningInstance = null;
        }

        if (auraRing2Instance != null)
        {
            auraRing2Instance.GetComponent<NetworkObject>().Despawn(false);
            ObjectPooler.Instance.Despawn("FireAuraRingOuter", auraRing2Instance);
            auraRing2Instance = null;
        }
    }

    [ServerRpc]
    public void GlobalFullHealServerRpc()
    {
        foreach (var allies in GameManager.Instance.SpawnedAllies)
        {
            if (allies.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                if (allies.TryGetComponent<PlayerNetworkHealth>(out PlayerNetworkHealth player))
                {
                    damageable.HealServerRpc(player.maxHealth.Value);
                }
                else if (allies.TryGetComponent<Golem>(out Golem golem))
                {
                    damageable.HealServerRpc(golem.MaxHealth.Value);
                }

            }
        }
    }

    public void Knockback()
    {
        playerSkills.DealDamageInCircle(transform.position, AttackRange, Damage, KnockbackForce);
    }

    private IEnumerator ApplyFiftyBuff()
    {
        float duration = 30f;
        float endTime = Time.time + duration;

        // Flags for golem buffs
        bool golemHasDamageAndSpeedBuff = false;
        bool golemHasDamageReductionAndRegenBuff = false;

        // Flags for player buffs
        bool playerHasDamageAndSpeedBuff = false;
        bool playerHasDamageReductionAndRegenBuff = false;

        golemManager.MassRecall();

        while (Time.time < endTime)
        {
            // Apply buffs to golems
            foreach (var golem in golemManager.SpawnedGolems)
            {
                bool golemIsAboveHalfHealth = golem.CurrentHealth.Value > golem.MaxHealth.Value * 0.5f;

                if (golemIsAboveHalfHealth && !golemHasDamageAndSpeedBuff)
                {
                    golem.MultiplyDamageBy(2.0f);
                    golem.MultiplyMovementSpeedBy(2.0f);
                    golemHasDamageAndSpeedBuff = true;

                    if (golemHasDamageReductionAndRegenBuff)
                    {
                        golem.DivideHealthRegenRateBy(3.0f);
                        golem.IncreaseDamageReduction(-0.9f);
                        golemHasDamageReductionAndRegenBuff = false;
                    }
                }
                else if (!golemIsAboveHalfHealth && !golemHasDamageReductionAndRegenBuff)
                {
                    golem.IncreaseDamageReduction(0.9f);
                    golem.MultiplyHealthRegenRateBy(3.0f);
                    golemHasDamageReductionAndRegenBuff = true;

                    if (golemHasDamageAndSpeedBuff)
                    {
                        golem.DivideDamageBy(2.0f);
                        golem.DivideMovementSpeedBy(2.0f);
                        golemHasDamageAndSpeedBuff = false;
                    }
                }
            }

            // Apply buffs to the player
            bool playerIsAboveHalfHealth = playerNetworkHealth.currentHealth.Value > playerNetworkHealth.maxHealth.Value * 0.5f;

            if (playerIsAboveHalfHealth && !playerHasDamageAndSpeedBuff)
            {
                // Apply double damage and speed to player
                playerSkills.MultiplyMeleeDamageBy(2.0f);
                playerWeapon.Damage *= 2.0f;
                playerNetworkMovement.MoveSpeed *= 2.0f;

                playerHasDamageAndSpeedBuff = true;

                if (playerHasDamageReductionAndRegenBuff)
                {
                    playerNetworkHealth.DivideHealthRegenRateBy(3f); // Normalize health regen
                    playerNetworkHealth.PermanentDamageReductionIncreaseBy(-0.9f); // Remove damage reduction
                    playerHasDamageReductionAndRegenBuff = false;
                }
            }
            else if (!playerIsAboveHalfHealth && !playerHasDamageReductionAndRegenBuff)
            {
                // Apply damage reduction and increased regen to player
                playerNetworkHealth.PermanentDamageReductionIncreaseBy(0.9f);
                playerNetworkHealth.MultiplyHealthRegenBy(3f);
                playerHasDamageReductionAndRegenBuff = true;

                if (playerHasDamageAndSpeedBuff)
                {
                    playerSkills.DivideMeleeDamageBy(2.0f);
                    playerWeapon.Damage /= 2.0f;
                    playerNetworkMovement.MoveSpeed /= 2.0f;
                    playerHasDamageAndSpeedBuff = false;
                }
            }

            yield return new WaitForSeconds(1f);
        }

        // Reset buffs for golems
        foreach (var golem in golemManager.SpawnedGolems)
        {
            if (golemHasDamageAndSpeedBuff)
            {
                golem.DivideDamageBy(2.0f);
                golem.DivideMovementSpeedBy(2.0f);
            }
            if (golemHasDamageReductionAndRegenBuff)
            {
                golem.IncreaseDamageReduction(-0.9f);
                golem.DivideHealthRegenRateBy(3.0f);
            }
        }

        // Reset buffs for player
        if (playerHasDamageAndSpeedBuff)
        {
            playerSkills.DivideMeleeDamageBy(2.0f);
            playerWeapon.Damage /= 2.0f;
            playerNetworkMovement.MoveSpeed /= 2.0f;
        }
        if (playerHasDamageReductionAndRegenBuff)
        {
            playerNetworkHealth.PermanentDamageReductionIncreaseBy(-0.5f);
            playerNetworkHealth.DivideHealthRegenRateBy(3f);
        }
    }



}
