using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossMeleeEnemy : MeleeEnemy
{
    [SerializeField] Transform leftHandWeapon;
    [SerializeField] Transform rightHandWeapon;
    [SerializeField] List<string> AttacksFirstPhase;
    [SerializeField] List<string> AttacksSecondPhase;
    [SerializeField] List<string> AttacksThirdPhase;
    [SerializeField] ParticleSystem slashEffect;



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

    public void SpawnSlashEffectLeftHand()
    {
        StartCoroutine(SpawnSlashEffect(true));
    }

    public void SpawnSlashEffectRightHand()
    {
        StartCoroutine(SpawnSlashEffect(false));
    }



    IEnumerator SpawnSlashEffect(bool leftHand)
    {
        if (slashEffect != null)
        {
            slashEffect.gameObject.SetActive(true);
            slashEffect.transform.rotation = leftHand ? leftHandWeapon.rotation : rightHandWeapon.rotation;
            slashEffect.transform.position = leftHand ? leftHandWeapon.position : rightHandWeapon.position;
            slashEffect.Play();

            yield return new WaitForSeconds(1.5f);
            slashEffect.gameObject.SetActive(false);
            slashEffect.Stop();
        }
    }
    public void DealDamageFromLeftHand()
    {
        DealDamageFrom(leftHandWeapon.position, 1f);
    }

    public void DealDamageFromRightHand()
    {
        DealDamageFrom(rightHandWeapon.position, 1f);
    }

    public void DealDamageFrom(Vector3 position, float extraRange)
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
