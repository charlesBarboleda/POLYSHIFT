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

        user.GetComponent<PlayerSkills>().LifeGuardianPlus();
    }
}
