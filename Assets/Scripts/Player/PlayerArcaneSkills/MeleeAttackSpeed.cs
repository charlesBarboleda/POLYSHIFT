using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackSpeed", menuName = "Skills/MeleeAttackSpeed")]
public class MeleeAttackSpeed : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in MeleeAttackSpeed.");
            return;
        }
        Debug.Log("Applying MeleeAttackSpeed skill effect.");
        user.GetComponent<PlayerSkills>().PermanentAttackSpeedIncreaseByServerRpc(0.5f);
    }
}

