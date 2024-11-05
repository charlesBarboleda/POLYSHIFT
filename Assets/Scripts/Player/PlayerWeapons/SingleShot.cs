using Unity.Netcode;
using UnityEngine;

public class SingleShot : IWeaponBehavior
{
    public void FireFirstPerson(PlayerWeapon weapon)
    {
        RaycastHit hit;
        Vector3 startPoint = weapon.bulletSpawnPoint.position;
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = weapon.Camera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out hit))
        {
            // Spawn the blood splatter effect if the hit object is an enemy
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnRaycastHitServerRpc(hit.point, hit.normal);
            }

            if (hit.collider.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                weapon.ApplyDamageServerRpc(networkObject.NetworkObjectId);
            }

            weapon.FireSingleShotServerRpc(startPoint, hit.point); // Server handles visual spawning
        }

    }

    public void FireIsometric(PlayerWeapon weapon)
    {
        RaycastHit hit;
        Vector3 startPoint = weapon.bulletSpawnPoint.position;
        Vector3 mouseScreenPos = Input.mousePosition;
        Ray ray = weapon.Camera.ScreenPointToRay(mouseScreenPos);

        if (Physics.Raycast(ray, out hit))
        {
            // Spawn the blood splatter effect if the hit object is an enemy
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnRaycastHitServerRpc(hit.point, hit.normal);
            }

            // Check for nearby enemies within a certain radius
            Collider[] nearbyEnemies = Physics.OverlapSphere(hit.point, 5); // adjust radius as needed
            NetworkObject targetNetworkObject = null;

            foreach (var col in nearbyEnemies)
            {
                if (col.CompareTag("Enemy"))
                {
                    if (col.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
                    {
                        targetNetworkObject = networkObject;
                        break;
                    }
                }
            }

            if (targetNetworkObject != null)
            {
                // If a nearby enemy is found, apply damage to it using NetworkObjectId and hit position
                weapon.ApplyDamageServerRpc(targetNetworkObject.NetworkObjectId);
            }
            else
            {
                // If no nearby enemy is found, apply damage to the direct hit target
                if (hit.collider.TryGetComponent<NetworkObject>(out NetworkObject directHitNetworkObject))
                {
                    weapon.ApplyDamageServerRpc(directHitNetworkObject.NetworkObjectId);
                }
            }

            // Call the RPC to spawn the visual effect for the shot
            weapon.FireSingleShotServerRpc(startPoint, hit.point);
        }
    }

}
