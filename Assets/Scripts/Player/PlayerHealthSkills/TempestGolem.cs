using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class TempestGolem : Golem
{
    private List<NetworkClient> playersWithBuff = new List<NetworkClient>(); // Track players with the buff
    private List<Enemy> enemiesWithDebuff = new List<Enemy>(); // Track enemies with the debuff
    [SerializeField] GameObject healthBar;
    protected override void BuffEffect(float buffRadius)
    {
        foreach (NetworkClient networkClient in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject player = networkClient.PlayerObject.gameObject;
            float distanceToBuff = Vector3.Distance(player.transform.position, transform.position);

            bool isInRange = distanceToBuff <= buffRadius;

            // Check if the player has the buff or not
            if (isInRange && !playersWithBuff.Contains(networkClient))
            {
                // Apply the buff
                var movement = player.GetComponent<PlayerNetworkMovement>();
                movement.MoveSpeedIncreaseBy(2f);

                playersWithBuff.Add(networkClient); // Track this player as having the buff
            }
            else if ((!isInRange && playersWithBuff.Contains(networkClient)) || IsDead)
            {
                // Remove the buff
                var movement = player.GetComponent<PlayerNetworkMovement>();
                movement.MoveSpeedIncreaseBy(-2f);

                playersWithBuff.Remove(networkClient); // Stop tracking this player
            }
        }

        foreach (Enemy enemy in GameManager.Instance.SpawnedEnemies)
        {
            float distanceToDebuff = Vector3.Distance(enemy.transform.position, transform.position);

            bool isInRange = distanceToDebuff <= buffRadius;

            // Check if the enemy has the debuff or not
            if (isInRange && !enemiesWithDebuff.Contains(enemy))
            {
                // Apply the debuff
                var enemyMovement = enemy.GetComponent<AIKinematics>();
                enemyMovement.MoveSpeedDecreaseByPercentage(1.5f);


                enemiesWithDebuff.Add(enemy); // Track this enemy as having the debuff
            }
            else if ((!isInRange && enemiesWithDebuff.Contains(enemy)) || IsDead)
            {
                // Remove the debuff
                var enemyMovement = enemy.GetComponent<AIKinematics>();
                enemyMovement.MoveSpeedIncreaseByPercentage(1.5f);

                enemiesWithDebuff.Remove(enemy); // Stop tracking this enemy
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (IsServer)
            BuffEffect(BuffRadius);
    }

    public void DealDamageInConeTempestGolem()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, AttackRange);
        Vector3 forward = transform.forward; // Golem's forward direction

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("Destroyables"))
            {
                // Calculate the direction to the target
                Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;

                // Calculate the angle between the forward direction and the direction to the target
                float angleToTarget = Vector3.Angle(forward, directionToTarget);

                // Only apply damage if the target is within the specified cone angle
                if (angleToTarget <= 15f) // 45 degrees for example, adjust as needed for cone width
                {
                    hitCollider.GetComponent<IDamageable>().RequestTakeDamageServerRpc(Damage, Owner.GetComponent<NetworkObject>().NetworkObjectId);
                    if (IsServer)
                    {
                        GameObject lifehit = ObjectPooler.Instance.Spawn("LifeSlashHit", hitCollider.transform.position + transform.up * 2f, Quaternion.identity);
                        lifehit.GetComponent<NetworkObject>().Spawn();
                        GameObject bloodSplatter = ObjectPooler.Instance.Spawn($"BloodSplatter{Random.Range(1, 6)}", hitCollider.transform.position + transform.up * 2f, Quaternion.identity);
                        bloodSplatter.GetComponent<NetworkObject>().Spawn();
                    }
                }
            }
        }
    }




    public override void HandleDeath(ulong networkObjectId)
    {
        if (IsServer)
        {
            GameManager.Instance.SpawnedAllies.Remove(gameObject);
            StartCoroutine(DeathRoutine());
            IsDead = true;
            // Call a ClientRpc to hide the movement bar for all clients
            SetHealthBarVisibilityClientRpc(false);
        }
    }

    private IEnumerator DeathRoutine()
    {
        Animator.SetTrigger("IsDead");

        // Disable movement and interactions
        CanAttack = false;
        collider.enabled = false;
        rb.isKinematic = true;
        Agent.enabled = false;
        Agent.isStopped = true;
        Agent.canMove = false;

        yield return new WaitForSeconds(2.5f);

        Animator.enabled = false;
        StartCoroutine(ReviveAfter(ReviveTime));
    }

    [ClientRpc]
    private void SetHealthBarVisibilityClientRpc(bool isVisible)
    {
        healthBar.SetActive(isVisible);
    }


    IEnumerator ReviveAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (IsServer)
        {
            SetHealthBarVisibilityClientRpc(true);  // Show the movement bar on all clients
            CurrentHealth.Value = MaxHealth.Value;
            transform.position = Owner.transform.position + transform.forward * 2f;
            GameManager.Instance.SpawnedAllies.Add(gameObject);
            IsDead = false;
            CanAttack = true;
            collider.enabled = true;
            rb.isKinematic = false;
            Agent.enabled = true;
            Agent.isStopped = false;
            Agent.canMove = true;
            Animator.enabled = true;
            Animator.ResetTrigger("IsDead");
        }
    }



}
