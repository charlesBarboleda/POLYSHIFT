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
    CameraController cameraController;
    GameObject Devil;
    GameObject DevilPortal;
    GameObject DevilPortal2;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Devil = null;
        DevilPortal = null;
        DevilPortal2 = null;
        ResetSkill();
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        cameraController = GetComponent<CameraController>();
        animator = GetComponent<Animator>();
    }

    public void ResetSkill()
    {
        Damage = 10000f;
        KnockbackForce = 10f;
        AttackSpeedMultiplier.Value = 1f;
        AttackRange = 5f;
    }

    [Rpc(SendTo.ClientsAndHost)]
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


        Devil = ObjectPooler.Instance.Spawn("Devil", DevilPortal.transform.position + Vector3.down * 20f, transform.rotation);
        Devil.GetComponent<Animator>().SetFloat("AttackSpeedMultiplier", AttackSpeedMultiplier.Value);
        Devil.GetComponent<DevilManager>().SetPlayer(transform, Damage);

        DevilPortal.transform.localScale = Vector3.zero;
        DevilPortal2.transform.localScale = Vector3.zero;

        DevilPortal.transform.DOScale(Vector3.one, 3f);
        DevilPortal2.transform.DOScale(Vector3.one, 3f);

        Devil.transform.DOMove(DevilPortal.transform.position + Vector3.down * 13f, 1f);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DisablePortalsServerRpc()
    {
        if (DevilPortal != null) DisablePortal();
        if (DevilPortal2 != null) StartCoroutine(DisablePortal2());
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CameraShakeClientRpc()
    {
        cameraController.TriggerShake(10f, 1f);
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void DisableDevilServerRpc()
    {
        ObjectPooler.Instance.Despawn("Devil", Devil);
    }

    IEnumerator DisablePortal2()
    {
        DevilPortal2.transform.DOScale(Vector3.zero, 2f);
        yield return new WaitForSeconds(2.5f);
        ObjectPooler.Instance.Despawn("DevilPortal2", DevilPortal2);
    }

    void DisablePortal()
    {
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
