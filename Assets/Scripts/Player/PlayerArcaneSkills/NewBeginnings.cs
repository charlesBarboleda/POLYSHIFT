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
        user.GetComponent<PlayerNetworkHealth>().currentHealth.Value += 10f;
        user.GetComponent<PlayerNetworkMovement>().MoveSpeed += 0.25f;
        user.GetComponent<PlayerSkills>().PermanentMeleeDamageIncreaseBy(5f);
        user.GetComponent<PlayerWeapon>().PermanentWeaponDamageIncreaseBy(2.5f);
    }
}


