using Unity.Netcode;
using UnityEngine;

public class BombardierSentryManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; }
    public float AttackRange { get; set; }
    public Animator animator { get; set; }
}
