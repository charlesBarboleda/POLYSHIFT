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

    [Header("Title Screen")]
    [SerializeField] Button playButton;
    [SerializeField] TMP_Text titleText;

    [Header("Join Screen")]
    [SerializeField] TMP_InputField joinCodeInputField;
    [SerializeField] Button joinCodeButton;
    [SerializeField] Button joinLobbyBackButton;
    [SerializeField] TMP_Text wrongCodeText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AnimateTitleScreen();
    }

    void AnimateTitleScreen()
    {
        var titleCanvasGroup = titleText.GetComponent<CanvasGroup>();
        titleCanvasGroup.alpha = 0;
        titleCanvasGroup.DOFade(1, 3f).OnComplete(() =>
        {
            titleText.rectTransform.DOAnchorPos(new Vector2(0, 120), 2).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                playButton.GetComponent<CanvasGroup>().DOFade(1, 1f);
            });
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

            playerNameInputField.gameObject.SetActive(true);
            playerNameInputField.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            nameText.gameObject.SetActive(true);
            nameText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
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

        playerNameInputField.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playerNameInputField.gameObject.SetActive(false);
        });

        nameText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            nameText.gameObject.SetActive(false);
        });

    }

    IEnumerator StartGameButtonCoroutine()
    {
        yield return new WaitForSeconds(3f);
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

        playerNameInputField.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playerNameInputField.gameObject.SetActive(false);
        });

        nameText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            nameText.gameObject.SetActive(false);
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
