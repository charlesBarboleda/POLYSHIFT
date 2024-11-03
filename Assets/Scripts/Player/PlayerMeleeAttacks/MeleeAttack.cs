using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class MeleeAttack : MonoBehaviour
{
    protected Animator animator;
    public abstract void ExecuteAttack();

    public virtual void Initialize(Animator animator)
    {
        this.animator = animator;
    }


}
