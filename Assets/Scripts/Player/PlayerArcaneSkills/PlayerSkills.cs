using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSkills : NetworkBehaviour
{
    public List<Skill> unlockedSkills = new List<Skill>();
    public List<ActiveSkill> hotbarSkills = new List<ActiveSkill>(10); // Fixed-size hotbar
    ISkillManager[] skillManagers;
    ActiveSkill currentAttack;
    bool canAttack = true;
    Animator animator;
    PlayerNetworkMovement playerMovement;
    PlayerNetworkRotation playerRotation;

    public int attackIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer) return;
        base.OnNetworkSpawn();

        animator = GetComponent<Animator>();
        skillManagers = GetComponentsInChildren<ISkillManager>();
        playerMovement = GetComponent<PlayerNetworkMovement>();
        playerRotation = GetComponent<PlayerNetworkRotation>();
    }

    void Update()
    {
        if (IsLocalPlayer)
        {
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
