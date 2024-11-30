using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public enum PlayerState
{
    Lobby,
    Dead,
    Alive
}
public class PlayerStateController : NetworkBehaviour
{
    public NetworkVariable<PlayerState> playerState = new NetworkVariable<PlayerState>(PlayerState.Lobby, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Components")]
    Rigidbody rb;
    PlayerCameraBehavior playerCameraBehavior;
    PlayerWeapon playerWeapon;
    PlayerSkills playerSkills;
    Animator animator;
    [Header("UI")]
    [SerializeField] TMP_Text youDiedText;
    [SerializeField] GameObject firstPersonCanvas;
    [SerializeField] GameObject infoCanvas;
    [SerializeField] GameObject hotbarUI;
    [SerializeField] GameObject bodyMesh;
    [SerializeField] GameObject bodyRoot;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerState.Value = PlayerState.Lobby;
        }
        playerState.OnValueChanged += OnPlayerStateChanged;

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerCameraBehavior = GetComponent<PlayerCameraBehavior>();
        playerWeapon = GetComponent<PlayerWeapon>();
        playerSkills = GetComponent<PlayerSkills>();
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
        firstPersonCanvas.SetActive(true);
        hotbarUI.SetActive(true);
        infoCanvas.SetActive(true);

        rb.isKinematic = false;
        animator.enabled = true;
    }

    public override void OnNetworkDespawn()
    {
        playerState.OnValueChanged -= OnPlayerStateChanged;


    }
}
