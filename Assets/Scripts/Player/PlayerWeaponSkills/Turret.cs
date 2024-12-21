using JetBrains.Annotations;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using FIMSpace.FLook;
using DG.Tweening;


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
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] GameObject muzzleFlash2;

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
        }
    }

    public virtual void Update()
    {
        if (!IsServer) return;


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
                MuzzleFlashRpc();
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
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void MuzzleFlashRpc()
    {
        muzzleFlash.SetActive(true);
        muzzleFlash2.SetActive(true);

        muzzleFlash.transform.position = turretMuzzlePosition.position;
        muzzleFlash2.transform.position = turretMuzzlePosition2.position;

        muzzleFlash.transform.rotation = turretMuzzlePosition.rotation;
        muzzleFlash2.transform.rotation = turretMuzzlePosition2.rotation;

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
            SpawnTurretExplosionRpc();
            audioSource.PlayOneShot(turretExplosionSound);
            GameManager.Instance.SpawnedAllies.Remove(gameObject);
            ObjectPooler.Instance.Despawn(ObjectPoolTag, gameObject);
            NetworkObject.Despawn(false);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnTurretExplosionRpc()
    {
        GameObject turretExplosion = ObjectPooler.Instance.Spawn("TurretExplosion", transform.position, Quaternion.identity);

    }
    public void OnHealthChanged(float previousValue, float newValue)
    {
        if (healthbarFill != null)
        {
            float targetFill = CurrentHealth.Value / MaxHealth.Value;
            healthbarFill.DOFillAmount(targetFill, 0.5f).SetEase(Ease.OutQuad);
        }
    }

    public void SetOwner(GameObject owner)
    {
        Owner = owner;
        playerWeapon = owner.GetComponent<PlayerWeapon>();
    }


}
