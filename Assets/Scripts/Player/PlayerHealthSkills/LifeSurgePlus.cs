using UnityEngine;

[CreateAssetMenu(fileName = "LifeSurgePlus", menuName = "Skills/LifeSurgePlus")]
public class LifeSurgePlus : PassiveSkill
{

    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in LifeSurgePlus.");
            return;
        }
        Debug.Log("Applying LifeSurgePlus skill effect.");
        user.GetComponent<PlayerSkills>().LifeSurgePlus();


    }
}
