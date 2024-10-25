using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerNetworkRotation : NetworkBehaviour
{
    public NetworkVariable<float> FirstPersonTurnSpeed = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    PlayerNetworkMovement playerNetworkMovement;
    public override void OnNetworkSpawn()
    {
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerNetworkMovement.IsIsometric.Value)
        {
            RotatePlayerFirstPerson();
        }
        else
        {
            RotatePlayerIsometric();
        }
    }



    public void RotatePlayerIsometric()
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



    public void RotatePlayerFirstPerson()
    {
        // Rotate the player based on the mouse input
        float horizontalMouseInput = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, horizontalMouseInput * FirstPersonTurnSpeed.Value);
    }
}
