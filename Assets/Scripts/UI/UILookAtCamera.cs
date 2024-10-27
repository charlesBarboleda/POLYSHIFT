using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
    Camera cameraToLookAt;
    void Start()
    {
        cameraToLookAt = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cameraToLookAt.transform);
    }
}
