using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class DevilManager : NetworkBehaviour
{
    public float damage;
    Transform player;
    PlayerSkills PlayerSkills;

    AudioSource audioSource;
    [SerializeField] AudioSource audioSource2;
    [SerializeField] AudioClip devilRoar;
    [SerializeField] AudioClip explosionSound;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TryGetComponent(out audioSource);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayDevilRoarServerRpc()
    {
        audioSource.PlayOneShot(devilRoar);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayExplosionSoundServerRpc()
    {
        audioSource.PlayOneShot(explosionSound);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DealAreaDamageServerRpc(float radius)
    {
        PlayerSkills.DealDamageInCircle(transform.position, radius, damage, 5f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnDevilSlam1ServerRpc()
    {
        // Server handles spawning of the pillars and ground crack
        SpawnDevilSlam1Effects();

    }


    void SpawnDevilSlam1Effects()
    {
        // Spawn pillars around the player on the server
        for (int i = 0; i < 4; i++)
        {
            Vector3 spawnPosition = player.position + new Vector3(Mathf.Cos(i * Mathf.PI / 2) * 5, 0, Mathf.Sin(i * Mathf.PI / 2) * 5);
            GameObject devilPillar = ObjectPooler.Instance.Spawn("DevilPillarBlast", spawnPosition, Quaternion.identity);
            devilPillar.GetComponent<NetworkObject>().Spawn(); // Ensure pillar is networked and visible to all clients
        }

        // Spawn ground crack on the server
        GameObject groundCrack = ObjectPooler.Instance.Spawn("GroundCrackDecal", player.position + Vector3.left, Quaternion.Euler(90, 0, 0));
        groundCrack.GetComponent<NetworkObject>().Spawn();
        groundCrack.GetComponent<DecalProjector>().size = new Vector3(25, 25, 25);

    }

    [ServerRpc(RequireOwnership = false)]
    public void OnDevilSlam2ServerRpc()
    {
        SpawnDevilSlam2Effects();
    }

    void SpawnDevilSlam2Effects()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 spawnPosition = player.position + new Vector3(Mathf.Cos(i * Mathf.PI / 4) * 15, 0, Mathf.Sin(i * Mathf.PI / 4) * 15);
            GameObject devilPillar = ObjectPooler.Instance.Spawn("DevilPillarBlast", spawnPosition, Quaternion.identity);
            devilPillar.GetComponent<NetworkObject>().Spawn();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnDevilSlam3ServerRpc()
    {
        SpawnDevilSlam3Effects();

    }
    void SpawnDevilSlam3Effects()
    {
        for (int i = 0; i < 12; i++)
        {
            Vector3 spawnPosition = player.position + new Vector3(Mathf.Cos(i * Mathf.PI / 6) * 30, 0, Mathf.Sin(i * Mathf.PI / 6) * 30);
            GameObject devilPillar = ObjectPooler.Instance.Spawn("DevilPillarBlast", spawnPosition, Quaternion.identity);
            devilPillar.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void DespawnDevil()
    {
        StartCoroutine(MoveDevilDown());
    }

    IEnumerator MoveDevilDown()
    {
        transform.DOMoveY(-20, 1f);
        yield return new WaitForSeconds(1f);
        ObjectPooler.Instance.Despawn("Devil", gameObject);
        GetComponent<NetworkObject>().Despawn(false);
    }

    public void SetPlayer(Transform player, float damage)
    {
        this.damage = damage;
        this.player = player;
        PlayerSkills = player.GetComponentInParent<PlayerSkills>();
    }
}
