using UnityEngine;

[CreateAssetMenu(fileName = "ExtendedCapacityPlus", menuName = "Skills/ExtendedCapacityPlus")]
public class ExtendedCapacityPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in ExtendedCapacityPlus.");
            return;
        }

        user.GetComponent<PlayerWeapon>().ExtendedCapacityPlus();
    }
}
