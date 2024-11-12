using System.Collections;
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
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth;
            IsDead = false;
            animator = GetComponent<Animator>();
            kinematics = GetComponent<AIKinematics>();
            enemy = GetComponent<Enemy>();
            rb = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
            collider.enabled = true;
            CurrentHealth.OnValueChanged += OnHitAnimation;
            CurrentHealth.OnValueChanged += OnHitEffects;
        }
        EventManager.Instance.EnemySpawnedEvent(gameObject);
    }

    void OnDisable()
    {
        CurrentHealth.OnValueChanged -= OnHitAnimation;
        CurrentHealth.OnValueChanged -= OnHitEffects;

    }


    void Update()
    {
        if (!IsServer) return;

        if (CurrentHealth.Value < MaxHealth)
        {
            CurrentHealth.Value += HealthRegenRate * Time.deltaTime;
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
    public void RequestTakeDamageServerRpc(float damage, ulong clientId)
    {
        TakeDamage(damage, clientId);
    }

    public void TakeDamage(float damage, ulong clientId)
    {
        if (!IsServer) return;
        {
            if (IsDead) return;
            CurrentHealth.Value -= damage;
            if (CurrentHealth.Value <= 0)
            {
                IsDead = true;
                HandleDeathClientRpc(clientId);
            }
        }
    }


    [ClientRpc]
    public void HandleDeathClientRpc(ulong clientId)
    {
        HandleDeath(clientId);
    }
    public void HandleDeath(ulong clientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(clientId, out var networkObject))
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

        EventManager.Instance.EnemyDespawnedEvent(gameObject);
        StartCoroutine(DeathAnim());


    }

    IEnumerator DeathAnim()
    {

        animator.SetTrigger("isDead");
        GameManager.Instance.SpawnedEnemies.Remove(enemy);
        yield return new WaitForSeconds(5f);
        enemy.enabled = true;
        kinematics.enabled = true;
        GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn(enemyName, gameObject);
    }


}
