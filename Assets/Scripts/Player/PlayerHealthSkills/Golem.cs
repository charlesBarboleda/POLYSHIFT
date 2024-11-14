using System.Collections.Generic;
using UnityEngine.UI;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public abstract class Golem : NetworkBehaviour, IDamageable
{
    [field: SerializeField] public GameObject Owner { get; private set; }
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(1000f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(1000f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float Damage = 50f;
    public float AttackRange = 3f;
    public float MovementSpeed = 6f;
    public float DamageReduction = 0.5f;
    public float BuffRadius = 20f;
    public bool CanAttack = true;
    public float AttackCooldown = 3f;
    public float ReviveTime;
    public bool IsDead = false;
    float elapsedCooldown = 0f;
    [SerializeField] Image healthFill;
    protected List<Enemy> spawnedEnemies = new List<Enemy>();
    protected GameObject ClosestTarget;
    protected AIPath Agent;
    protected Rigidbody rb;
    protected Collider collider;
    protected Animator Animator;

    protected abstract void BuffEffect(float buffRadius);
    public abstract void HandleDeath(ulong networkObjectId);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"Golem spawned on {(IsServer ? "Server" : "Client")} with ClientID: {NetworkManager.Singleton.LocalClientId}. Owner: {GetComponent<NetworkObject>().OwnerClientId}");
        // Initialize components for both server and client
        Animator = GetComponent<Animator>();
        Debug.Log($"Animator is {(Animator == null ? "null" : "not null")}");
        Agent = GetComponent<AIPath>();
        Debug.Log($"Agent is {(Agent == null ? "null" : "not null")}");
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        Debug.Log("Golem spawned");

        // Health UI update for all instances
        healthFill.fillAmount = CurrentHealth.Value / MaxHealth.Value;
        CurrentHealth.OnValueChanged += UpdateHealthbar;

        // Only run server-specific setup
        if (IsServer)
        {
            Debug.Log("Golem is server side");

            // Server-only setup
            CurrentHealth.Value = MaxHealth.Value;
            Agent.maxSpeed = MovementSpeed;

            spawnedEnemies = GameManager.Instance.SpawnedEnemies;
            GameManager.Instance.SpawnedAllies.Add(gameObject);
        }
    }


    protected virtual void Update()
    {
        if (IsDead) return;
        if (IsServer) // Only run on server
        {
            if (Animator != null)
                Animator.SetBool("IsMoving", Agent.velocity.magnitude > 0.2f);


            // Follow owner or move randomly around them if far away
            float distanceToOwner = Vector3.Distance(transform.position, Owner.transform.position);
            if (distanceToOwner > 15f)
            {

                if (Agent != null)
                    Agent.destination = Owner.transform.position + transform.forward * 5f;

                if (distanceToOwner > 30f)
                {
                    transform.position = Owner.transform.position + transform.forward * 5f; // Teleport to owner if too far
                }
            }
            else
            {
                // Find and approach the closest target
                ClosestTarget = FindClosestEnemyWithinRange();
                if (ClosestTarget != null)
                {
                    if (Agent != null)
                        Agent.destination = ClosestTarget.transform.position;

                    // Check if within attack range
                    float distanceToTarget = Vector3.Distance(transform.position, ClosestTarget.transform.position);
                    if (distanceToTarget <= AttackRange)
                    {
                        if (elapsedCooldown <= 0 && CanAttack)
                        {
                            // Stop and attack
                            Debug.Log("In attack range");
                            FaceTarget();
                            Agent.isStopped = true;
                            Attack();
                            elapsedCooldown = AttackCooldown;
                        }
                        else
                        {
                            Agent.isStopped = true; // Stop agent while within attack range
                        }
                    }
                    else
                    {
                        // Resume movement if not within range
                        Agent.isStopped = false;
                    }
                }
                else
                {
                    // Move randomly around the owner if no target is found

                    if (Agent != null)
                        Agent.destination = Owner.transform.position + transform.forward * 10f;
                }
            }

            // Update cooldown timer
            if (elapsedCooldown > 0)
            {
                elapsedCooldown -= Time.deltaTime;
            }
        }
    }


    protected virtual void Attack()
    {
        if (IsServer)
        {
            Animator.SetTrigger("IsAttacking");
        }
    }

    void FaceTarget()
    {
        transform.LookAt(ClosestTarget.transform);
    }

    GameObject FindClosestEnemyWithinRange()
    {
        GameObject closestEnemy = null;
        float closestDistance = 20f;
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.gameObject;
                }
            }
        }
        return closestEnemy;


    }
    public void SetOwner(GameObject owner)
    {
        Owner = owner;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage, ulong clientId)
    {
        TakeDamage(damage, clientId);
    }
    public void TakeDamage(float damage, ulong clientId)
    {
        if (IsDead) return;
        if (IsServer)
        {
            Animator.SetTrigger("IsHit");
            damage = damage * (1 - DamageReduction);
            CurrentHealth.Value -= damage;
            if (CurrentHealth.Value <= 0)
            {
                HandleDeath(clientId);
            }
        }
    }

    void UpdateHealthbar(float prev, float cur)
    {
        healthFill.fillAmount = cur / MaxHealth.Value;
    }

    public virtual void IncreaseDamageReduction(float amount)
    {
        DamageReduction += amount;
    }

    public virtual void IncreaseHealth(float amount)
    {
        MaxHealth.Value += amount;
        CurrentHealth.Value += amount;
    }

    public virtual void IncreaseDamage(float amount)
    {
        Damage += amount;
    }

    public virtual void IncreaseAttackRange(float amount)
    {
        AttackRange += amount;
    }

    public virtual void IncreaseMovementSpeed(float amount)
    {
        MovementSpeed += amount;
    }

    public virtual void IncreaseBuffRadius(float amount)
    {
        BuffRadius += amount;
    }

}
