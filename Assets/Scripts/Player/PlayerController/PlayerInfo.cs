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

        // Set the local player name via the GameManager
        if (IsLocalPlayer)
        {
            GameManager.Instance.SetLocalPlayer(this);
        }

        // Update UI on spawn
        UpdatePlayerNameUI(PlayerName.Value.ToString());
    }

    private void OnPlayerNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        UpdatePlayerNameUI(newValue.ToString());
        Debug.Log($"Player name changed from {previousValue} to {newValue} on client {OwnerClientId}");
    }

    private void UpdatePlayerNameUI(string name)
    {
        if (_txtPlayerName != null)
        {
            _txtPlayerName.SetText(name);
        }
        else
        {
            Debug.LogWarning("Player name text component not assigned.");
        }
    }

    public void SetName(string name)
    {
        if (IsOwner)
        {
            PlayerName.Value = new FixedString64Bytes(name);
            UpdateNameForClientsClientRpc(name); // Explicitly update for all clients
        }
    }

    [ClientRpc]
    private void UpdateNameForClientsClientRpc(string name)
    {
        UpdatePlayerNameUI(name);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        PlayerName.OnValueChanged -= OnPlayerNameChanged;
    }
}
