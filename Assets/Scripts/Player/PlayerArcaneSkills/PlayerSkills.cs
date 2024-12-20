using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSkills : NetworkBehaviour
{
    public List<Skill> unlockedSkills = new List<Skill>();
    public List<ActiveSkill> hotbarSkills = new List<ActiveSkill>(10); // Fixed-size hotbar
    [SerializeField] GameObject SkillTreeCanvas;
    [SerializeField] HotbarUIManager hotbarUIManager;
    [SerializeField] SkillTreeManager skillTreeManager;
    [SerializeField] Skill SingleCrescentSlashInstance;
    [SerializeField] GameObject firstPersonCanvas;
    [SerializeField] GameObject hotbarUI;
    [SerializeField] GameObject ammoCountUI;
    ISkillManager[] skillManagers;
    ActiveSkill currentAttack;
    bool canAttack = true;
    bool bloodBond = false;
    float bloodBondRange = 10f;
    Animator animator;
    PlayerWeapon playerWeapon;
    PlayerNetworkHealth playerHealth;
    PlayerNetworkMovement playerMovement;
    PlayerNetworkRotation playerRotation;
    GolemManager golemManager;

    HashSet<Enemy> enemiesInRange = new HashSet<Enemy>();

    public int attackIndex = 0;

    public override void OnNetworkSpawn()
    {
        Debug.Log("PlayerSkills OnNetworkSpawn called.");
        if (!IsLocalPlayer)
        {
            skillTreeManager.gameObject.SetActive(false);
        }
        base.OnNetworkSpawn();
        animator = GetComponent<Animator>();
        playerWeapon = GetComponent<PlayerWeapon>();
        playerHealth = GetComponent<PlayerNetworkHealth>();
        playerMovement = GetComponent<PlayerNetworkMovement>();
        playerRotation = GetComponent<PlayerNetworkRotation>();
        golemManager = GetComponent<GolemManager>();
        skillManagers = GetComponents<ISkillManager>();
        Debug.Log($"SkillManagers count: {skillManagers.Length}");
        skillTreeManager.SetPlayerSkills(this);
        UnlockSkill(SingleCrescentSlashInstance);
        SkillTreeCanvas.SetActive(false);

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

            for (int i = 0; i < hotbarSkills.Count && i < 9; i++) // Limit to 1-9 keys
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    if (hotbarSkills[i] != null) // Ensure slot is not empty
                    {
                        attackIndex = i;
                    }
                    break;
                }
            }

            foreach (var skill in hotbarSkills)
            {
                skill.Update();
            }

            if (bloodBond)
            {
                BloodBondEffect();
            }

        }
    }

    public void GiveSkillPoints(int points)
    {
        skillTreeManager.AddSkillPoint(points);
    }

    public void ResetGolems()
    {
        golemManager.ResetGolems();
    }

    public void ResetPlayerSkills()
    {
        foreach (var skillManager in skillManagers)
        {
            skillManager.ResetSkill();
        }
    }
    public void ResetSkillTree()
    {
        skillTreeManager.ResetSkillTree();
    }
    public bool SkillTreeOpen()
    {
        return SkillTreeCanvas.activeSelf;
    }
    public void ActivateBloodBond()
    {
        bloodBond = true;
    }
    public void IncreaseBloodBondRange(float rangeIncrease)
    {
        bloodBondRange += rangeIncrease;
    }

    void BloodBondEffect()
    {
        // Track which enemies are currently within range
        HashSet<Enemy> currentEnemiesInRange = new HashSet<Enemy>();

        foreach (Enemy enemy in GameManager.Instance.SpawnedEnemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= bloodBondRange)
                {
                    currentEnemiesInRange.Add(enemy);

                    // If this is a new enemy within range, increase health regen
                    if (!enemiesInRange.Contains(enemy))
                    {
                        playerHealth.PermanentHealthRegenIncreaseBy(2f);
                    }
                }
            }
        }

        // Remove regen for enemies that left the range
        foreach (Enemy enemy in enemiesInRange)
        {
            if (!currentEnemiesInRange.Contains(enemy))
            {
                playerHealth.PermanentHealthRegenIncreaseBy(-2f);
            }
        }

        // Update the enemies currently in range
        enemiesInRange = currentEnemiesInRange;
    }


    public void UnlockSkill(Skill skill)
    {
        if (!unlockedSkills.Contains(skill))
        {
            unlockedSkills.Add(skill);
            if (skill is ActiveSkill)
            {
                AddToHotbar((ActiveSkill)skill);
                ((ActiveSkill)skill).Initialize(animator);
                hotbarUIManager.AssignHotbarIcon();
            }
            else if (skill is PassiveSkill)
            {
                if (IsLocalPlayer)
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
            SpawnSlashImpactRpc("MeleeSlash1Hit", collider.transform.position, Quaternion.identity);
            PopUpNumberManager.Instance.SpawnMeleeDamageNumber(collider.transform.position, damage);
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
            if (Vector3.Angle(transform.forward, directionToTarget) <= coneAngle && (collider.CompareTag("Enemy") || collider.CompareTag("Destroyables")))
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

    public void DealDamageInExpandingCircle(Vector3 origin, float initialRange, float maxRange, float damage, float knockbackForce, float duration, float tickRate, int maxHits = 3)
    {
        StartCoroutine(ExpandingDamageOverTimeCoroutine(origin, initialRange, maxRange, damage, knockbackForce, duration, tickRate, maxHits));
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

    private IEnumerator ExpandingDamageOverTimeCoroutine(
     Vector3 origin,
     float initialRange,
     float maxRange,
     float damage,
     float knockbackForce,
     float duration,
     float tickRate,
     int maxHits = 3
 )
    {
        float elapsedTime = 0f;
        float currentRange = initialRange;

        // Track hit counts
        var hitCounts = new Dictionary<Collider, int>();

        while (elapsedTime < duration)
        {
            // Expand the range linearly over time
            currentRange = Mathf.Lerp(initialRange, maxRange, elapsedTime / duration);

            // Visualize and log the sphere
            Debug.Log($"[Expanding Damage] Checking range {currentRange} at {origin}");
            Debug.DrawRay(origin, Vector3.up * 5f, Color.red, 1f);

            // Check colliders in range
            Collider[] collidersInRange = Physics.OverlapSphere(origin, currentRange);
            Debug.Log($"[Expanding Damage] Found {collidersInRange.Length} colliders");

            foreach (Collider collider in collidersInRange)
            {
                if (collider == null)
                {
                    Debug.LogWarning("Null collider detected!");
                    continue;
                }

                Debug.Log($"Found collider: {collider.name}, Tag: {collider.tag}");

                // Check valid tags
                if (!collider.CompareTag("Enemy") && !collider.CompareTag("Destroyables"))
                {
                    Debug.Log($"Skipping collider {collider.name} due to invalid tag");
                    continue;
                }

                // Hit logic
                if (!hitCounts.ContainsKey(collider)) hitCounts[collider] = 0;

                if (hitCounts[collider] < maxHits)
                {
                    Debug.Log($"Dealing {damage} damage to {collider.name} (hit count: {hitCounts[collider]})");
                    DealDamage(collider, origin, damage, knockbackForce);
                    hitCounts[collider]++;
                }
                else
                {
                    Debug.Log($"Collider {collider.name} reached max hits ({maxHits})");
                }
            }

            yield return new WaitForSeconds(tickRate);
            elapsedTime += tickRate;
        }
    }




    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnSlashImpactRpc(string impactName, Vector3 position, Quaternion rotation)
    {
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler.Instance is null. Cannot spawn effect.");
            return;
        }

        var impact = ObjectPooler.Instance.Spawn(impactName, position, rotation);
        if (impact == null) Debug.LogError("Failed to spawn effect. Check ObjectPooler configuration.");
    }

    [ServerRpc]
    public void SummonGolemServerRpc(string golemName, float health, float damage, float attackRange, float moveSpeed, float attackSpeed, float damageReduction, float reviveTime)
    {
        if (!IsServer)
        {
            Debug.LogError("Only the server can spawn golems.");
            return;
        }
        // Only the server spawns the golem
        GameObject Golem = ObjectPooler.Instance.Spawn(golemName, transform.position, Quaternion.identity);
        if (Golem != null)
        {
            Golem script = Golem.GetComponent<Golem>();
            script.SetOwner(gameObject);  // Set the owner to the player who summoned it
            script.Damage = damage;
            script.MaxHealth.Value = health;
            script.CurrentHealth.Value = health;
            script.AttackRange = attackRange;
            script.MovementSpeed = moveSpeed;
            script.AttackCooldown = attackSpeed;
            script.DamageReduction = damageReduction;
            script.ReviveTime = reviveTime;
            // Register the golem in game manager if needed
            Golem.GetComponent<NetworkObject>().Spawn();
            Debug.Log($"Golem spawned on {(IsServer ? "Server" : "Client")} with ClientID: {NetworkManager.Singleton.LocalClientId}. Owner: {GetComponent<NetworkObject>().OwnerClientId}");
            golemManager.SpawnedGolems.Add(script);

        }
    }

    public void VitalityPlus()
    {
        var vitality = unlockedSkills.Find(skill => skill is Vitality) as Vitality;
        if (vitality != null)
        {
            PermanentHealthIncreaseBy(100f);
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }

    public void BloodBondPlus()
    {
        var bloodBond = unlockedSkills.Find(skill => skill is BloodBond) as BloodBond;
        if (bloodBond != null)
        {
            IncreaseBloodBondRange(4f);
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }

    public void BlastforgedGuardianPlus()
    {
        var blastforgedGuardian = unlockedSkills.Find(skill => skill is BlastforgedGuardian) as BlastforgedGuardian;
        if (blastforgedGuardian != null)
        {
            var golemManager = GetComponent<GolemManager>();
            golemManager.IncreaseGolemHealth(100f);
            golemManager.IncreaseGolemDamage(150f);
            golemManager.IncreaseGolemAttackRange(0.5f);
            golemManager.IncreaseGolemMovementSpeed(0.5f);
            golemManager.IncreaseBuffRadius(2f);
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }

    public void TempestGuardianPlus()
    {
        var tempestGuardian = unlockedSkills.Find(skill => skill is TempestGuardian) as TempestGuardian;
        if (tempestGuardian != null)
        {
            var golemManager = GetComponent<GolemManager>();
            golemManager.IncreaseGolemHealth(150f);
            golemManager.IncreaseGolemDamage(100f);
            golemManager.IncreaseGolemAttackRange(0.5f);
            golemManager.IncreaseBuffRadius(1f);
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }

    public void RegenerativeAuraPlus()
    {
        var regenerativeAura = unlockedSkills.Find(skill => skill is RegenerativeAura) as RegenerativeAura;
        if (regenerativeAura != null)
        {
            playerHealth.PermanentHealthRegenIncreaseBy(10f);
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }

    public void IronResolvePlus()
    {
        var ironResolve = unlockedSkills.Find(skill => skill is IronResolve) as IronResolve;
        if (ironResolve != null)
        {
            playerHealth.IncreaseIronResolveDamageReduction(0.15f);
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }

    public void LifeGuardianPlus()
    {
        var lifeGuardian = unlockedSkills.Find(skill => skill is LifeGuardian) as LifeGuardian;
        if (lifeGuardian != null)
        {
            var golemManager = GetComponent<GolemManager>();
            golemManager.IncreaseGolemDamageReduction(0.025f);
            golemManager.IncreaseGolemHealth(300f);
            golemManager.IncreaseGolemDamage(60f);
            golemManager.IncreaseGolemAttackRange(0.5f);
            golemManager.IncreaseGolemMovementSpeed(0.5f);
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }


    public void LifeSurgePlus()
    {
        var lifeSurge = unlockedSkills.Find(skill => skill is LifeSurge) as LifeSurge;
        if (lifeSurge != null)
        {
            LifeSurgeManager script = GetComponent<LifeSurgeManager>();
            script.IncreaseHealRadius(4f);
            script.IncreaseHealStrength(0.1f);
            playerHealth.PermanentHealthIncreaseByRpc(20f);

            foreach (Skill skill in unlockedSkills)
            {
                if (skill is LifeSurge lifeSurgeSkill)
                {
                    lifeSurgeSkill.Cooldown -= 5f;
                    break;
                }
            }
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }

    public void BladeVortexPlus()
    {
        var BladeVortex = unlockedSkills.Find(skill => skill is ArcaneBladeVortex) as ArcaneBladeVortex;
        if (BladeVortex != null)
        {
            ArcaneBladeVortexManager script = GetComponent<ArcaneBladeVortexManager>();
            script.Damage += 10f;
            script.AttackRange += 0.5f;
            script.Duration += 5f;

            foreach (Skill skill in unlockedSkills)
            {
                if (skill is ArcaneBladeVortex arcaneBladeVortex)
                {
                    arcaneBladeVortex.Cooldown -= 10f;
                    break;
                }
            }
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }

    }

    public void ArcanePlaguePlus()
    {
        // Increase the duration of the debuff
        var ArcanePlague = unlockedSkills.Find(skill => skill is ArcanePlague) as ArcanePlague;
        if (ArcanePlague != null)
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
            playerWeapon.Damage += 2f;
        }
        else
        {
            // If arcanePlague is not unlocked, apply the stat increases
            PermanentTravelNodeStatIncrease();

        }
    }


    public void ArcaneCleavePlus()
    {
        var ArcaneCleave = unlockedSkills.Find(skill => skill is ArcaneCleave) as ArcaneCleave;
        if (ArcaneCleave != null)
        {
            ArcaneCleaveManager script = GetComponent<ArcaneCleaveManager>();
            script.Damage += 30f;
            script.AttackRange += 0.5f;

            foreach (Skill skill in unlockedSkills)
            {
                if (skill is ArcaneCleave arcaneCleave)
                {
                    arcaneCleave.Cooldown -= 0.5f;
                    break; // Exit loop once ArcaneCleave is found
                }
            }
        }
        else
        {
            PermanentTravelNodeStatIncrease();

        }
    }


    public void ArcaneBarrierPlus()
    {
        // Try to find an instance of ArcaneBarrier within unlockedSkills
        var arcaneBarrierSkill = unlockedSkills.Find(skill => skill is ArcaneBarrier) as ArcaneBarrier;

        if (arcaneBarrierSkill != null)
        {
            ArcaneBarrierManager script = GetComponent<ArcaneBarrierManager>();
            script.Duration += 5f;
            script.DamageReduction += 7.5f;

            foreach (Skill skill in unlockedSkills)
            {
                if (skill is ArcaneBarrier arcaneBarrier)
                {
                    arcaneBarrier.Cooldown -= 10f;  // Now accessing Cooldown safely
                    break;
                }
            }
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }



    public void DoubleCrescentSlashPlus()
    {
        // Try to find an instance of DoubleCrescentSlash within unlockedSkills
        var doubleCrescentSlashSkill = unlockedSkills.Find(skill => skill is DoubleCrescentSlash) as DoubleCrescentSlash;

        if (doubleCrescentSlashSkill != null)
        {
            DoubleCrescentSlashManager script = GetComponent<DoubleCrescentSlashManager>();
            script.Damage += 20f;
            script.AttackRange += 0.5f;

            foreach (Skill skill in unlockedSkills)
            {
                if (skill is DoubleCrescentSlash doubleCrescentSlash)
                {
                    doubleCrescentSlash.Cooldown -= 0.5f;  // Safely accessing Cooldown
                    break;
                }
            }
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }
    }


    public void RelentlessOnslaughtPlus()
    {
        var relentlessOnslaughtSkill = unlockedSkills.Find(skill => skill is RelentlessOnslaught) as RelentlessOnslaught;
        if (relentlessOnslaughtSkill != null)
        {
            RelentlessOnslaughtManager script = GetComponent<RelentlessOnslaughtManager>();
            script.Duration += 5f;

            foreach (Skill skill in unlockedSkills)
            {
                if (skill is RelentlessOnslaught relentlessOnslaught)
                {
                    relentlessOnslaught.Cooldown -= 10f;
                    break;
                }
            }
        }
        else
        {
            PermanentTravelNodeStatIncrease();
        }

    }
    public void PermanentTravelNodeStatIncrease()
    {
        PermanentMeleeDamageIncreaseBy(7.5f);
        playerWeapon.Damage += 2.5f;
        PermanentAttackSpeedIncreaseBy(0.03f);
        PermanentHealthIncreaseBy(2.5f);
        playerMovement.MoveSpeed += 0.1f;
    }

    public void MultiplyMeleeDamageBy(float multiplier)
    {
        foreach (var skillManager in skillManagers)
        {
            skillManager.Damage *= multiplier;
        }
    }

    public void DivideMeleeDamageBy(float divisor)
    {
        foreach (var skillManager in skillManagers)
        {
            skillManager.Damage /= divisor;
        }
    }


    public void PermanentHealthIncreaseBy(float healthIncrease)
    {
        playerHealth.PermanentHealthIncreaseByRpc(healthIncrease);
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
            if (skillManager != null)
            {
                skillManager.AttackSpeedMultiplier.Value += attackSpeedIncrease;
            }
            else
            {
                Debug.LogWarning("Null skill manager encountered.");
            }
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
