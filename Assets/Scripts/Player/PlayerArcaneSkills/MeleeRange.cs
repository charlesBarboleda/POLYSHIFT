using UnityEngine;

[CreateAssetMenu(fileName = "MeleeRange", menuName = "Skills/MeleeRange")]
public class MeleeRange : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in MeleeRange.");
            return;
        }
        Debug.Log("Applying MeleeRange skill effect.");
        user.GetComponent<PlayerSkills>().PermanentMeleeRangeIncreaseBy(0.1f);
    }
}
