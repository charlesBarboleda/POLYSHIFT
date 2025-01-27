using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class WeaponPositionController : NetworkBehaviour
{
    [SerializeField] float weaponPositionYOffset = 1.5f;
    [SerializeField] float weaponPositionZOffset = 0.5f;
    [SerializeField] float weaponPositionXOffset = 0.5f;
    [SerializeField] CinemachineCamera weaponCam;

    public override void OnNetworkSpawn()
    {
        transform.SetParent(null);
    }
    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = weaponCam.transform.rotation;
        transform.position = weaponCam.transform.position + (weaponCam.transform.forward * weaponPositionZOffset) + (-weaponCam.transform.up * weaponPositionYOffset) + (weaponCam.transform.right * weaponPositionXOffset);
    }
}
