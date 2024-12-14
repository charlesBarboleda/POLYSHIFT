using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class PlayerInfo : NetworkBehaviour
{
    [SerializeField] private TMP_Text _txtPlayerName;

    // NetworkVariable to synchronize the player's name
    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("Player"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to name changes
        PlayerName.OnValueChanged += OnPlayerNameChanged;

        // Update the UI immediately
        UpdatePlayerNameUI(PlayerName.Value.ToString());
    }

    // Called to set the player's name
    public void SetName(string name)
    {
        if (IsOwner)
        {
            PlayerName.Value = new FixedString64Bytes(name);
            Debug.Log($"PlayerInfo: Set name to {name}");
        }
    }

    private void OnPlayerNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        Debug.Log($"PlayerInfo: Name changed from {previousValue} to {newValue}");
        UpdatePlayerNameUI(newValue.ToString());
    }

    private void UpdatePlayerNameUI(string name)
    {
        if (_txtPlayerName != null)
        {
            _txtPlayerName.text = name;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        PlayerName.OnValueChanged -= OnPlayerNameChanged;
    }
}
