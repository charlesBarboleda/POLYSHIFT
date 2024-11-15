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
        user.GetComponent<PlayerSkills>().BlastforgedGuardianPlus();
    }
}
