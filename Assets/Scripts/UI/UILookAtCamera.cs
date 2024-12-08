using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
    void Update()
    {
        // Make the UI face the camera but with a 180-degree rotation around the Y-axis to correct the reversed appearance.
        Vector3 direction = Camera.main.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-direction);
    }
}
