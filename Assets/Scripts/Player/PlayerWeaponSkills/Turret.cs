using JetBrains.Annotations;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using FIMSpace.FLook;


public abstract class Turret : NetworkBehaviour, IDamageable
{
    public NetworkVariable<float> CurrentHealth { get; } = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> MaxHealth { get; } = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    FLookAnimator lookAnimator;
    public GameObject Owner;
    public float Damage;
    public float AttackSpeed;
    public string MuzzleFlashTag;
    public string ObjectPoolTag;
    public PlayerWeapon playerWeapon;
    AudioSource audioSource;
    [SerializeField] AudioClip turretFireSound;
    [SerializeField] AudioClip turretExplosionSound;

    [SerializeField] Transform turretMuzzlePosition;
    [SerializeField] Transform turretMuzzlePosition2;
    [SerializeField] Image healthbarFill;

    float fireCooldownTimer = 0f; // Timer to control firing rate

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        lookAnimator = GetComponent<FLookAnimator>();
        audioSource = GetComponent<AudioSource>();

        if (healthbarFill != null)
            healthbarFill.fillAmount = CurrentHealth.Value / MaxHealth.Value;

        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth.Value;
            CurrentHealth.OnValueChanged += OnHealthChanged;
            GameManager.Instance.SpawnedAllies.Add(gameObject);
            GameObject TurretPortal1 = ObjectPooler.Instance.Spawn("TurretPortal1", transform.position, Quaternion.identity);
            TurretPortal1.GetComponent<NetworkObject>().Spawn();
            GameObject TurretPortal2 = ObjectPooler.Instance.Spawn("TurretPortal2", transform.position, Quaternion.identity);
            TurretPortal2.GetComponent<NetworkObject>().Spawn();
        }
    }

    public virtual void Update()
    {
        if (!IsServer) return;

        // Update stats from the player weapon
        if (Owner != null)
        {
            AttackSpeed = playerWeapon.ShootRate;
            Damage = playerWeapon.Damage;
        }


        Enemy targetEnemy = FindClosestEnemy();
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
                MuzzleFlashServerRpc();
                fireCooldownTimer = AttackSpeed; // Reset cooldown
            }
        }
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


    public virtual void FireAtEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            // Deal damage
            enemy.GetComponent<IDamageable>()?.RequestTakeDamageServerRpc(Damage, Owner.GetComponent<NetworkObject>().NetworkObjectId);
            enemy.GetComponent<Enemy>().OnRaycastHitServerRpc(enemy.transform.position, enemy.transform.forward);
            // Optionally, add effects or spawn projectiles
            Debug.Log($"Firing at enemy {enemy.name}");
        }
    }

    [ServerRpc]
    void MuzzleFlashServerRpc()
    {
        GameObject muzzleFlash = ObjectPooler.Instance.Spawn(MuzzleFlashTag, turretMuzzlePosition.position, turretMuzzlePosition.rotation);
        Debug.Log("Muzzle Flash location: " + muzzleFlash.transform.position);

        muzzleFlash.GetComponent<NetworkObject>().Spawn();

        Debug.Log("Muzzle Flash position: " + muzzleFlash.transform.position);


        GameObject muzzleFlash2 = ObjectPooler.Instance.Spawn(MuzzleFlashTag, turretMuzzlePosition2.position, turretMuzzlePosition2.rotation);
        muzzleFlash2.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage, ulong networkObjectId)
    {
        TakeDamage(damage, networkObjectId);
    }
    public void TakeDamage(float damage, ulong networkObjectId)
    {
        if (IsServer)
        {
            CurrentHealth.Value -= damage;
            if (CurrentHealth.Value <= 0)
            {
                HandleDeath(networkObjectId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float healAmount)
    {
        CurrentHealth.Value += healAmount;
        if (CurrentHealth.Value > MaxHealth.Value)
        {
            CurrentHealth.Value = MaxHealth.Value;
        }
    }

    public void HandleDeath(ulong networkObjectId)
    {
        Die();
    }


    public void Die()
    {
        if (IsServer)
        {
            GameObject turretExplosion = ObjectPooler.Instance.Spawn("TurretExplosion", transform.position, Quaternion.identity);
            audioSource.PlayOneShot(turretExplosionSound);
            turretExplosion.GetComponent<NetworkObject>().Spawn();
            GameManager.Instance.SpawnedAllies.Remove(gameObject);
            ObjectPooler.Instance.Despawn(ObjectPoolTag, gameObject);
            NetworkObject.Despawn(false);
        }
    }

    public void OnHealthChanged(float previousValue, float newValue)
    {
        if (healthbarFill != null)
            healthbarFill.fillAmount = CurrentHealth.Value / MaxHealth.Value;
    }

    public void SetOwner(GameObject owner)
    {
        Owner = owner;
        playerWeapon = owner.GetComponent<PlayerWeapon>();
    }


}
