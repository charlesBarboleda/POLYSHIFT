using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Vitality")]
public class Vitality : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in Vitality.");
            return;
        }
        Debug.Log("Applying Vitality skill effect.");
        user.GetComponent<PlayerNetworkHealth>().PermanentHealthIncreaseByServerRpc(25f);
    }
}
