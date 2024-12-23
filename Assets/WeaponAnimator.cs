using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class WeaponAnimator : NetworkBehaviour
{
    [SerializeField] Renderer axe1;
    [SerializeField] Renderer axe2;
    [SerializeField] Material glowMaterial;
    [SerializeField] GameObject weaponTrail;
    Material axe1Material;
    Material axe2Material;

    void Start()
    {
        axe1Material = axe1.material;
        axe2Material = axe2.material;
    }

    public void SetGlow()
    {
        axe1.material = glowMaterial;
        axe2.material = glowMaterial;
    }

    public void ResetGlow()
    {
        axe1.material = axe1Material;
        axe2.material = axe2Material;
    }

    public void EnableWeaponTrail()
    {
        weaponTrail.SetActive(true);
    }

    public void DisableWeaponTrail()
    {
        weaponTrail.SetActive(false);
    }


}
