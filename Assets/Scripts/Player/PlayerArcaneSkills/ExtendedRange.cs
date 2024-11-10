using UnityEngine;

[CreateAssetMenu(menuName = "Skills/ExtendedRange")]
public class ExtendedRange : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in ExtendedRange.");
            return;
        }
        Debug.Log("Applying ExtendedRange skill effect.");
        user.GetComponent<PlayerSkills>().PermanentMeleeRangeIncreaseBy(1f);
    }
}
