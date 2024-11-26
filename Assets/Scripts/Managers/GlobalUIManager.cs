using Unity.Netcode;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GlobalUIManager : NetworkBehaviour
{
    public static GlobalUIManager Instance { get; private set; }
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _waitForHostText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedClientRpc;
        }
    }

    [ClientRpc]
    void OnClientConnectedClientRpc(ulong clientId)
    {
        if (IsHost) return;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            _waitForHostText.gameObject.SetActive(true);
        }
    }

    [ClientRpc]
    public void DisableTitleTextClientRpc()
    {
        _titleText.DOFade(0, 0.5f).OnComplete(() =>
        {
            _titleText.gameObject.SetActive(false);
        });
    }

    [ClientRpc]
    public void DisableWaitForHostTextClientRpc()
    {
        if (IsHost) return;
        _waitForHostText.DOFade(0, 0.5f).OnComplete(() =>
        {
            _waitForHostText.gameObject.SetActive(false);
        });
    }


}
