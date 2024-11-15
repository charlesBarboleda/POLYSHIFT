using UnityEngine;

[CreateAssetMenu(menuName = "Skills/RegenerativeAuraPlus")]
public class RegenerativeAuraPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in RegenerativeAuraPlus.");
            return;
        }
        Debug.Log("Applying RegenerativeAuraPlus skill effect.");
        user.GetComponent<PlayerSkills>().RegenerativeAuraPlus();
    }
}
