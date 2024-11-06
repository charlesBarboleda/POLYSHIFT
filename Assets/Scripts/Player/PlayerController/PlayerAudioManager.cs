using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class PlayerAudioManager : NetworkBehaviour
{
    AudioSource audioSource;
    AudioSource audioSource2;
    [SerializeField] AudioClip levelUpSound;
    [SerializeField] AudioClip shootSound;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        audioSource = GetComponent<AudioSource>();
        audioSource2 = GetComponentInChildren<AudioSource>();

    }

    public void PlayLevelUpSound()
    {
        audioSource.PlayOneShot(levelUpSound);
        audioSource.DOFade(0, 5f).OnComplete(() =>
        {
            audioSource.Stop();
            audioSource.volume = 1;
        }
        );
    }

    public void PlayShootSound()
    {
        audioSource2.volume = 0.5f;
        audioSource2.PlayOneShot(shootSound);
    }

}
