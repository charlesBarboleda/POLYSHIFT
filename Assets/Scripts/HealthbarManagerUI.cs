using System.Collections.Generic;
using UnityEngine.UI;
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
            healthbar.GetComponent<Image>().enabled = false;

            // Disable all child Image components
            Image[] childImages = healthbar.GetComponentsInChildren<Image>();
            foreach (var childImage in childImages)
            {
                childImage.enabled = false;
            }
        }
    }

    public void ActivateAllHealthbars()
    {
        foreach (var healthbar in _healthbars)
        {
            healthbar.GetComponent<Image>().enabled = true;

            // Disable all child Image components
            Image[] childImages = healthbar.GetComponentsInChildren<Image>();
            foreach (var childImage in childImages)
            {
                childImage.enabled = true;
            }
        }
    }




}
