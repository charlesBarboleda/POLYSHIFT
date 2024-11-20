using UnityEngine;

[CreateAssetMenu(fileName = "BombardierSentryPlus", menuName = "Skills/BombardierSentryPlus")]
public class BombardierSentryPlus : PassiveSkill
{
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in BombardierSentryPlus.");
            return;
        }

        user.GetComponent<PlayerWeapon>().BombardierSentryPlus();
    }
}
