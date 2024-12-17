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
                if (pierceCount == 0 || pierceCount < weapon.maxPierceTargets)
                {
                    if (hit.collider != null)
                    {
                        // Retrieve the parent NetworkObject
                        NetworkObject networkObject = hit.collider.GetComponentInParent<NetworkObject>();
                        if (networkObject != null)
                        {
                            float finalDamage;

                            // Check for headshot or body shot
                            if (hit.collider.CompareTag("Head"))
                            {
                                finalDamage = weapon.Damage * 1.6f;
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, finalDamage);
                            }
                            else
                            {
                                finalDamage = weapon.Damage * 0.8f;
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, finalDamage);
                            }

                            // Apply damage to the parent NetworkObject
                            weapon.ApplyDamageServerRpc(networkObject.NetworkObjectId, finalDamage);

                            // Spawn blood splatter or other hit effects
                            Enemy enemy = networkObject.GetComponent<Enemy>();
                            if (enemy != null)
                            {
                                enemy.OnRaycastHitServerRpc(hit.point, hit.normal);
                            }
                        }

                        // Spawn visual effects for the shot
                        weapon.FireSingleShotServerRpc(startPoint, hit.point);
                        pierceCount++;
                    }
                }
                else
                {
                    break; // Stop processing further hits if max pierce count is reached
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

        if (Physics.Raycast(ray, out RaycastHit targetHit))
        {
            Vector3 direction = (targetHit.point - startPoint).normalized;
            RaycastHit[] hits = Physics.RaycastAll(startPoint, direction);

            if (hits.Length > 0)
            {
                System.Array.Sort(hits, (h1, h2) => h1.distance.CompareTo(h2.distance));

                int pierceCount = 0;

                foreach (var hit in hits)
                {
                    if (pierceCount == 0 || pierceCount < weapon.maxPierceTargets)
                    {
                        if (hit.collider.CompareTag("Player")) continue;

                        NetworkObject networkObject = hit.collider.GetComponentInParent<NetworkObject>();
                        if (networkObject != null)
                        {
                            float finalDamage;

                            // Check for headshot or body shot
                            if (hit.collider.CompareTag("Head"))
                            {
                                finalDamage = weapon.Damage * 1.6f;
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, finalDamage);
                            }
                            else
                            {
                                finalDamage = weapon.Damage * 0.8f;
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, finalDamage);
                            }

                            weapon.ApplyDamageServerRpc(networkObject.NetworkObjectId, finalDamage);

                            // Spawn blood splatter or other hit effects
                            Enemy enemy = networkObject.GetComponent<Enemy>();
                            if (enemy != null)
                            {
                                enemy.OnRaycastHitServerRpc(hit.point, hit.normal);
                            }
                        }

                        weapon.FireSingleShotServerRpc(startPoint, hit.point);
                        pierceCount++;
                    }
                    else
                    {
                        break; // Stop processing further hits if max pierce count is reached
                    }
                }
            }
        }
    }
}
