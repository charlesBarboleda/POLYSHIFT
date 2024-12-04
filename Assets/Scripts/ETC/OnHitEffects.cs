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
        health.Health.OnValueChanged += OnHitEffectsServerRpc;
    }

    [ServerRpc(RequireOwnership = false)]
    void OnHitEffectsServerRpc(float current, float prev)
    {
        OnHitEffectsClientRpc(current, prev);
    }

    [ClientRpc]
    void OnHitEffectsClientRpc(float current, float prev)
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
        // Scale up the parent smoothly (transform itself)
        transform.DOScale(originalScale * 1.06f, 0.1f);

        // Scale up the children of the object
        foreach (Transform child in transform)
        {
            child.DOScale(child.localScale * 1.06f, 0.1f);
            UpdateRendererBounds(child); // Use renderer bounds for visualization
        }

        // Wait for 0.1 seconds (duration of scale tween)
        yield return new WaitForSeconds(0.1f);

        // Scale back down the children smoothly
        foreach (Transform child in transform)
        {
            child.DOScale(Vector3.one, 0.1f);
            UpdateRendererBounds(child); // Use renderer bounds for visualization
        }

        // Scale back down the parent smoothly
        transform.DOScale(originalScale, 0.1f);

        // Wait for the scale-down tween to finish
        yield return new WaitForSeconds(0.1f);

        // Reset coroutine reference
        hitEffectCoroutine = null;
    }

    void UpdateRendererBounds(Transform obj)
    {
        // Use Renderer bounds instead of recalculating mesh bounds
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            Debug.Log($"Renderer bounds for {obj.name}: Center={bounds.center}, Size={bounds.size}");
        }
    }
}
