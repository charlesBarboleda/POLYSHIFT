using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraBehavior : MonoBehaviour
{
    [SerializeField] CinemachineCamera firstPersonCamera;
    [SerializeField] CinemachineCamera isometricCamera;

    PlayerNetworkRotation playerNetworkRotation;

    void Start()
    {
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
    }

    void Update()
    {
        if (playerNetworkRotation.IsIsometric.Value)
        {
            EnableIsometricCamera();
        }
        else
        {
            EnableFirstPersonCamera();
        }
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
