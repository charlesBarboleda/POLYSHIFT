using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] GameObject firstPersonCanvas;
    PlayerNetworkMovement playerNetworkMovement;


    void Start()
    {
        playerNetworkMovement = GetComponent<PlayerNetworkMovement>();
    }
    void Update()
    {
        if (playerNetworkMovement.IsLocalPlayer)
        {
            if (!playerNetworkMovement.IsIsometric)
            {
                firstPersonCanvas.SetActive(true);
            }
            else
            {
                firstPersonCanvas.SetActive(false);
            }
        }
    }
}
