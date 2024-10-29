using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;

public class IsometricUIManager : NetworkBehaviour
{
    [SerializeField] Image healthbarFill;
    [SerializeField] GameObject healthbarContainer;

    [SerializeField] NetworkObject player;
    PlayerNetworkHealth playerHealth;


    void Update()
    {
        if (player == null)
        {
            return;
        }
        healthbarContainer.transform.position = player.transform.position + new Vector3(0, 2.5f, 0);
    }
    public void SetPlayer(NetworkObject player)
    {
        this.player = player;
        playerHealth = player.GetComponent<PlayerNetworkHealth>();

    }

    [ClientRpc]
    public void UpdateHealthbarClientRpc()
    {
        healthbarFill.fillAmount = playerHealth.currentHealth.Value / playerHealth.maxHealth.Value;
        Debug.Log("Healthbar updated visually to " + healthbarFill.fillAmount);
    }
}
