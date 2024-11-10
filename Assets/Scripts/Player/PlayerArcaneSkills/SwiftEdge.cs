using UnityEngine;

[CreateAssetMenu(fileName = "SwiftEdge", menuName = "Skills/SwiftEdge")]
public class SwiftEdge : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in SwiftEdge.");
            return;
        }
        Debug.Log("Applying SwiftEdge skill effect.");
        user.GetComponent<PlayerNetworkMovement>().MoveSpeed += 0.25f;
        user.GetComponent<PlayerSkills>().PermanentAttackSpeedIncreaseBy(1);
    }
}
