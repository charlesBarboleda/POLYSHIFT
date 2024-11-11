using UnityEngine;

[CreateAssetMenu(fileName = "ArcaneBarrierPlus", menuName = "Skills/ArcaneBarrierPlus")]
public class ArcaneBarrierPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in ArcaneBarrierPlus.");
            return;
        }
        Debug.Log("Applying ArcaneBarrierPlus skill effect.");
        user.GetComponent<PlayerSkills>().ArcaneBarrierPlus();
    }
}
