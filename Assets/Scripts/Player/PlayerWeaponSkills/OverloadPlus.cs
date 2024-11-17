using UnityEngine;

[CreateAssetMenu(fileName = "OverloadPlus", menuName = "Skills/OverloadPlus")]
public class OverloadPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in OverloadPlus.");
            return;
        }

        user.GetComponent<PlayerWeapon>().OverloadPlus();
    }
}
