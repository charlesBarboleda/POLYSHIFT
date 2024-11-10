using UnityEngine;

public abstract class ActiveSkill : Skill
{
    public float Cooldown = 5f;
    public bool OnCooldown { get; private set; }
    private float cooldownTimer;
    protected Animator animator;

    public virtual void Initialize(Animator animator)
    {
        this.animator = animator;
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

    protected void StartCooldown()
    {
        OnCooldown = true;
        cooldownTimer = Cooldown;
    }

    public virtual void ExecuteAttack()
    {
        Debug.Log("Executing attack.");
    }

    private void CountdownCooldown()
    {
        if (OnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                OnCooldown = false;
                cooldownTimer = 0;
            }
        }
    }

    protected void Update()
    {
        CountdownCooldown();
    }
}
