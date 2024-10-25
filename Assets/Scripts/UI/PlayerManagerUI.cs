using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManagerUI : MonoBehaviour
{
    public static PlayerManagerUI Instance { get; private set; }

    [SerializeField] Image firstPersonHealthbar;
    [SerializeField] GameObject firstPersonUI;
    [SerializeField] GameObject isometricUI;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] PlayerNetworkRotation playerNetworkRotation;
    [SerializeField] PlayerNetworkHealth playerNetworkHealth;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        // Subscribe to health changes
        playerNetworkHealth.currentHealth.OnValueChanged += FirstPersonOnHealthChanged;
        playerNetworkHealth.maxHealth.OnValueChanged += FirstPersonOnHealthChanged;
    
    }

    void OnDisable()
    {
        // Unsubscribe from health changes
        playerNetworkHealth.currentHealth.OnValueChanged -= FirstPersonOnHealthChanged;
        playerNetworkHealth.maxHealth.OnValueChanged -= FirstPersonOnHealthChanged;
    }

    void FirstPersonOnHealthChanged(float previousValue, float newValue)
    {
        UpdateFirstPersonHealthBar(playerNetworkHealth.currentHealth.Value, playerNetworkHealth.maxHealth.Value);
    }

    void UpdateFirstPersonHealthBar(float currentHealth, float maxHealth)
    {
        firstPersonHealthbar.fillAmount = currentHealth / maxHealth;
        healthText.text = $"{currentHealth} / {maxHealth}";
    }

    public void OnPerspectiveChange(bool isIsometric)
    {
        if (isIsometric)
        {
            firstPersonUI.SetActive(false);
            isometricUI.SetActive(true);
        }
        else
        {
            StartCoroutine(ActivateFirstPersonUI());
            isometricUI.SetActive(false);
        }
    }
    IEnumerator ActivateFirstPersonUI()
    {
        yield return new WaitForSeconds(0.9f);
        firstPersonUI.SetActive(true);
    }



}
