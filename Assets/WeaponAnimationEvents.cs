using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponAnimationEvents : NetworkBehaviour
{
    public List<ParticleSystem> weaponOrbOrbit = new List<ParticleSystem>();
    public ParticleSystem weaponOrbMiddle;
    public ParticleSystem weaponOrbStars;

    public void OnMeleeAttackEnd()
    {
        foreach (ParticleSystem orb in weaponOrbOrbit)
        {
            StartCoroutine(TweenParticleColor(orb, Color.white, 1f));
        }

        StartCoroutine(TweenParticleColor(weaponOrbMiddle, Color.white, 1f));
        StartCoroutine(TweenParticleColor(weaponOrbStars, Color.white, 1f));
    }

    [ServerRpc]
    public void OnDoubleCrescentSlashStartServerRpc()
    {
        OnDoubleCrescentSlashStartClientRpc();
    }

    [ClientRpc]
    void OnDoubleCrescentSlashStartClientRpc()
    {
        // Change the color of the weapon orbs to magenta
        foreach (ParticleSystem orb in weaponOrbOrbit)
        {
            StartCoroutine(TweenParticleColor(orb, Color.magenta, 0.1f));
        }

        StartCoroutine(TweenParticleColor(weaponOrbMiddle, Color.magenta, 0.1f));
        StartCoroutine(TweenParticleColor(weaponOrbStars, Color.magenta, 0.1f));


    }

    private IEnumerator TweenParticleColor(ParticleSystem particleSystem, Color targetColor, float duration)
    {
        var mainModule = particleSystem.main;
        Color startColor = mainModule.startColor.color;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            mainModule.startColor = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }

        mainModule.startColor = targetColor; // Ensure the final color is set
    }
}
