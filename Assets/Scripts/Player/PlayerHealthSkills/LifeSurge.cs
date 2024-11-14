using UnityEngine;

public class LifeSurge : ActiveSkill
{
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("Life Surge is on cooldown!");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator not initialized in Life Surge.");
            return;
        }
        animator.SetTrigger("isLifeSurge");
        StartCooldown();
    }
}
