using Unity.Netcode;
using UnityEngine;

public class BombardierSentryManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; }
    public float AttackRange { get; set; }
    public Animator animator { get; set; }

    public void ResetSkill()
    {
        Damage = 0;
        KnockbackForce = 0;
        AttackRange = 0;
        AttackSpeedMultiplier.Value = 1f;
    }

}
