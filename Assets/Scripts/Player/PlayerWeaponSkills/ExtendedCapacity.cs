using UnityEngine;

[CreateAssetMenu(fileName = "ExtendedCapacity", menuName = "Skills/ExtendedCapacity")]
public class ExtendedCapacity : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in ExtendedCapacity.");
            return;
        }

        var weapon = user.GetComponent<PlayerWeapon>();
        weapon.maxAmmoCount.Value += 10;
        weapon.Damage += 10f;
        weapon.DecreaseFireRateByServerRpc(0.2f);
    }
}
