using UnityEngine;

[CreateAssetMenu(fileName = "MimicSentry", menuName = "Skills/MimicSentry")]
public class MimicSentry : ActiveSkill
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
            Debug.LogError("Animator is null in MimicSentry.");
            return;
        }
        animator.SetTrigger("isMimicSentry");
        StartCooldown();
    }


}
