using UnityEngine;

[CreateAssetMenu(fileName = "RelentlessOnslaughtPlus", menuName = "Skills/RelentlessOnslaughtPlus")]
public class RelentlessOnslaughtPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in RelentlessOnslaughtPlus.");
            return;
        }
        Debug.Log("Applying RelentlessOnslaughtPlus skill effect.");
        user.GetComponent<PlayerSkills>().RelentlessOnslaughtPlus();
    }
}
