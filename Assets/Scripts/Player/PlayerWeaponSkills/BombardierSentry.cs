using UnityEngine;

[CreateAssetMenu(fileName = "BombardierSentry", menuName = "Skills/BombardierSentry", order = 1)]
public class BombardierSentry : ActiveSkill
{
    public override void ExecuteAttack()
    {
        if (animator == null) return;

        animator.SetTrigger("isBombardierSentry");
        StartCooldown();
    }
}
