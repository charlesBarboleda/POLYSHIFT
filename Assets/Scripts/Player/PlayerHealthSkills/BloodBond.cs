using UnityEngine;

public class BloodBond : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in BloodBond.");
            return;
        }
        Debug.Log("Applying BloodBond skill effect.");
        user.GetComponent<PlayerSkills>().ActivateBloodBond();
    }
}
