using UnityEngine;

[CreateAssetMenu(menuName = "Skills/LifeGuardian")]
public class LifeGuardian : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in LifeGuardian.");
            return;
        }
        Debug.Log("Applying LifeGuardian skill effect.");
        user.GetComponent<PlayerSkills>().SummonGuardianGolemServerRpc("GuardianGolem", 1000, 30, 6, 6, 3, 0.5f);
        user.GetComponent<PlayerNetworkHealth>().PermanentDamageReductionIncreaseBy(0.1f);
    }
}
