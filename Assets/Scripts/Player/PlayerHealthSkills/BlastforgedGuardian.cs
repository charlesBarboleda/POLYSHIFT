using UnityEngine;

[CreateAssetMenu(menuName = "Skills/BlastforgedGuardian")]
public class BlastforgedGuardian : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in BlastforgedGuardian.");
            return;
        }
        Debug.Log("Applying BlastforgedGuardian skill effect.");
        user.GetComponent<PlayerSkills>().SummonGolemServerRpc("BlastforgedGolem", 100, 50, 8, 8, 2, 0f, 5f);
        user.GetComponent<PlayerNetworkHealth>().PermanentDamageReductionIncreaseByServerRpc(0.1f);
    }
}
