using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class OverloadManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; }
    public Animator animator { get; set; }
    public float Duration;
    PlayerWeapon playerWeapon;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Duration = 15f;
        animator = GetComponent<Animator>();
        playerWeapon = GetComponent<PlayerWeapon>();
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
    }


    public void Overload()
    {
        if (IsServer)
        {
            OverloadServerRpc();
        }
    }

    [ServerRpc]
    private void OverloadServerRpc()
    {
        StartCoroutine(OverloadCoroutine());
    }

    IEnumerator OverloadCoroutine()
    {
        // Spawn the overload visual effects
        GameObject waterSpinning = ObjectPooler.Instance.Spawn("WaterSpinning", transform.position, transform.rotation);
        waterSpinning.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        waterSpinning.GetComponent<NetworkObject>().Spawn();
        waterSpinning.transform.SetParent(transform);

        GameObject waterCast = ObjectPooler.Instance.Spawn("WaterCast", transform.position, transform.rotation);
        waterCast.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        waterCast.GetComponent<NetworkObject>().Spawn();

        GameObject waterCircling = ObjectPooler.Instance.Spawn("WaterCircling", transform.position, transform.rotation);
        waterCircling.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        waterCircling.GetComponent<NetworkObject>().Spawn();

        // Apply the overload stat changes
        float oldFireRate = playerWeapon.ShootRate;
        float oldReloadRate = playerWeapon.ReloadTime;
        playerWeapon.DecreaseFireRateByServerRpc(0.80f);
        playerWeapon.DecreaseReloadTimeByServerRpc(0.80f);

        yield return new WaitForSeconds(Duration);

        playerWeapon.ReloadTime = oldReloadRate;
        playerWeapon.ShootRate = oldFireRate;
        ObjectPooler.Instance.Despawn("WaterSpinning", waterSpinning);
        waterCircling.GetComponent<NetworkObject>().Despawn(false);


    }

    void SetAttackSpeedMultiplier(float value)
    {
        animator.SetFloat("AttackSpeedMultiplier", value);
    }
}
