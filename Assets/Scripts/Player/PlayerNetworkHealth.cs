using System;
using Netcode.Extensions;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerNetworkHealth : NetworkBehaviour, IDamageable
{
    public static event Action<ulong> OnPlayerDeath;
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(100f);
    public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(100f);
    public NetworkVariable<float> HealthRegenRate = new NetworkVariable<float>(1f);
    IsometricPlayerHealthbar healthbarScript;

    void Start()
    {
        if (!IsServer) return;

        CurrentHealth.Value = MaxHealth.Value;
        ActivePlayersList.Instance.RegisterPlayer(this);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SpawnIndividualHealthBar();
            EventManager.Instance.PlayerSpawned();
        }

        if (IsClient)
        {
            SpawnHealthbars();
        }

    }

    void OnDisable()
    {
        if (!IsServer) return;
        ActivePlayersList.Instance.UnregisterPlayer(this);
    }


    void Update()
    {
        if (!IsServer) return;

        if (CurrentHealth.Value < MaxHealth.Value)
        {
            RegenHealth(HealthRegenRate.Value);
        }


    }
    void SpawnHealthbars()
    {
        SpawnPlayerHealthbars();
    }

    void SpawnPlayerHealthbars()
    {
        foreach (PlayerNetworkHealth player in ActivePlayersList.Instance.GetAlivePlayers())
        {
            SpawnIndividualHealthBar();
        }
    }

    void SpawnIndividualHealthBar()
    {
        GameObject healthbar = ObjectPooler.Generate("IsometricPlayerHealthbar");
        if (healthbar == null)
        {
            Debug.LogError("IsometricPlayerHealthbar not found in pool!");
            return;
        }
        IsometricPlayerHealthbar healthbarScript = healthbar.GetComponentInChildren<IsometricPlayerHealthbar>();
        HealthbarManagerUI.Instance.AddHealthbar(healthbarScript.gameObject);
        healthbarScript.SetPlayer(transform);
    }

    void RegenHealth(float regenAmount)
    {

        CurrentHealth.Value += regenAmount * Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        {
            CurrentHealth.Value -= damage;
            if (CurrentHealth.Value <= 0)
            {
                Die();
            }
        }
    }

    public void TakeDamage(float damage, ulong attackerId)
    {
        TakeDamage(damage);
        Debug.Log("Player was attacked by " + attackerId);
    }

    void Die()
    {
        Debug.Log("Player died");
        OnPlayerDeath?.Invoke(OwnerClientId);
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        CurrentHealth.Value = MaxHealth.Value;
        gameObject.SetActive(true);
    }
}
