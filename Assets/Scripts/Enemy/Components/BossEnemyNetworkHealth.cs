using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEnemyNetworkHealth : EnemyNetworkHealth
{
    public string BossName;
    [SerializeField] List<ParticleSystem> onDeathParticles;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip onDeathSound;
    public override void HandleDeath(ulong networkObjectId)
    {
        base.HandleDeath(networkObjectId);

        if (IsServer)
        {
            GameManager.Instance.HealAllPlayersClientRpc();
            GameManager.Instance.GiveAllPlayersLevelClientRpc(4);
            StartCoroutine(SpawnDeathEffects());
            audioSource.volume = 0.75f;
            audioSource.PlayOneShot(onDeathSound);
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
}
