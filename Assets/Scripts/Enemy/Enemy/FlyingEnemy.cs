using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    public float attackRange = 10f;
    public float attackDamage = 50f;
    public Transform projectileSpawnPoint;
    public string projectilePoolTag;
    EnemyNetworkHealth health;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enemyType = EnemyType.Flying;

        if (IsServer)
        {
            health = GetComponent<EnemyNetworkHealth>();
            health.MaxHealth += GameManager.Instance.GameLevel.Value * maxHealthScalingFactor;
            health.CurrentHealth.Value = health.MaxHealth;
            attackDamage += GameManager.Instance.GameLevel.Value * attackDamageScalingFactor;
            health.ExperienceDrop += GameManager.Instance.GameLevel.Value * experienceDropScalingFactor;
            enemyMovement.MoveSpeed += Random.Range(GameManager.Instance.GameLevel.Value * 0.3f, GameManager.Instance.GameLevel.Value * 0.5f);
            enemyMovement.MoveSpeed = Mathf.Clamp(enemyMovement.MoveSpeed, 0f, moveSpeedCap);

        }
    }

    public override IEnumerator Attack()
    {
        if (ClosestTarget != null)
        {
            isAttacking = true;
            animator.SetTrigger("isAttacking");

            // Wait until the attack animation starts
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));

            // Wait for the duration of the attack animation
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            isAttacking = false;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnProjectileServerRpc()
    {
        GameObject projectile = ObjectPooler.Instance.Spawn(projectilePoolTag, projectileSpawnPoint.position, Quaternion.identity);


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
