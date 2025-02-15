using UnityEngine;

[CreateAssetMenu(fileName = "DevilSlam", menuName = "Skills/DevilSlam")]
public class DevilSlam : ActiveSkill, IUltimateSkill
{
    public override void Initialize(Animator animator)
    {
        base.Initialize(animator);
        Cooldown = 300f;
    }
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("Devil Slam is on cooldown!");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in DevilSlam.");
            return;
        }
        animator.SetTrigger("isDevilSlam");
        StartCooldown();
    }
}
