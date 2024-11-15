using UnityEngine;

[CreateAssetMenu(menuName = "Skills/BloodBondPlus")]
public class BloodBondPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in BloodBondPlus.");
            return;
        }
        Debug.Log("Applying BloodBondPlus skill effect.");
        user.GetComponent<PlayerSkills>().BloodBondPlus();
    }
}
