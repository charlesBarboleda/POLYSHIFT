using UnityEngine;

public class TestHeadRotation : MonoBehaviour
{
    public float rotationSpeed = 5f;
    private float verticalRotation = 0f;

    void Update()
    {
        float verticalInput = Input.GetAxis("Mouse Y");
        verticalRotation -= verticalInput * rotationSpeed;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}
