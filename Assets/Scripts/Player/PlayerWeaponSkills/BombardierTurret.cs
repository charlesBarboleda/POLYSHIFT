using Unity.Netcode;
using UnityEngine;

public class BombardierTurret : Turret
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var levels = Owner.GetComponent<PlayerNetworkLevel>();
        AttackSpeed = 1.5f / (1 + levels.Level.Value / 75f);
        Damage = levels.Level.Value * 10f;
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
            GameObject explosion = ObjectPooler.Instance.Spawn("BombardierExplosion", enemy.transform.position, Quaternion.identity);
            explosion.transform.localRotation = enemy.transform.rotation;
            explosion.GetComponent<NetworkObject>().Spawn();
        }
    }


}
