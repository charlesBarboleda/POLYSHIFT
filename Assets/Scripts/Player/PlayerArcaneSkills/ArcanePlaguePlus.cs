using UnityEngine;

[CreateAssetMenu(fileName = "ArcanePlaguePlus", menuName = "Skills/ArcanePlaguePlus")]
public class ArcanePlaguePlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in ArcanePlaguePlus.");
            return;
        }
        Debug.Log("Applying ArcanePlaguePlus skill effect.");
        user.GetComponent<PlayerSkills>().ArcanePlaguePlus();
    }
}

