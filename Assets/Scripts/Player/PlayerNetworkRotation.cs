using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkRotation : NetworkBehaviour
{
    public float FirstPersonTurnSpeed = 5f;
    private PlayerNetworkMovement playerNetworkMovement;


    public override void OnNetworkSpawn()
    {
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!playerNetworkMovement.IsIsometric)
        {
            RotatePlayerFirstPerson();
        }
        else
        {
            RotatePlayerIsometric();
        }
    }

    void RotatePlayerFirstPerson()
    {
        // Only handle horizontal rotation for the body
        float horizontalMouseInput = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, horizontalMouseInput * FirstPersonTurnSpeed);
    }

    void RotatePlayerIsometric()
    {

        // If no mouse input, rotate based on keyboard input
        Vector3 movementDirection = GetMovementDirectionFromInput();
        if (movementDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }


    // Get movement direction based on keyboard input and camera orientation
    Vector3 GetMovementDirectionFromInput()
    {
        Vector3 direction = Vector3.zero;

        // Get the camera's forward and right vectors projected onto the XZ plane
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Determine direction based on input keys
        if (Input.GetKey(KeyCode.W)) direction += cameraForward;   // Forward (relative to camera)
        if (Input.GetKey(KeyCode.S)) direction -= cameraForward;   // Backward
        if (Input.GetKey(KeyCode.A)) direction -= cameraRight;     // Left
        if (Input.GetKey(KeyCode.D)) direction += cameraRight;     // Right

        return direction.normalized; // Normalize to avoid faster diagonal movement
    }


}
