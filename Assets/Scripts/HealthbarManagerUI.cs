using System.Collections.Generic;
using UnityEngine;


public class HealthbarManagerUI : MonoBehaviour
{
    public static HealthbarManagerUI Instance { get; private set; }

    [SerializeField] List<GameObject> _healthbars = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void AddHealthbar(GameObject healthbar)
    {
        if (!_healthbars.Contains(healthbar))
        {
            _healthbars.Add(healthbar);
        }

    }

    public void RemoveHealthbar(GameObject healthbar)
    {
        if (_healthbars.Contains(healthbar))
        {
            _healthbars.Remove(healthbar);
        }
    }

    public void DeactivateAllHealthbars()
    {
        foreach (var healthbar in _healthbars)
        {
            healthbar.SetActive(false);
        }
    }

    public void ActivateAllHealthbars()
    {
        foreach (var healthbar in _healthbars)
        {
            healthbar.SetActive(true);
        }
    }


}
