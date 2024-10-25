using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    public UnityEvent OnPlayerSpawned = new UnityEvent();
    public UnityEvent<bool> OnPerspectiveChange = new UnityEvent<bool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PerspectiveChange(bool isIsometric)
    {
        OnPerspectiveChange.Invoke(isIsometric);
    }

    public void PlayerSpawned()
    {
        OnPlayerSpawned.Invoke();
    }
}
