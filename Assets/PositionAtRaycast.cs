using UnityEngine;

public class PositionAtRaycast : MonoBehaviour
{
    [SerializeField] PlayerNetworkMovement playerNetworkMovement;
    // Update is called once per frame
    void Update()
    {
        if (!playerNetworkMovement.IsIsometric.Value)
        {
            PositionAtRaycastFirstPerson();
        }
        else
        {
            PositionAtMouseIsometric();
        }

    }

    void PositionAtRaycastFirstPerson()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        float maxDistance = 500f; // Set your desired maximum distance

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            // If the ray hits something, set position to the hit point
            transform.position = hit.point;
        }
        else
        {
            // If the ray doesn't hit anything, calculate an end point at the max distance
            transform.position = ray.origin + ray.direction * maxDistance;
        }
    }

    void PositionAtMouseIsometric()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Move this game object to the point where the raycast hit another game object
            transform.position = hit.point;
        }
    }



}
