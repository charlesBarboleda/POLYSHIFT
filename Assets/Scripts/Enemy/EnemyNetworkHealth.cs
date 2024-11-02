using System.Collections;
using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNetworkHealth : NetworkBehaviour, IDamageable
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>();
    public float MaxHealth;
    public float HealthRegenRate;
    Animator animator;
    AIKinematics kinematics;
    MeleeEnemy meleeEnemy;
    NavMeshAgent agent;
    [SerializeField] string enemyName;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth;
        }
        animator = GetComponentInChildren<Animator>();
        kinematics = GetComponent<AIKinematics>();
        meleeEnemy = GetComponent<MeleeEnemy>();
        agent = GetComponent<NavMeshAgent>();
        CurrentHealth.OnValueChanged += OnHitAnimation;
        CurrentHealth.OnValueChanged += OnHitEffects;
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
    public void RequestTakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        {
            CurrentHealth.Value -= damage;
            if (CurrentHealth.Value <= 0)
            {
                HandleDeathClientRpc();
            }
        }
    }

    public void TakeDamage(float damage, ulong attackerId)
    {
        TakeDamage(damage);
        Debug.Log("Enemy was attacked by " + attackerId);
    }

    [ClientRpc]
    public void HandleDeathClientRpc()
    {
        HandleDeath();
    }
    public void HandleDeath()
    {
        Debug.Log("Handle death");
        EventManager.Instance.EnemyDespawnedEvent(gameObject);
        kinematics.Agent.velocity = Vector3.zero;
        meleeEnemy.enabled = false;
        kinematics.enabled = false;
        agent.enabled = false;
        StartCoroutine(DeathAnim());

    }

    IEnumerator DeathAnim()
    {
        animator.SetTrigger("isDead");
        yield return new WaitForSeconds(5f);
        meleeEnemy.enabled = true;
        kinematics.enabled = true;
        agent.enabled = true;
        NetworkObjectPool.Instance.ReturnNetworkObject(gameObject.GetComponent<NetworkObject>(), enemyName);
    }
}
