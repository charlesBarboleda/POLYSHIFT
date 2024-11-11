using UnityEngine;

public class PassiveSkill : Skill
{
    public override void ApplySkillEffect(GameObject user)
    {
        Debug.Log("Applying passive skill effect.");
    }
}
