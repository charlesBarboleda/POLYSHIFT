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
        if (IsServer)
        {
            PlayReloadSoundClientRpc();
        }
        else
        {
            PlayReloadSoundServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayReloadSoundServerRpc()
    {
        PlayReloadSoundClientRpc();
    }

    [ClientRpc]
    private void PlayReloadSoundClientRpc()
    {
        weaponAudioSource.volume = 0.1f;
        weaponAudioSource.PlayOneShot(reloadSound);
    }

    public void PlayLevelUpSound()
    {
        if (IsServer)
        {
            PlayLevelUpSoundClientRpc();
        }
        else
        {
            PlayLevelUpSoundServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayLevelUpSoundServerRpc()
    {
        PlayLevelUpSoundClientRpc();
    }

    [ClientRpc]
    private void PlayLevelUpSoundClientRpc()
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
        if (IsServer)
        {
            PlayFootstepSoundClientRpc();
        }
        else
        {
            PlayFootstepSoundServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayFootstepSoundServerRpc()
    {
        PlayFootstepSoundClientRpc();
    }

    [ClientRpc]
    private void PlayFootstepSoundClientRpc()
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
        if (IsServer)
        {
            PlayShootSoundClientRpc();
        }
        else
        {
            PlayShootSoundServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayShootSoundServerRpc()
    {
        PlayShootSoundClientRpc();
    }

    [ClientRpc]
    private void PlayShootSoundClientRpc()
    {
        weaponAudioSource.pitch = Random.Range(0.7f, 1.3f);
        weaponAudioSource.volume = 0.1f;
        weaponAudioSource.PlayOneShot(shootSound);
    }

    public void PlayMeleeSlash1Sound()
    {
        if (IsServer)
        {
            PlayMeleeSlash1SoundClientRpc();
        }
        else
        {
            PlayMeleeSlash1SoundServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayMeleeSlash1SoundServerRpc()
    {
        PlayMeleeSlash1SoundClientRpc();
    }

    [ClientRpc]
    private void PlayMeleeSlash1SoundClientRpc()
    {
        generalAudioSource.volume = 0.1f;
        generalAudioSource.PlayOneShot(meleeSlash1Sound);
    }

    public void PlayArcaneDevilSlamShoutSound()
    {
        if (IsServer)
        {
            PlayArcaneDevilSlamShoutSoundClientRpc();
        }
        else
        {
            PlayArcaneDevilSlamShoutSoundServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayArcaneDevilSlamShoutSoundServerRpc()
    {
        PlayArcaneDevilSlamShoutSoundClientRpc();
    }

    [ClientRpc]
    private void PlayArcaneDevilSlamShoutSoundClientRpc()
    {
        generalAudioSource.volume = 0.05f;
        generalAudioSource.PlayOneShot(arcaneDevilSlamShoutSound);
    }
}
