
using Unity.Netcode;
using UnityEngine;

public class SingleShot : IWeaponBehavior
{
    public void FireFirstPerson(PlayerWeapon weapon)
    {
        Vector3 startPoint = weapon.bulletSpawnPoint.position;
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = weapon.Camera.ScreenPointToRay(screenCenter);

        weapon.SpawnMuzzleFlashServerRpc();

        // Perform a RaycastAll to detect all objects along the bullet's path
        RaycastHit[] hits = Physics.RaycastAll(ray);

        // Pierce Logic
        if (hits.Length > 0)
        {
            // Sort hits by distance from the starting point
            System.Array.Sort(hits, (h1, h2) => h1.distance.CompareTo(h2.distance));

            int pierceCount = 0;

            foreach (var hit in hits)
            {
                // Always apply damage to the first target, regardless of piercing
                if (pierceCount == 0 || pierceCount < weapon.maxPierceTargets)
                {
                    // Apply damage to the hit object
                    if (hit.collider.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
                    {
                        if (hit.collider.CompareTag("Player")) continue; // Skip the player object

                        weapon.ApplyDamageServerRpc(networkObject.NetworkObjectId);
                        PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, hit.collider.CompareTag("Destroyables") ? 1f : weapon.Damage);
                        // Spawn the blood splatter effect if the hit object is an enemy
                        Enemy enemy = hit.collider.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            enemy.OnRaycastHitServerRpc(hit.point, hit.normal);
                        }
                    }

                    // Spawn visual effects for the shot
                    Debug.Log("Calling FireSingleShotServerRpc from SingleShot");
                    weapon.FireSingleShotServerRpc(startPoint, hit.point);
                    Debug.Log("Finished FireSingleShotServerRpc from SingleShot");
                    pierceCount++;
                }
                else
                {
                    // Stop processing further hits if maxPierceTargets is reached
                    break;
                }
            }
        }
    }



    public void FireIsometric(PlayerWeapon weapon)
    {
        Vector3 startPoint = weapon.bulletSpawnPoint.position;
        Vector3 mouseScreenPos = Input.mousePosition;
        Ray ray = weapon.Camera.ScreenPointToRay(mouseScreenPos);

        weapon.SpawnMuzzleFlashServerRpc();

        // Perform a raycast from the camera to detect what the mouse is pointing at
        if (Physics.Raycast(ray, out RaycastHit targetHit))
        {
            // Calculate direction from bullet spawn point to the target hit point
            Vector3 direction = (targetHit.point - startPoint).normalized;

            // Perform a RaycastAll from the bullet spawn point in the calculated direction
            RaycastHit[] hits = Physics.RaycastAll(startPoint, direction);
            if (hits.Length > 0)
            {
                // Sort hits by distance from the starting point
                System.Array.Sort(hits, (h1, h2) => h1.distance.CompareTo(h2.distance));

                int pierceCount = 0;

                foreach (var hit in hits)
                {
                    if (hit.collider.CompareTag("Player")) continue; // Skip the player object

                    // Always apply damage to the first target, regardless of piercing
                    if (pierceCount == 0 || pierceCount < weapon.maxPierceTargets)
                    {
                        // Apply damage to the hit object
                        if (hit.collider.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
                        {
                            // Aim assist: check for nearby enemies
                            Collider[] nearbyEnemies = Physics.OverlapSphere(hit.point, 0.5f); // Adjust radius for aim assist
                            NetworkObject targetNetworkObject = null;

                            foreach (var col in nearbyEnemies)
                            {
                                if (col.CompareTag("Enemy"))
                                {
                                    if (col.TryGetComponent<NetworkObject>(out NetworkObject nearbyNetworkObject))
                                    {
                                        targetNetworkObject = nearbyNetworkObject;
                                        break;
                                    }
                                }
                            }

                            // Apply damage to the nearby enemy (if found) or the directly hit target
                            if (targetNetworkObject != null)
                            {
                                weapon.ApplyDamageServerRpc(targetNetworkObject.NetworkObjectId);
                                targetNetworkObject.GetComponent<Enemy>().OnRaycastHitServerRpc(hit.point, hit.normal);
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, weapon.Damage);


                            }
                            else
                            {
                                weapon.ApplyDamageServerRpc(networkObject.NetworkObjectId);
                                networkObject.GetComponent<Enemy>().OnRaycastHitServerRpc(hit.point, hit.normal);
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, weapon.Damage);

                            }

                            // Spawn impact visual at the hit point
                            weapon.FireSingleShotServerRpc(startPoint, hit.point);

                            pierceCount++;
                        }
                        else
                        {
                            // Stop processing further hits if maxPierceTargets is reached
                            break;
                        }
                    }
                }
            }
        }



    }
}
