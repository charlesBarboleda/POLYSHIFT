using System.Collections.Generic;
using UnityEngine;

public class RootMotionController : MonoBehaviour
{
    Animator animator;
    public List<string> clipNameWithRootMotion = new List<string>(); // The name of the animation state where you want root motion off

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Toggle root motion based on the animation state
        foreach (string clipName in clipNameWithRootMotion)
        {
            if (stateInfo.IsName(clipName))
            {
                animator.applyRootMotion = true;
                return;
            }
            else
            {
                animator.applyRootMotion = false;
            }
        }
    }
}
