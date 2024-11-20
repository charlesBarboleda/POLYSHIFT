using UnityEngine;

[CreateAssetMenu(fileName = "MimicSentry", menuName = "Skills/MimicSentry")]
public class MimicSentry : ActiveSkill
{
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
