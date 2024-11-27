using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PlayerAudioManager : NetworkBehaviour
{
    AudioSource generalAudioSource;
    [SerializeField] AudioSource weaponAudioSource;
    [SerializeField] AudioSource footstepAudioSource;
    [SerializeField] List<AudioClip> grassFootstepSounds;
    [SerializeField] List<AudioClip> stoneFootstepSounds;
    [SerializeField] AudioClip levelUpSound;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip meleeSlash1Sound;
    [SerializeField] AudioClip arcaneDevilSlamShoutSound;
    [SerializeField] AudioClip reloadSound;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        generalAudioSource = GetComponent<AudioSource>();

    }

    public void PlayReloadSound()
    {
        weaponAudioSource.volume = 0.1f;
        weaponAudioSource.PlayOneShot(reloadSound);
    }

    public void PlayLevelUpSound()
    {
        generalAudioSource.PlayOneShot(levelUpSound);
        generalAudioSource.DOFade(0, 5f).OnComplete(() =>
        {
            generalAudioSource.Stop();
            generalAudioSource.volume = 1;
        }
        );
    }

    public void PlayFootstepSound()
    {
        footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);
        footstepAudioSource.volume = 0.01f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f, LayerMask.GetMask("Grass")))
        {
            footstepAudioSource.PlayOneShot(grassFootstepSounds[Random.Range(0, grassFootstepSounds.Count)]);
        }
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit2, 1.5f, LayerMask.GetMask("Ground")))
        {
            footstepAudioSource.PlayOneShot(stoneFootstepSounds[Random.Range(0, stoneFootstepSounds.Count)]);
        }
    }



    public void PlayShootSound()
    {
        // Set a random pitch within a range, e.g., 0.9 to 1.1 for slight variation
        weaponAudioSource.pitch = Random.Range(0.9f, 1.1f);
        weaponAudioSource.volume = 0.1f;
        weaponAudioSource.PlayOneShot(shootSound);
    }

    public void PlayMeleeSlash1Sound()
    {
        generalAudioSource.volume = 0.1f;
        generalAudioSource.PlayOneShot(meleeSlash1Sound);
    }

    public void PlayArcaneDevilSlamShoutSound()
    {
        generalAudioSource.volume = 0.05f;
        generalAudioSource.PlayOneShot(arcaneDevilSlamShoutSound);
    }

}
