using UnityEngine;

[CreateAssetMenu(fileName = "DualStancePlus", menuName = "Skills/DualStancePlus")]
public class DualStancePlus : PassiveSkill
{

    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in DualStancePlus.");
            return;
        }

        user.GetComponent<PlayerWeapon>().DualStancePlus();
    }
}
