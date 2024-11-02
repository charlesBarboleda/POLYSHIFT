using System.Collections;
using UnityEngine;

public abstract class MeleeAttack : MonoBehaviour
{
    public float damage = 10;
    public float attackAnimDuration;

    protected Animator animator;
    protected PlayerNetworkMovement playerMovement;
    protected PlayerNetworkRotation playerRotation;
    public abstract IEnumerator ExecuteAttack();
    public virtual void Initialize(Animator animator, PlayerNetworkMovement playerMovement, PlayerNetworkRotation playerRotation)
    {
        this.animator = animator;
        this.playerMovement = playerMovement;
        this.playerRotation = playerRotation;
    }

    protected void SetAnimationDuration()
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "DoubleCrescentSlash")
            {
                attackAnimDuration = clip.length / animator.GetFloat("MeleeAttackSpeedMultiplier");
                break;
            }
        }


    }


}
