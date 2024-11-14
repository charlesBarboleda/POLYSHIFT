using UnityEngine;

[CreateAssetMenu(menuName = "Skills/BlastforgedGuardianPlus")]
public class BlastforgedGuardianPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in BlastforgedGuardianPlus.");
            return;
        }
        Debug.Log("Applying BlastforgedGuardianPlus skill effect.");
        var golemManager = user.GetComponent<GolemManager>();
        golemManager.IncreaseGolemHealth(50f);
        golemManager.IncreaseGolemDamage(5f);
        golemManager.IncreaseGolemAttackRange(0.5f);
        golemManager.IncreaseGolemMovementSpeed(0.5f);
        golemManager.IncreaseBuffRadius(2f);
    }
}
