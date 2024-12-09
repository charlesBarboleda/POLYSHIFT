using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class PlayerInfo : NetworkBehaviour
{
    [SerializeField] TMP_Text _txtPlayerName;

    // NetworkVariable for the player's name, with owner write permission
    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("Player Name"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to name changes
        PlayerName.OnValueChanged += OnPlayerNameChanged;

        if (IsLocalPlayer)
        {
            // Set the local player name via MainMenuManager
            MainMenuManager.Instance.SetLocalPlayer(this);

            // Update the player name for the server
            MainMenuManager.Instance.SetPlayerName(GetComponent<NetworkObject>(), PlayerName.Value.ToString());
        }

        // Update UI on spawn for all players
        UpdatePlayerNameUI(PlayerName.Value.ToString());
    }

    private void OnPlayerNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        // Update the UI for all clients when the name changes
        UpdatePlayerNameUI(newValue.ToString());
    }

    private void UpdatePlayerNameUI(string name)
    {
        if (_txtPlayerName != null)
        {
            _txtPlayerName.SetText(name);
        }
    }

    public void SetName(string name)
    {
        if (IsOwner)
        {
            PlayerName.Value = new FixedString64Bytes(name);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        PlayerName.OnValueChanged -= OnPlayerNameChanged;
    }
}

