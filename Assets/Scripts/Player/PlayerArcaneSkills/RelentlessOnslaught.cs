using UnityEngine;

[CreateAssetMenu(fileName = "RelentlessOnslaught", menuName = "Skills/RelentlessOnslaught")]
public class RelentlessOnslaught : ActiveSkill
{
    public override void Initialize(Animator animator)
    {
        base.Initialize(animator);
        Cooldown = 240f;
    }
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("Relentless Onslaught is on cooldown!");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in RelentlessOnslaught.");
            return;
        }
        animator.SetTrigger("isRelentlessOnslaught");
        StartCooldown();
    }
}

