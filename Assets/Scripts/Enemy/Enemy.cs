using UnityEngine;
using Unity.Netcode;

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
    public EnemyNetworkHealth enemyHealth;
    public AIKinematics enemyMovement;
    public Rigidbody rb;
    public NetworkObject networkObject;
    public Transform ClosestTarget;

    protected abstract void Attack();
    public abstract void OnRaycastHit(Vector3 hitPoint, Vector3 hitNormal);

    public override void OnNetworkSpawn()
    {
        TryGetComponent(out rb);
        TryGetComponent(out enemyHealth);
        TryGetComponent(out enemyMovement);
        TryGetComponent(out networkObject);
        if (enemyMovement != null)
        {
            ClosestTarget = enemyMovement.ClosestPlayer;
        }
        else
        {
            Debug.LogError("Enemy Movement is null");
        }

    }

    public virtual void Update()
    {
        if (IsServer)
        {
            ClosestTarget = enemyMovement.ClosestPlayer;
        }
    }


}
