using UnityEngine;
using Unity.Netcode;
public class LifeSurgeManager : NetworkBehaviour, ISkillManager
{
    public float Damage { get; set; }
    public float KnockbackForce { get; set; }
    public VariableWithEvent<float> AttackSpeedMultiplier { get; set; } = new VariableWithEvent<float>();
    public float AttackRange { get; set; }
    public Animator animator { get; set; }
    float healRadius = 10f;
    float healStrength = 0.5f;

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        AttackSpeedMultiplier.OnValueChanged += SetAttackSpeedMultiplier;
    }

    public void IncreaseHealStrength(float amount)
    {
        healStrength += amount;
    }

    public void IncreaseHealRadius(float amount)
    {
        healRadius += amount;
    }

    public void AreaHeal()
    {
        GameObject healEffect = ObjectPooler.Instance.Spawn("LifeCast", transform.position, Quaternion.identity);
        healEffect.transform.rotation = Quaternion.Euler(-90, 0, 90);
        healEffect.GetComponent<NetworkObject>().Spawn();
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                if (collider.TryGetComponent<PlayerNetworkHealth>(out PlayerNetworkHealth player))
                {
                    player.currentHealth.Value += player.maxHealth.Value * healStrength;

                }
                if (collider.TryGetComponent<Golem>(out Golem golem))
                {
                    golem.CurrentHealth.Value += golem.MaxHealth.Value * healStrength;
                }
            }
        }
    }

    public void SetAttackSpeedMultiplier(float current)
    {
        if (animator == null)
        {
            Debug.LogError("Animator not initialized in Life Surge Manager.");
            return;
        }
        animator.SetFloat("MeleeAttackSpeedMultiplier", current);
    }
}
