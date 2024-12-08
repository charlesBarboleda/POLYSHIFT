using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerState
{
    Dead,
    Alive
}
public class PlayerStateController : NetworkBehaviour
{
    public NetworkVariable<PlayerState> playerState = new NetworkVariable<PlayerState>(PlayerState.Alive, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Components")]
    Rigidbody rb;
    PlayerCameraBehavior playerCameraBehavior;
    PlayerWeapon playerWeapon;
    PlayerSkills playerSkills;
    PlayerNetworkHealth playerHealth;
    PlayerNetworkMovement playerMovement;
    PlayerNetworkRotation playerRotation;

    Animator animator;
    [Header("UI")]
    [SerializeField] TMP_Text youDiedText;
    [SerializeField] GameObject firstPersonCanvas;
    [SerializeField] GameObject infoCanvas;
    [SerializeField] GameObject ammoCountUI;
    [SerializeField] GameObject hotbarUI;
    [SerializeField] GameObject bodyMesh;
    [SerializeField] GameObject bodyRoot;
    [SerializeField] GameObject playAgainButton;
    [SerializeField] GameObject mainMenuButton;
    [SerializeField] GameObject waitingForHostText;
    [SerializeField] GameObject titleText;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (GameManager.Instance != null)
            {
                playAgainButton.GetComponent<Button>().onClick.AddListener(GameManager.Instance.StartGameServerRpc);
                mainMenuButton.GetComponent<Button>().onClick.AddListener(GameManager.Instance.DisconnectGameServerRpc);
            }
            playerState.Value = PlayerState.Alive;
        }
        if (!IsOwner)
        {
            firstPersonCanvas.SetActive(false);
            hotbarUI.SetActive(false);
            ammoCountUI.SetActive(false);
        }
        playerState.OnValueChanged += OnPlayerStateChanged;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerCameraBehavior = GetComponent<PlayerCameraBehavior>();
        playerWeapon = GetComponent<PlayerWeapon>();
        playerSkills = GetComponent<PlayerSkills>();
        playerHealth = GetComponent<PlayerNetworkHealth>();
        playerMovement = GetComponent<PlayerNetworkMovement>();
        playerRotation = GetComponent<PlayerNetworkRotation>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerStateServerRpc(PlayerState newState)
    {
        playerState.Value = newState;
    }

    void OnPlayerStateChanged(PlayerState previousState, PlayerState newState)
    {
        if (newState == PlayerState.Dead)
        {
            HandleDeathState();
        }
        else if (newState == PlayerState.Alive)
        {
            HandleAliveState();
        }

    }

    void HandleDeathState()
    {
        youDiedText.gameObject.SetActive(true);
        youDiedText.GetComponent<CanvasGroup>()?.DOFade(1, 0.5f);

        ammoCountUI.GetComponent<CanvasGroup>()?.DOFade(0, 0.5f).OnComplete(() =>
        {
            ammoCountUI.SetActive(false);
        });

        firstPersonCanvas.GetComponent<CanvasGroup>()?.DOFade(0, 0.5f).OnComplete(() =>
        {
            firstPersonCanvas.SetActive(false);
        });

        bodyMesh.SetActive(false);
        bodyRoot.SetActive(false);

        rb.isKinematic = true;

        playerCameraBehavior.EnableSpectatorMode();

        animator.enabled = false;
        playerWeapon.enabled = false;
        playerSkills.enabled = false;
    }

    void HandleAliveState()
    {
        youDiedText.gameObject.SetActive(false);
        infoCanvas.SetActive(true);
        firstPersonCanvas.SetActive(true);
        hotbarUI.SetActive(true);
        ammoCountUI.SetActive(true);

        rb.isKinematic = false;
        animator.enabled = true;
        playerWeapon.enabled = true;

        playerSkills.enabled = true;
        playerMovement.canMove = true;
        playerRotation.canRotate = true;

        bodyMesh.SetActive(true);
        bodyRoot.SetActive(true);

        playerCameraBehavior.DisableSpectatorMode();
        playerCameraBehavior.EnableFirstPersonCamera();
    }

    public void ShowGameOverUI()
    {
        if (IsLocalPlayer) // Show UI only for the local player
        {
            if (IsHost)
            {
                // Host-specific UI
                titleText.SetActive(true);
                titleText.GetComponent<CanvasGroup>()?.DOFade(1, 0.5f);

                playAgainButton.SetActive(true);
                playAgainButton.GetComponent<CanvasGroup>()?.DOFade(1, 0.5f);

                mainMenuButton.SetActive(true);
                mainMenuButton.GetComponent<CanvasGroup>()?.DOFade(1, 0.5f);

                waitingForHostText.SetActive(false);
            }
            else
            {
                // Client-specific UI
                titleText.SetActive(true);
                titleText.GetComponent<CanvasGroup>()?.DOFade(1, 0.5f);

                waitingForHostText.SetActive(true);
                waitingForHostText.GetComponent<CanvasGroup>()?.DOFade(1, 0.5f);

                mainMenuButton.SetActive(true);
                mainMenuButton.GetComponent<CanvasGroup>()?.DOFade(1, 0.5f);

                playAgainButton.SetActive(false);
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    public void ShowLobbyUI()
    {
        if (IsLocalPlayer) // Show UI only for the local player
        {
            if (IsHost)
            {
                GlobalUIManager.Instance.EnableHostLobbyUI();
            }
            else
            {
                GlobalUIManager.Instance.EnableClientLobbyUI();
            }
        }
    }


    public override void OnNetworkDespawn()
    {
        playerState.OnValueChanged -= OnPlayerStateChanged;

    }


}
