using UnityEngine;
using System.Collections;

public class DoubleCrescentSlash : MeleeAttack
{

    public override void ExecuteAttack()
    {
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in DoubleCrescentSlash.");
            return;
        }

        animator.SetTrigger("isDoubleCrescentSlash");

    }


}
