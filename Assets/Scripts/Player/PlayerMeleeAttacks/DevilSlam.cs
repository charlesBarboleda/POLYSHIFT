using UnityEngine;

public class DevilSlam : MeleeAttack
{
    public override void ExecuteAttack()
    {
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in DevilSlam.");
            return;
        }

        animator.SetTrigger("isDevilSlam");
    }
}
