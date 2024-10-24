using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraBehavior : MonoBehaviour
{
    [SerializeField] Transform targetTransform;
    [SerializeField] CinemachineCamera firstPersonCamera;
    [SerializeField] CinemachineCamera isometricCamera;

    PlayerNetworkRotation playerNetworkRotation;

    void Start()
    {
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
    }

    void OnEnable()
    {
        isometricCamera.transform.SetParent(null);
        firstPersonCamera.transform.SetParent(null);
    }

    void Update()
    {
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
        firstPersonCamera.transform.position = targetTransform.transform.position + new Vector3(0, 0f, 0);
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
