using Unity.Netcode;
using UnityEngine;

public class PlayerAnimationEvents : NetworkBehaviour
{
    [ServerRpc]
    public void SpawnCrescentSlashServerRpc()
    {
        // Spawn the effect and move the player forward slightly to match the animation
        GameObject slash = ObjectPooler.Instance.Spawn("MeleeSlash1", transform.position + transform.forward * 2f, transform.rotation * Quaternion.Euler(0, 0, 20));
    }
}
