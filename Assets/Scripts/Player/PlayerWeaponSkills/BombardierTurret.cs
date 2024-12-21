using Unity.Netcode;
using UnityEngine;

public class BombardierTurret : Turret
{
    GameObject explosion;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var levels = Owner.GetComponent<PlayerNetworkLevel>();
        AttackSpeed = Mathf.Max(1f, 2f - levels.Level.Value * 0.001f);
        Damage = levels.Level.Value * 2.5f;
    }


    public override void FireAtEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(enemy.transform.position, 10f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.CompareTag("Enemy") || hitCollider.gameObject.CompareTag("Destroyables"))
                {

                    hitCollider.gameObject.GetComponent<IDamageable>().RequestTakeDamageServerRpc(Damage, Owner.GetComponent<NetworkObject>().NetworkObjectId);
                    hitCollider.gameObject.GetComponent<Enemy>()?.OnRaycastHitServerRpc(hitCollider.gameObject.transform.position, hitCollider.gameObject.transform.forward);
                }
            }
            SpawnExplosionRpc();
            explosion.transform.position = enemy.transform.position;
            explosion.transform.localRotation = enemy.transform.rotation;

        }


    }

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnExplosionRpc()
    {

        explosion = ObjectPooler.Instance.Spawn("BombardierExplosion", Vector3.zero, Quaternion.identity);
    }


}
