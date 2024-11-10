using Unity.Netcode;
using UnityEngine;

public class BladeVortexCollisionHandler : NetworkBehaviour
{
    float damage;
    private float hitInterval; // Time interval between hits
    private float timeSinceLastHit; // Tracks time since the last hit
    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Destroyables"))
        {
            // Increment the timer
            timeSinceLastHit += Time.deltaTime;

            // Check if the interval has passed
            if (timeSinceLastHit >= hitInterval)
            {
                // Execute your desired function
                other.GetComponent<IDamageable>().RequestTakeDamageServerRpc(damage, NetworkObjectId);

                // Reset the timer
                timeSinceLastHit = 0f;
            }
        }

    }

    public void SetStats(float damage, float hitInterval)
    {
        this.damage = damage;
        this.hitInterval = hitInterval;
    }
}
