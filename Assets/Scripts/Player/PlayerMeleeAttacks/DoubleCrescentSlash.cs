using UnityEngine;

public class DoubleCrescentSlash : MeleeAttack
{
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("Double Crescent Slash is on cooldown!");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator not initialized in DoubleCrescentSlash.");
            return;
        }

        animator.SetTrigger("isDoubleCrescentSlash");
        StartCooldown();
    }
}
