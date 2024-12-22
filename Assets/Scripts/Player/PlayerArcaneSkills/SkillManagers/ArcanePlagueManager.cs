using Unity.Netcode;
using UnityEngine;

public class ArcanePlagueManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; }
    public Animator animator { get; set; }

    public override void OnNetworkSpawn()
    {
        Damage = 20f;
    }

    public void ResetSkill()
    {
        Damage = 20f;
    }

}
