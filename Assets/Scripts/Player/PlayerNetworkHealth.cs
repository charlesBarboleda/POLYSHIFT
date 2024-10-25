using System;
using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerNetworkHealth : NetworkBehaviour, IDamageable
{
    public static event Action<ulong> OnPlayerDeath;

    const float DefaultHealth = 100f;
    const float DefaultRegenRate = 1f;
    const string HealthBarTag = "IsometricPlayerHealthbar";

    [FormerlySerializedAs("CurrentHealth")] public NetworkVariable<float> currentHealth = new NetworkVariable<float>(DefaultHealth);
    [FormerlySerializedAs("MaxHealth")] public NetworkVariable<float> maxHealth = new NetworkVariable<float>(DefaultHealth);
    [FormerlySerializedAs("HealthRegenRate")] public NetworkVariable<float> healthRegenRate = new NetworkVariable<float>(DefaultRegenRate);

    private IsometricPlayerHealthbar _healthbarScript;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth.Value;
        }
        ActivePlayersList.Instance.RegisterPlayer(this);
        if (IsOwner)
        {
            SpawnIndividualHealthBar();
            EventManager.Instance.PlayerSpawned();
        }

        if (IsClient)
        {
            SpawnAllPlayerHealthbars();
        }

    }
    void OnEnable()
    {
        if (!IsServer) return;
        ActivePlayersList.Instance.RegisterPlayer(this);
    }

    void OnDisable()
    {
        if (!IsServer) return;

        ActivePlayersList.Instance.UnregisterPlayer(this);
    }

    void Update()
    {
        if (!IsServer) return;

        if (currentHealth.Value < maxHealth.Value)
        {
            RegenerateHealth(healthRegenRate.Value);
        }
    }


    void SpawnAllPlayerHealthbars()
    {
        foreach (PlayerNetworkHealth player in ActivePlayersList.Instance.GetAlivePlayers())
        {
            if (player == this && IsOwner) continue;  // Skip the local player if they are the owner
            player.SpawnIndividualHealthBar();
        }
    }

    void SpawnIndividualHealthBar()
    {
        // Check if a health bar is already assigned to avoid duplicates
        if (_healthbarScript != null) return;

        GameObject healthbar = NetworkObjectPool.Instance.GetNetworkObject(HealthBarTag).gameObject;
        if (healthbar == null)
        {
            Debug.LogError($"{HealthBarTag} not found in pool!");
            return;
        }

        _healthbarScript = healthbar.GetComponentInChildren<IsometricPlayerHealthbar>();
        HealthbarManagerUI.Instance.AddHealthbar(_healthbarScript.gameObject);
        _healthbarScript.SetPlayer(transform, this);
    }


    void RegenerateHealth(float regenAmount)
    {
        currentHealth.Value += regenAmount * Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;

        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0)
        {
            HandleDeath();
        }
    }

    public void TakeDamage(float damage, ulong attackerId)
    {
        TakeDamage(damage);
        Debug.Log($"Player was attacked by {attackerId}");
    }

    public void HandleDeath()
    {
        Debug.Log("Player died");
        OnPlayerDeath?.Invoke(OwnerClientId);
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        currentHealth.Value = maxHealth.Value;
        gameObject.SetActive(true);
    }
}
