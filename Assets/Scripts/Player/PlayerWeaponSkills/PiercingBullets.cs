using UnityEngine;

[CreateAssetMenu(fileName = "PiercingBullets", menuName = "Skills/PiercingBullets")]
public class PiercingBullets : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in PiercingBullets.");
            return;
        }

        var weapon = user.GetComponent<PlayerWeapon>();
        weapon.maxPierceTargets += 3;
        weapon.Damage += 25f;

    }
}
