using UnityEngine;

[CreateAssetMenu(fileName = "MeleeDamage", menuName = "Skills/MeleeDamage")]
public class MeleeDamage : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in MeleeDamage.");
            return;
        }
        Debug.Log("Applying MeleeDamage skill effect.");
        user.GetComponent<PlayerSkills>().PermanentMeleeDamageIncreaseByServerRpc(4f);
    }
}
