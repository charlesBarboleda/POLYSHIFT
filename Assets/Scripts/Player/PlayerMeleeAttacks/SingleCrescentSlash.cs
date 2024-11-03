using UnityEngine;
using System.Collections;

public class SingleCrescentSlash : MeleeAttack
{

    public override void ExecuteAttack()
    {
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in SinglesCrescentSlash.");
            return;
        }

        animator.SetTrigger("isSingleCrescentSlash");

    }

}
