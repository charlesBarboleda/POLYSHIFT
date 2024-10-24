using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraBehavior : NetworkBehaviour
{
    [SerializeField] Transform targetTransform;
    [SerializeField] CinemachineCamera firstPersonCamera;
    [SerializeField] CinemachineCamera isometricCamera;


    PlayerNetworkRotation playerNetworkRotation;

    void Start()
    {
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        if (!IsOwner)
        {
            // If this is not the local player, disable cameras
            firstPersonCamera.gameObject.SetActive(false);
            isometricCamera.gameObject.SetActive(false);
            return;
        }
        isometricCamera.transform.SetParent(null);
        firstPersonCamera.transform.SetParent(null);
        EnableFirstPersonCamera();
    }


    void Update()
    {
        if (!IsOwner) return;
        playerNetworkRotation.IsIsometric.Value = Input.GetKeyDown(KeyCode.Space) ? !playerNetworkRotation.IsIsometric.Value : playerNetworkRotation.IsIsometric.Value;
        if (playerNetworkRotation.IsIsometric.Value)
        {
            EnableIsometricCamera();
        }
        else
        {
            EnableFirstPersonCamera();
            FollowPlayerHead();  // Update camera position
        }

    }

    void FollowPlayerHead()
    {
        firstPersonCamera.transform.position = targetTransform.transform.position;
        firstPersonCamera.transform.rotation = targetTransform.transform.rotation;
    }

    void EnableFirstPersonCamera()
    {
        firstPersonCamera.Priority = 1;
        isometricCamera.Priority = 0;
    }

    void EnableIsometricCamera()
    {
        firstPersonCamera.Priority = 0;
        isometricCamera.Priority = 1;
    }

}
