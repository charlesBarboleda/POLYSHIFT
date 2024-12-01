using System.Collections;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class OnHitEffects : NetworkBehaviour
{

    DestroyableHealth health;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health = GetComponent<DestroyableHealth>();
        health.health.OnValueChanged += OnHitEffectsServerRpc;

    }

    [ServerRpc]
    void OnHitEffectsServerRpc(float current)
    {
        StartCoroutine(HitEffectCoroutine());
    }

    IEnumerator HitEffectCoroutine()
    {
        // Scale up smoothly
        transform.DOScale(transform.localScale * 1.03f, 0.1f);

        // Wait for 0.1 seconds (duration of scale tween)
        yield return new WaitForSeconds(0.1f);

        // Scale back down smoothly
        transform.DOScale(transform.localScale / 1.03f, 0.1f);

        // Wait for the scale-down tween to finish
        yield return new WaitForSeconds(0.1f);
    }
}
