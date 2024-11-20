using UnityEngine;

[CreateAssetMenu(fileName = "WeaponMastery", menuName = "Skills/WeaponMastery")]
public class WeaponMastery : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in WeaponMastery.");
            return;
        }
        var weapon = user.GetComponent<PlayerWeapon>();
        weapon.Damage += 12;
        weapon.DecreaseReloadTimeByServerRpc(0.3f);
        weapon.DecreaseFireRateByServerRpc(0.3f);
    }
}
