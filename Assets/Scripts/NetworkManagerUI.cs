using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button clientBtn;
    [SerializeField] Button hostBtn;

    [SerializeField] Button serverBtn;

    void Awake()
    {
        clientBtn.onClick.AddListener(OnClientBtnClicked);
        hostBtn.onClick.AddListener(OnHostBtnClicked);
        serverBtn.onClick.AddListener(OnServerBtnClicked);
    }

    void OnClientBtnClicked()
    {
        NetworkManager.Singleton.StartClient();
    }

    void OnHostBtnClicked()
    {
        NetworkManager.Singleton.StartHost();
    }

    void OnServerBtnClicked()
    {
        NetworkManager.Singleton.StartServer();
    }

}
