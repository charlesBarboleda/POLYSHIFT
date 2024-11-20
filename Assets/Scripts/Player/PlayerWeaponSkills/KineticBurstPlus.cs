using UnityEngine;

[CreateAssetMenu(fileName = "KineticBurstPlus", menuName = "Skills/KineticBurstPlus")]
public class KineticBurstPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in KineticBurstPlus.");
            return;
        }

        user.GetComponent<PlayerWeapon>().KineticBurstPlus();
    }
}
