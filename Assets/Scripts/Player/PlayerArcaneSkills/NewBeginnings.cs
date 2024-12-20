using UnityEngine;

[CreateAssetMenu(fileName = "NewBeginnings", menuName = "Skills/NewBeginnings")]
public class NewBeginnings : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in NewBeginnings.");
            return;
        }
        Debug.Log("Applying NewBeginnings skill effect.");
        user.GetComponent<PlayerNetworkMovement>().MoveSpeed += 0.5f;
        user.GetComponent<PlayerNetworkHealth>().PermanentHealthIncreaseByRpc(20f);
        user.GetComponent<PlayerSkills>().PermanentMeleeDamageIncreaseBy(10f);
        user.GetComponent<PlayerWeapon>().Damage += 3f;
    }
}


