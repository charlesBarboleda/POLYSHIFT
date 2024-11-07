using UnityEngine;

public class DevilSlam : MeleeAttack
{
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("Devil Slam is on cooldown!");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in DevilSlam.");
            return;
        }
        animator.SetTrigger("isDevilSlam");
        StartCooldown();
    }
}
