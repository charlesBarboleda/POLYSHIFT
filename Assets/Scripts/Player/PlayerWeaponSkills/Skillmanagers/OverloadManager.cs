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
        Duration = 10f;
        animator = GetComponent<Animator>();
        playerWeapon = GetComponent<PlayerWeapon>();
        AttackSpeedMultiplier.Value = 1f;
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
    }

    public void ResetSkill()
    {
        Damage = 0;
        KnockbackForce = 0;
        AttackRange = 0;
        AttackSpeedMultiplier.Value = 1f;
        Duration = 10f;
    }


    public void Overload()
    {
        StartCoroutine(OverloadCoroutine());
    }


    IEnumerator OverloadCoroutine()
    {
        // Spawn the overload visual effects
        GameObject waterSpinning = ObjectPooler.Instance.Spawn("WaterSpinning", transform.position, transform.rotation);
        waterSpinning.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        waterSpinning.transform.parent = transform;
        Debug.Log("WaterSpinning: " + waterSpinning.name + " Parented to: " + waterSpinning.transform.parent.name);
        waterSpinning.transform.SetParent(transform);
        Debug.Log("WaterSpinning: " + waterSpinning.name + " Parented to: " + waterSpinning.transform.parent.name);


        GameObject waterCast = ObjectPooler.Instance.Spawn("WaterCast", transform.position, transform.rotation);
        waterCast.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        waterCast.transform.parent = transform;
        waterCast.transform.SetParent(transform);

        GameObject waterCircling = ObjectPooler.Instance.Spawn("WaterCircling", transform.position, transform.rotation);
        waterCircling.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        waterCircling.transform.parent = transform;
        waterCircling.transform.SetParent(transform);

        // Apply the overload stat changes
        float oldFireRate = playerWeapon.ShootRate;
        float oldReloadRate = playerWeapon.ReloadTime;
        playerWeapon.DecreaseFireRateBy(0.50f);
        playerWeapon.DecreaseReloadTimeBy(0.50f);

        yield return new WaitForSeconds(Duration);

        playerWeapon.ReloadTime = oldReloadRate;
        playerWeapon.ShootRate = oldFireRate;
        ObjectPooler.Instance.Despawn("WaterSpinning", waterSpinning);

    }

    void SetAttackSpeedMultiplier(float value)
    {
        animator.SetFloat("AttackSpeedMultiplier", value);
    }
}
