using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class TitleScreenUIManager : MonoBehaviour
{
    [Header("Lobby Screen")]
    [SerializeField] Button startGameButton;
    [SerializeField] TMP_Text lobbyCodeText;
    [Header("Play Screen")]
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] TMP_Text nameText;
    [SerializeField] Button backButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button instructionsButton;

    [Header("Instructions Screen")]
    [SerializeField] Button instructionsBackButton;
    [SerializeField] TMP_Text instructionsText;

    [Header("Title Screen")]
    [SerializeField] Button playButton;
    [SerializeField] TMP_Text titleText;

    [Header("Join Screen")]
    [SerializeField] TMP_InputField joinCodeInputField;
    [SerializeField] Button joinCodeButton;
    [SerializeField] Button joinLobbyBackButton;
    [SerializeField] TMP_Text wrongCodeText;

    [Header("Settings Screen")]
    [SerializeField] GameObject ambientOcclusion;
    [SerializeField] GameObject bloom;
    [SerializeField] GameObject antiAliasing;
    [SerializeField] GameObject audioMasterVolume;
    [SerializeField] GameObject fieldOfView;
    [SerializeField] GameObject resolution;
    [SerializeField] GameObject shadows;
    [SerializeField] GameObject quality;
    [SerializeField] GameObject vsync;
    [SerializeField] GameObject windowMode;
    [SerializeField] GameObject frameRate;
    [SerializeField] GameObject shadowsResolution;
    [SerializeField] Button settingsButton;
    [SerializeField] Button settingsBackButton;

    [Header("Audio UI Elements")]
    AudioSource audioSource;
    [SerializeField] AudioClip buttonClickSound;
    [SerializeField] AudioClip buttonHoverSound;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AnimateTitleScreen();
        audioSource = GetComponent<AudioSource>();
        startGameButton.onClick.AddListener(MainMenuManager.Instance.StartGameServerRpc);
    }

    void AnimateTitleScreen()
    {
        var titleCanvasGroup = titleText.GetComponent<CanvasGroup>();
        titleCanvasGroup.alpha = 0;
        titleCanvasGroup.DOFade(1, 3f).OnComplete(() =>
        {
            titleText.rectTransform.DOAnchorPos(new Vector2(0, 120), 2).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                playButton.gameObject.SetActive(true);
                playButton.GetComponent<CanvasGroup>().DOFade(1, 1f);
            });
        });
    }

    public void OnButtonPressSound()
    {
        audioSource.PlayOneShot(buttonClickSound);
    }

    public void OnButtonHoverSound()
    {
        audioSource.PlayOneShot(buttonHoverSound);
    }

    public void BackButtonInstructionsScreen()
    {
        instructionsBackButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            instructionsBackButton.gameObject.SetActive(false);

            instructionsButton.gameObject.SetActive(true);
            instructionsButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            joinButton.gameObject.SetActive(true);
            joinButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            hostButton.gameObject.SetActive(true);
            hostButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            backButton.gameObject.SetActive(true);
            backButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            playerNameInputField.gameObject.SetActive(true);
            playerNameInputField.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            nameText.gameObject.SetActive(true);
            nameText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });

        instructionsText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            instructionsText.gameObject.SetActive(false);
        });


    }

    public void BackButtonSettingsScreen()
    {
        settingsBackButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            settingsBackButton.gameObject.SetActive(false);

            settingsButton.gameObject.SetActive(true);
            settingsButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            joinButton.gameObject.SetActive(true);
            joinButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            hostButton.gameObject.SetActive(true);
            hostButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            backButton.gameObject.SetActive(true);
            backButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            playerNameInputField.gameObject.SetActive(true);
            playerNameInputField.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            instructionsButton.gameObject.SetActive(true);
            instructionsButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            nameText.gameObject.SetActive(true);
            nameText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });

        ambientOcclusion.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            ambientOcclusion.gameObject.SetActive(false);
        });

        bloom.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            bloom.gameObject.SetActive(false);
        });

        antiAliasing.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            antiAliasing.gameObject.SetActive(false);
        });

        audioMasterVolume.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            audioMasterVolume.gameObject.SetActive(false);
        });

        fieldOfView.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            fieldOfView.gameObject.SetActive(false);
        });

        resolution.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            resolution.gameObject.SetActive(false);
        });

        shadows.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            shadows.gameObject.SetActive(false);
        });

        quality.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            quality.gameObject.SetActive(false);
        });

        vsync.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            vsync.gameObject.SetActive(false);
        });

        windowMode.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            windowMode.gameObject.SetActive(false);
        });

        frameRate.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            frameRate.gameObject.SetActive(false);
        });

        shadowsResolution.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            shadowsResolution.gameObject.SetActive(false);
        });


    }

    public void SettingsButton()
    {
        settingsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
       {
           settingsButton.gameObject.SetActive(false);

           settingsBackButton.gameObject.SetActive(true);
           settingsBackButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           ambientOcclusion.SetActive(true);
           ambientOcclusion.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           bloom.SetActive(true);
           bloom.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           antiAliasing.SetActive(true);
           antiAliasing.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           audioMasterVolume.SetActive(true);
           audioMasterVolume.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           fieldOfView.SetActive(true);
           fieldOfView.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           resolution.SetActive(true);
           resolution.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           shadows.SetActive(true);
           shadows.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           quality.SetActive(true);
           quality.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           vsync.SetActive(true);
           vsync.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           windowMode.SetActive(true);
           windowMode.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           frameRate.SetActive(true);
           frameRate.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

           shadowsResolution.SetActive(true);
           shadowsResolution.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
       });

        joinButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            joinButton.gameObject.SetActive(false);
        });

        hostButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            hostButton.gameObject.SetActive(false);
        });

        backButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            backButton.gameObject.SetActive(false);
        });

        nameText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            nameText.gameObject.SetActive(false);
        });

        playerNameInputField.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playerNameInputField.gameObject.SetActive(false);
        });

        instructionsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            instructionsButton.gameObject.SetActive(false);
        });



    }

    public void InstructionsButton()
    {
        instructionsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            instructionsButton.gameObject.SetActive(false);

            instructionsBackButton.gameObject.SetActive(true);
            instructionsBackButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            instructionsText.gameObject.SetActive(true);
            instructionsText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });

        joinButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            joinButton.gameObject.SetActive(false);
        });

        hostButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            hostButton.gameObject.SetActive(false);
        });

        backButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            backButton.gameObject.SetActive(false);
        });

        nameText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            nameText.gameObject.SetActive(false);
        });

        playerNameInputField.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playerNameInputField.gameObject.SetActive(false);
        });

        settingsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            settingsButton.gameObject.SetActive(false);
        });
    }

    public void PlayButton()
    {
        playButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playButton.gameObject.SetActive(false);

            joinButton.gameObject.SetActive(true);
            joinButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            hostButton.gameObject.SetActive(true);
            hostButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            backButton.gameObject.SetActive(true);
            backButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            instructionsButton.gameObject.SetActive(true);
            instructionsButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            playerNameInputField.gameObject.SetActive(true);
            playerNameInputField.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            nameText.gameObject.SetActive(true);
            nameText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            settingsButton.gameObject.SetActive(true);
            settingsButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });
    }

    public void BackButtonPlayScreen()
    {
        backButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            backButton.gameObject.SetActive(false);

            playButton.gameObject.SetActive(true);
            playButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });

        joinButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
            {
                joinButton.gameObject.SetActive(false);
            });

        hostButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
       {
           hostButton.gameObject.SetActive(false);
       });

        instructionsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            instructionsButton.gameObject.SetActive(false);
        });

        playerNameInputField.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playerNameInputField.gameObject.SetActive(false);
        });

        nameText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            nameText.gameObject.SetActive(false);
        });
    }

    public void HostButton()
    {
        hostButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            hostButton.gameObject.SetActive(false);

            StartCoroutine(StartGameButtonCoroutine());
        });

        joinButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            joinButton.gameObject.SetActive(false);
        });

        backButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            backButton.gameObject.SetActive(false);
        });

        instructionsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            instructionsButton.gameObject.SetActive(false);
        });

        playerNameInputField.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playerNameInputField.gameObject.SetActive(false);
        });

        nameText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            nameText.gameObject.SetActive(false);
        });

        settingsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            settingsButton.gameObject.SetActive(false);
        });

    }

    IEnumerator StartGameButtonCoroutine()
    {
        yield return new WaitForSeconds(2f);
        startGameButton.gameObject.SetActive(true);
        startGameButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
    }

    public void StartGameButton()
    {
        titleText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            titleText.gameObject.SetActive(false);
        });

        startGameButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            startGameButton.gameObject.SetActive(false);
        });

        lobbyCodeText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            lobbyCodeText.gameObject.SetActive(false);
        });


        foreach (var connectedPlayer in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerLobbyController = connectedPlayer.PlayerObject.GetComponent<PlayerLobbyController>();
            var playerStateController = connectedPlayer.PlayerObject.GetComponent<PlayerStateController>();
            playerLobbyController.EnablePlayerControlsServerRpc();
            playerStateController.SetPlayerStateServerRpc(PlayerState.Alive);
            GlobalUIManager.Instance.DisableTitleTextClientRpc();
            GlobalUIManager.Instance.DisableWaitForHostTextClientRpc();
        }
    }

    public void JoinButton()
    {
        joinButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            joinButton.gameObject.SetActive(false);

            joinCodeInputField.gameObject.SetActive(true);
            joinCodeInputField.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            joinCodeButton.gameObject.SetActive(true);
            joinCodeButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            joinLobbyBackButton.gameObject.SetActive(true);
            joinLobbyBackButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });

        hostButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            hostButton.gameObject.SetActive(false);
        });

        backButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            backButton.gameObject.SetActive(false);
        });

        instructionsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            instructionsButton.gameObject.SetActive(false);
        });

        playerNameInputField.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playerNameInputField.gameObject.SetActive(false);
        });

        nameText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            nameText.gameObject.SetActive(false);
        });

        settingsButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            settingsButton.gameObject.SetActive(false);
        });
    }

    public void JoinLobbyBackButton()
    {
        joinCodeInputField.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            joinCodeInputField.gameObject.SetActive(false);

            joinButton.gameObject.SetActive(true);
            joinButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            hostButton.gameObject.SetActive(true);
            hostButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            backButton.gameObject.SetActive(true);
            backButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            instructionsButton.gameObject.SetActive(true);
            instructionsButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            playerNameInputField.gameObject.SetActive(true);
            playerNameInputField.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            settingsButton.gameObject.SetActive(true);
            settingsButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            nameText.gameObject.SetActive(true);
            nameText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });

        joinCodeButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            joinCodeButton.gameObject.SetActive(false);
        });

        wrongCodeText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            wrongCodeText.gameObject.SetActive(false);
        });

        joinLobbyBackButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            joinLobbyBackButton.gameObject.SetActive(false);
        });



    }


}
