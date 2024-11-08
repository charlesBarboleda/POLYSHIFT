using UnityEngine;

public class ArcaneCleave : MeleeAttack
{
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("ArcaneCleave is on cooldown!");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in ArcaneCleave.");
            return;
        }
        animator.SetTrigger("isArcaneCleave");
        StartCooldown();
    }
}
