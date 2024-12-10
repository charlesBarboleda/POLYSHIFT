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
    public int maxAmmoCount = 10;
    public int currentAmmoCount;
    public int maxPierceTargets = 0;
    public Transform bulletSpawnPoint;
    public PlayerNetworkMovement playerNetworkMovement;
    public PlayerSkills playerSkills;
    public Camera Camera;
    public List<Debuff> weaponDebuffs = new List<Debuff>();
    public float kineticBurstKnockbackForce = 10f;
    public float kineticBurstDamageMultiplier = 2f;
    public float kineticBurstRange = 5f;
    public Animator weaponAnimator;
    PlayerAudioManager audioManager;
    IWeaponBehavior currentWeaponBehavior;
    WeaponAnimationEvents weaponAnimationEvents;
    float _nextShotTime;
    bool _isReloading;
    bool _kineticBurst;
    bool _dualStance;
    bool _isStandingStill = true; // Tracks if the player is standing still
    bool _dualStanceBuffApplied = false;  // Tracks if movement buffs are applied;


    public override void OnNetworkSpawn()
    {
        TryGetComponent(out playerNetworkMovement);
        TryGetComponent(out audioManager);
        TryGetComponent(out playerSkills);
        TryGetComponent(out weaponAnimationEvents);
        currentAmmoCount = maxAmmoCount;
        SetWeaponBehavior(WeaponType.SingleShot);
        Camera = Camera.main;
        Debug.Log("Setting camera to main camera in scene.");
        ShootRate = 1f;
        ReloadTime = 4f;
        maxAmmoCount = 10;
        currentAmmoCount = maxAmmoCount;

        // Add a debug log that references the current scene
        Debug.Log("PlayerWeapon spawned in scene.");

    }

    void Update()
    {
        if (!IsOwner) return; // Only the owner can control the weapon

        if (Camera == null)
        {
            Camera = Camera.main;
        }
        if (Input.GetMouseButton(0) && Time.time >= _nextShotTime && !_isReloading && !playerSkills.SkillTreeOpen())
        {
            if (currentAmmoCount > 0)
            {


                _nextShotTime = Time.time + 1f * ShootRate;
                currentAmmoCount--;
                if (playerNetworkMovement.IsIsometric.Value)
                    currentWeaponBehavior.FireIsometric(this);
                else
                    currentWeaponBehavior.FireFirstPerson(this);

                audioManager.PlayShootSound();
                weaponAnimator.SetTrigger("isShooting");
            }
            else
            {
                Debug.Log("Out of ammo, reloading...");
                StartCoroutine(Reload());
            }

        }
        if (currentAmmoCount <= 0 && !_isReloading)
        {
            Debug.Log("Out of ammo, reloading...");
            StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.R) && !_isReloading && currentAmmoCount < maxAmmoCount)
        {
            StartCoroutine(Reload());
        }

        bool isMoving = playerNetworkMovement.inputDirection.x != 0 || playerNetworkMovement.inputDirection.y != 0;

        if (_dualStance)
        {
            if (isMoving)
            {
                if (!_dualStanceBuffApplied)
                {
                    // Transition to moving: apply movement buffs, remove standing-still buffs
                    DualStanceApplyMovementBuffs();
                    DualStanceRemoveStandingStillBuffs();
                    _dualStanceBuffApplied = true;
                    _isStandingStill = false;
                }
            }
            else // Player is standing still
            {
                if (!_isStandingStill)
                {
                    // Transition to standing still: apply standing-still buffs, remove movement buffs
                    DualStanceApplyStandingStillBuffs();
                    DualStanceRemoveMovementBuffs();
                    _isStandingStill = true;
                    _dualStanceBuffApplied = false;
                }
            }
        }
    }
    private void DualStanceApplyMovementBuffs()
    {
        Damage *= 1.3f;
        playerNetworkMovement.MoveSpeed *= 1.3f;
        Debug.Log("Movement buffs applied.");
    }

    private void DualStanceRemoveMovementBuffs()
    {
        Damage /= 1.3f;
        playerNetworkMovement.MoveSpeed /= 1.3f;
        Debug.Log("Movement buffs removed.");
    }

    private void DualStanceApplyStandingStillBuffs()
    {
        ShootRate *= 0.7f;  // 50% decreased fire rate = 0.7 multiplier
        ReloadTime *= 0.7f; // 50% decreased reload time = 0.7 multiplier
        Debug.Log("Standing-still buffs applied.");
    }

    private void DualStanceRemoveStandingStillBuffs()
    {
        ShootRate /= 0.7f;  // Reset to original fire rate
        ReloadTime /= 0.7f; // Reset to original reload time
        Debug.Log("Standing-still buffs removed.");
    }

    public void DualStance()
    {
        _dualStance = true;
    }
    public void DualStancePlus()
    {
        var dualStance = playerSkills.unlockedSkills.Find(skill => skill is DualStance) as DualStance;

        if (dualStance != null)
        {
            Damage += 5f;
            DecreaseReloadTimeByServerRpc(0.05f);
            DecreaseFireRateByServerRpc(0.05f);
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }

    public void BombardierSentryPlus()
    {
        var bombardierSentry = playerSkills.unlockedSkills.Find(skill => skill is BombardierSentry) as BombardierSentry;

        if (bombardierSentry != null)
        {
            Damage += 6f;
            DecreaseFireRateByServerRpc(0.025f);
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }

    public void MimicSentryPlus()
    {
        var mimicSentry = playerSkills.unlockedSkills.Find(skill => skill is MimicSentry) as MimicSentry;

        if (mimicSentry != null)
        {
            Damage += 3f;
            DecreaseFireRateByServerRpc(0.05f);
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }

    public void KineticBurst()
    {
        _kineticBurst = true;
    }

    public void KineticBurstPlus()
    {
        var kineticBurst = playerSkills.unlockedSkills.Find(skill => skill is KineticBurst) as KineticBurst;

        if (kineticBurst != null)
        {
            kineticBurstKnockbackForce += 2f;
            kineticBurstDamageMultiplier += 2f;
            kineticBurstRange += 1f;
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }

    public void ExtendedCapacityPlus()
    {
        var extendedCapacity = playerSkills.unlockedSkills.Find(skill => skill is ExtendedCapacity) as ExtendedCapacity;

        if (extendedCapacity != null)
        {
            maxAmmoCount += 2;
            DecreaseFireRateByServerRpc(0.05f);
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
            DecreaseReloadTimeByServerRpc(0.05f);
            DecreaseFireRateByServerRpc(0.05f);
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
            DecreaseReloadTimeByServerRpc(0.05f);
            DecreaseFireRateByServerRpc(0.05f);
        }
        else
        {
            playerSkills.PermanentTravelNodeStatIncrease();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DecreaseFireRateByServerRpc(float multiplier)
    {
        DecreaseFireRateByClientRpc(multiplier);
    }

    [ClientRpc]
    public void DecreaseFireRateByClientRpc(float multiplier)
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

    [ServerRpc]
    public void DecreaseReloadTimeByServerRpc(float multiplier)
    {
        DecreaseReloadTimeByClientRpc(multiplier);
    }

    [ClientRpc]
    public void DecreaseReloadTimeByClientRpc(float multiplier)
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

    [ServerRpc(RequireOwnership = false)]
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
        SpawnBullet(startPoint, hitPoint, 500f);
        SpawnImpact(hitPoint);
    }
    [ClientRpc]
    void SpawnMuzzleFlashClientRpc()
    {
        Debug.Log("Spawning muzzle flash");

        // Spawn the muzzle flash prefab
        GameObject muzzleFlash = ObjectPooler.Instance.Spawn("LaserMuzzleFlash", bulletSpawnPoint.position, Quaternion.identity);

        if (muzzleFlash == null)
        {
            Debug.LogError("Failed to spawn muzzle flash!");
            return;
        }

        // Align the muzzle flash with the bullet spawn point
        muzzleFlash.transform.position = bulletSpawnPoint.position;
        muzzleFlash.transform.rotation = Quaternion.LookRotation(bulletSpawnPoint.forward);

        Debug.Log($"Muzzle flash spawned at position: {bulletSpawnPoint.position}, forward: {bulletSpawnPoint.forward}");

        // Spawn the muzzle flash as a networked object
        muzzleFlash.GetComponent<NetworkObject>().Spawn();
    }



    [ServerRpc]
    public void SpawnMuzzleFlashServerRpc()
    {
        SpawnMuzzleFlashClientRpc();
    }

    void SpawnBullet(Vector3 startPoint, Vector3 hitPoint, float speed)
    {


        // Spawn the bullet at the start point
        GameObject bullet = ObjectPooler.Instance.Spawn("LaserBullet", startPoint, Quaternion.identity);
        if (bullet != null)
        {
            // Calculate the direction from start point to hit point
            Vector3 direction = (hitPoint - startPoint).normalized;

            // Set the bullet's rotation to face the direction it will travel
            bullet.transform.rotation = Quaternion.LookRotation(direction);

            // Move the bullet towards the hit point and destroy it after the journey
            StartCoroutine(MoveObject(bullet, direction, Vector3.Distance(startPoint, hitPoint), speed, () => Destroy(bullet)));
        }
    }


    void SpawnImpact(Vector3 position)
    {
        GameObject impact = ObjectPooler.Instance.Spawn("LaserBulletImpact", transform.position, Quaternion.identity);
        if (impact != null)
        {
            impact.transform.position = position;
            impact.GetComponent<NetworkObject>().Spawn();
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


    [ServerRpc]
    public void KineticBurstVisualEffectServerRpc()
    {
        GameObject lightningNova = ObjectPooler.Instance.Spawn("LightningNova", bulletSpawnPoint.position, Quaternion.identity);
        lightningNova.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        lightningNova.GetComponent<NetworkObject>().Spawn();

        GameObject lightningSphereBlast = ObjectPooler.Instance.Spawn("LightningSphereBlast", bulletSpawnPoint.position, Quaternion.identity);
        lightningSphereBlast.transform.localRotation = Quaternion.Euler(-90, 0, 90);
        lightningSphereBlast.GetComponent<NetworkObject>().Spawn();
    }

    IEnumerator Reload()
    {
        if (currentAmmoCount <= 0)
        {
            // Explode
            if (_kineticBurst)
            {

                KineticBurstVisualEffectServerRpc();
                playerSkills.DealDamageInCircle(bulletSpawnPoint.position, kineticBurstRange, Damage * kineticBurstDamageMultiplier, kineticBurstKnockbackForce);
            }
        }
        weaponAnimationEvents.TurnWeaponRedServerRpc();
        audioManager.PlayReloadSound();
        _isReloading = true;
        yield return new WaitForSeconds(ReloadTime);
        weaponAnimationEvents.TurnWeaponWhiteServerRpc();
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

    public void ResetWeapon()
    {
        maxAmmoCount = 10;
        currentAmmoCount = maxAmmoCount;
        ShootRate = 2f;
        ReloadTime = 4f;
        Damage = 10f;
        SetWeaponBehavior(WeaponType.SingleShot);
    }
}
