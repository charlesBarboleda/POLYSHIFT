using UnityEngine;

public class ArcaneBarrier : MeleeAttack
{
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("ArcaneBarrier is on cooldown!");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in ArcaneBarrier.");
            return;
        }
        animator.SetTrigger("isArcaneBarrier");
        StartCooldown();
    }
}
