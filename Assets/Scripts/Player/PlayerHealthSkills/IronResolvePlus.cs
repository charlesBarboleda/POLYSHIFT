using UnityEngine;

[CreateAssetMenu(menuName = "Skills/IronResolvePlus")]
public class IronResolvePlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in IronResolvePlus.");
            return;
        }
        Debug.Log("Applying IronResolvePlus skill effect.");
        user.GetComponent<PlayerSkills>().IronResolvePlus();
    }
}
