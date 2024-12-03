using UnityEngine;

[CreateAssetMenu(fileName = "Overload", menuName = "Skills/Overload")]
public class Overload : ActiveSkill
{
    public override void Initialize(Animator animator)
    {
        base.Initialize(animator);
        Cooldown = 180f;
    }
    public override void ExecuteAttack()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is null in Overload.");
            return;
        }
        animator.SetTrigger("isOverload");
        StartCooldown();
    }
}
