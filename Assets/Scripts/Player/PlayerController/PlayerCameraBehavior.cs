using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraBehavior : NetworkBehaviour
{
    [SerializeField] private Transform playerHead; // The head target for first-person view
    [SerializeField] private CinemachineCamera firstPersonCamera;
    [SerializeField] private CinemachineCamera isometricCamera;
    [SerializeField] private CinemachineCamera freeViewCamera;
    [SerializeField] GameObject skillTreeCanvas;

    private PlayerNetworkMovement playerNetworkMovement;
    private PlayerNetworkRotation playerNetworkRotation;
    private PlayerStateController playerState;

    float verticalRotation = 0f;
    float horizontalRotation = 0f;

    float _lastSwitchTime = 0f;
    float switchCooldown = 1;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            firstPersonCamera.gameObject.SetActive(false);
            isometricCamera.gameObject.SetActive(false);
            return;
        }

        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        playerState = GetComponent<PlayerStateController>();

        firstPersonCamera.transform.SetParent(null);
        isometricCamera.transform.SetParent(null);
        DontDestroyOnLoad(firstPersonCamera.gameObject);
        DontDestroyOnLoad(isometricCamera.gameObject);


        EnableFirstPersonCamera();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (playerState.playerState.Value == PlayerState.Alive)
        {
            if (Input.GetKeyDown(KeyCode.Tab) && Time.time - _lastSwitchTime > switchCooldown && !playerNetworkMovement.skillTreeCanvas.activeSelf)
            {
                playerNetworkMovement.IsIsometric.Value = !playerNetworkMovement.IsIsometric.Value;
                _lastSwitchTime = Time.time;
            }

            if (IsIsometricMode())
            {
                EnableIsometricCamera();
            }
            else
            {
                EnableFirstPersonCamera();
                RotateCameraIndependently();
            }
            FollowPlayerHead();
        }

        if (playerState.playerState.Value == PlayerState.Dead)
        {
            RotateFreeViewCamera();
        }
    }

    void FollowPlayerHead()
    {
        // Position the first-person camera at the head's position
        if (firstPersonCamera != null)
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
        if (firstPersonCamera != null)
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
        if (!IsOwner) return; // Only allow the owning player to adjust the spectator mode

        freeViewCamera.gameObject.SetActive(true);
        freeViewCamera.Priority = 1;
        firstPersonCamera.Priority = 0;
        isometricCamera.Priority = 0;
    }

    public void DisableSpectatorMode()
    {
        if (!IsOwner) return; // Only allow the owning player to adjust the spectator mode

        freeViewCamera.Priority = 0;
        freeViewCamera.gameObject.SetActive(false);
        firstPersonCamera.Priority = 1;
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
        return playerNetworkMovement.IsIsometric.Value;
    }

}
