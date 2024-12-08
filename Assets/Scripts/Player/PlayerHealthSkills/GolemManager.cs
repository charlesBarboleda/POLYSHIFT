using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemManager : MonoBehaviour
{
    public List<Golem> SpawnedGolems = new List<Golem>();

    public void ResetGolems()
    {
        foreach (var golem in SpawnedGolems)
        {
            golem.DespawnGolemServerRpc();
        }
    }

    public void IncreaseGolemDamageReduction(float amount)
    {
        foreach (var golem in SpawnedGolems)
        {
            // Apply diminishing returns to ensure DamageReduction never reaches 100%
            golem.DamageReduction = 1 - (1 - golem.DamageReduction) * (1 - amount);
        }
    }

    public void MassRecall()
    {
        foreach (var golem in SpawnedGolems)
        {
            golem.transform.position = transform.position + Random.insideUnitSphere * 10;
        }
    }





    public void IncreaseGolemHealth(float amount)
    {
        foreach (var golem in SpawnedGolems)
        {
            golem.IncreaseHealthServerRpc(amount);
        }
    }

    public void IncreaseGolemDamage(float amount)
    {
        foreach (var golem in SpawnedGolems)
        {
            golem.IncreaseDamage(amount);
        }
    }

    public void IncreaseGolemAttackRange(float amount)
    {
        foreach (var golem in SpawnedGolems)
        {
            golem.IncreaseAttackRange(amount);
        }
    }

    public void IncreaseGolemMovementSpeed(float amount)
    {
        foreach (var golem in SpawnedGolems)
        {
            golem.IncreaseMovementSpeed(amount);
        }
    }

    public void IncreaseBuffRadius(float amount)
    {
        foreach (var golem in SpawnedGolems)
        {
            golem.IncreaseBuffRadius(amount);
        }
    }




}
