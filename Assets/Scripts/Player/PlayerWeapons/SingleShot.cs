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
                                finalDamage = weapon.Damage * Random.Range(1.5f, 2f);
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, finalDamage);
                            }
                            else
                            {
                                finalDamage = weapon.Damage * Random.Range(0.4f, 0.8f);
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, finalDamage);
                            }

                            float finalStaggerDamage = finalDamage / 150;

                            var bossHealth = networkObject.GetComponent<BossEnemyNetworkHealth>();
                            if (networkObject.GetComponent<IStaggerable>() != null && bossHealth != null)
                            {
                                if (bossHealth.CanBeStaggered.Value)
                                {
                                    networkObject.GetComponent<IStaggerable>().ApplyStaggerDamageServerRpc(finalStaggerDamage);
                                    PopUpNumberManager.Instance.SpawnStaggerNumber(hit.point, finalStaggerDamage);
                                }
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
                                finalDamage = weapon.Damage * Random.Range(1.5f, 2f);
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, finalDamage);
                            }
                            else
                            {
                                finalDamage = weapon.Damage * Random.Range(0.4f, 0.8f);
                                PopUpNumberManager.Instance.SpawnWeaponDamageNumber(hit.point, finalDamage);
                            }

                            float finalStaggerDamage = finalDamage / 150;
                            networkObject.TryGetComponent(out BossEnemyNetworkHealth bossHealth);
                            if (networkObject.GetComponent<IStaggerable>() != null && bossHealth != null)
                            {
                                if (bossHealth.CanBeStaggered.Value)
                                {
                                    networkObject.GetComponent<IStaggerable>().ApplyStaggerDamageServerRpc(finalStaggerDamage);
                                    PopUpNumberManager.Instance.SpawnStaggerNumber(hit.point, finalStaggerDamage);
                                }
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
