using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
    Camera cameraToLookAt;

    void Start()
    {
        cameraToLookAt = Camera.main;
    }

    void Update()
    {
        // Make the UI face the camera but with a 180-degree rotation around the Y-axis to correct the reversed appearance.
        Vector3 direction = cameraToLookAt.transform.position - transform.position;
        direction.y = 0; // Keep the UI upright (ignore vertical tilt)
        transform.rotation = Quaternion.LookRotation(-direction);
    }
}
