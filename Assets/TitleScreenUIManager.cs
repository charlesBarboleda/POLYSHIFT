using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using Unity.Cinemachine;

public class TitleScreenUIManager : MonoBehaviour
{
    [Header("Lobby Screen")]
    [SerializeField] Button _startGameButton;
    [Header("Play Screen")]
    [SerializeField] GameObject _playScreen;

    [Header("Instructions Screen")]
    [SerializeField] GameObject _instructionsScreen;

    [Header("Title Screen")]
    [SerializeField] GameObject _titleScreen;

    [Header("Join Screen")]
    [SerializeField] GameObject _joinScreen;

    [Header("Cameras")]
    [SerializeField] CinemachineCamera titleScreenCamera;
    [SerializeField] CinemachineCamera playScreenCamera;

    [Header("Audio UI Elements")]
    AudioSource audioSource;
    [SerializeField] AudioClip buttonClickSound;
    [SerializeField] AudioClip buttonHoverSound;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        audioSource = GetComponent<AudioSource>();
        _startGameButton.onClick.AddListener(MainMenuManager.Instance.StartGameServerRpc);
    }


    public void OnButtonPressSound()
    {
        audioSource.PlayOneShot(buttonClickSound);
    }

    public void OnButtonHoverSound()
    {
        audioSource.PlayOneShot(buttonHoverSound);
    }

    public void PlayButton()
    {
        _titleScreen.SetActive(false);
        titleScreenCamera.Priority = 0;
        playScreenCamera.Priority = 1;
    }
    public void PlayBackButton()
    {
        _titleScreen.SetActive(true);
        titleScreenCamera.Priority = 1;
        playScreenCamera.Priority = 0;
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void HostButton()
    {
        DisableAllUIElements();
        _startGameButton.gameObject.SetActive(true);
    }

    void DisableAllUIElements()
    {
        _playScreen.SetActive(false);
        _titleScreen.SetActive(false);

    }



}
