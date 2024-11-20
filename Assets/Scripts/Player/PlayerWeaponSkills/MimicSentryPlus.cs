using UnityEngine;

[CreateAssetMenu(fileName = "MimicSentryPlus", menuName = "Skills/MimicSentryPlus")]
public class MimicSentryPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in MimicSentryPlus.");
            return;
        }

        user.GetComponent<PlayerWeapon>().MimicSentryPlus();
    }
}
