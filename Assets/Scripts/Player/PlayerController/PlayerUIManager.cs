using UnityEngine;
using DG.Tweening;
using Unity.Netcode;
using TMPro;

public class PlayerUIManager : NetworkBehaviour
{
    [SerializeField] GameObject firstPersonCanvas;
    [SerializeField] GameObject hotbarUI;
    [SerializeField] GameObject ammoCountUI;
    [SerializeField] GameObject countdownText;
    [SerializeField] GameObject gameLevelText;
    [SerializeField] GameObject playAgainButton;
    [SerializeField] GameObject mainMenuButton;
    [SerializeField] GameObject waitingForHostText;
    [SerializeField] GameObject youAreDeadText;
    [SerializeField] GameObject titleText;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject pauseMenu;

    private PlayerNetworkMovement playerNetworkMovement;
    private PlayerWeapon playerWeapon;
    private TMP_Text countdownTextText;
    private GameManager gameManager;

    private bool isPaused = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerWeapon = GetComponent<PlayerWeapon>();
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerNetworkMovement.IsIsometric.OnValueChanged += OnIsometricChanged;
        countdownTextText = countdownText.GetComponent<TextMeshProUGUI>();

        if (GameManager.Instance != null)
            gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (countdownText.activeSelf && gameManager != null)
            UpdateCountdownText();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsMenu.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                TogglePauseMenu();
            }
        }

        if (isPaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    public void TogglePauseMenu()
    {
        if (IsHost)
        {
            if (!pauseMenu.activeSelf && Time.timeScale == 1)
            {
                // Pause the game and show the menu
                gameManager.PauseUnpauseGame(forcePause: true);
                pauseMenu.SetActive(true);
                isPaused = true;
            }
            else if (pauseMenu.activeSelf && Time.timeScale == 0)
            {
                // Unpause the game and hide the menu
                gameManager.PauseUnpauseGame(forceUnpause: true);
                pauseMenu.SetActive(false);
                isPaused = false;
            }
        }
        else
        {
            // For clients, just toggle the pause menu visibility
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }

        // Ensure settings menu is closed
        if (!pauseMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
        }
    }

    public void PlayButton()
    {
        if (IsHost)
        {
            gameManager.PauseUnpauseGame();
        }

        pauseMenu.SetActive(false);
        isPaused = false;
    }

    public void SettingsBackButton()
    {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void SettingsButton()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);

        if (IsHost)
        {
            gameManager.PauseUnpauseGame();
        }
    }

    void OnIsometricChanged(bool current)
    {
        if (playerNetworkMovement.IsLocalPlayer)
        {
            RectTransform hotbarRectTransform = hotbarUI.GetComponent<RectTransform>();
            RectTransform ammoCountRectTransform = ammoCountUI.GetComponent<RectTransform>();
            Vector2 hotbarTargetPosition;
            Vector2 ammoCountTargetPosition;

            if (!current)
            {
                firstPersonCanvas.SetActive(true);
                firstPersonCanvas.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

                // Adjust the X, Y position for first-person view
                hotbarTargetPosition = new Vector2(hotbarRectTransform.anchoredPosition.x, -140);
                ammoCountTargetPosition = new Vector2(75, 75); // Vector2 position relative to the anchored position
            }
            else
            {
                // Adjust the X, Y position for isometric view
                hotbarTargetPosition = new Vector2(hotbarRectTransform.anchoredPosition.x, -190);
                ammoCountTargetPosition = new Vector2(405, 75);

                firstPersonCanvas.GetComponent<CanvasGroup>()
                    .DOFade(0, 0.5f)
                    .OnComplete(() => firstPersonCanvas.SetActive(false));
            }

            // Tween to the new anchored position
            ammoCountRectTransform.DOAnchorPos(ammoCountTargetPosition, 1f);
            hotbarRectTransform.DOAnchorPos(hotbarTargetPosition, 1f);
        }
    }

    public void EnableCountdownText()
    {
        countdownText.SetActive(true);
        countdownText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
    }

    void UpdateCountdownText()
    {
        countdownTextText.text = Mathf.Round(gameManager.GameCountdown.Value).ToString();
    }

    public void DisableCountdownText()
    {
        countdownText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => countdownText.SetActive(false));
    }

    public void EnableGameLevelText()
    {
        gameLevelText.SetActive(true);
        gameLevelText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
    }

    public void DisableGameLevelText()
    {
        gameLevelText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => gameLevelText.SetActive(false));
    }

    public void DisableGameOverUI()
    {
        if (IsHost)
        {
            playAgainButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => playAgainButton.SetActive(false));
        }
        else
        {
            waitingForHostText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => waitingForHostText.SetActive(false));
        }

        titleText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => titleText.SetActive(false));
        mainMenuButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => mainMenuButton.SetActive(false));
        youAreDeadText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => youAreDeadText.SetActive(false));
    }
}
