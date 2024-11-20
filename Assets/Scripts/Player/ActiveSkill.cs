using System;
using UnityEngine;

public abstract class ActiveSkill : Skill
{
    public float Cooldown = 5f;
    [field: SerializeField] public bool OnCooldown { get; private set; } = false;
    public float cooldownTimer;
    protected Animator animator;
    public abstract void ExecuteAttack();

    public virtual void Initialize(Animator animator)
    {
        this.animator = animator;
        cooldownTimer = Cooldown;
    }

    public override void ApplySkillEffect(GameObject user)
    {
        if (OnCooldown)
        {
            Debug.Log($"{skillName} is on cooldown.");
            return;
        }
        ExecuteAttack();
        StartCooldown();
    }




    public void StartCooldown()
    {
        OnCooldown = true;
        cooldownTimer = 0;
    }

    private void CountdownCooldown()
    {
        if (OnCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= Cooldown)
            {
                OnCooldown = false;
                cooldownTimer = 0;
            }
        }
    }

    public void Update()
    {
        CountdownCooldown();
    }
}
