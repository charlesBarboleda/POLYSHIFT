using UnityEngine;

[CreateAssetMenu(fileName = "DoubleCrescentSlashPlus", menuName = "Skills/DoubleCrescentSlashPlus")]
public class DoubleCrescentSlashPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in DoubleCrescentSlashPlus.");
            return;
        }
        Debug.Log("Applying DoubleCrescentSlashPlus skill effect.");
        user.GetComponent<PlayerSkills>().DoubleCrescentSlashPlus();
    }
}
