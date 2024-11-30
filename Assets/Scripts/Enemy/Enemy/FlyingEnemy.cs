using Unity.Netcode;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    public float attackRange = 10f;
    public float attackDamage = 50f;
    public Transform projectileSpawnPoint;
    EnemyNetworkHealth health;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enemyType = EnemyType.Flying;
        transform.position = Vector3.up * Random.Range(6, 8);

        if (IsServer)
        {
            health = GetComponent<EnemyNetworkHealth>();
            health.MaxHealth += GameManager.Instance.GameLevel.Value * 5;
            health.CurrentHealth.Value = health.MaxHealth;
            attackDamage += GameManager.Instance.GameLevel.Value * 3;
            health.ExperienceDrop += GameManager.Instance.GameLevel.Value;

        }
    }

    protected override void Attack()
    {
        if (ClosestTarget != null)
        {
            Debug.Log("Attacking");
            animator.SetTrigger("isAttacking");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnProjectileServerRpc()
    {
        GameObject projectile = ObjectPooler.Instance.Spawn("RedDragonProjectile", projectileSpawnPoint.position, Quaternion.identity);


        // Calculate the direction to the target
        Vector3 direction = (ClosestTarget.position - projectile.transform.position).normalized;
        // Rotate the projectile to face the target
        projectile.transform.rotation = Quaternion.LookRotation(direction);
        foreach (Transform child in projectile.transform)
        {
            child.transform.rotation = Quaternion.LookRotation(direction);
        }

        // Initialize the projectile with the target and damage values
        projectile.GetComponent<Projectile>().Initialize(ClosestTarget, 5f, attackDamage);
        projectile.GetComponent<NetworkObject>().Spawn();

        Debug.DrawRay(projectile.transform.position, projectile.transform.forward * 5, Color.red, 2f);
    }
}
