using UnityEngine;

public class RootMotionController : MonoBehaviour
{
    Animator animator;
    public string clipNameWithoutRootMotion; // The name of the animation state where you want root motion off

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Toggle root motion based on the animation state
        if (stateInfo.IsName(clipNameWithoutRootMotion))
        {
            animator.applyRootMotion = false;
        }
        else
        {
            animator.applyRootMotion = true;
        }
    }
}
