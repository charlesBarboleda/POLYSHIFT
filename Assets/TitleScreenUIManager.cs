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

    [Header("Settings Screen")]
    [SerializeField] GameObject _settingsScreen;

    [Header("Cameras")]
    [SerializeField] CinemachineCamera titleScreenCamera;
    [SerializeField] CinemachineCamera playScreenCamera;
    [SerializeField] CinemachineCamera joinScreenCamera;
    [SerializeField] CinemachineCamera instructionsScreenCamera;
    [SerializeField] CinemachineCamera settingsScreenCamera;
    [Header("Audio UI Elements")]
    AudioSource audioSource;
    [SerializeField] AudioClip buttonClickSound;
    [SerializeField] AudioClip buttonHoverSound;
    [Header("ETC")]
    [SerializeField] GameObject _loadingScreen;


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
        _titleScreen.GetComponent<CanvasGroup>().DOFade(0, 0.25f).OnComplete(() =>
        {
            _titleScreen.SetActive(false);
            titleScreenCamera.Priority = 0;
            playScreenCamera.Priority = 1;
        });
    }

    public void JoinButton()
    {
        playScreenCamera.Priority = 0;
        joinScreenCamera.Priority = 1;
    }
    public void SettingsButton()
    {
        playScreenCamera.Priority = 0;
        settingsScreenCamera.Priority = 1;
        StartCoroutine(EnableSettingsScreen());
    }

    public void SettingsBackButton()
    {
        settingsScreenCamera.Priority = 0;
        playScreenCamera.Priority = 1;
        _settingsScreen.SetActive(false);
    }

    IEnumerator EnableSettingsScreen()
    {
        yield return new WaitForSeconds(1f);
        _settingsScreen.SetActive(true);
        _settingsScreen.GetComponentInChildren<CanvasGroup>().DOFade(1, 1f);
    }
    public void PlayBackButton()
    {
        StartCoroutine(TitleScreenAnimations());
        titleScreenCamera.Priority = 1;
        playScreenCamera.Priority = 0;
    }

    IEnumerator TitleScreenAnimations()
    {
        yield return new WaitForSeconds(1f);
        _titleScreen.SetActive(true);
        _titleScreen.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void HostButton()
    {
        DisableAllUIElements();
        StartCoroutine(EnableStartGameButton());
    }

    public void InstructionsButton()
    {
        playScreenCamera.Priority = 0;
        instructionsScreenCamera.Priority = 1;
    }

    public void InstructionsBackButton()
    {
        instructionsScreenCamera.Priority = 0;
        playScreenCamera.Priority = 1;
    }

    IEnumerator EnableStartGameButton()
    {
        yield return new WaitForSeconds(3f);
        _startGameButton.gameObject.SetActive(true);
        _startGameButton.GetComponent<CanvasGroup>().DOFade(1, 1f);
    }

    public void JoinBackButton()
    {
        joinScreenCamera.Priority = 0;
        playScreenCamera.Priority = 1;
    }

    public void DisableAllUIElements()
    {
        _playScreen.SetActive(false);
        _titleScreen.SetActive(false);
        _joinScreen.SetActive(false);
        _settingsScreen.SetActive(false);
        _instructionsScreen.SetActive(false);


    }


}
