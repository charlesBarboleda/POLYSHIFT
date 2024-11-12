using System.Collections.Generic;
using UnityEngine.UI;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public abstract class Golem : NetworkBehaviour, IDamageable
{
    [field: SerializeField] public GameObject Owner { get; private set; }
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(1000f);
    public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(1000f);

    public float Damage = 50f;
    public float AttackRange = 3f;
    public float MovementSpeed = 6f;
    public float DamageReduction = 0.5f;
    public float BuffRadius = 20f;
    public bool CanAttack = true;
    public float AttackCooldown = 3f;
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
        Animator = GetComponent<Animator>();
        Agent = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        healthFill.fillAmount = CurrentHealth.Value / MaxHealth.Value;

        if (IsServer)
        {
            // Server-only setup
            CurrentHealth.Value = MaxHealth.Value;
            Agent.maxSpeed = MovementSpeed;
            CurrentHealth.OnValueChanged += UpdateHealthbar;

            spawnedEnemies = GameManager.Instance.SpawnedEnemies;
            GameManager.Instance.SpawnedAllies.Add(gameObject);
        }
    }

    protected virtual void Update()
    {
        if (IsServer)
        {
            // Buff effect and animation update
            BuffEffect(BuffRadius);


            Animator.SetBool("IsMoving", Agent.velocity.magnitude > 0.1f);


            // Follow owner or move randomly around them if far away
            float distanceToOwner = Vector3.Distance(transform.position, Owner.transform.position);
            if (distanceToOwner > 15f)
            {
                Agent.destination = Owner.transform.position + transform.forward * 5f;

                if (distanceToOwner > 30f)
                {
                    transform.position = Owner.transform.position + transform.forward * 5f; // Teleport to owner if too far
                }
            }
            else
            {
                // Find and approach the closest target
                ClosestTarget = FindClosestEnemy();
                if (ClosestTarget != null)
                {
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
                    Vector3 randomDirection = (Vector3)Random.insideUnitCircle * 5f;
                    Agent.destination = Owner.transform.position + randomDirection;
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

    GameObject FindClosestEnemy()
    {
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy == null) continue;
            Vector3 diff = enemy.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = enemy.gameObject;
                distance = curDistance;
            }
        }
        return closest;
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
        if (IsServer)
        {
            Animator.SetTrigger("IsHit");
            CurrentHealth.Value -= damage * DamageReduction;
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
}
