using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraBehavior : NetworkBehaviour
{
    [SerializeField] Transform targetTransform;
    [SerializeField] CinemachineCamera firstPersonCamera;
    [SerializeField] CinemachineCamera isometricCamera;
    public Camera MainCamera;


    float _lastSwitchTime = 0f;  // Tracks last switch time
    float switchCooldown = 1;  // Cooldown duration (adjust as needed)

    void Start()
    {

        if (!IsOwner)
        {
            // Disable cameras for non-local players
            firstPersonCamera.gameObject.SetActive(false);
            isometricCamera.gameObject.SetActive(false);
            return;
        }

        // Detach cameras from the player
        isometricCamera.transform.SetParent(null);
        firstPersonCamera.transform.SetParent(null);
        EnableFirstPersonCamera();
    }

    void Update()
    {
        if (!IsOwner) return;

        // Check if enough time has passed since the last switch
        if (Input.GetKeyDown(KeyCode.Space) && Time.time - _lastSwitchTime > switchCooldown)
        {
            // Toggle perspective
            PlayerManagerUI.Instance.IsIsometric = !PlayerManagerUI.Instance.IsIsometric;
            EventManager.Instance.PerspectiveChange(PlayerManagerUI.Instance.IsIsometric);
            _lastSwitchTime = Time.time;  // Update last switch time
        }

        // Set camera based on the current mode
        if (IsIsometricMode())
        {
            EnableIsometricCamera();
        }
        else
        {
            EnableFirstPersonCamera();
            FollowPlayerHead();  // Update first-person camera position
        }
    }

    void FollowPlayerHead()
    {
        firstPersonCamera.transform.position = targetTransform.position;
        firstPersonCamera.transform.rotation = targetTransform.rotation;
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

    public bool IsIsometricMode()
    {
        return PlayerManagerUI.Instance.IsIsometric;
    }
}
