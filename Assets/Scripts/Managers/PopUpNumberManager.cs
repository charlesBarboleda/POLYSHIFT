using DamageNumbersPro;
using UnityEngine;

public class PopUpNumberManager : MonoBehaviour
{
    public static PopUpNumberManager Instance { get; private set; }

    [SerializeField] DamageNumber XPNumberPrefab;
    [SerializeField] DamageNumber WeaponDamageNumberPrefab;
    [SerializeField] DamageNumber MeleeDamageNumberPrefab;
    [SerializeField] DamageNumber HealNumberPrefab;
    [SerializeField] DamageNumber StaggerNumberPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void SpawnStaggerNumber(Vector3 position, float amount)
    {
        DamageNumber number = StaggerNumberPrefab.Spawn(position, amount);
        number.SetScale(1.5f);
    }

    public void SpawnXPNumber(Vector3 position, float amount)
    {
        DamageNumber number = XPNumberPrefab.Spawn(position, amount);
        number.SetScale(2f);
    }

    public void SpawnWeaponDamageNumber(Vector3 position, float amount)
    {
        DamageNumber number = WeaponDamageNumberPrefab.Spawn(position, amount);
        number.SetScale(2f);
    }

    public void SpawnMeleeDamageNumber(Vector3 position, float amount)
    {
        DamageNumber number = MeleeDamageNumberPrefab.Spawn(position, amount);
        number.transform.position += Vector3.up;
        number.SetScale(2f);
    }

    public void SpawnHealNumber(Vector3 position, float amount)
    {
        DamageNumber number = HealNumberPrefab.Spawn(position, amount);
        number.SetScale(1.5f);
    }



}
