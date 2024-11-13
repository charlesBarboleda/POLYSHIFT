using UnityEngine;

[CreateAssetMenu(menuName = "Skills/IronResolve")]
public class IronResolve : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in IronResolve.");
            return;
        }
        Debug.Log("Applying IronResolve skill effect.");
        user.GetComponent<PlayerNetworkHealth>().UnlockIronResolve();
    }
}
