using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class PlayerUIManager : NetworkBehaviour
{
    [SerializeField] GameObject firstPersonCanvas;
    [SerializeField] GameObject hotbarUI;
    PlayerNetworkMovement playerNetworkMovement;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
        playerNetworkMovement.IsIsometric.OnValueChanged += OnIsometricChanged;
    }


    void OnIsometricChanged(bool current)
    {
        if (playerNetworkMovement.IsLocalPlayer)
        {
            RectTransform hotbarRectTransform = hotbarUI.GetComponent<RectTransform>();
            Vector2 targetPosition;

            if (!current)
            {
                firstPersonCanvas.SetActive(true);
                firstPersonCanvas.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

                // Adjust the Y position for first-person view
                targetPosition = new Vector2(hotbarRectTransform.anchoredPosition.x, -140);
            }
            else
            {
                // Adjust the Y position for isometric view
                targetPosition = new Vector2(hotbarRectTransform.anchoredPosition.x, -190);

                firstPersonCanvas.GetComponent<CanvasGroup>()
                    .DOFade(0, 0.5f)
                    .OnComplete(() => firstPersonCanvas.SetActive(false));
            }

            // Tween to the new anchored position
            hotbarRectTransform.DOAnchorPos(targetPosition, 1f);
        }
    }

}
