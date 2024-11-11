using UnityEngine;

[CreateAssetMenu(fileName = "ArcaneCleavePlus", menuName = "Skills/ArcaneCleavePlus")]
public class ArcaneCleavePlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in ArcaneCleavePlus.");
            return;
        }
        Debug.Log("Applying ArcaneCleavePlus skill effect.");
        user.GetComponent<PlayerSkills>().ArcaneCleavePlus();
    }
}
