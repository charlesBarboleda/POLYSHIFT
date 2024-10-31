using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerNetworkRotation : NetworkBehaviour
{
    public float FirstPersonTurnSpeed;
    [SerializeField] GameObject playerHead;
    PlayerNetworkMovement playerNetworkMovement;
    public override void OnNetworkSpawn()
    {
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return; // Only the owner of the object should be able to move it
        if (!playerNetworkMovement.IsIsometric.Value)
        {
            RotatePlayerFirstPerson();
            RotateHeadVertical();
        }
        else
        {
            RotatePlayerIsometric();
        }
    }



    void RotatePlayerIsometric()
    {
        // Get the mouse position in screen space
        Vector3 mouseScreenPos = Input.mousePosition;

        // Convert the screen space mouse position to world space
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
        Plane groundPlane = new Plane(Vector3.up, 0f);  // Ground plane at y = 0
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // Get the world position where the mouse ray intersects with the ground plane
            Vector3 worldMousePos = ray.GetPoint(rayDistance);

            // Calculate the direction to look at (player to mouse position)
            Vector3 directionToLook = worldMousePos - transform.position;

            // Make sure we are only rotating on the horizontal plane (no vertical rotation)
            directionToLook.y = 0f;

            // Check if the direction is valid (non-zero) to avoid errors
            if (directionToLook.sqrMagnitude > 0.01f) // Using square magnitude for performance
            {
                // Calculate the target rotation for the player
                Quaternion targetRotation = Quaternion.LookRotation(directionToLook);

                // Smoothly rotate the player towards the target rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }



    void RotatePlayerFirstPerson()
    {
        // Rotate the player based on the mouse input
        float horizontalMouseInput = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, horizontalMouseInput * FirstPersonTurnSpeed);
    }

    void RotateHeadVertical()
    {
        // Get vertical mouse input
        float verticalMouseInput = Input.GetAxis("Mouse Y");

        // Calculate new vertical rotation based on cumulative angle
        float rotationChange = verticalMouseInput * FirstPersonTurnSpeed;

        // Add the rotation change to the current rotation
        float newRotation = playerHead.transform.localEulerAngles.x - rotationChange;

        // Clamp the rotation to avoid flipping
        if (newRotation > 180f) newRotation -= 360f; // Convert to a -180 to 180 range for clamping
        newRotation = Mathf.Clamp(newRotation, -90f, 90f);

        // Apply the new clamped rotation to the head on the local X-axis only
        playerHead.transform.localRotation = Quaternion.Euler(newRotation, 0f, 0f);
    }

}
