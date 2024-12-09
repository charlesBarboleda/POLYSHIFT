using UnityEngine;

public class TrackLifeCycle : MonoBehaviour
{
    // Awake is called once before the first execution of Start after the MonoBehaviour is created
    void Awake()
    {
        Debug.Log("Awake called for " + gameObject.name);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start called for " + gameObject.name);
    }

    // OnEnable is called when the object becomes active and enabled
    void OnEnable()
    {
        Debug.Log("OnEnable called for " + gameObject.name);
    }


    void OnDisable()
    {
        Debug.Log("OnDisable called for " + gameObject.name);
    }

    // OnDestroy is called when the MonoBehaviour will be destroyed

    void OnDestroy()
    {
        Debug.Log("OnDestroy called for " + gameObject.name);
        Debug.LogError("OnDestroy StackTrace: " + System.Environment.StackTrace);
    }
}
