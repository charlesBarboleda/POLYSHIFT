using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManagerUI : MonoBehaviour
{
    [SerializeField] Image firstPersonHealthbar;
    [SerializeField] GameObject firstPersonUI;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] PlayerNetworkHealth playerNetworkHealth;


    void OnDisable()
    {
        // Unsubscribe from health changes
        playerNetworkHealth.currentHealth.OnValueChanged -= FirstPersonOnHealthChanged;
        playerNetworkHealth.maxHealth.OnValueChanged -= FirstPersonOnHealthChanged;
    }


    public void OnPlayerSpawnReference(PlayerNetworkHealth player)
    {
        playerNetworkHealth = player;
        player.currentHealth.OnValueChanged += FirstPersonOnHealthChanged;
        player.maxHealth.OnValueChanged += FirstPersonOnHealthChanged;
        UpdateHealthText();
    }

    public void UpdateHealthText()
    {
        UpdateFirstPersonHealthBar(playerNetworkHealth.currentHealth.Value, playerNetworkHealth.maxHealth.Value);
    }
    void FirstPersonOnHealthChanged(float previousValue, float newValue)
    {
        UpdateFirstPersonHealthBar(playerNetworkHealth.currentHealth.Value, playerNetworkHealth.maxHealth.Value);
    }

    void UpdateFirstPersonHealthBar(float currentHealth, float maxHealth)
    {
        firstPersonHealthbar.fillAmount = currentHealth / maxHealth;
        healthText.text = $"{Mathf.Round(currentHealth)} / {Mathf.Round(maxHealth)}";
    }

    public void OnPerspectiveChange(bool isIsometric)
    {
        if (isIsometric)
        {
            firstPersonUI.SetActive(false);
            StartCoroutine(ActivateIsometricUI());
        }
        else
        {
            StartCoroutine(ActivateFirstPersonUI());
            HealthbarManagerUI.Instance.DeactivateAllHealthbars();
        }
    }

    IEnumerator ActivateFirstPersonUI()
    {
        yield return new WaitForSeconds(0.9f);
        firstPersonUI.SetActive(true);

    }

    IEnumerator ActivateIsometricUI()
    {
        yield return new WaitForSeconds(0.9f);
        HealthbarManagerUI.Instance.ActivateAllHealthbars();
    }

}
