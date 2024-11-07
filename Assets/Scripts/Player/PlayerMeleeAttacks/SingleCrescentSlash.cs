using UnityEngine;

public class SingleCrescentSlash : MeleeAttack
{
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("Single Crescent Slash is on cooldown!");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator not initialized in SingleCrescentSlash.");
            return;
        }


        animator.SetTrigger("isSingleCrescentSlash");
        StartCooldown(); // Begin the cooldown
    }
}
