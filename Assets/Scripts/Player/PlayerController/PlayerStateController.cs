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
    PlayerNetworkMovement movement;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerState.Value = PlayerState.Lobby;
        }
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
                // Set player-related alive logic here
                break;
            case PlayerState.Dead:
                // Set player-related dead logic here
                break;
        }
    }
}
