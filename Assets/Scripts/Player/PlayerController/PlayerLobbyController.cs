using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerLobbyController : NetworkBehaviour
{
    PlayerCameraBehavior _playerCameraBehavior;
    PlayerNetworkMovement _playerNetworkMovement;
    PlayerNetworkRotation _playerNetworkRotation;
    PlayerSkills _playerSkills;
    PlayerWeapon _playerWeapon;
    CameraController _cameraController;
    [SerializeField] GameObject isometricCamera;
    [SerializeField] GameObject firstPersonCamera;

    public override void OnNetworkSpawn()
    {
        _playerCameraBehavior = GetComponent<PlayerCameraBehavior>();
        _playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        _playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        _playerSkills = GetComponent<PlayerSkills>();
        _playerWeapon = GetComponent<PlayerWeapon>();
        _cameraController = GetComponent<CameraController>();

        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 5;
        transform.LookAt(Camera.main.transform);
        DisablePlayerControls();

    }

    public void DisablePlayerControls()
    {
        StartCoroutine(DisablePlayerControlsCoroutine());
    }

    public void EnablePlayerControls()
    {
        _playerCameraBehavior.enabled = true;
        _playerNetworkMovement.enabled = true;
        _playerNetworkRotation.enabled = true;
        _playerSkills.enabled = true;
        _playerWeapon.enabled = true;
        _cameraController.enabled = true;
        isometricCamera.SetActive(true);
        firstPersonCamera.SetActive(true);
        _playerCameraBehavior.EnableFirstPersonCamera();

    }
    IEnumerator DisablePlayerControlsCoroutine()
    {
        yield return new WaitForEndOfFrame();
        _playerCameraBehavior.enabled = false;
        _playerNetworkMovement.enabled = false;
        _playerNetworkRotation.enabled = false;
        _playerSkills.enabled = false;
        _playerWeapon.enabled = false;
        _cameraController.enabled = false;
    }


}
