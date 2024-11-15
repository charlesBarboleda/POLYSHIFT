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
        user.GetComponent<PlayerNetworkMovement>().MoveSpeed += 0.25f;
        user.GetComponent<PlayerNetworkHealth>().PermanentHealthIncreaseByServerRpc(10f);
        user.GetComponent<PlayerSkills>().PermanentMeleeDamageIncreaseByServerRpc(5f);
        user.GetComponent<PlayerWeapon>().Damage += 2.5f;
    }
}


