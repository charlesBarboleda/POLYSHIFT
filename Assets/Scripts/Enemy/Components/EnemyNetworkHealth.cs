using System.Collections;
using UnityEngine.UI;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode.Components;
using Pathfinding;
using System.Collections.Generic;

[RequireComponent(typeof(AIKinematics))]
[RequireComponent(typeof(ClientNetworkAnimator))]
[RequireComponent(typeof(ClientNetworkTransform))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkRigidbody))]
[RequireComponent(typeof(DebuffManager))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(AIPath))]
public class EnemyNetworkHealth : NetworkBehaviour, IDamageable
{
    public NetworkVariable<float> CurrentHealth = new();
    public float MaxHealth;
    public float HealthRegenRate;
    public bool IsDead;
    public float ExperienceDrop = 10f;
    public float height = 2f;
    protected Animator animator;
    protected AIKinematics kinematics;
    protected Enemy enemy;
    protected Rigidbody rb;
    protected Collider collider;
    [SerializeField] Collider headCollider;
    [SerializeField] string enemyName;
    [SerializeField] Image healthbarFill;
    protected AudioSource audioSource;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth;
            UpdateHealthbar(MaxHealth, MaxHealth);
        }
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        kinematics = GetComponent<AIKinematics>();
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        IsDead = false;
        collider.enabled = true;
        headCollider.enabled = true;
        enemy.isAttacking = false;
        CurrentHealth.OnValueChanged += OnHitAnimation;
        CurrentHealth.OnValueChanged += OnHitEffects;
        if (healthbarFill != null)
        {
            CurrentHealth.OnValueChanged += UpdateHealthbar;
        }
        EventManager.Instance.EnemySpawnedEvent(enemy);
    }

    void OnDisable()
    {
        CurrentHealth.OnValueChanged -= OnHitAnimation;
        CurrentHealth.OnValueChanged -= OnHitEffects;
        if (healthbarFill != null)
        {
            CurrentHealth.OnValueChanged -= UpdateHealthbar;
        }

    }


    protected virtual void Update()
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
            float targetFill = current / MaxHealth;

            healthbarFill.DOFillAmount(targetFill, 0.5f).SetEase(Ease.OutQuad);

        }
    }

    public virtual void OnHitEffects(float prev, float current)
    {
        if (prev > current)
        {
            // Slow down the enemy when hit
            kinematics.Agent.maxSpeed = 0;
        }
    }

    public virtual void OnHitAnimation(float prev, float current)
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

    public virtual void TakeDamage(float damage, ulong networkObjectId)
    {
        if (!IsServer) return;

        CurrentHealth.Value -= damage;
        if (CurrentHealth.Value <= 0)
        {
            HandleDeathClientRpc(networkObjectId);
            IsDead = true;
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
    public virtual void HandleDeath(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            var playerLevel = networkObject.GetComponent<PlayerNetworkLevel>();
            if (playerLevel != null)
            {
                playerLevel.AddExperience(ExperienceDrop);
            }
        }

        if (audioSource != null)
        {
            audioSource.Stop();
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
            kinematics.CanMove = false;
            kinematics.Agent.isStopped = true;
            kinematics.Agent.canMove = false;
        }
        else
        {
            Debug.LogError("Kinematics component is null on client: " + NetworkManager.Singleton.LocalClientId);
        }
        if (collider != null)
            collider.enabled = false;

        if (headCollider != null)
            headCollider.enabled = false;

        StartCoroutine(DeathAnim());
    }


    IEnumerator DeathAnim()
    {

        animator.SetTrigger("isDead");
        PopUpNumberManager.Instance.SpawnXPNumber(transform.position + transform.up * height, ExperienceDrop);
        if (healthbarFill != null)
            healthbarFill.transform.parent.gameObject.SetActive(false);
        if (GameManager.Instance.SpawnedEnemies.Contains(enemy))
            GameManager.Instance.SpawnedEnemies.Remove(enemy);
        EventManager.Instance.EnemyDespawnedEvent(enemy);
        yield return new WaitForSeconds(3f);
        if (healthbarFill != null)
            healthbarFill.transform.parent.gameObject.SetActive(true);
        enemy.enabled = true;
        kinematics.CanMove = true;
        GetComponent<NetworkObject>().Despawn(false);
        ObjectPooler.Instance.Despawn(enemyName, gameObject);
    }


}
