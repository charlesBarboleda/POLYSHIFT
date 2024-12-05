using UnityEngine;

[CreateAssetMenu(fileName = "BondOfTheColossus", menuName = "Skills/BondOfTheColossus")]
public class BondOfTheColossus : ActiveSkill, IUltimateSkill
{
    public override void ExecuteAttack()
    {
        if (animator == null)
        {
            Debug.Log("Animator is null in BondOfTheColossus");
            return;
        }

        animator.SetTrigger("isBondOfTheColossus");
        StartCooldown();
    }
}
