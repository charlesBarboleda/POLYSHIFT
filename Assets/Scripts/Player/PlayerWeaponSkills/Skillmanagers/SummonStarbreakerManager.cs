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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AttackSpeedMultiplier.OnValueChanged += OnAttackSpeedMultiplierChanged;
    }

    private void OnAttackSpeedMultiplierChanged(float newValue)
    {
        if (animator != null)
        {
            animator.SetFloat("AttackSpeedMultiplier", newValue);
        }
    }
    [ServerRpc]
    public void SummonPortalServerRpc()
    {
        GameObject starbreakerPortal = ObjectPooler.Instance.Spawn("StarbreakerPortal", transform.position, Quaternion.identity);
        starbreakerPortal.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        starbreakerPortal.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    void SummonStarbreakerServerRpc()
    {
        GameObject auraCastWater = ObjectPooler.Instance.Spawn("AuraCastWater", transform.position, Quaternion.identity);
        auraCastWater.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        auraCastWater.GetComponent<NetworkObject>().Spawn();

        StartCoroutine(SummonStarbreakerCoroutine());
    }

    IEnumerator SummonStarbreakerCoroutine()
    {
        GameObject starbreaker = ObjectPooler.Instance.Spawn("Starbreaker", transform.position + transform.up * 7.5f, Quaternion.identity);
        starbreaker.GetComponent<Starbreaker>().SetOwners(gameObject);
        starbreaker.GetComponent<NetworkObject>().Spawn();

        yield return new WaitForSeconds(60f);

        GameObject starbreakerExplosion = ObjectPooler.Instance.Spawn("StarbreakerExplosion", starbreaker.transform.position, Quaternion.identity);
        ObjectPooler.Instance.Despawn("Starbreaker", starbreaker);
        starbreaker.GetComponent<NetworkObject>().Despawn(false);
    }
}
