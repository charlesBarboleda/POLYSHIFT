using UnityEngine;

[CreateAssetMenu(fileName = "TempestGuardian", menuName = "Skills/TempestGuardian")]
public class TempestGuardian : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        PlayerNetworkHealth playerHealth = user.GetComponent<PlayerNetworkHealth>();
        PlayerNetworkMovement playerMovement = user.GetComponent<PlayerNetworkMovement>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerNetworkHealth not found in TempestGuardian.");
            return;
        }
        playerHealth.PermanentDamageReductionIncreaseByServerRpc(0.1f);
        playerMovement.MoveSpeedIncreaseBy(1f);
        user.GetComponent<PlayerSkills>().SummonGolemServerRpc("TempestGolem", 250, 50, 3, 10, 1, 0f, 30f);
    }
}
