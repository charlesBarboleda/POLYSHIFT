using UnityEngine;

[CreateAssetMenu(menuName = "Skills/VitalityPlus")]
public class VitalityPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in VitalityPlus.");
            return;
        }
        Debug.Log("Applying VitalityPlus skill effect.");
        user.GetComponent<PlayerNetworkHealth>().PermanentHealthIncreaseBy(5f);
    }
}
