using System.Collections;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerLobbyController : NetworkBehaviour
{
    PlayerCameraBehavior _playerCameraBehavior;
    PlayerNetworkMovement _playerNetworkMovement;
    PlayerNetworkRotation _playerNetworkRotation;
    PlayerUIManager _playerUIManager;
    PlayerSkills _playerSkills;
    PlayerWeapon _playerWeapon;
    CameraController _cameraController;
    [SerializeField] GameObject hotbarCanvas;
    [SerializeField] GameObject firstPersonCanvas;
    [SerializeField] GameObject isometricCamera;
    [SerializeField] GameObject firstPersonCamera;
    [SerializeField] GameObject infoCanvas;

    public override void OnNetworkSpawn()
    {
        _playerCameraBehavior = GetComponent<PlayerCameraBehavior>();
        _playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        _playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        _playerUIManager = GetComponent<PlayerUIManager>();
        _playerSkills = GetComponent<PlayerSkills>();
        _playerWeapon = GetComponent<PlayerWeapon>();
        _cameraController = GetComponent<CameraController>();
        Debug.Log("OnNetworkSpawn position: " + transform.position);
        DisablePlayerControls();


    }


    public void DisablePlayerControls()
    {

        DisablePlayerControlsServerRpc();

    }

    [ServerRpc(RequireOwnership = false)]
    public void DisablePlayerControlsServerRpc()
    {
        DisablePlayerControlsClientRpc();
    }

    [ClientRpc]
    public void DisablePlayerControlsClientRpc()
    {

        _playerUIManager.enabled = false;
        _playerCameraBehavior.enabled = false;
        _playerNetworkMovement.enabled = false;
        _playerNetworkRotation.enabled = false;
        _playerSkills.enabled = false;
        _playerWeapon.enabled = false;
        _cameraController.enabled = false;
        infoCanvas.SetActive(false);
        isometricCamera.SetActive(false);
        firstPersonCamera.SetActive(false);
        hotbarCanvas.SetActive(false);
        firstPersonCanvas.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EnablePlayerControlsServerRpc()
    {
        EnablePlayerControlsClientRpc();
    }


    [ClientRpc]
    void EnablePlayerControlsClientRpc()
    {
        infoCanvas.SetActive(true);

        if (!IsLocalPlayer) return;

        _playerCameraBehavior.enabled = true;
        _playerNetworkMovement.enabled = true;
        _playerNetworkRotation.enabled = true;
        _playerSkills.enabled = true;
        _playerWeapon.enabled = true;
        _playerUIManager.enabled = true;
        _cameraController.enabled = true;
        isometricCamera.SetActive(true);
        firstPersonCamera.SetActive(true);
        hotbarCanvas.SetActive(true);
        hotbarCanvas.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        firstPersonCanvas.SetActive(true);
        _playerCameraBehavior.EnableFirstPersonCamera();

    }


}
