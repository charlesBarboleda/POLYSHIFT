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
    SkinnedMeshRenderer skinnedMeshRenderer;
    Animator animator;
    [Header("UI")]
    [SerializeField] TMP_Text youDiedText;
    [SerializeField] GameObject firstPersonCanvas;
    [SerializeField] GameObject infoCanvas;
    [SerializeField] GameObject hotbarUI;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerState.Value = PlayerState.Lobby;
        }
        rb = GetComponent<Rigidbody>();
        playerCameraBehavior = GetComponent<PlayerCameraBehavior>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }


    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerStateServerRpc(PlayerState state)
    {
        playerState.Value = state;

        switch (state)
        {
            case PlayerState.Lobby:
                // Set player-related lobby logic here
                break;
            case PlayerState.Alive:
                rb.isKinematic = false;
                break;
            case PlayerState.Dead:
                youDiedText.gameObject.SetActive(true);
                youDiedText.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

                rb.isKinematic = true;

                firstPersonCanvas.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
                {
                    firstPersonCanvas.SetActive(false);
                });

                hotbarUI.SetActive(false);

                infoCanvas.SetActive(false);

                playerCameraBehavior.EnableSpectatorMode();

                animator.enabled = false;

                skinnedMeshRenderer.gameObject.transform.SetParent(null);

                break;
        }
    }


}
