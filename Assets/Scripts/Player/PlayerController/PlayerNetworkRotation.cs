using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkRotation : NetworkBehaviour
{
    public float FirstPersonTurnSpeed = 5f;
    private PlayerNetworkMovement playerNetworkMovement;
    private PlayerSkills playerSkills;
    public bool canRotate = true;


    public override void OnNetworkSpawn()
    {
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerSkills = GetComponent<PlayerSkills>();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (!canRotate) return;
        if (!playerNetworkMovement.IsIsometric.Value)
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
        // Check for mouse input first
        if (Input.GetMouseButtonDown(1))
        {
            RotateTowardsMouse();
            return; // Skip further processing if the mouse button is pressed
        }

        // If no mouse input, rotate based on keyboard input
        Vector3 movementDirection = GetMovementDirectionFromInput();
        if (movementDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // Rotate towards the mouse position instantly
    void RotateTowardsMouse()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
        Plane groundPlane = new Plane(Vector3.up, 0f);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 worldMousePos = ray.GetPoint(rayDistance);
            Vector3 directionToLook = worldMousePos - transform.position;
            directionToLook.y = 0f;

            if (directionToLook.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
                transform.rotation = targetRotation; // Instantly set rotation
            }
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
