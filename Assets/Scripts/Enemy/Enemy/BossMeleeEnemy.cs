using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;

public class BossMeleeEnemy : MeleeEnemy
{
    [SerializeField] Transform leftHandWeapon;
    [SerializeField] Transform rightHandWeapon;
    [SerializeField] List<string> AttacksFirstPhase;
    [SerializeField] List<string> AttacksSecondPhase;
    [SerializeField] List<string> AttacksThirdPhase;
    [SerializeField] ParticleSystem slashEffect;
    [SerializeField] float jumpAttackCooldown = 10f;
    float jumpAttackCooldownElapsed = 0f;
    [SerializeField] float jumpAttackDamage = 500f;
    [SerializeField] float jumpAttackRange = 15f;
    [SerializeField] float tripleJumpAttackCooldown = 10f;
    float tripleJumpAttackCooldownElapsed = 0f;

    [SerializeField] AudioClip onLandSoundEffect;
    [SerializeField] AudioClip onLandSoundEffect2;
    [SerializeField] AudioClip jumpAttackSoundEffect;

    protected override void Update()
    {
        base.Update();

        // JumpAttackLogic();
        TripleJumpAttackLogic();

    }

    public override IEnumerator Attack()
    {
        if (ClosestTarget != null)
        {

            if (enemyHealth.CurrentHealth.Value <= enemyHealth.MaxHealth / 3 && AttacksThirdPhase.Count > 0)
            {
                isAttacking = true;
                int randomIndex = Random.Range(0, AttacksThirdPhase.Count);
                animator.SetTrigger(AttacksThirdPhase[randomIndex]);

                // Wait until the attack animation starts
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(AttacksThirdPhase[randomIndex]));

                // Wait for the duration of the attack animation
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

                isAttacking = false;
            }
            else if (enemyHealth.CurrentHealth.Value <= enemyHealth.MaxHealth / 3 * 2 && AttacksSecondPhase.Count > 0)
            {
                isAttacking = true;
                int randomIndex = Random.Range(0, AttacksSecondPhase.Count);
                animator.SetTrigger(AttacksSecondPhase[randomIndex]);

                // Wait until the attack animation starts
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(AttacksSecondPhase[randomIndex]));

                // Wait for the duration of the attack animation
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

                isAttacking = false;
            }
            else
            {
                isAttacking = true;
                int randomIndex = Random.Range(0, AttacksFirstPhase.Count);
                animator.SetTrigger(AttacksFirstPhase[randomIndex]);

                // Wait until the attack animation starts
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(AttacksFirstPhase[randomIndex]));

                // Wait for the duration of the attack animation
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

                isAttacking = false;
            }
        }
    }

    void TripleJumpAttackLogic()
    {
        if (enemyHealth.CurrentHealth.Value <= enemyHealth.MaxHealth / 1)
        {
            if (ClosestTarget != null && !isAttacking && tripleJumpAttackCooldownElapsed <= 0)
            {
                if (Vector3.Distance(transform.position, ClosestTarget.position) <= jumpAttackRange)
                {

                    TripleJumpAttackRpc();
                    tripleJumpAttackCooldownElapsed = tripleJumpAttackCooldown;

                }
            }
        }

        if (tripleJumpAttackCooldownElapsed > 0)
            tripleJumpAttackCooldownElapsed -= Time.deltaTime;
    }

    void JumpAttackLogic()
    {
        if (enemyHealth.CurrentHealth.Value <= enemyHealth.MaxHealth / 2f)
        {
            if (ClosestTarget != null && !isAttacking && jumpAttackCooldownElapsed <= 0)
            {
                if (Vector3.Distance(transform.position, ClosestTarget.position) <= jumpAttackRange)
                {

                    JumpAttackRpc();
                    jumpAttackCooldownElapsed = jumpAttackCooldown;
                }
            }
        }

        if (jumpAttackCooldownElapsed > 0)
            jumpAttackCooldownElapsed -= Time.deltaTime;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void JumpAttackRpc()
    {
        StartCoroutine(JumpAttackCoroutine());
    }
    IEnumerator JumpAttackCoroutine()
    {
        transform.LookAt(ClosestTarget);

        isAttacking = true;
        jumpAttackCooldown = Random.Range(20f, 40f);
        // Simulate a jump attack using the tweening library DOTween

        Vector3 targetPosition = ClosestTarget.position;

        var WarningZone = ObjectPooler.Instance.Spawn("WarningZone", targetPosition, Quaternion.identity);
        WarningZone.transform.localScale = Vector3.zero;
        WarningZone.transform.DOScale(attackRange / 1.35f, 2.75f);

        animator.SetTrigger("JumpAttack");

        yield return new WaitForSeconds(1f);

        audioSource.PlayOneShot(jumpAttackSoundEffect);
        transform.DOJump(targetPosition, 10f, 1, 2.75f).OnComplete(() =>
       {
           DealDamageJumpAttackDamage(7f);
           SpawnOnLandEffects();
           AudioSource.PlayClipAtPoint(onLandSoundEffect, transform.position + Vector3.down, 1.5f);
           AudioSource.PlayClipAtPoint(onLandSoundEffect2, transform.position + Vector3.down, 1.5f);
           ObjectPooler.Instance.Despawn("WarningZone", WarningZone);
           isAttacking = false;
       }).SetEase(Ease.Linear);

    }


    [Rpc(SendTo.ClientsAndHost)]
    void TripleJumpAttackRpc()
    {
        transform.LookAt(ClosestTarget);

        isAttacking = true;

        var warningZone1 = ObjectPooler.Instance.Spawn("WarningZone", transform.position, Quaternion.identity);
        warningZone1.transform.localScale = Vector3.zero;
        var warningZone2 = ObjectPooler.Instance.Spawn("WarningZone", transform.position, Quaternion.identity);
        warningZone2.transform.localScale = Vector3.zero;
        var warningZone3 = ObjectPooler.Instance.Spawn("WarningZone", transform.position, Quaternion.identity);
        warningZone3.transform.localScale = Vector3.zero;

        tripleJumpAttackCooldown = 60f;

        // First jump
        Vector3 targetPosition = ClosestTarget.position;
        warningZone1.transform.position = targetPosition;
        warningZone1.transform.DOScale(attackRange / 1.75f, 3f);
        animator.SetTrigger("JumpAttack");
        audioSource.PlayOneShot(jumpAttackSoundEffect);

        transform.DOJump(targetPosition, 7.5f, 1, 3f).OnUpdate(() =>
        {
            transform.LookAt(targetPosition);
        }).OnComplete(() =>
        {

            // First jump completes
            DealDamageJumpAttackDamage(5f);
            SpawnOnLandEffects();
            AudioSource.PlayClipAtPoint(onLandSoundEffect, transform.position + Vector3.down, 1.25f);
            AudioSource.PlayClipAtPoint(onLandSoundEffect2, transform.position + Vector3.down, 1.25f);

            if (warningZone1 != null)
                ObjectPooler.Instance.Despawn("WarningZone", warningZone1);

            // Second jump
            PerformSecondJump(warningZone2, warningZone3);
        }).SetEase(Ease.Linear);
    }

    void PerformSecondJump(GameObject warningZone2, GameObject warningZone3)
    {
        transform.LookAt(ClosestTarget);

        Vector3 targetPosition = ClosestTarget.position;
        warningZone2.transform.position = targetPosition;
        warningZone2.transform.DOScale(attackRange / 1.75f, 3f);

        animator.ResetTrigger("JumpAttack");
        animator.SetTrigger("JumpAttack");

        audioSource.PlayOneShot(jumpAttackSoundEffect);

        transform.DOJump(targetPosition, 7.5f, 1, 3f).OnUpdate(() =>
        {
            transform.LookAt(targetPosition);
        }).OnComplete(() =>
        {


            // Second jump completes
            DealDamageJumpAttackDamage(5f);
            SpawnOnLandEffects();
            AudioSource.PlayClipAtPoint(onLandSoundEffect, transform.position + Vector3.down, 1.25f);
            AudioSource.PlayClipAtPoint(onLandSoundEffect2, transform.position + Vector3.down, 1.25f);
            if (warningZone2 != null)
                ObjectPooler.Instance.Despawn("WarningZone", warningZone2);

            // Third jump
            PerformThirdJump(warningZone3);
        }).SetEase(Ease.Linear);
    }

    void PerformThirdJump(GameObject warningZone3)
    {
        transform.LookAt(ClosestTarget);

        Vector3 targetPosition = ClosestTarget.position;

        warningZone3.transform.position = targetPosition;
        warningZone3.transform.DOScale(attackRange / 1.25f, 4f);

        animator.ResetTrigger("JumpAttack");
        animator.SetTrigger("JumpAttack");

        audioSource.PlayOneShot(jumpAttackSoundEffect);

        transform.DOJump(targetPosition, 10f, 1, 4f).OnComplete(() =>
        {
            // Third jump completes
            DealDamageJumpAttackDamage(8f);
            SpawnOnLandEffects(1.25f);
            AudioSource.PlayClipAtPoint(onLandSoundEffect, transform.position + Vector3.down, 1.25f);
            AudioSource.PlayClipAtPoint(onLandSoundEffect2, transform.position + Vector3.down, 1.25f);
            if (warningZone3 != null)
                ObjectPooler.Instance.Despawn("WarningZone", warningZone3);
            isAttacking = false; // End attack
        }).OnUpdate(() =>
        {
            transform.LookAt(targetPosition);
        }).SetEase(Ease.Linear);
    }


    void SpawnOnLandEffects(float attackRangeScalingFactor = 1.5f)
    {
        ObjectPooler.Instance.Spawn("DefaultLargeParticle", transform.position, Quaternion.identity);
        ObjectPooler.Instance.Spawn("DefaultSmallParticle", transform.position, Quaternion.identity);

        var earthSlam = ObjectPooler.Instance.Spawn("EarthSlam", transform.position, Quaternion.identity);
        earthSlam.transform.localScale = new Vector3(attackRange / attackRangeScalingFactor, attackRange / attackRangeScalingFactor, attackRange / attackRangeScalingFactor);

        var pillarBlast = ObjectPooler.Instance.Spawn("EarthPillarBlast", transform.position, Quaternion.identity);
        pillarBlast.transform.localScale = new Vector3(attackRange / attackRangeScalingFactor, attackRange / attackRangeScalingFactor, attackRange / attackRangeScalingFactor);
        pillarBlast.transform.localRotation = Quaternion.Euler(-90, 0, 90);
    }

    public void DealDamageJumpAttackDamage(float damageRange)
    {
        DealDamageFrom(jumpAttackDamage, transform.position, damageRange);
    }

    public void DealDamageFromLeftHand()
    {
        DealDamageFrom(attackDamage, leftHandWeapon.position, 1f);
    }

    public void DealDamageFromRightHand()
    {
        DealDamageFrom(attackDamage, rightHandWeapon.position, 1f);
    }

    public void DealDamageFrom(float attackDamage, Vector3 position, float extraRange)
    {
        if (ClosestTarget != null &&
            Vector3.Distance(position, ClosestTarget.position) <= attackRange + extraRange)
        {
            var damageable = ClosestTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.RequestTakeDamageServerRpc(attackDamage, NetworkObjectId);
            }
        }

        // AoE Damage
        Collider[] hitColliders = Physics.OverlapSphere(position, attackRange + extraRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Destroyables"))
            {
                var damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.RequestTakeDamageServerRpc(attackDamage, NetworkObjectId);
                }
            }
        }
    }
}
