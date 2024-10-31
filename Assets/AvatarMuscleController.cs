using UnityEngine;

public class HeadAimControl : MonoBehaviour
{
    [SerializeField] private Transform aimTarget;
    public float turnSpeed = 2f;
    public float maxVerticalAngle = 45f;
    private float verticalRotation = 0f;

    void Update()
    {
        float verticalInput = Input.GetAxis("Mouse Y") * turnSpeed;
        float horizontalInput = Input.GetAxis("Mouse X") * turnSpeed;

        // Adjust vertical rotation within specified limits
        verticalRotation = Mathf.Clamp(verticalRotation - verticalInput, -maxVerticalAngle, maxVerticalAngle);

        // Apply rotation to the aim target
        aimTarget.localPosition = new Vector3(horizontalInput, verticalRotation, aimTarget.localPosition.z);
    }
}
