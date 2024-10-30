using System.Collections;
using Netcode.Extensions;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public enum WeaponType
{
    SingleShot,
    Shotgun,
    SingleAutomatic,
    Burst,
    Sniper,

}
public class PlayerWeapon : NetworkBehaviour
{
    public WeaponType WeaponType;
    public float Damage = 10f;
    public float ReloadTime = 1f;
    public float ShootRate = 0.5f;
    public int currentAmmoCount;
    public int maxAmmoCount = 30;
    [SerializeField] Transform bulletSpawnPoint;
    PlayerNetworkMovement playerNetworkMovement;
    Camera _camera;
    float _nextShotTime;
    bool _isReloading;

    public override void OnNetworkSpawn()
    {
        TryGetComponent(out playerNetworkMovement);
        currentAmmoCount = maxAmmoCount;
        WeaponType = WeaponType.SingleShot;
        _camera = Camera.main;

    }

    void Update()
    {
        if (!IsOwner) return; // Only the owner can control the weapon


        if (Input.GetMouseButton(0) && Time.time >= _nextShotTime && !_isReloading)
        {
            if (currentAmmoCount > 0)
            {
                Shoot(WeaponType);
                _nextShotTime = Time.time + 1f * ShootRate;
                currentAmmoCount--;
            }
            else
            {
                Debug.Log("Out of ammo, reloading...");
                StartCoroutine(Reload());
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
            case WeaponType.SingleShot:

                // Create a bullet and set its properties
                FireSingleShot();
                break;
            case WeaponType.Shotgun:

                break;
            case WeaponType.SingleAutomatic:

                break;
            case WeaponType.Burst:

                break;
            case WeaponType.Sniper:
                ;
                break;
        }
    }

    void FireSingleShot()
    {
        // Raycast a bullet from the player's position
        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.TransformDirection(Vector3.forward), out hit))
        {
            Debug.Log("Hit detected on " + hit.transform.name);
            hit.transform.GetComponent<IDamageable>()?.RequestTakeDamageServerRpc(Damage);
        }
    }




    IEnumerator Reload()
    {
        _isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(ReloadTime);
        currentAmmoCount = maxAmmoCount;
        _isReloading = false;
        Debug.Log("Reloaded");
    }

    public void SwitchWeapon(WeaponType weaponType)
    {
        WeaponType = weaponType;
    }

}
