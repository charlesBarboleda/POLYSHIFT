using System.Collections;
using FIMSpace.FLook;
using Unity.Netcode;
using UnityEngine;

public class Starbreaker : NetworkBehaviour
{
    GameObject Owner { get; set; }
    public float Damage { get; set; }
    public float AttackSpeed { get; set; }
    public float Speed { get; set; }
    public Transform homingMissileSpawnPoint;
    FLookAnimator lookAnimator;
    PlayerWeapon playerWeapon;
    AudioSource audioSource;
    [SerializeField] AudioClip turretFireSound;
    [SerializeField] Doppelbomber doppelbomberTurret1;
    [SerializeField] Doppelbomber doppelbomberTurret2;

    Enemy targetEnemy;
    float fireCooldownTimer = 0f; // Timer to control firing rate

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        lookAnimator = GetComponent<FLookAnimator>();
        audioSource = GetComponent<AudioSource>();
        AttackSpeed = 5f;
    }

    void Update()
    {
        if (!IsServer) return;

        if (Owner != null)
        {
            Damage = playerWeapon.Damage * 10;
        }

        targetEnemy = FindClosestEnemy();
        if (targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.transform.position) < 30f)
        {
            // Rotate turret to face the closest enemy
            lookAnimator.ObjectToFollow = targetEnemy.transform;

            // Handle firing cooldown
            fireCooldownTimer -= Time.deltaTime;
            if (fireCooldownTimer <= 0f)
            {
                FireAtEnemy(targetEnemy);
                audioSource.PlayOneShot(turretFireSound);
                fireCooldownTimer = AttackSpeed; // Reset cooldown
            }
        }

        // Ship hovers over the player at all times
        transform.position = Owner.transform.position + new Vector3(0, 20, 0);

    }


    void FireAtEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            StartCoroutine(FireHomingMissiles());
        }
    }

    IEnumerator FireHomingMissiles()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.5f);
            SpawnMissileServerRpc();
        }
    }

    [ServerRpc]
    void SpawnMissileServerRpc()
    {
        audioSource.PlayOneShot(turretFireSound);

        GameObject homingMissile = ObjectPooler.Instance.Spawn("HomingMissile", homingMissileSpawnPoint.position, Quaternion.identity);
        var missileScript = homingMissile.GetComponent<HomingMissile>();
        missileScript.SetTarget(targetEnemy.gameObject, Owner);
        missileScript.Damage = Damage;
        homingMissile.GetComponent<NetworkObject>().Spawn();
    }

    public void SetOwners(GameObject owner)
    {
        Owner = owner;
        doppelbomberTurret1.SetOwner(owner);
        doppelbomberTurret2.SetOwner(owner);
        playerWeapon = owner.GetComponent<PlayerWeapon>();
    }

    Enemy FindClosestEnemy()
    {
        Enemy closestEnemy = null;
        float closestDistance = 30;

        foreach (var enemy in GameManager.Instance.SpawnedEnemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }
}
