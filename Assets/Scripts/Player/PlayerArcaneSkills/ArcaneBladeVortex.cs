using UnityEngine;

[CreateAssetMenu(fileName = "ArcaneBladeVortex", menuName = "Skills/ArcaneBladeVortex")]
public class ArcaneBladeVortex : ActiveSkill
{
    public override void ExecuteAttack()
    {
        if (OnCooldown)
        {
            Debug.Log("Arcane Blade Vortex is on cooldown!");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in ArcaneBladeVortexManager.");
            return;
        }
        animator.SetTrigger("isArcaneBladeVortex");
        StartCooldown();
    }
}
