using UnityEngine;

[CreateAssetMenu(fileName = "WeaponMasteryPlus", menuName = "Skills/WeaponMasteryPlus")]
public class WeaponMasteryPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in WeaponMasteryPlus.");
            return;
        }
        user.GetComponent<PlayerWeapon>().WeaponMasteryPlus();
    }
}
