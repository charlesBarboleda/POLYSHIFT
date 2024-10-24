using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerNetworkRotation : NetworkBehaviour
{
    public NetworkVariable<bool> IsIsometric = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> FirstPersonTurnSpeed = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Update is called once per frame
    void Update()
    {
        if (!IsIsometric.Value)
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
        // Rotate the player based on mouse cursor position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    public void RotatePlayerFirstPerson()
    {
        // Rotate the player based on the mouse input
        float horizontalMouseInput = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, horizontalMouseInput * FirstPersonTurnSpeed.Value);
    }
}
