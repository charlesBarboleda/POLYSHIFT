using UnityEngine;

public class RelentlessOnslaught : MeleeAttack
{
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("Relentless Onslaught is on cooldown!");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in RelentlessOnslaught.");
            return;
        }
        animator.SetTrigger("isRelentlessOnslaught");
        StartCooldown();
    }
}

