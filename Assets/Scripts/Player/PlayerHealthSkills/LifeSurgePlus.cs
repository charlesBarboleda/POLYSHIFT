using UnityEngine;

[CreateAssetMenu(fileName = "LifeSurgePlus", menuName = "Skills/LifeSurgePlus")]
public class LifeSurgePlus : PassiveSkill
{

    public override void ApplySkillEffect(GameObject user)
    {
        PlayerNetworkHealth playerHealth = user.GetComponent<PlayerNetworkHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerNetworkHealth not found in LifeSurgePlus.");
            return;
        }
        LifeSurgeManager lifeSurge = user.GetComponent<LifeSurgeManager>();
        if (lifeSurge == null)
        {
            Debug.LogError("LifeSurgeManager not found in LifeSurgePlus.");
            return;
        }
        lifeSurge.IncreaseHealRadius(2f);
        lifeSurge.IncreaseHealStrength(0.05f);
        playerHealth.PermanentHealthIncreaseBy(5f);
    }
}
