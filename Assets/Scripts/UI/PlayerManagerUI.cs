using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManagerUI : MonoBehaviour
{
    public static PlayerManagerUI Instance { get; private set; }

    [SerializeField] Image healthBarFill;
    [SerializeField] GameObject firstPersonUI;
    [SerializeField] GameObject isometricUI;
    [SerializeField] PlayerNetworkRotation playerNetworkRotation;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthBarFill.fillAmount = currentHealth / maxHealth;
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
