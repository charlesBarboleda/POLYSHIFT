using UnityEngine;

[CreateAssetMenu(fileName = "ArcaneBarrier", menuName = "Skills/ArcaneBarrier", order = 1)]
public class ArcaneBarrier : ActiveSkill
{
    public override void Initialize(Animator animator)
    {
        base.Initialize(animator);
        Cooldown = 180f;
    }
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
