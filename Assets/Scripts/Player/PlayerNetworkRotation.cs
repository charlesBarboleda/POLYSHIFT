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
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
}
