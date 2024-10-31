using UnityEngine;

public class SingleShot : IWeaponBehavior
{
    public void Fire(PlayerWeapon weapon)
    {
        RaycastHit hit;
        Vector3 startPoint = weapon.bulletSpawnPoint.position;
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = weapon.Camera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out hit))
        {
            weapon.ApplyDamage(hit);
            weapon.FireSingleShotServerRpc(startPoint, hit.point); // Server handles visual spawning
        }
    }
}
