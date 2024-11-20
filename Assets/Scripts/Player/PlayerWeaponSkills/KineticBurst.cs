using UnityEngine;

[CreateAssetMenu(fileName = "KineticBurst", menuName = "Skills/KineticBurst")]
public class KineticBurst : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in KineticBurst.");
            return;
        }

        user.GetComponent<PlayerWeapon>().KineticBurst();
    }
}
