using System.Collections;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    public float ShootRate = 2f;
    public int currentAmmoCount;
    public int maxAmmoCount = 30;
    public int maxPierceTargets = 0;
    public Transform bulletSpawnPoint;
    public PlayerNetworkMovement playerNetworkMovement;
    public PlayerSkills playerSkills;
    public Camera Camera;
    public List<Debuff> weaponDebuffs = new List<Debuff>();
    PlayerAudioManager audioManager;
    IWeaponBehavior currentWeaponBehavior;
    float _nextShotTime;
    bool _isReloading;

    public override void OnNetworkSpawn()
    {
        TryGetComponent(out playerNetworkMovement);
        TryGetComponent(out audioManager);
        TryGetComponent(out playerSkills);
        currentAmmoCount = maxAmmoCount;
        SetWeaponBehavior(WeaponType.SingleShot);
        Camera = Camera.main;
        ShootRate = 1.5f;
        ReloadTime = 3f;


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
                audioManager.PlayShootSound();
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

    public void ExtendedCapacityPlus()
    {
        var extendedCapacity = playerSkills.unlockedSkills.Find(skill => skill is ExtendedCapacity) as ExtendedCapacity;

        if (extendedCapacity != null)
        {
            maxAmmoCount += 2;
            DecreaseFireRateBy(0.05f);
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }

    public void PiercingBulletsPlus()
    {
        var piercingBullets = playerSkills.unlockedSkills.Find(skill => skill is PiercingBullets) as PiercingBullets;

        if (piercingBullets != null)
        {
            maxPierceTargets += 1;
            Damage += 3f;
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }

    public void OverloadPlus()
    {
        var overload = playerSkills.unlockedSkills.Find(skill => skill is Overload) as Overload;

        if (overload != null)
        {
            var script = GetComponent<OverloadManager>();
            script.Duration += 1f;
            DecreaseReloadTimeBy(0.05f);
            DecreaseFireRateBy(0.05f);
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }

    public void WeaponMasteryPlus()
    {
        var weaponMastery = playerSkills.unlockedSkills.Find(skill => skill is WeaponMastery) as WeaponMastery;

        if (weaponMastery != null)
        {
            Damage += 2f;
            DecreaseReloadTimeBy(0.05f);
            DecreaseFireRateBy(0.05f);
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }


    public void DecreaseFireRateBy(float multiplier)
    {
        if (multiplier <= 0 || multiplier >= 1)
        {
            Debug.LogWarning("Multiplier must be between 0 and 1 (e.g., 0.2 for 20%). No changes applied.");
            return;
        }

        // Calculate the new fire rate
        float newFireRate = ShootRate * (1f - multiplier);

        // Ensure the fire rate doesn't go below a reasonable minimum (e.g., 0.1 seconds)
        float minimumFireRate = 0.01f; // Adjust this as necessary
        ShootRate = Mathf.Max(newFireRate, minimumFireRate);

        Debug.Log($"Fire rate decreased by {multiplier * 100}% to {ShootRate} seconds per shot.");
    }

    public void DecreaseReloadTimeBy(float multiplier)
    {
        if (multiplier <= 0 || multiplier >= 1)
        {
            Debug.LogWarning("Multiplier must be between 0 and 1 (e.g., 0.2 for 20%). No changes applied.");
            return;
        }

        // Calculate the new reload time
        float newReloadTime = ReloadTime * (1f - multiplier);

        // Ensure the reload time doesn't go below a reasonable minimum (e.g., 0.1 seconds)
        float minimumReloadTime = 0.01f; // Adjust this as necessary
        ReloadTime = Mathf.Max(newReloadTime, minimumReloadTime);

        Debug.Log($"Reload time decreased by {multiplier * 100}% to {ReloadTime} seconds.");
    }




    public void IncreaseWeaponDamageBy(float multiplier, float duration)
    {
        Damage *= multiplier;
        StartCoroutine(ReduceDamageAfterDuration(multiplier, duration));
    }


    IEnumerator ReduceDamageAfterDuration(float multiplier, float duration)
    {
        yield return new WaitForSeconds(duration);
        Damage /= multiplier;
    }

    public void AddWeaponDebuff(Debuff debuff)
    {
        if (!weaponDebuffs.Contains(debuff))
            weaponDebuffs.Add(debuff);
    }

    [ServerRpc]
    public void ApplyDamageServerRpc(ulong targetNetworkObjectId)
    {
        NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        if (targetObject != null && targetObject.TryGetComponent(out IDamageable iDamageable))
        {
            iDamageable.RequestTakeDamageServerRpc(Damage, NetworkObjectId); // Pass NetworkObjectId instead of clientId
            ApplyDebuffsOnHitServerRpc(targetNetworkObjectId);
        }
    }

    [ServerRpc]
    void ApplyDebuffsOnHitServerRpc(ulong networkObjectID)
    {
        if (weaponDebuffs.Count > 0)
        {
            NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectID];
            if (targetObject != null && targetObject.TryGetComponent(out DebuffManager debuffManager))
            {
                foreach (Debuff debuff in weaponDebuffs)
                {
                    Debuff debuffInstance = Instantiate(debuff);
                    debuffManager.AddDebuff(debuffInstance);
                }
            }
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
                currentAmmoCount = maxAmmoCount;
                break;
                // Add cases for other weapon types...
        }
    }


}
