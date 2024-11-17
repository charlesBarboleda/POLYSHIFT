using UnityEngine;

[CreateAssetMenu(fileName = "Overload", menuName = "Skills/Overload")]
public class Overload : ActiveSkill
{
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
