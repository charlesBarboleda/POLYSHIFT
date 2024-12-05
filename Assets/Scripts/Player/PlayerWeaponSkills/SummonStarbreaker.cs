using UnityEngine;

[CreateAssetMenu(fileName = "SummonStarbreaker", menuName = "Skills/SummonStarbreaker")]
public class SummonStarbreaker : ActiveSkill, IUltimateSkill
{
    public override void Initialize(Animator animator)
    {
        base.Initialize(animator);
        Cooldown = 300f;
    }
    public override void ExecuteAttack()
    {
        if (animator == null) return;

        animator.SetTrigger("isSummonStarbreaker");
        StartCooldown();
    }
}
