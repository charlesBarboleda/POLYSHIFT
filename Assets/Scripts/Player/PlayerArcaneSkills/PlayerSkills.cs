using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerSkills : NetworkBehaviour
{
    public List<Skill> unlockedSkills = new List<Skill>();
    public List<ActiveSkill> hotbarSkills = new List<ActiveSkill>(10); // Fixed-size hotbar
    [SerializeField] SkillTreeManager skillTreeManager;
    ISkillManager[] skillManagers;
    ActiveSkill currentAttack;
    bool canAttack = true;
    Animator animator;
    PlayerWeapon playerWeapon;
    PlayerNetworkHealth playerHealth;
    PlayerNetworkMovement playerMovement;
    PlayerNetworkRotation playerRotation;

    public int attackIndex = 0;

    public override void OnNetworkSpawn()
    {
        Debug.Log("PlayerSkills OnNetworkSpawn called.");
        if (!IsLocalPlayer)
        {
            skillTreeManager.gameObject.SetActive(false);
            return;
        }
        base.OnNetworkSpawn();

        animator = GetComponent<Animator>();
        playerWeapon = GetComponent<PlayerWeapon>();
        playerHealth = GetComponent<PlayerNetworkHealth>();
        skillManagers = GetComponentsInChildren<ISkillManager>();
        playerMovement = GetComponent<PlayerNetworkMovement>();
        playerRotation = GetComponent<PlayerNetworkRotation>();
        skillTreeManager.SetPlayerSkills(this);

    }

    void Update()
    {
        if (IsLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                skillTreeManager.ToggleSkillTree();
            }
            if (Input.GetMouseButtonDown(1) && canAttack)
            {
                UseSkill(attackIndex);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
                attackIndex = 0;
            if (Input.GetKeyDown(KeyCode.Alpha2))

                attackIndex = 1;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                attackIndex = 2;
            if (Input.GetKeyDown(KeyCode.Alpha4))
                attackIndex = 3;
            if (Input.GetKeyDown(KeyCode.Alpha5))

                attackIndex = 4;
            if (Input.GetKeyDown(KeyCode.Alpha6))
                attackIndex = 5;
            if (Input.GetKeyDown(KeyCode.Alpha7))
                attackIndex = 6;
            if (Input.GetKeyDown(KeyCode.Alpha8))
                attackIndex = 7;
            if (Input.GetKeyDown(KeyCode.Alpha9))

                attackIndex = 8;
            if (Input.GetKeyDown(KeyCode.Alpha0))

                attackIndex = 9;
        }
    }


    public void UnlockSkill(Skill skill)
    {
        if (!unlockedSkills.Contains(skill))
        {
            unlockedSkills.Add(skill);
            if (skill is ActiveSkill)
            {
                AddToHotbar((ActiveSkill)skill);
            }
            else if (skill is PassiveSkill)
            {
                skill.ApplySkillEffect(gameObject);
            }
        }
    }

    private void AddToHotbar(ActiveSkill skill)
    {
        if (hotbarSkills.Count < 10)
        {
            hotbarSkills.Add(skill);
        }
        else
        {
            Debug.LogWarning("Hotbar is full!");
        }
    }

    public void UseSkill(int index)
    {
        if (index >= 0 && index < hotbarSkills.Count && hotbarSkills[index] != null)
        {
            hotbarSkills[index].ApplySkillEffect(gameObject);  // Pass player as the user
        }
    }





    private void DealDamage(Collider collider, Vector3 origin, float damage, float knockbackForce)
    {
        var damageable = collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            SpawnSlashImpactClientRpc("MeleeSlash1Hit", collider.transform.position, Quaternion.identity);
            damageable.RequestTakeDamageServerRpc(damage, NetworkObjectId);
        }

        var rb = collider.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce((collider.transform.position - origin).normalized * knockbackForce, ForceMode.Impulse);

        var enemy = collider.GetComponent<Enemy>();
        if (enemy != null)
            enemy.OnRaycastHitServerRpc(collider.transform.position, (collider.transform.position - origin).normalized);
    }

    public void DealDamageInCone(Vector3 origin, float attackRange, float coneAngle, float damage, float knockbackForce)
    {
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRange);
        foreach (var collider in hitColliders)
        {
            Vector3 directionToTarget = (collider.transform.position - origin).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) <= coneAngle && collider.CompareTag("Enemy"))
            {
                DealDamage(collider, origin, damage, knockbackForce);
            }
        }
    }

    public void DealDamageInCircle(Vector3 origin, float attackRange, float damage, float knockbackForce)
    {
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRange);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy") || collider.CompareTag("Destroyables"))
            {
                DealDamage(collider, origin, damage, knockbackForce);
            }
        }
    }

    public void DealDamageInExpandingCircle(Vector3 origin, float initialRange, float maxRange, float damage, float knockbackForce, float duration, float tickRate)
    {
        StartCoroutine(ExpandingDamageOverTimeCoroutine(origin, initialRange, maxRange, damage, knockbackForce, duration, tickRate));
    }

    public void DealPeriodicDamageInCircle(Vector3 origin, float attackRange, float damage, float knockbackForce, float duration, float tickRate)
    {
        StartCoroutine(DealPeriodicDamageInCircleCoroutine(origin, attackRange, damage, knockbackForce, duration, tickRate));
    }

    IEnumerator DealPeriodicDamageInCircleCoroutine(Vector3 origin, float attackRange, float damage, float knockbackForce, float duration, float tickRate)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            Collider[] hitColliders = Physics.OverlapSphere(origin, attackRange);
            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Enemy") || collider.CompareTag("Destroyables"))
                {
                    DealDamage(collider, origin, damage, knockbackForce);
                }
            }
            yield return new WaitForSeconds(tickRate);
            elapsedTime += tickRate;
        }
    }

    private IEnumerator ExpandingDamageOverTimeCoroutine(Vector3 origin, float initialRange, float maxRange, float damage, float knockbackForce, float duration, float tickRate, int maxHits = 3)
    {
        float elapsedTime = 0f;
        float currentRange = initialRange;
        var hitCount = new Dictionary<Collider, int>();

        while (elapsedTime < duration)
        {
            currentRange = Mathf.Lerp(initialRange, maxRange, elapsedTime / duration);
            Collider[] hitColliders = Physics.OverlapSphere(origin, currentRange);

            foreach (var collider in hitColliders)
            {
                if ((collider.CompareTag("Enemy") || collider.CompareTag("Destroyables")) && (!hitCount.ContainsKey(collider) || hitCount[collider] < maxHits))
                {
                    DealDamage(collider, origin, damage, knockbackForce);
                    hitCount[collider] = hitCount.ContainsKey(collider) ? hitCount[collider] + 1 : 1;
                }
            }
            yield return new WaitForSeconds(tickRate);
            elapsedTime += tickRate;
        }
    }

    [ClientRpc]
    private void SpawnSlashImpactClientRpc(string impactName, Vector3 position, Quaternion rotation)
    {
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler.Instance is null. Cannot spawn effect.");
            return;
        }

        var impact = ObjectPooler.Instance.Spawn(impactName, position, rotation);
        if (impact == null) Debug.LogError("Failed to spawn effect. Check ObjectPooler configuration.");
    }

    public void BladeVortexPlus()
    {
        ArcaneBladeVortexManager script = GetComponent<ArcaneBladeVortexManager>();
        script.Damage += 0.5f;
        script.AttackRange += 0.2f;
        script.Duration += 1f;

        bool bladeVortexUnlocked = false;

        foreach (ActiveSkill skill in unlockedSkills)
        {
            if (skill is ArcaneBladeVortex)
            {
                skill.Cooldown -= 2f;
                bladeVortexUnlocked = true;
                break;
            }
        }
        if (!bladeVortexUnlocked)
        {
            PermanentMeleeDamageIncreaseBy(2f);
            PermanentWeaponDamageIncreaseBy(0.5f);
            PermanentHealthIncreaseBy(2.5f);
            PermanentMovementSpeedIncreaseBy(0.1f);
        }
    }

    public void ArcanePlaguePlus()
    {
        // Increase the duration of the debuff
        if (unlockedSkills.Find(skill => skill is ArcanePlague) != null)
        {
            // If arcanePlague is unlocked, only apply the duration increase
            foreach (Debuff debuff in playerWeapon.weaponDebuffs)
            {
                if (debuff is ArcanePoison)
                {
                    debuff.duration += 0.5f;
                    break;
                }
            }
            playerWeapon.PermanentWeaponDamageIncreaseBy(0.5f);
        }
        else
        {
            // If arcanePlague is not unlocked, apply the stat increases
            PermanentMeleeDamageIncreaseBy(2f);
            PermanentWeaponDamageIncreaseBy(0.5f);
            PermanentHealthIncreaseBy(2.5f);
            PermanentMovementSpeedIncreaseBy(0.1f);

        }
    }


    public void ArcaneCleavePlus()
    {
        ArcaneCleaveManager script = GetComponent<ArcaneCleaveManager>();
        script.Damage += 10f;
        script.AttackRange += 0.2f;

        bool arcaneCleaveUnlocked = false;

        foreach (ActiveSkill skill in unlockedSkills)
        {
            if (skill is ArcaneCleave)
            {
                skill.Cooldown -= 0.2f;
                arcaneCleaveUnlocked = true;
                break; // Exit loop once ArcaneCleave is found
            }
        }

        // Apply stat increases if ArcaneCleave was not already unlocked
        if (!arcaneCleaveUnlocked)
        {
            PermanentMeleeDamageIncreaseBy(2f);
            PermanentWeaponDamageIncreaseBy(0.5f);
            PermanentHealthIncreaseBy(2.5f);
            PermanentMovementSpeedIncreaseBy(0.1f);

        }
    }


    public void ArcaneBarrierPlus()
    {
        ArcaneBarrierManager script = GetComponent<ArcaneBarrierManager>();
        script.Duration += 5f;
        script.DamageReduction += 5f;

        bool arcaneBarrierUnlocked = false;

        foreach (ActiveSkill skill in unlockedSkills)
        {
            if (skill is ArcaneBarrier)
            {
                skill.Cooldown -= 5f;
                arcaneBarrierUnlocked = true;
                break;
            }
        }
        if (!arcaneBarrierUnlocked)
        {
            PermanentMeleeDamageIncreaseBy(2f);
            PermanentWeaponDamageIncreaseBy(0.25f);
            PermanentHealthIncreaseBy(2.5f);
            PermanentMovementSpeedIncreaseBy(0.1f);

        }
    }


    public void DoubleCrescentSlashPlus()
    {
        DoubleCrescentSlashManager script = GetComponent<DoubleCrescentSlashManager>();
        script.Damage += 5f;
        script.coneAngle += 5f;
        script.AttackRange += 0.2f;

        bool skillUnlocked = false;

        foreach (ActiveSkill skill in unlockedSkills)
        {
            if (skill is DoubleCrescentSlash)
            {
                skill.Cooldown -= 0.2f;
                break;
            }
        }
        if (!skillUnlocked)
        {
            PermanentMeleeDamageIncreaseBy(2f);
            PermanentWeaponDamageIncreaseBy(0.5f);
            PermanentHealthIncreaseBy(2.5f);
            PermanentMovementSpeedIncreaseBy(0.1f);
        }
    }

    public void RelentlessOnslaughtPlus()
    {
        RelentlessOnslaughtManager script = GetComponent<RelentlessOnslaughtManager>();
        script.Duration += 2.5f;

        bool skillUnlocked = false;

        foreach (ActiveSkill skill in unlockedSkills)
        {
            if (skill is RelentlessOnslaught)
            {
                skill.Cooldown -= 7.5f;
                skillUnlocked = true;
                break;
            }
        }
        if (!skillUnlocked)
        {
            PermanentMeleeDamageIncreaseBy(2f);
            PermanentWeaponDamageIncreaseBy(0.5f);
            PermanentHealthIncreaseBy(2.5f);
            PermanentMovementSpeedIncreaseBy(0.1f);
        }
    }


    public void PermanentHealthIncreaseBy(float healthIncrease)
    {
        playerHealth.maxHealth.Value += healthIncrease;
        playerHealth.currentHealth.Value += healthIncrease;
    }

    public void PermanentMovementSpeedIncreaseBy(float speedIncrease)
    {
        playerMovement.MoveSpeed += speedIncrease;
    }


    public void PermanentWeaponFireRateIncreaseBy(float fireRateIncrease)
    {
        playerWeapon.ShootRate += fireRateIncrease;
    }

    public void PermanentWeaponDamageIncreaseBy(float damageIncrease)
    {
        playerWeapon.Damage += damageIncrease;
    }

    public void PermanentMeleeRangeIncreaseBy(float rangeIncrease)
    {
        foreach (var skillManager in skillManagers)
        {
            skillManager.AttackRange += rangeIncrease;
        }
    }
    public void PermanentAttackSpeedIncreaseBy(float attackSpeedIncrease)
    {
        foreach (var skillManager in skillManagers)
        {
            skillManager.AttackSpeedMultiplier.Value += attackSpeedIncrease;
        }
    }
    public void PermanentMeleeDamageIncreaseBy(float damageIncrease)
    {
        foreach (var skillManager in skillManagers)
        {
            skillManager.Damage += damageIncrease;
        }
    }

    public void IncreaseMeleeDamageBy(float multiplier, float duration)
    {
        StartCoroutine(IncreaseMeleeDamageCoroutine(multiplier, duration));
    }

    IEnumerator IncreaseMeleeDamageCoroutine(float multiplier, float duration)
    {
        foreach (var skillManager in skillManagers)
        {
            skillManager.Damage *= multiplier;
        }
        yield return new WaitForSeconds(duration);
        foreach (var skillManager in skillManagers)
        {
            skillManager.Damage /= multiplier;
        }
    }

    public void IncreaseAttackSpeedBy(float multiplier, float duration)
    {
        StartCoroutine(IncreaseAttackSpeedCoroutine(multiplier, duration));
    }

    IEnumerator IncreaseAttackSpeedCoroutine(float multiplier, float duration)
    {
        foreach (var skillManager in skillManagers)
        {
            skillManager.AttackSpeedMultiplier.Value *= multiplier;
        }
        yield return new WaitForSeconds(duration);
        foreach (var skillManager in skillManagers)
        {
            skillManager.AttackSpeedMultiplier.Value /= multiplier;
        }
    }

    public void ReduceCooldownsBy(float multiplier, float duration)
    {
        StartCoroutine(ReduceCooldownCoroutine(multiplier, duration));
    }
    IEnumerator ReduceCooldownCoroutine(float multiplier, float duration)
    {
        foreach (var attack in hotbarSkills)
        {
            attack.Cooldown *= multiplier;
        }
        yield return new WaitForSeconds(duration);
        foreach (var attack in hotbarSkills)
        {
            attack.Cooldown /= multiplier;
        }
    }
    public void DisableMovementAndRotation()
    {
        playerMovement.canMove = false;
        playerRotation.canRotate = false;
    }

    public void EnableMovementAndRotation()
    {
        playerMovement.canMove = true;
        playerRotation.canRotate = true;
    }
}
