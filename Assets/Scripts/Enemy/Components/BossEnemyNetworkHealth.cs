using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using DG.Tweening;

public class BossEnemyNetworkHealth : EnemyNetworkHealth, IStaggerable
{

    public string BossName;
    public float StaggerMaxHealth;
    public NetworkVariable<float> netStaggerMaxHealth = new();
    public NetworkVariable<float> StaggerCurrentHealth = new();
    public NetworkVariable<bool> IsStaggered = new(false);
    public NetworkVariable<bool> CanBeStaggered = new(true);
    float staggerCooldown = 180f;
    [SerializeField] float yPosOffset = -1.5f;

    [SerializeField] GameObject staggeredText;
    [SerializeField] List<ParticleSystem> onDeathParticles;
    [SerializeField] AudioClip[] staggerSound;
    [SerializeField] AudioClip onDeathSound;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            netStaggerMaxHealth.Value = StaggerMaxHealth;
            StaggerCurrentHealth.Value = netStaggerMaxHealth.Value;
            CanBeStaggered.Value = true;
            IsStaggered.Value = false;
        }
    }

    public override void HandleDeath(ulong networkObjectId)
    {
        base.HandleDeath(networkObjectId);

        if (IsServer)
        {

            staggerCooldown = 180f;
            GameManager.Instance.GiveAllPlayersSkillPointsClientRpc(2);
            if (GameManager.Instance.SpawnedEnemies.Contains(enemy))
                GameManager.Instance.SpawnedEnemies.Remove(enemy);
            StartCoroutine(SpawnDeathEffects());
            AudioSource.PlayClipAtPoint(onDeathSound, transform.position, 3f);
        }
    }

    [Rpc(SendTo.Server)]
    public void StaggerRpc()
    {
        StartCoroutine(StaggerCoroutine());
        StartCoroutine(StaggerCooldown());
    }

    IEnumerator StaggerCoroutine()
    {
        IsStaggered.Value = true;

        staggeredText.SetActive(true);

        animator.SetTrigger("IsStaggered");

        transform.DOMoveY(yPosOffset, 0.5f).SetEase(Ease.Linear);

        ObjectPooler.Instance.Spawn("StaggerEffect1", transform.position, Quaternion.identity);
        ObjectPooler.Instance.Spawn("StaggerEffect2", transform.position, Quaternion.identity);

        foreach (AudioClip clip in staggerSound)
        {
            audioSource.PlayOneShot(clip, 2f);
        }

        enemy.isAttacking = true;

        yield return new WaitForSeconds(10f);

        transform.DOMoveY(0, 0.5f).SetEase(Ease.Linear);

        staggeredText.SetActive(false);

        netStaggerMaxHealth.Value *= 2;
        StaggerCurrentHealth.Value = netStaggerMaxHealth.Value;

        animator.SetTrigger("IsFinishedStagger");


        yield return new WaitForSeconds(3f);

        enemy.isAttacking = false;
        IsStaggered.Value = false;
    }

    IEnumerator StaggerCooldown()
    {
        CanBeStaggered.Value = false;
        yield return new WaitForSeconds(staggerCooldown);
        CanBeStaggered.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyStaggerDamageServerRpc(float damage)
    {
        if (!IsServer || IsStaggered.Value || !CanBeStaggered.Value)
            return;

        StaggerCurrentHealth.Value -= damage;
        if (StaggerCurrentHealth.Value <= 0)
        {
            StaggerCurrentHealth.Value = 0;
            StaggerRpc();
        }
    }

    IEnumerator SpawnDeathEffects()
    {
        foreach (ParticleSystem particle in onDeathParticles)
        {
            particle.gameObject.SetActive(true);
            particle.Play();
        }
        yield return new WaitForSeconds(3f);
        foreach (ParticleSystem particle in onDeathParticles)
        {
            particle.Stop();
            particle.gameObject.SetActive(false);
        }
    }


    public override void OnHitEffects(float prev, float current)
    {
        // Do nothing
    }

    public override void OnHitAnimation(float prev, float current)
    {
        // Do nothing
    }
}
