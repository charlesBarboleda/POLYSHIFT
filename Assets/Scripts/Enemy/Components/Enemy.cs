using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine.UIElements;

public enum EnemyType
{
    Melee,
    Ranged,
    Flying,
    Tank,
    Boss,
}

public abstract class Enemy : NetworkBehaviour
{
    public EnemyType enemyType;
    public Transform ClosestTarget;
    public EnemyNetworkHealth enemyHealth;
    public AIKinematics enemyMovement;
    public Rigidbody rb;
    public NetworkObject networkObject;
    public Animator animator;
    public AIPath agent;
    public List<Debuff> debuffs = new List<Debuff>();
    public float attackCooldown = 3f;
    public float moveSpeedCap = 6f;
    public float maxHealthScalingFactor = 8f;
    public float attackDamageScalingFactor = 2f;
    public float experienceDropScalingFactor = 2f;
    public bool canAttack = false;
    public List<AudioClip> attackSounds = new List<AudioClip>();
    public List<AudioClip> movementSounds = new List<AudioClip>();

    private float elapsedCooldown = 0f;
    private AudioSource audioSource;
    private List<string> bloodSplatterEffects = new List<string> { "BloodSplatter1", "BloodSplatter2", "BloodSplatter3", "BloodSplatter4", "BloodSplatter5" };

    public abstract void Attack();


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TryGetComponent(out rb);
        TryGetComponent(out enemyHealth);
        TryGetComponent(out enemyMovement);
        TryGetComponent(out networkObject);
        TryGetComponent(out audioSource);
        agent = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
        StartCoroutine(AttackGracePeriod());
        if (enemyMovement != null)
        {
            ClosestTarget = enemyMovement.ClosestPlayer;
        }
        else
        {
            Debug.LogError("Enemy Movement is null");
        }
        if (IsServer)
        {
            GameManager.Instance.SpawnedEnemies.Add(this);
        }

        InvokeRepeating("PlayRandomMovementSound", 0f, 5f);


    }
    protected virtual void Update()
    {

        if (IsServer)
        {
            AttackingLogic();
        }
    }

    void AttackingLogic()
    {
        ClosestTarget = enemyMovement.ClosestPlayer;
        if (ClosestTarget != null)
        {

            float flatDistance = Vector3.Distance(
                new Vector3(agent.destination.x, transform.position.y, agent.destination.z),
                transform.position
            );

            if (flatDistance <= agent.endReachedDistance)
            {
                agent.isStopped = true;
                if (elapsedCooldown <= 0 && canAttack)
                {
                    RotateTowardsTarget();
                    PlayRandomAttackSound();
                    Attack();
                    elapsedCooldown = attackCooldown;
                }
            }
            else
            {
                agent.isStopped = false;
            }

        }

        // Update cooldown timer
        if (elapsedCooldown > 0)
        {
            elapsedCooldown -= Time.deltaTime;
        }
    }
    void StartMoving()
    {
        agent.isStopped = false;
        enemyMovement.CanMove = true;
    }
    void StopMoving()
    {
        agent.isStopped = true;
        enemyMovement.CanMove = false;
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = (ClosestTarget.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    IEnumerator AttackGracePeriod()
    {
        canAttack = false;
        yield return new WaitForSeconds(3f);
        canAttack = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnRaycastHitServerRpc(Vector3 hitPoint, Vector3 hitNormal)
    {
        SpawnBloodSplatterClientRpc(hitPoint, hitNormal);
    }

    [ClientRpc]
    private void SpawnBloodSplatterClientRpc(Vector3 hitPoint, Vector3 hitNormal)
    {
        StartCoroutine(SpawnBloodSplatterCoroutine(hitPoint, hitNormal));
    }

    IEnumerator SpawnBloodSplatterCoroutine(Vector3 hitPoint, Vector3 hitNormal)
    {
        // Instantiate the blood splatter effect locally on each client
        int randomIndex = Random.Range(0, bloodSplatterEffects.Count);
        GameObject bloodSplatter = ObjectPooler.Instance.Spawn(bloodSplatterEffects[randomIndex], hitPoint, Quaternion.identity);
        bloodSplatter.transform.position = hitPoint;
        bloodSplatter.transform.rotation = Quaternion.LookRotation(hitNormal);

        yield return new WaitForSeconds(3f);
        // Optionally, destroy after a short time to prevent clutter
        ObjectPooler.Instance.Despawn(bloodSplatterEffects[randomIndex], bloodSplatter);
    }

    void PlayRandomMovementSound()
    {
        if (movementSounds.Count > 0)
        {
            int randomIndex = Random.Range(0, movementSounds.Count);
            audioSource.PlayOneShot(movementSounds[randomIndex]);
        }
    }

    void PlayRandomAttackSound()
    {
        if (attackSounds.Count > 0)
        {
            int randomIndex = Random.Range(0, attackSounds.Count);
            audioSource.PlayOneShot(attackSounds[randomIndex]);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        CancelInvoke();
        StopAllCoroutines();
    }







}
