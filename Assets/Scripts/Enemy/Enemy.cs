using UnityEngine;
using Unity.Netcode;
using System.Collections;

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
        GameObject bloodSplatter = ObjectPooler.Generate("BloodSplatter");
        bloodSplatter.transform.position = hitPoint;
        bloodSplatter.transform.rotation = Quaternion.LookRotation(hitNormal);

        yield return new WaitForSeconds(3f);
        // Optionally, destroy after a short time to prevent clutter
        ObjectPooler.Destroy(bloodSplatter);
    }

    public virtual void Update()
    {
        if (IsServer)
        {
            ClosestTarget = enemyMovement.ClosestPlayer;
        }
    }


}
