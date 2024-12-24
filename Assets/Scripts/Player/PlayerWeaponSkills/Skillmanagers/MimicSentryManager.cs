using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class MimicSentryManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>(1f);
    public float KnockbackForce { get; set; }
    public float AttackRange { get; set; }
    public Animator animator { get; set; }
    public GameObject playerHand;
    GameObject sentryOrb;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        animator = GetComponent<Animator>();
        if (IsServer)
        {
            AttackSpeedMultiplier.OnValueChanged += OnAttackSpeedChanged;

        }
    }

    public void ResetSkill()
    {
        Damage = 0;
        KnockbackForce = 0;
        AttackRange = 0;
        AttackSpeedMultiplier.Value = 1f;
    }

    public void OnAttackSpeedChanged(float current)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is null in MimicSentryManager.");
            return;
        }
        animator.SetFloat("AttackSpeedMultiplier", current);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnSentryOrbOnHandServerRpc()
    {
        if (playerHand == null)
        {
            Debug.LogError("Player hand is null in MimicSentryManager.");
            return;
        }
        sentryOrb = ObjectPooler.Instance.Spawn("LightningOrbitSphere", playerHand.transform.position, Quaternion.identity);
        sentryOrb.GetComponent<NetworkObject>().Spawn();
    }


    [ServerRpc]
    public void ApplyForceToSentryOrbServerRpc(string turretTag)
    {
        if (sentryOrb == null)
        {
            Debug.LogError("Sentry orb is null in MimicSentryManager.");
            return;
        }
        Rigidbody rb = sentryOrb.GetComponent<Rigidbody>();
        rb.AddForce((transform.forward + transform.up) * 10f, ForceMode.Impulse);

        SentryOrbSpawnsTurretRpc(turretTag);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SentryOrbSpawnsTurretRpc(string turretTag)
    {
        StartCoroutine(SentryOrbSpawnsTurret(turretTag));
    }

    public IEnumerator SentryOrbSpawnsTurret(string turretTag)
    {
        yield return new WaitForSeconds(1.5f);


        GameObject spawnEffect = ObjectPooler.Instance.Spawn("FrostSphereBlast", sentryOrb.transform.position, Quaternion.identity);
        spawnEffect.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        GameObject mimicTurret = ObjectPooler.Instance.Spawn(turretTag, sentryOrb.transform.position, Quaternion.identity);

        var mimicScript = mimicTurret.GetComponent<Turret>();
        mimicScript.MaxHealth.Value = GetComponent<PlayerNetworkLevel>().Level.Value * 50;
        mimicScript.CurrentHealth.Value = mimicScript.MaxHealth.Value;
        mimicScript.SetOwner(gameObject);
        mimicTurret.GetComponent<NetworkObject>().Spawn();

        ObjectPooler.Instance.Despawn("LightningOrbitSphere", sentryOrb);
        sentryOrb.GetComponent<NetworkObject>().Despawn(false);
    }



}
