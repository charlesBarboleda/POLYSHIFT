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
        user.GetComponent<PlayerNetworkMovement>().MoveSpeed += 1.5f;
        user.GetComponent<PlayerSkills>().PermanentMeleeRangeIncreaseBy(3f);
    }
}
