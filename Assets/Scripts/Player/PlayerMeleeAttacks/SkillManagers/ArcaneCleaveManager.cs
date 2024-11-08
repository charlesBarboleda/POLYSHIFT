using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ArcaneCleaveManager : NetworkBehaviour, IMeleeSkillManager
{
    [field: SerializeField] public float Damage { get; set; } = 30f;
    [field: SerializeField] public float KnockbackForce { get; set; } = 1f;
    [field: SerializeField] public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; } = 2f;
    Animator animator;
    PlayerMelee playerMelee;

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        playerMelee = GetComponent<PlayerMelee>();
        Damage = 30f;
        KnockbackForce = 1f;
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        AttackRange = 2f;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnArcaneCleaveSpawnServerRpc()
    {
        for (int i = 0; i <= 12; i++)
        {
            GameObject cleave = ObjectPooler.Instance.Spawn("ArcaneCleave", transform.position, transform.rotation);
            cleave.transform.localScale = new Vector3(AttackRange / 5, AttackRange / 5, AttackRange / 5);
            // Ensure each instance has its own unique Damage value
            var cleaveCollision = cleave.GetComponent<ArcaneCleaveCollision>();
            if (cleaveCollision != null)
            {
                cleaveCollision.SetDamage(Damage); // Uncomment and verify this line
            }

            cleave.transform.Rotate(0, i * 30, 0);
            cleave.GetComponent<NetworkObject>().Spawn(); // Ensure cleave is only spawned on the server
        }
    }
    public void DealExpandingDamage()
    {
        playerMelee.DealDamageInExpandingCircle(transform.position, 0, AttackRange * 4, Damage, KnockbackForce, 0.1f, 0.01f);
    }

    void SetAttackSpeedMultiplier(float AttackSpeedMultiplier)
    {
        animator.SetFloat("MeleeAttackSpeedMultiplier", AttackSpeedMultiplier);
    }


}
