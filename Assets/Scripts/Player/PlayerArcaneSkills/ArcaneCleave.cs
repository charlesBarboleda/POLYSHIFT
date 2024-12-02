using UnityEngine;

[CreateAssetMenu(fileName = "ArcaneCleave", menuName = "Skills/ArcaneCleave", order = 1)]
public class ArcaneCleave : ActiveSkill
{
    public override void Initialize(Animator animator)
    {
        base.Initialize(animator);
        Cooldown = 12f;
    }
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
