using UnityEngine;

[CreateAssetMenu(fileName = "SummonStarbreaker", menuName = "Skills/SummonStarbreaker")]
public class SummonStarbreaker : ActiveSkill
{
    public override void ExecuteAttack()
    {
        if (animator == null) return;

        animator.SetTrigger("isSummonStarbreaker");
        StartCooldown();
    }
}
