using UnityEngine;

[CreateAssetMenu(fileName = "ArcanePlague", menuName = "Skills/ArcanePlague", order = 1)]
public class ArcanePlague : PassiveSkill
{
    [SerializeField] ArcanePoison arcanePoison;
    public override void ApplySkillEffect(GameObject user)
    {
        if (user == null)
        {
            Debug.LogError("User is null in ArcanePlague.");
            return;
        }
        Debug.Log("Applying ArcanePlague skill effect.");
        ArcanePoison debuff = Instantiate(arcanePoison);
        user.GetComponent<PlayerWeapon>().AddWeaponDebuff(debuff);
    }
}
