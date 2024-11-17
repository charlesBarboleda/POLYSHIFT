using UnityEngine;

[CreateAssetMenu(fileName = "PiercingBulletsPlus", menuName = "Skills/PiercingBulletsPlus")]
public class PiercingBulletsPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in PiercingBulletsPlus.");
            return;
        }

        user.GetComponent<PlayerWeapon>().PiercingBulletsPlus();
    }
}
