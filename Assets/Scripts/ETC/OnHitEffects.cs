using System.Collections;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class OnHitEffects : NetworkBehaviour
{
    private DestroyableHealth health;
    private Coroutine hitEffectCoroutine;
    private Vector3 originalScale;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health = GetComponent<DestroyableHealth>();
        originalScale = transform.localScale;
        health.health.OnValueChanged += OnHitEffectsServerRpc;
    }

    [ServerRpc]
    void OnHitEffectsServerRpc(float current)
    {
        if (hitEffectCoroutine != null)
        {
            StopCoroutine(hitEffectCoroutine); // Stop any currently running coroutine
            transform.localScale = originalScale; // Reset scale to prevent cumulative effects
        }
        hitEffectCoroutine = StartCoroutine(HitEffectCoroutine());
    }

    IEnumerator HitEffectCoroutine()
    {
        // Scale up smoothly
        transform.DOScale(originalScale * 1.06f, 0.1f);

        // Wait for 0.1 seconds (duration of scale tween)
        yield return new WaitForSeconds(0.1f);

        // Scale back down smoothly
        transform.DOScale(originalScale, 0.1f);

        // Wait for the scale-down tween to finish
        yield return new WaitForSeconds(0.1f);

        // Reset coroutine reference
        hitEffectCoroutine = null;
    }
}
