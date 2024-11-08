using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ArcaneBarrierManager : NetworkBehaviour, IMeleeSkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public float AttackSpeedMultiplier { get; set; }
    public float AttackRange { get; set; }
    public float Duration { get; set; }
    GameObject ArcaneBarrier;
    PlayerMelee playerMelee;
    PlayerNetworkHealth playerNetworkHealth;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Damage = 0f;
        KnockbackForce = 10f;
        AttackSpeedMultiplier = 1f;
        AttackRange = 2f;
        playerMelee = GetComponent<PlayerMelee>();
        playerNetworkHealth = GetComponent<PlayerNetworkHealth>();
    }

    void FixedUpdate()
    {
        if (ArcaneBarrier != null)
        {
            ArcaneBarrier.transform.position = transform.position;
            ArcaneBarrier.transform.rotation = Quaternion.Euler(-90, 0, 90);

        }
    }

    [ServerRpc]
    public void ArcaneBarrierSpawnServerRpc()
    {
        ArcaneBarrier = ObjectPooler.Instance.Spawn("ArcaneDome", transform.position, transform.rotation);
        DestroyArcaneBarrier();
        GameObject ArcaneEnchant = ObjectPooler.Instance.Spawn("ArcaneEnchant", transform.position, transform.rotation);
        GameObject ArcaneMuzzle = ObjectPooler.Instance.Spawn("ArcaneMuzzle", transform.position, transform.rotation);
        ArcaneEnchant.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        ArcaneMuzzle.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        ArcaneBarrier.transform.localScale = new Vector3(AttackRange / 2, AttackRange / 2, AttackRange / 2);
        playerNetworkHealth.ReduceDamageTakenBy(0.5f, 60f);
        ArcaneBarrier.GetComponent<NetworkObject>().Spawn();
        ArcaneEnchant.GetComponent<NetworkObject>().Spawn();
        ArcaneMuzzle.GetComponent<NetworkObject>().Spawn();
    }

    IEnumerator DestroyArcaneBarrier()
    {
        yield return new WaitForSeconds(Duration);
        ArcaneBarrier.GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn("ArcaneDome", ArcaneBarrier);
    }


    public void DealDamageInCircle()
    {
        playerMelee.DealDamageInCircle(transform.position, AttackRange, Damage, KnockbackForce);
    }
}
