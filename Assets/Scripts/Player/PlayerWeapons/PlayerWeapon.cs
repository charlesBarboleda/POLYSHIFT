using System.Collections;
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
    public float Damage = 10f;
    public float ReloadTime = 1f;
    public float ShootRate = 0.5f;
    public int currentAmmoCount;
    public int maxAmmoCount = 30;
    public int addedAmmoCount = 0;
    public float shootRateReduction = 1f;
    public float reloadTimeReduction = 1f;
    public Transform bulletSpawnPoint;
    public PlayerNetworkMovement playerNetworkMovement;
    IWeaponBehavior currentWeaponBehavior;
    public Camera Camera;
    float _nextShotTime;
    bool _isReloading;

    public override void OnNetworkSpawn()
    {
        TryGetComponent(out playerNetworkMovement);
        currentAmmoCount = maxAmmoCount;
        SetWeaponBehavior(WeaponType.SingleShot);
        Camera = Camera.main;

    }

    void Update()
    {
        if (!IsOwner) return; // Only the owner can control the weapon

        if (Input.GetMouseButton(0) && Time.time >= _nextShotTime && !_isReloading)
        {

            if (currentAmmoCount > 0)
            {


                _nextShotTime = Time.time + 1f * ShootRate;
                currentAmmoCount--;
                if (playerNetworkMovement.IsIsometric)
                    currentWeaponBehavior.FireIsometric(this);
                else
                    currentWeaponBehavior.FireFirstPerson(this);
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

    [ServerRpc]
    public void ApplyDamageServerRpc(ulong targetNetworkObjectId)
    {
        NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        if (targetObject != null && targetObject.TryGetComponent(out IDamageable iDamageable))
        {
            iDamageable.RequestTakeDamageServerRpc(Damage, NetworkObjectId); // Pass NetworkObjectId instead of clientId
        }
    }



    [ServerRpc]
    public void FireSingleShotServerRpc(Vector3 startPoint, Vector3 hitPoint)
    {
        Vector3 direction = (hitPoint - startPoint).normalized;
        SpawnBulletVisualClientRpc(startPoint, hitPoint, direction);
    }

    [ClientRpc]
    void SpawnBulletVisualClientRpc(Vector3 startPoint, Vector3 hitPoint, Vector3 direction)
    {
        SpawnBullet(startPoint, direction, Vector3.Distance(startPoint, hitPoint));
        SpawnImpact(hitPoint);
    }

    void SpawnBullet(Vector3 startPoint, Vector3 direction, float distance)
    {
        GameObject bullet = ObjectPooler.Instance.Spawn("LaserBullet", transform.position, Quaternion.identity);
        if (bullet != null)
        {
            bullet.transform.position = startPoint;
            bullet.transform.rotation = Quaternion.LookRotation(direction);
            StartCoroutine(MoveObject(bullet, direction, distance, 500f, () => ObjectPooler.Destroy(bullet)));
        }
    }

    void SpawnImpact(Vector3 position)
    {
        GameObject impact = ObjectPooler.Instance.Spawn("LaserBulletImpact", transform.position, Quaternion.identity);
        if (impact != null)
        {
            impact.transform.position = position;
            StartCoroutine(DelayedDestroy(impact, 1f));
        }
    }

    IEnumerator MoveObject(GameObject obj, Vector3 direction, float distance, float speed, System.Action onComplete)
    {
        float traveled = 0;
        while (traveled < distance)
        {
            float step = speed * Time.deltaTime;
            obj.transform.position += direction * step;
            traveled += step;
            yield return null;
        }
        onComplete?.Invoke();
    }

    IEnumerator DelayedDestroy(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPooler.Destroy(obj);
    }

    IEnumerator Reload()
    {
        _isReloading = true;
        yield return new WaitForSeconds(ReloadTime);
        currentAmmoCount = maxAmmoCount;
        _isReloading = false;
    }

    void SetWeaponBehavior(WeaponType WeaponType)
    {
        switch (WeaponType)
        {
            case WeaponType.SingleShot:
                currentWeaponBehavior = new SingleShot();
                ShootRate = 2f * shootRateReduction;
                ReloadTime = 1f * reloadTimeReduction;
                maxAmmoCount = 10 + addedAmmoCount;
                currentAmmoCount = maxAmmoCount;
                break;
                // Add cases for other weapon types...
        }
    }


}
