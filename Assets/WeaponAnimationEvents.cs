using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponAnimationEvents : NetworkBehaviour
{
    public List<ParticleSystem> weaponOrbOrbit = new List<ParticleSystem>();
    public ParticleSystem weaponOrbMiddle;
    public ParticleSystem weaponOrbStars;


    [ServerRpc]
    public void TurnWeaponWhiteServerRpc()
    {
        TurnWeaponWhiteClientRpc();
    }

    [ClientRpc]
    public void TurnWeaponWhiteClientRpc()
    {
        foreach (ParticleSystem orb in weaponOrbOrbit)
        {
            StartCoroutine(TweenParticleColor(orb, Color.white, 1f));
        }

        StartCoroutine(TweenParticleColor(weaponOrbMiddle, Color.white, 1f));
        StartCoroutine(TweenParticleColor(weaponOrbStars, Color.white, 1f));
    }

    [ServerRpc]
    public void TurnWeaponRedServerRpc()
    {
        TurnWeaponRedClientRpc();
    }

    [ClientRpc]
    void TurnWeaponRedClientRpc()
    {
        // Change the color of the weapon orbs to red
        foreach (ParticleSystem orb in weaponOrbOrbit)
        {
            StartCoroutine(TweenParticleColor(orb, Color.red, 0.5f));
        }

        StartCoroutine(TweenParticleColor(weaponOrbMiddle, Color.red, 0.5f));
        StartCoroutine(TweenParticleColor(weaponOrbStars, Color.red, 0.5f));


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
