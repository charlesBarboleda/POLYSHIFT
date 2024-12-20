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
    [SerializeField] AudioClip arcaneCleaveSound;
    [SerializeField] AudioClip dashSound;
    [SerializeField] List<AudioClip> onPlayerHitSounds;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        generalAudioSource = GetComponent<AudioSource>();
    }

    public void PlayDashSound()
    {
        PlayDashSoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PlayDashSoundRpc()
    {
        generalAudioSource.volume = 0.05f;
        generalAudioSource.PlayOneShot(dashSound);
    }

    public void PlayReloadSound()
    {
        PlayReloadSoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayReloadSoundRpc()
    {
        weaponAudioSource.volume = 0.1f;
        weaponAudioSource.PlayOneShot(reloadSound);
    }

    public void PlayLevelUpSound()
    {
        PlayLevelUpSoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayLevelUpSoundRpc()
    {
        generalAudioSource.PlayOneShot(levelUpSound);
        generalAudioSource.DOFade(0, 5f).OnComplete(() =>
        {
            generalAudioSource.Stop();
            generalAudioSource.volume = 1;
        });
    }

    public void PlayFootstepSound()
    {
        PlayFootstepSoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayFootstepSoundRpc()
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
        PlayShootSoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayShootSoundRpc()
    {
        weaponAudioSource.pitch = Random.Range(0.7f, 1.3f);
        weaponAudioSource.volume = 0.1f;
        weaponAudioSource.PlayOneShot(shootSound);
    }

    public void PlayMeleeSlash1Sound()
    {
        PlayMeleeSlash1SoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayMeleeSlash1SoundRpc()
    {
        generalAudioSource.volume = 0.1f;
        generalAudioSource.PlayOneShot(meleeSlash1Sound);
    }

    public void PlayArcaneDevilSlamShoutSound()
    {
        PlayArcaneDevilSlamShoutSoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayArcaneDevilSlamShoutSoundRpc()
    {
        generalAudioSource.volume = 0.05f;
        generalAudioSource.PlayOneShot(arcaneDevilSlamShoutSound);
    }

    public void PlayArcaneCleaveSound()
    {
        PlayArcaneCleaveSoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayArcaneCleaveSoundRpc()
    {
        generalAudioSource.volume = 0.05f;
        generalAudioSource.PlayOneShot(arcaneCleaveSound);
    }

    public void PlayOnPlayerHitSound()
    {
        PlayOnPlayerHitSoundRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayOnPlayerHitSoundRpc()
    {
        generalAudioSource.volume = 0.05f;
        generalAudioSource.PlayOneShot(onPlayerHitSounds[Random.Range(0, onPlayerHitSounds.Count)]);
    }
}
