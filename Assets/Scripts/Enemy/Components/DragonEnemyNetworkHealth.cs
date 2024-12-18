using UnityEngine;
using DG.Tweening;
using UnityEditor.Animations;
using Unity.VisualScripting;

public class DragonEnemyNetworkHealth : BossEnemyNetworkHealth
{
    bool Grounded = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth;
            Grounded = true;
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
        // If the current hp is less than half
        if (CurrentHealth.Value >= MaxHealth / 2)
        {
            animator.SetTrigger("isHit");
        }
    }

    void FlyUp()
    {
        Grounded = false;
        rb.useGravity = false;
        rb.isKinematic = true;
        animator.SetTrigger("FlyUp");
        // Set the state machine default state to the fly up state  

        transform.DOMoveY(10, animator.GetAnimatorTransitionInfo(0).duration).SetEase(Ease.InOutQuad);
    }
}
