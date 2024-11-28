using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraBehavior : NetworkBehaviour
{
    [SerializeField] private Transform playerHead; // The head target for first-person view
    [SerializeField] private CinemachineCamera firstPersonCamera;
    [SerializeField] private CinemachineCamera isometricCamera;
    [SerializeField] private CinemachineCamera freeViewCamera;
    public Camera MainCamera;
    private PlayerNetworkMovement playerNetworkMovement;
    private PlayerNetworkRotation playerNetworkRotation;
    private PlayerStateController playerState;

    float verticalRotation = 0f;
    float horizontalRotation = 0f;

    float _lastSwitchTime = 0f;
    float switchCooldown = 1;

    public override void OnNetworkSpawn()
    {
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        playerState = GetComponent<PlayerStateController>();

        if (!IsOwner)
        {
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

        if (playerState.playerState.Value == PlayerState.Alive)
        {
            if (Input.GetKeyDown(KeyCode.Space) && Time.time - _lastSwitchTime > switchCooldown)
            {
                playerNetworkMovement.IsIsometric = !playerNetworkMovement.IsIsometric;
                _lastSwitchTime = Time.time;
            }

            if (IsIsometricMode())
            {
                EnableIsometricCamera();
            }
            else
            {
                EnableFirstPersonCamera();
                FollowPlayerHead();
                RotateCameraIndependently();
            }
        }
        else if (playerState.playerState.Value == PlayerState.Dead)
        {
            RotateFreeViewCamera();
        }
    }

    void FollowPlayerHead()
    {
        // Position the first-person camera at the head's position
        firstPersonCamera.transform.position = playerHead.position + playerHead.forward * 0.2f;
    }
    void RotateCameraIndependently()
    {
        // Handle horizontal and vertical rotation independently of the player
        float horizontalMouseInput = Input.GetAxis("Mouse X") * playerNetworkRotation.FirstPersonTurnSpeed;
        float verticalMouseInput = Input.GetAxis("Mouse Y") * playerNetworkRotation.FirstPersonTurnSpeed;

        // Update rotations based on input
        horizontalRotation += horizontalMouseInput;
        verticalRotation -= verticalMouseInput;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        // Apply the rotation to the camera
        firstPersonCamera.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);

        // Rotate the player’s body to match only the horizontal rotation of the camera
        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }

    void RotateFreeViewCamera()
    {
        // Handle horizontal and vertical rotation independently of the player
        float horizontalMouseInput = Input.GetAxis("Mouse X") * 5;
        float verticalMouseInput = Input.GetAxis("Mouse Y") * 5;

        // Update rotations based on input
        horizontalRotation += horizontalMouseInput;
        verticalRotation -= verticalMouseInput;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        // Apply the rotation to the camera
        freeViewCamera.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

    public void EnableSpectatorMode()
    {
        freeViewCamera.gameObject.SetActive(true);
        freeViewCamera.Priority = 1;
        firstPersonCamera.Priority = 0;
        isometricCamera.Priority = 0;
    }


    public void EnableFirstPersonCamera()
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
        return playerNetworkMovement.IsIsometric;
    }
}
