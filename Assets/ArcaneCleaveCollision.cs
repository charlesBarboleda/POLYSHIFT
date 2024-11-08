using UnityEngine;

public class ArcaneCleaveCollision : MonoBehaviour
{
    float damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Destroyables") || other.gameObject.CompareTag("Enemy"))
        {
            IDamageable health = other.gameObject.GetComponent<IDamageable>();
            health.RequestTakeDamageServerRpc(damage, 0);
            Debug.Log("ArcaneCleave collided with " + other.gameObject.name);
        }
    }
    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
}
