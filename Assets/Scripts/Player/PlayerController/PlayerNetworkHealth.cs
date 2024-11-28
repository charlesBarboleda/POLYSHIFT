using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkHealth : NetworkBehaviour, IDamageable
{

    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> healthRegenRate = new NetworkVariable<float>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float DamageReduction = 0;
    public bool IsDead;
    bool ironResolve = false;
    float ironResolveDamageReduction = 0.50f;
    Animator animator;
    PlayerLobbyController playerLobbyController;
    PlayerCameraBehavior playerCameraBehavior;
    PlayerNetworkMovement playerNetworkMovement;
    PlayerStateController playerStateController;
    PlayerNetworkRotation playerNetworkRotation;
    [SerializeField] Image healthbarFill;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            DamageReduction = 0;
            maxHealth.Value = 100f;
            currentHealth.Value = maxHealth.Value;
            healthRegenRate.Value = 1f;
            GameManager.Instance.SpawnedAllies.Add(gameObject);
            GameManager.Instance.AlivePlayers.Add(gameObject);

        }

        currentHealth.OnValueChanged += OnHealthChangedClientRpc;
        maxHealth.OnValueChanged += OnHealthChangedClientRpc;

        animator = GetComponent<Animator>();
        playerStateController = GetComponent<PlayerStateController>();
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerLobbyController = GetComponent<PlayerLobbyController>();
        playerCameraBehavior = GetComponent<PlayerCameraBehavior>();
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        currentHealth.OnValueChanged -= OnHealthChangedClientRpc;
        maxHealth.OnValueChanged -= OnHealthChangedClientRpc;

    }

    void Update()
    {
        if (IsLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                RequestTakeDamageServerRpc(99, NetworkManager.LocalClientId);
            }
        }
        if (!IsServer) return;

        if (IsDead) return;

        if (currentHealth.Value < maxHealth.Value)
        {
            RegenerateHealth(healthRegenRate.Value);
        }

        if (currentHealth.Value < maxHealth.Value)
        {
            RegenerateHealth(healthRegenRate.Value);
        }
    }

    [ClientRpc]
    void OnHealthChangedClientRpc(float oldHealth, float newHealth)
    {
        healthbarFill.fillAmount = currentHealth.Value / maxHealth.Value;
    }

    [ServerRpc]
    public void HealServerRpc(float amount)
    {
        currentHealth.Value += amount;
    }

    [ServerRpc]
    public void PermanentHealthIncreaseByServerRpc(float healthIncrease)
    {
        Debug.Log($"PermanentHealthIncreaseByServerRpc called with healthIncrease: {healthIncrease}");
        maxHealth.Value += healthIncrease;
        currentHealth.Value += healthIncrease;
    }

    [ServerRpc]
    public void PermanentHealthRegenIncreaseByServerRpc(float regenIncrease)
    {
        healthRegenRate.Value += regenIncrease;
    }

    [ServerRpc]
    public void MultiplyHealthRegenByServerRpc(float multiplier)
    {
        healthRegenRate.Value *= multiplier;
    }

    [ServerRpc]
    public void DivideHealthRegenRateByServerRpc(float divisor)
    {
        healthRegenRate.Value /= divisor;
    }

    public void UnlockIronResolve()
    {
        ironResolve = true;
    }

    [ServerRpc]
    public void IncreaseIronResolveDamageReductionServerRpc(float amount)
    {
        ironResolveDamageReduction += amount;
    }

    [ServerRpc]
    public void PermanentDamageReductionIncreaseByServerRpc(float damageReductionIncrease)
    {
        if (damageReductionIncrease > 0)
        {
            // Increase damage reduction with diminishing returns
            DamageReduction = 1 - (1 - DamageReduction) * (1 - damageReductionIncrease);
        }
        else
        {
            // Decrease damage reduction by reversing the formula
            float reductionFactor = 1 + damageReductionIncrease; // Note: damageReductionIncrease is negative here
            DamageReduction = 1 - (1 - DamageReduction) / reductionFactor;
        }
    }




    void RegenerateHealth(float regenAmount)
    {
        currentHealth.Value += regenAmount * Time.deltaTime;
    }

    [ServerRpc]
    public void ReduceDamageTakenByServerRpc(float damageReduction, float duration)
    {
        StartCoroutine(ReduceDamageTakenCoroutine(damageReduction, duration));
    }

    IEnumerator ReduceDamageTakenCoroutine(float damageReduction, float duration)
    {
        // Store the original DamageReduction to revert back to it later
        float originalDamageReduction = DamageReduction;

        // Apply the damage reduction modification with diminishing returns
        DamageReduction = 1 - (1 - originalDamageReduction) * (1 - damageReduction);

        yield return new WaitForSeconds(duration);

        // Revert to the original DamageReduction
        DamageReduction = originalDamageReduction;
    }




    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage, ulong clientId)
    {
        TakeDamage(damage, clientId);
    }

    public void TakeDamage(float damage, ulong clientId)
    {
        if (IsDead) return;
        if (IsServer)
        {
            // Check if the player is below 30% health to apply extra damage reduction
            if (currentHealth.Value / maxHealth.Value <= 0.3f && ironResolve)
            {
                // Apply an additional damage reduction boost when health is below 30%
                float lowHealthReduction = ironResolveDamageReduction; // Example: additional 20% reduction
                DamageReduction = 1 - (1 - DamageReduction) * (1 - lowHealthReduction);
            }

            // Calculate the effective damage after reduction
            if (DamageReduction < 1)
            {
                damage = damage * (1 - DamageReduction);
            }

            currentHealth.Value -= damage;

            if (currentHealth.Value <= 0)
            {
                HandleDeath(clientId);
                IsDead = true;
                EventManager.Instance.PlayerDeathEvent(this);
            }
        }
    }

    public void HandleDeath(ulong clientId)
    {
        if (IsServer)
        {
            GameManager.Instance.SpawnedAllies.Remove(gameObject);
            GameManager.Instance.AlivePlayers.Remove(gameObject);

        }

        if (IsLocalPlayer)
        {
            DeathClientRpc();
        }

    }


    [ClientRpc]
    void DeathClientRpc()
    {
        animator.SetTrigger("isDead");
        playerNetworkRotation.canRotate = false;
        playerNetworkMovement.canMove = false;
        StartCoroutine(DeathCoroutine());
    }
    IEnumerator DeathCoroutine()
    {
        // Wait for the animation to finish
        yield return new WaitForSeconds(2f);
        if (IsOwner)
        {
            playerStateController.SetPlayerStateServerRpc(PlayerState.Dead);

        }
    }

    public void Respawn()
    {
        currentHealth.Value = maxHealth.Value;
        gameObject.SetActive(true);
    }


}
