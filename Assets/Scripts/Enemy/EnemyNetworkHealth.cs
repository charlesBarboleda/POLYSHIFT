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
    NavMeshAgent agent;
    [SerializeField] string enemyName;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth;
            IsDead = false;
            animator = GetComponentInChildren<Animator>();
            kinematics = GetComponent<AIKinematics>();
            enemy = GetComponent<Enemy>();
            agent = GetComponent<NavMeshAgent>();
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
            kinematics.Agent.velocity = Vector3.one;
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
        Debug.Log("HandleDeathClientRpc executed on client: " + NetworkManager.Singleton.LocalClientId);
        HandleDeath(clientId);
    }
    public void HandleDeath(ulong clientId)
    {
        Debug.Log("1Enemy died called on client: " + NetworkManager.Singleton.LocalClientId);
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(clientId, out var networkObject))
        {
            var playerLevel = networkObject.GetComponent<PlayerNetworkLevel>();
            if (playerLevel != null)
            {
                playerLevel.AddExperience(ExperienceDrop);
            }
        }
        Debug.Log("2Enemy died called on client: " + NetworkManager.Singleton.LocalClientId);
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
            kinematics.Agent.velocity = Vector3.zero;
            kinematics.enabled = false;
        }
        else
        {
            Debug.LogError("Kinematics component is null on client: " + NetworkManager.Singleton.LocalClientId);
        }

        if (agent != null)
        {
            agent.enabled = false;
        }
        else
        {
            Debug.LogError("NavMeshAgent component is null on client: " + NetworkManager.Singleton.LocalClientId);
        }

        Debug.Log("3Enemy despawned Event called on client: " + NetworkManager.Singleton.LocalClientId);
        EventManager.Instance.EnemyDespawnedEvent(gameObject);
        StartCoroutine(DeathAnim());


    }

    IEnumerator DeathAnim()
    {
        animator.SetTrigger("isDead");
        yield return new WaitForSeconds(5f);
        enemy.enabled = true;
        kinematics.enabled = true;
        agent.enabled = true;
        GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn(enemyName, gameObject);
    }


}
