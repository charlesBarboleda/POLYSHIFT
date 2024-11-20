using UnityEngine;

[CreateAssetMenu(fileName = "DualStance", menuName = "Skills/DualStance")]
public class DualStance : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in DualStance.");
            return;
        }

        user.GetComponent<PlayerWeapon>().DualStance();
    }
}
