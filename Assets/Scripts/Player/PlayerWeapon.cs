using Netcode.Extensions;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public enum WeaponType
{
    Pistol,
    Shotgun,
    AssaultRifle,
    SniperRifle,
    RocketLauncher,
    GrenadeLauncher,
    Flamethrower,
}
public class PlayerWeapon : NetworkBehaviour
{
    public WeaponType WeaponType;
    public float Damage = 10f;
    public float bulletLifetime = 3f;
    public float ReloadTime = 1f;
    public float ShootRate = 0.5f;

    public float ProjectileSpeed = 100f;
    public int currentAmmoCount;
    public int maxAmmoCount = 30;
    public string bulletTag;
    [SerializeField] Transform bulletSpawnPoint;
    PlayerNetworkMovement playerNetworkMovement;
    float _nextShotTime;
    bool _isReloading;

    public override void OnNetworkSpawn()
    {
        TryGetComponent(out playerNetworkMovement);
        currentAmmoCount = maxAmmoCount;
        WeaponType = WeaponType.Pistol;
    }

    void Update()
    {
        if (!IsOwner) return; // Only the owner can control the weapon

        if (Input.GetMouseButtonDown(0) && Time.time >= _nextShotTime && !_isReloading)
        {
            if (currentAmmoCount > 0)
            {
                Shoot(WeaponType);
                _nextShotTime = Time.time + 1f / ShootRate;
                currentAmmoCount--;
            }
            else
            {
                Debug.Log("Out of ammo, reloading...");
                Reload();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }

    }

    public void Shoot(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:

                // Create a bullet and set its properties
                FirePistol();
                break;
            case WeaponType.Shotgun:
                Debug.Log("Enemy shooting shotgun");
                break;
            case WeaponType.AssaultRifle:
                Debug.Log("Enemy shooting assault rifle");
                break;
            case WeaponType.SniperRifle:
                Debug.Log("Enemy shooting sniper rifle");
                break;
            case WeaponType.RocketLauncher:
                Debug.Log("Enemy shooting rocket launcher");
                break;
            case WeaponType.GrenadeLauncher:
                Debug.Log("Enemy shooting grenade launcher");
                break;
            case WeaponType.Flamethrower:
                Debug.Log("Enemy shooting flamethrower");
                break;
        }
    }

    void FirePistol()
    {
        // Obtain a bullet from the object pool
        GameObject bullet = NetworkObjectPool.Instance.GetNetworkObject(bulletTag).gameObject;
        bullet.transform.position = bulletSpawnPoint.position;

        // Set up the bullet's properties
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(Damage, ProjectileSpeed, bulletLifetime, bulletTag);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (playerNetworkMovement.IsIsometric.Value)
        {
            // Get the target point in world space where the cursor is pointing
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 direction = (hit.point - bulletSpawnPoint.position).normalized;
                rb.linearVelocity = direction * ProjectileSpeed;
            }
        }
        else
        {
            // In first-person view, raycast from the center of the camera view
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of the screen
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 direction = (hit.point - bulletSpawnPoint.position).normalized;
                rb.linearVelocity = direction * ProjectileSpeed;
            }
            else
            {
                // If no hit, shoot straight from the camera center
                Vector3 direction = ray.direction.normalized;
                rb.linearVelocity = direction * ProjectileSpeed;
            }
        }
    }




    void Reload()
    {
        Debug.Log("Enemy reloading");
    }

    void SwitchWeapon(WeaponType weaponType)
    {
        WeaponType = weaponType;
    }

}
