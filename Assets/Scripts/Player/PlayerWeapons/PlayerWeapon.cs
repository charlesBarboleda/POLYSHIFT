using System.Collections;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;

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
    public VariableWithEvent<int> maxAmmoCount = new VariableWithEvent<int>(10);
    public VariableWithEvent<int> currentAmmoCount = new VariableWithEvent<int>(10);
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
    [SerializeField] TMP_Text ammoCountText;
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
        currentAmmoCount.Value = maxAmmoCount.Value;
        currentAmmoCount.OnValueChanged += UpdateAmmoCountText;
        maxAmmoCount.OnValueChanged += UpdateAmmoCountText;
        SetWeaponBehavior(WeaponType.SingleShot);
        Camera = Camera.main;
        Debug.Log("Setting camera to main camera in scene.");
        ShootRate = 0.5f;
        ReloadTime = 4f;
        maxAmmoCount.Value = 10;
        currentAmmoCount.Value = maxAmmoCount.Value;

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
            if (currentAmmoCount.Value > 0)
            {


                _nextShotTime = Time.time + 1f * ShootRate;
                currentAmmoCount.Value--;
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
        if (currentAmmoCount.Value <= 0 && !_isReloading)
        {
            Debug.Log("Out of ammo, reloading...");
            StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.R) && !_isReloading && currentAmmoCount.Value < maxAmmoCount.Value)
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
                    // Apply standing-still buffs, remove movement buffs
                    DualStanceApplyStandingStillBuffs();
                    DualStanceRemoveMovementBuffs();
                    _isStandingStill = true; // Correctly mark the player as standing still
                    _dualStanceBuffApplied = false;
                }

            }
        }
    }

    void UpdateAmmoCountText(int currentAmmo)
    {
        ammoCountText.text = $"{currentAmmo} / {maxAmmoCount.Value}";
    }
    private void DualStanceApplyMovementBuffs()
    {
        Damage *= 2f;
        playerNetworkMovement.MoveSpeed *= 1.5f;
        Debug.Log("Movement buffs applied.");
    }

    private void DualStanceRemoveMovementBuffs()
    {
        Damage /= 2f;
        playerNetworkMovement.MoveSpeed /= 1.5f;
        Debug.Log("Movement buffs removed.");
    }

    float originalShootRate;
    float originalReloadTime;

    private void DualStanceApplyStandingStillBuffs()
    {
        originalShootRate = ShootRate;
        originalReloadTime = ReloadTime;
        ShootRate *= 0.5f;
        ReloadTime *= 0.5f;
        Debug.Log("Standing-still buffs applied.");
    }

    private void DualStanceRemoveStandingStillBuffs()
    {
        ShootRate = originalShootRate;
        ReloadTime = originalReloadTime;
        Debug.Log("Standing-still buffs removed.");
    }

    public void DualStance()
    {
        _dualStance = true;
        if (_isStandingStill)
        {
            DualStanceApplyStandingStillBuffs();
        }
    }

    public void DualStancePlus()
    {
        var dualStance = playerSkills.unlockedSkills.Find(skill => skill is DualStance) as DualStance;

        if (dualStance != null)
        {
            Damage += 7.5f;
            DecreaseReloadTimeBy(0.075f);
            DecreaseFireRateBy(0.075f);
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
            Damage += 10f;
            DecreaseFireRateBy(0.05f);
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
            Damage += 5f;
            DecreaseFireRateBy(0.05f);
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
            kineticBurstKnockbackForce += 1f;
            kineticBurstDamageMultiplier += 1f;
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
            maxAmmoCount.Value += 3;
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
            Damage += 7.5f;
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
            Damage += 7.5f;
            DecreaseReloadTimeBy(0.1f);
            DecreaseFireRateBy(0.1f);
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




    public void AddWeaponDebuff(Debuff debuff)
    {
        if (!weaponDebuffs.Contains(debuff))
            weaponDebuffs.Add(debuff);
    }

    [ServerRpc]
    public void ApplyDamageServerRpc(ulong targetNetworkObjectId, float damage)
    {
        NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        if (targetObject != null && targetObject.TryGetComponent(out IDamageable iDamageable))
        {
            iDamageable.RequestTakeDamageServerRpc(damage, NetworkObjectId); // Pass NetworkObjectId instead of clientId
            ApplyDebuffsOnHitServerRpc(targetNetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
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



    [Rpc(SendTo.ClientsAndHost)]
    public void FireSingleShotServerRpc(Vector3 startPoint, Vector3 hitPoint)
    {
        SpawnBullet(startPoint, hitPoint, 500f);
        SpawnImpact(hitPoint);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnMuzzleFlashServerRpc()
    {
        // Spawn the muzzle flash prefab
        GameObject muzzleFlash = ObjectPooler.Instance.Spawn("LaserMuzzleFlash", bulletSpawnPoint.position, Quaternion.identity);


        // Align the muzzle flash with the bullet spawn point
        muzzleFlash.transform.position = bulletSpawnPoint.position;
        muzzleFlash.transform.rotation = Quaternion.LookRotation(bulletSpawnPoint.forward);

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


    [Rpc(SendTo.ClientsAndHost)]
    public void KineticBurstVisualEffectServerRpc()
    {
        GameObject lightningNova = ObjectPooler.Instance.Spawn("LightningNova", bulletSpawnPoint.position, Quaternion.identity);
        lightningNova.transform.localRotation = Quaternion.Euler(-90, 0, 90);

        GameObject lightningSphereBlast = ObjectPooler.Instance.Spawn("LightningSphereBlast", bulletSpawnPoint.position, Quaternion.identity);
        lightningSphereBlast.transform.localRotation = Quaternion.Euler(-90, 0, 90);
    }

    IEnumerator Reload()
    {
        if (currentAmmoCount.Value <= 0)
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
        currentAmmoCount.Value = maxAmmoCount.Value;
        _isReloading = false;
    }

    void SetWeaponBehavior(WeaponType WeaponType)
    {
        switch (WeaponType)
        {
            case WeaponType.SingleShot:
                currentWeaponBehavior = new SingleShot();
                currentAmmoCount.Value = maxAmmoCount.Value;
                break;
                // Add cases for other weapon types...
        }
    }

    public void ResetWeapon()
    {
        maxAmmoCount.Value = 10;
        currentAmmoCount.Value = maxAmmoCount.Value;
        ShootRate = 2f;
        ReloadTime = 4f;
        Damage = 10f;
        SetWeaponBehavior(WeaponType.SingleShot);
    }
}
