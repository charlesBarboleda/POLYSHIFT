using System.Collections;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

public class ArcaneBladeVortexManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; }
    public Animator animator { get; set; }
    public float Duration;
    PlayerSkills PlayerSkills;

    GameObject BladeVortex;

    public override void OnNetworkSpawn()
    {
        Damage = 5f;
        Duration = 30f;
        KnockbackForce = 1f;
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        AttackRange = 1f;
        PlayerSkills = GetComponent<PlayerSkills>();
        animator = GetComponent<Animator>();
    }

    public void ActivateArcaneBladeVortex()
    {
        if (IsOwner)
        {
            ArcaneBladeVortexServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ArcaneBladeVortexServerRpc()
    {
        if (BladeVortex == null)
        {
            BladeVortex = ObjectPooler.Instance.Spawn("ArcaneBladeVortex", transform.position + transform.up, transform.rotation);
            BladeVortex.GetComponent<BladeVortexCollisionHandler>().SetStats(Damage, Mathf.Max(0.35f - (AttackSpeedMultiplier.Value / 20), 0.05f));
            SpawnCastingEffects();
            BladeVortex.transform.localScale = new Vector3(AttackRange, AttackRange, AttackRange);
            BladeVortex.GetComponent<NetworkObject>().Spawn();
            BladeVortex.transform.SetParent(transform);
            StartBladeVortexDespawnClientRpc(Duration);
        }

    }

    [ClientRpc]
    private void StartBladeVortexDespawnClientRpc(float duration)
    {
        StartCoroutine(DestroyArcaneBladeVortexAfterDuration(Duration));
    }

    IEnumerator DestroyArcaneBladeVortexAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (BladeVortex != null)
        {
            BladeVortex.GetComponent<NetworkObject>().Despawn(false);
            ObjectPooler.Instance.Despawn("ArcaneBladeVortex", BladeVortex);
            BladeVortex = null;
        }

    }

    void SpawnCastingEffects()
    {
        GameObject muzzle = ObjectPooler.Instance.Spawn("ArcaneMuzzle", transform.position, transform.rotation);
        GameObject cast = ObjectPooler.Instance.Spawn("ArcaneCast", transform.position, transform.rotation);
        GameObject enchant = ObjectPooler.Instance.Spawn("ArcaneEnchant", transform.position, transform.rotation);
        muzzle.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        cast.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        enchant.transform.localRotation = Quaternion.Euler(-90, 0, 90);

        muzzle.GetComponent<NetworkObject>().Spawn();
        cast.GetComponent<NetworkObject>().Spawn();
        enchant.GetComponent<NetworkObject>().Spawn();
    }


    private void SetAttackSpeedMultiplier(float value)
    {
        animator.SetFloat("AttackSpeedMultiplier", value);
    }

}
