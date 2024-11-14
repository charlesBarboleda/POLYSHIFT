using UnityEngine;

[CreateAssetMenu(menuName = "Skills/LifeGuardianPlus")]
public class LifeGuardianPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in LifeGuardianPlus.");
            return;
        }
        Debug.Log("Applying LifeGuardianPlus skill effect.");
        var golemManager = user.GetComponent<GolemManager>();
        golemManager.IncreaseGolemDamageReduction(0.025f);
        golemManager.IncreaseGolemHealth(50f);
        golemManager.IncreaseGolemDamage(5f);
        golemManager.IncreaseGolemAttackRange(0.5f);
        golemManager.IncreaseGolemMovementSpeed(0.5f);
    }
}
