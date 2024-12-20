using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerNetworkHealth : NetworkBehaviour, IDamageable
{

    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> healthRegenRate = new NetworkVariable<float>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float DamageReduction = 0;
    public bool IsDead;
    public bool IsInvulnerable = false;
    bool ironResolve = false;
    float ironResolveDamageReduction = 0.50f;
    Animator animator;
    PlayerLobbyController playerLobbyController;
    PlayerCameraBehavior playerCameraBehavior;
    PlayerNetworkMovement playerNetworkMovement;
    PlayerStateController playerStateController;
    PlayerNetworkRotation playerNetworkRotation;
    PlayerAudioManager playeraudio;
    [SerializeField] Image healthbarFill;
    [SerializeField] GameObject hotbarUI;
    [SerializeField] GameObject infoCanvas;
    [SerializeField] Image screenEffectOverlay;
    [SerializeField] Volume localVolume;
    Vignette vignette;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            DamageReduction = 0;
            maxHealth.Value = 100f;
            currentHealth.Value = maxHealth.Value;
            healthRegenRate.Value = 1f;
        }
        if (IsOwner)
        {
            localVolume.profile.TryGet(out vignette);
        }
        if (!IsOwner)
        {
            // localVolume.gameObject.SetActive(false);
            hotbarUI.SetActive(false);
        }
        currentHealth.OnValueChanged += TakeDamageScreenOverlay;
        currentHealth.OnValueChanged += OnHealthChangedClientRpc;
        maxHealth.OnValueChanged += OnHealthChangedClientRpc;

        animator = GetComponent<Animator>();
        playerStateController = GetComponent<PlayerStateController>();
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerLobbyController = GetComponent<PlayerLobbyController>();
        playerCameraBehavior = GetComponent<PlayerCameraBehavior>();
        playerNetworkRotation = GetComponent<PlayerNetworkRotation>();
        playeraudio = GetComponent<PlayerAudioManager>();

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        currentHealth.OnValueChanged -= TakeDamageScreenOverlay;
        currentHealth.OnValueChanged -= OnHealthChangedClientRpc;
        maxHealth.OnValueChanged -= OnHealthChangedClientRpc;



    }

    void Update()
    {
        if (!IsServer) return;

        if (IsDead) return;

        if (currentHealth.Value < maxHealth.Value)
        {
            RegenerateHealth(healthRegenRate.Value);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {

        }


    }

    void TakeDamageScreenOverlay(float prev, float current)
    {
        if (current > prev)
            return;
        if (localVolume == null)
        {
            Debug.Log("Local Volume is null");
            return;
        }
        if (vignette == null)
        {
            Debug.Log("Vignette is null");
            return;
        }



        // Set the intensity of the vignette effect based on the current health
        vignette.color.value = Color.red;
        float tweenValue;
        if (currentHealth.Value / maxHealth.Value <= 0.7f)
        {
            // If the player is below 30% health, increase the intensity of the vignette effect
            tweenValue = 0.35f;
        }
        else if (currentHealth.Value / maxHealth.Value <= 0.5f)
        {
            // If the player is below 50% health, increase the intensity of the vignette effect
            tweenValue = 0.40f;
        }
        else if (currentHealth.Value / maxHealth.Value <= 0.3f)
        {
            // If the player is below 30% health, increase the intensity of the vignette effect
            tweenValue = 0.45f;
        }
        else
        {
            // Otherwise, set the intensity to 0.3f
            tweenValue = 0.3f;
        }
        // Use Dotween to increase the intensity of the vignette effect
        DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, tweenValue, 0.25f).OnComplete(() =>
        {
            // Reset the intensity back to 0.3f
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.3f, 0.25f).OnComplete(() =>
            {
                // Reset the color back to black
                DOTween.To(() => vignette.color.value, x => vignette.color.value = x, Color.black, 0.25f);
            });

        });

        screenEffectOverlay.color = Color.red;
        screenEffectOverlay.DOFade(0.05f, 0.25f).OnComplete(() =>
        {
            screenEffectOverlay.DOFade(0, 0.25f);
        });
    }



    [ClientRpc]
    void OnHealthChangedClientRpc(float oldHealth, float newHealth)
    {
        // Calculate the target fill amount
        float targetFill = currentHealth.Value / maxHealth.Value;

        // Tween the fill amount to the new target over 0.5 seconds
        healthbarFill.DOFillAmount(targetFill, 0.5f).SetEase(Ease.OutQuad);
    }


    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float amount)
    {
        PopUpNumberManager.Instance.SpawnHealNumber(transform.position + transform.up * 2, amount);
        currentHealth.Value += amount;
    }

    [Rpc(SendTo.Server)]
    public void PermanentHealthIncreaseByRpc(float healthIncrease)
    {
        maxHealth.Value += healthIncrease;
        currentHealth.Value += healthIncrease;
    }


    public void PermanentHealthRegenIncreaseBy(float regenIncrease)
    {
        healthRegenRate.Value += regenIncrease;
    }


    public void MultiplyHealthRegenBy(float multiplier)
    {
        healthRegenRate.Value *= multiplier;
    }


    public void DivideHealthRegenRateBy(float divisor)
    {
        healthRegenRate.Value /= divisor;
    }

    public void UnlockIronResolve()
    {
        ironResolve = true;
    }


    public void IncreaseIronResolveDamageReduction(float amount)
    {
        ironResolveDamageReduction += amount;
    }


    public void PermanentDamageReductionIncreaseBy(float damageReductionIncrease)
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

    public void ReduceDamageTakenBy(float damageReduction, float duration)
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

    public void Vulnerable()
    {
        IsInvulnerable = false;
    }

    public void Invulnerable()
    {
        IsInvulnerable = true;
    }

    public void TakeDamage(float damage, ulong clientId)
    {
        if (IsDead) return;
        if (IsInvulnerable) return;
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
            playeraudio.PlayOnPlayerHitSound();

            if (currentHealth.Value <= 0)
            {
                HandleDeath(clientId);
                IsDead = true;
                currentHealth.Value = 0;
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


        DeathEffectClientRpc();
    }


    [Rpc(SendTo.ClientsAndHost)]
    void DeathEffectClientRpc()
    {
        hotbarUI.SetActive(false);
        infoCanvas.SetActive(false);
        animator.SetTrigger("isDead");
        playerNetworkRotation.canRotate = false;
        playerNetworkMovement.canMove = false;
        StartCoroutine(DeathCoroutine());
    }
    IEnumerator DeathCoroutine()
    {
        // Wait for the animation to finish
        yield return new WaitForSeconds(2f);

        playerStateController.SetPlayerStateServerRpc(PlayerState.Dead);


    }


    public void Respawn()
    {

        IsDead = false;
        playerStateController.SetPlayerStateServerRpc(PlayerState.Alive);
        playerNetworkMovement.canMove = true;
        playerNetworkRotation.canRotate = true;
        hotbarUI.SetActive(true);
        infoCanvas.SetActive(true);

    }




}
