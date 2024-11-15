using UnityEngine;

[CreateAssetMenu(menuName = "Skills/RegenerativeAura")]
public class RegenerativeAura : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in RegenerativeAura.");
            return;
        }
        Debug.Log("Applying RegenerativeAura skill effect.");
        user.GetComponent<PlayerNetworkHealth>().PermanentHealthRegenIncreaseByServerRpc(5f);
    }
}
