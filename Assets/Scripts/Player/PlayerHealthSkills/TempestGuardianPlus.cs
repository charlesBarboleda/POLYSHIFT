using UnityEngine;

[CreateAssetMenu(fileName = "TempestGuardianPlus", menuName = "Skills/TempestGuardianPlus")]
public class TempestGuardianPlus : PassiveSkill
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
        golemManager.IncreaseBuffRadius(1f);
    }
}
