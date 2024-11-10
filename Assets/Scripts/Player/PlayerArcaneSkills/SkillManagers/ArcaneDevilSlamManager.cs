using System.Collections;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class ArcaneDevilSlamManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; } = 100f;
    public float KnockbackForce { get; set; } = 1f;
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; } = 3f;

    public Animator animator { get; set; }

    GameObject Devil;
    GameObject DevilPortal;
    GameObject DevilPortal2;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Devil = null;
        DevilPortal = null;
        DevilPortal2 = null;
        Damage = 100f;
        KnockbackForce = 5f;
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        AttackRange = 3f;
        animator = GetComponent<Animator>();
    }

    [ServerRpc]
    public void OnArcaneDevilSlamSpawnServerRpc()
    {
        // Spawn portal and devil on the server
        SpawnPortalAndDevil();
    }

    void SpawnPortalAndDevil()
    {
        // Server-only spawning of the portals and devil
        DevilPortal = ObjectPooler.Instance.Spawn("DevilPortal", transform.position + -transform.forward * 12f, transform.rotation);
        DevilPortal2 = ObjectPooler.Instance.Spawn("DevilPortal2", transform.position + -transform.forward * 12f, transform.rotation);
        DevilPortal.GetComponent<NetworkObject>().Spawn(); // Make sure portal is only spawned on the server
        DevilPortal2.GetComponent<NetworkObject>().Spawn(); // Make sure portal is only spawned on the server

        Devil = ObjectPooler.Instance.Spawn("Devil", DevilPortal.transform.position + Vector3.down * 20f, transform.rotation);
        Devil.GetComponent<Animator>().SetFloat("AttackSpeedMultiplier", AttackSpeedMultiplier.Value);
        Devil.GetComponent<NetworkObject>().Spawn(); // Make sure Devil is only spawned on the server
        Devil.GetComponent<DevilManager>().SetPlayer(transform, Damage);

        DevilPortal.transform.localScale = Vector3.zero;
        DevilPortal2.transform.localScale = Vector3.zero;

        DevilPortal.transform.DOScale(Vector3.one, 3f);
        DevilPortal2.transform.DOScale(Vector3.one, 3f);

        Devil.transform.DOMove(DevilPortal.transform.position + Vector3.down * 13f, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisablePortalsServerRpc()
    {
        // Notify clients to disable portals
        DisablePortalsClientRpc();
    }

    [ClientRpc]
    void DisablePortalsClientRpc()
    {
        // Client-side portal disable logic
        if (DevilPortal != null) DisablePortal();
        if (DevilPortal2 != null) StartCoroutine(DisablePortal2());
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisableDevilServerRpc()
    {

        DisableDevilClientRpc();
    }

    [ClientRpc]
    void DisableDevilClientRpc()
    {
        ObjectPooler.Instance.Despawn("Devil", Devil);
        if (IsServer)
        {
            Devil.GetComponent<NetworkObject>().Despawn(false);
        }
    }

    IEnumerator DisablePortal2()
    {
        DevilPortal2.transform.DOScale(Vector3.zero, 2f);
        yield return new WaitForSeconds(2.5f);
        DevilPortal2.GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn("DevilPortal2", DevilPortal2);
    }

    void DisablePortal()
    {
        DevilPortal.GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn("DevilPortal", DevilPortal);
    }

    void SetAttackSpeedMultiplier(float AttackSpeedMultiplier)
    {
        if (Devil != null)
        {
            Devil.GetComponent<Animator>().SetFloat("AttackSpeedMultiplier", AttackSpeedMultiplier);
        }
        animator.SetFloat("AttackSpeedMultiplier", AttackSpeedMultiplier);
    }
}
