using UnityEngine;

[CreateAssetMenu(fileName = "BladeVortexPlus", menuName = "Skills/BladeVortexPlus")]
public class BladeVortexPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in BladeVortexPlus.");
            return;
        }
        Debug.Log("Applying BladeVortexPlus skill effect.");
        user.GetComponent<PlayerSkills>().BladeVortexPlus();
    }
}
