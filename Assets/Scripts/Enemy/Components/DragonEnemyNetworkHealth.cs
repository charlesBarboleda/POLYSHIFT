using UnityEngine;
using DG.Tweening;
using UnityEditor.Animations;
using Unity.VisualScripting;
using System.Collections;

public class DragonEnemyNetworkHealth : BossEnemyNetworkHealth
{
    public bool Grounded = true;
    bool isMaxAltitude = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth;
            Grounded = true;
            rb.isKinematic = false;
            rb.useGravity = true;
            kinematics.MoveSpeed = 6f;
            animator.SetBool("IsFlying", !Grounded);
        }
    }
    protected override void Update()
    {
        base.Update();
        if (IsServer)
        {
            // Keep the dragon grounded if it's not flying
            if (Grounded)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
            if (isMaxAltitude && !IsDead)
            {
                transform.position = new Vector3(transform.position.x, 7.5f, transform.position.z);
            }
            else if (IsDead)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }
    public override void TakeDamage(float damage, ulong attackerId)
    {
        base.TakeDamage(damage, attackerId);
        if (IsServer)
        {
            if (CurrentHealth.Value <= MaxHealth / 2)
            {
                if (Grounded)
                    FlyUp();
            }
        }
    }

    public override void OnHitAnimation(float prev, float current)
    {
        // Play the hit animation only if the dragon is not flying
        if (Grounded)
        {
            animator.SetTrigger("isHit");
        }
    }

    void FlyUp()
    {
        Grounded = false;
        animator.SetBool("IsFlying", !Grounded);
        animator.SetTrigger("FlyUp");
        Debug.Log($"Root Motion: {animator.hasRootMotion}");
        rb.useGravity = false;
        rb.isKinematic = true;

        transform.DOMoveY(7.5f, 5f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            isMaxAltitude = true;
            kinematics.MoveSpeed = 10f;
        });
    }
}
