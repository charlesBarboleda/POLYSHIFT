using System.Collections;
using UnityEngine.UI;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNetworkHealth : NetworkBehaviour, IDamageable
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>();
    public float MaxHealth;
    public float HealthRegenRate;
    public bool IsDead;
    public float ExperienceDrop = 10f;
    Animator animator;
    AIKinematics kinematics;
    Enemy enemy;
    Rigidbody rb;
    Collider collider;
    [SerializeField] string enemyName;
    [SerializeField] Image healthbarFill;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth;
            UpdateHealthbar(MaxHealth, MaxHealth);
        }
        IsDead = false;
        animator = GetComponent<Animator>();
        kinematics = GetComponent<AIKinematics>();
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        collider.enabled = true;
        CurrentHealth.OnValueChanged += OnHitAnimation;
        CurrentHealth.OnValueChanged += OnHitEffects;
        CurrentHealth.OnValueChanged += UpdateHealthbar;
        EventManager.Instance.EnemySpawnedEvent(enemy);
    }

    void OnDisable()
    {
        CurrentHealth.OnValueChanged -= OnHitAnimation;
        CurrentHealth.OnValueChanged -= OnHitEffects;
        CurrentHealth.OnValueChanged -= UpdateHealthbar;

    }


    void Update()
    {
        if (!IsServer) return;

        if (IsDead) return;

        if (CurrentHealth.Value < MaxHealth)
        {
            CurrentHealth.Value += HealthRegenRate * Time.deltaTime;
        }

    }
    void UpdateHealthbar(float prev, float current)
    {

        if (healthbarFill != null)
        {
            healthbarFill.fillAmount = current / MaxHealth;
        }
    }

    void OnHitEffects(float prev, float current)
    {
        if (prev > current)
        {
            // Slow down the enemy when hit
            kinematics.Agent.maxSpeed = 0;
        }
    }

    void OnHitAnimation(float prev, float current)
    {
        if (prev > current)
        {
            animator.SetTrigger("isHit");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage, ulong networkObjectId)
    {
        TakeDamage(damage, networkObjectId);
    }

    public void TakeDamage(float damage, ulong networkObjectId)
    {
        if (!IsServer) return;
        {
            if (IsDead) return;
            CurrentHealth.Value -= damage;
            if (CurrentHealth.Value <= 0)
            {
                IsDead = true;
                HandleDeathClientRpc(networkObjectId);
            }
        }
    }

    [ServerRpc]
    public void HealServerRpc(float healAmount)
    {
        CurrentHealth.Value += healAmount;
    }


    [ClientRpc]
    public void HandleDeathClientRpc(ulong networkObjectId)
    {
        HandleDeath(networkObjectId);
    }
    public void HandleDeath(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            var playerLevel = networkObject.GetComponent<PlayerNetworkLevel>();
            if (playerLevel != null)
            {
                playerLevel.AddExperience(ExperienceDrop);
            }
        }

        if (enemy != null)
        {
            enemy.enabled = false;
        }
        else
        {
            Debug.LogError("Enemy component is null on client: " + NetworkManager.Singleton.LocalClientId);
        }

        if (kinematics != null)
        {
            kinematics.Agent.maxSpeed = 0f;
            kinematics.enabled = false;
        }
        else
        {
            Debug.LogError("Kinematics component is null on client: " + NetworkManager.Singleton.LocalClientId);
        }
        if (collider != null)
            collider.enabled = false;

        StartCoroutine(DeathAnim());
    }


    IEnumerator DeathAnim()
    {

        animator.SetTrigger("isDead");
        GameManager.Instance.SpawnedEnemies.Remove(enemy);
        EventManager.Instance.EnemyDespawnedEvent(enemy);
        yield return new WaitForSeconds(3f);
        enemy.enabled = true;
        kinematics.enabled = true;
        GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn(enemyName, gameObject);
    }


}
