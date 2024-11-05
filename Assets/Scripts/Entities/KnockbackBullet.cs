using UnityEngine;

public class KnockbackBullet : Bullet
{
    public float KnockbackForce = 1f;
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            IDamageable health = other.gameObject.GetComponent<IDamageable>();
            health.TakeDamage(Damage, OwnerClientId);
            Debug.Log("KnockbackBullet collided with " + other.gameObject.name);
            DestroyBullet();
        }
    }

}
