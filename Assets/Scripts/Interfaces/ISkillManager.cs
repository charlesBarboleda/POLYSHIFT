
using UnityEngine;

public interface ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; }
    public float AttackRange { get; set; }
    Animator animator { get; set; }

}
