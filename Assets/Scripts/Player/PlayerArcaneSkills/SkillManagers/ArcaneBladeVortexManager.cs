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
        ResetSkill();
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
        PlayerSkills = GetComponent<PlayerSkills>();
        animator = GetComponent<Animator>();
    }

    public void ResetSkill()
    {
        Damage = 15f;
        Duration = 45f;
        KnockbackForce = 3f;
        AttackSpeedMultiplier.Value = 1f;
        AttackRange = 2f;
    }

    public void ActivateArcaneBladeVortex()
    {

        ArcaneBladeVortex();

    }

    private void ArcaneBladeVortex()
    {
        if (BladeVortex == null)
        {
            SpawnBladeVortexRpc();
            StartBladeVortexDespawnRpc(Duration);
            SpawnCastingEffectsRpc();
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnBladeVortexRpc()
    {
        BladeVortex = ObjectPooler.Instance.Spawn("ArcaneBladeVortex", transform.position + transform.up, transform.rotation);
        BladeVortex.GetComponent<BladeVortexCollisionHandler>().SetStats(Damage, Mathf.Max(0.35f - (AttackSpeedMultiplier.Value / 60), 0.05f), NetworkObjectId);
        BladeVortex.transform.localScale = new Vector3(AttackRange / 5, AttackRange / 5, AttackRange / 5);
        BladeVortex.transform.SetParent(transform);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartBladeVortexDespawnRpc(float duration)
    {
        StartCoroutine(DestroyArcaneBladeVortexAfterDuration(Duration));
    }

    IEnumerator DestroyArcaneBladeVortexAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (BladeVortex != null)
        {
            ObjectPooler.Instance.Despawn("ArcaneBladeVortex", BladeVortex);
            BladeVortex = null;
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnCastingEffectsRpc()
    {
        GameObject muzzle = ObjectPooler.Instance.Spawn("ArcaneMuzzle", transform.position, transform.rotation);
        GameObject cast = ObjectPooler.Instance.Spawn("ArcaneCast", transform.position, transform.rotation);
        GameObject enchant = ObjectPooler.Instance.Spawn("ArcaneEnchant", transform.position, transform.rotation);
        muzzle.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        cast.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        enchant.transform.localRotation = Quaternion.Euler(-90, 0, 90);
    }


    private void SetAttackSpeedMultiplier(float value)
    {
        animator.SetFloat("AttackSpeedMultiplier", value);
    }

}
