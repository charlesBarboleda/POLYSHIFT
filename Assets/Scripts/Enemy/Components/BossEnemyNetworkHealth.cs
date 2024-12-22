using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEnemyNetworkHealth : EnemyNetworkHealth
{
    public string BossName;
    [SerializeField] List<ParticleSystem> onDeathParticles;
    [SerializeField] AudioClip onDeathSound;
    public override void HandleDeath(ulong networkObjectId)
    {
        base.HandleDeath(networkObjectId);

        if (IsServer)
        {
            GameManager.Instance.GiveAllPlayersLevelClientRpc(4);
            GameManager.Instance.GiveAllPlayersSkillPointsClientRpc(4);
            if (GameManager.Instance.SpawnedEnemies.Contains(enemy))
                GameManager.Instance.SpawnedEnemies.Remove(enemy);
            StartCoroutine(SpawnDeathEffects());
            AudioSource.PlayClipAtPoint(onDeathSound, transform.position);
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
