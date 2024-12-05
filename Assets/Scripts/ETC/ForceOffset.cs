using JetBrains.Annotations;
using UnityEngine;

public class ForceOffset : MonoBehaviour
{
    public float xOffset;
    public float yOffset;
    public float zOffset;


    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x + xOffset, transform.position.y + yOffset, transform.position.z + zOffset);
    }
}
