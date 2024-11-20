using Unity.Netcode;
using UnityEngine;

public class Doppelbomber : Turret
{
    public override void Update()
    {
        base.Update();

        // Update stats from the player weapon
        if (Owner != null)
        {
            AttackSpeed = playerWeapon.ShootRate / 2;
            Damage = playerWeapon.Damage * 5;
        }

    }

    public override void FireAtEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(enemy.transform.position, 20f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.CompareTag("Enemy") || hitCollider.gameObject.CompareTag("Destroyables"))
                {

                    hitCollider.gameObject.GetComponent<IDamageable>().RequestTakeDamageServerRpc(Damage, Owner.GetComponent<NetworkObject>().NetworkObjectId);
                    hitCollider.gameObject.GetComponent<Enemy>()?.OnRaycastHitServerRpc(hitCollider.gameObject.transform.position, hitCollider.gameObject.transform.forward);
                }
            }
            GameObject explosion = ObjectPooler.Instance.Spawn("BombardierExplosion", enemy.transform.position, Quaternion.identity);
            explosion.transform.localScale = new Vector3(2, 2, 2);
            explosion.transform.localRotation = enemy.transform.rotation;
            explosion.GetComponent<NetworkObject>().Spawn();
        }
    }
}
