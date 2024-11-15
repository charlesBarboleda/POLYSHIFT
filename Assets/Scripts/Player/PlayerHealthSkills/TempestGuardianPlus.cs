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
        user.GetComponent<PlayerSkills>().TempestGuardianPlus();
    }
}
