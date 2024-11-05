using UnityEngine;
using System.Collections;

public abstract class MeleeAttack : MonoBehaviour
{
    public bool OnCooldown = false;
    public float Cooldown = 1.0f; // Default cooldown, can be overridden in each derived attack class
    protected Animator animator;

    public abstract void ExecuteAttack();

    public virtual void Initialize(Animator animator)
    {
        this.animator = animator;
    }

    public void StartCooldown()
    {
        if (!OnCooldown)
        {
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        OnCooldown = true;
        yield return new WaitForSeconds(Cooldown);
        OnCooldown = false;
    }
}
