using Unity.Netcode;
using UnityEngine;

public class PlayerAnimationEvents : NetworkBehaviour
{
    Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        animator = GetComponent<Animator>();
    }

    public void RemoveRootMotion()
    {
        animator.applyRootMotion = false;
    }
}
