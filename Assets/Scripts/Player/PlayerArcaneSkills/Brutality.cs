using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Brutality")]
public class Brutality : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in Brutality.");
            return;
        }
        Debug.Log("Applying Brutality skill effect.");
        user.GetComponent<PlayerNetworkMovement>().MoveSpeed += 1.5f;

        user.GetComponent<PlayerSkills>().PermanentMeleeDamageIncreaseBy(250f);
    }
}
