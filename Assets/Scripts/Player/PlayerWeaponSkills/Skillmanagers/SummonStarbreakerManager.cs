using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SummonStarbreakerManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; } = 5f;
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>(1f);
    public float AttackRange { get; set; } = 15f;
    public Animator animator { get; set; }
    GameObject starbreaker;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AttackSpeedMultiplier.OnValueChanged += OnAttackSpeedMultiplierChanged;
    }

    public void ResetSkill()
    {
        Damage = 0;
        KnockbackForce = 0f;
        AttackRange = 0f;
        AttackSpeedMultiplier.Value = 1f;
    }

    private void OnAttackSpeedMultiplierChanged(float newValue)
    {
        if (animator != null)
        {
            animator.SetFloat("AttackSpeedMultiplier", newValue);
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SummonPortalRpc()
    {
        GameObject starbreakerPortal = ObjectPooler.Instance.Spawn("StarbreakerPortal", transform.position, Quaternion.identity);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SummonStarbreakerRpc()
    {
        GameObject auraCastWater = ObjectPooler.Instance.Spawn("AuraCastWater", transform.position, Quaternion.identity);

        StartCoroutine(SummonStarbreakerCoroutine());
    }

    IEnumerator SummonStarbreakerCoroutine()
    {
        if (starbreaker == null)
        {
            starbreaker = ObjectPooler.Instance.Spawn("Starbreaker", transform.position + transform.up * 5f, Quaternion.identity);
            starbreaker.GetComponent<Starbreaker>().SetOwners(gameObject);
        }

        yield return new WaitForSeconds(60f);

        if (starbreaker != null)
        {
            ObjectPooler.Instance.Despawn("Starbreaker", starbreaker);
        }
        GameObject starbreakerExplosion = ObjectPooler.Instance.Spawn("StarbreakerExplosion", starbreaker.transform.position, Quaternion.identity); ObjectPooler.Instance.Despawn("Starbreaker", starbreaker);
    }
}
