using System.Collections.Generic;
using UnityEngine;

public class GolemManager : MonoBehaviour
{
    public List<Golem> SpawnedGolems = new List<Golem>();

    public void IncreaseGolemDamageReduction(float amount)
    {
        var GuardianGolem = SpawnedGolems.Find(golem => golem is GuardianGolem) as GuardianGolem;

        if (GuardianGolem != null)
        {
            GuardianGolem.IncreaseDamageReduction(amount);
        }
    }

    public void IncreaseGolemHealth(float amount)
    {
        var GuardianGolem = SpawnedGolems.Find(golem => golem is GuardianGolem) as GuardianGolem;

        if (GuardianGolem != null)
        {
            GuardianGolem.IncreaseHealth(amount);
        }
    }

    public void IncreaseGolemDamage(float amount)
    {
        var GuardianGolem = SpawnedGolems.Find(golem => golem is GuardianGolem) as GuardianGolem;

        if (GuardianGolem != null)
        {
            GuardianGolem.IncreaseDamage(amount);
        }
    }

    public void IncreaseGolemAttackRange(float amount)
    {
        var GuardianGolem = SpawnedGolems.Find(golem => golem is GuardianGolem) as GuardianGolem;

        if (GuardianGolem != null)
        {
            GuardianGolem.IncreaseAttackRange(amount);
        }
    }

    public void IncreaseGolemMovementSpeed(float amount)
    {
        var GuardianGolem = SpawnedGolems.Find(golem => golem is GuardianGolem) as GuardianGolem;

        if (GuardianGolem != null)
        {
            GuardianGolem.IncreaseMovementSpeed(amount);
        }
    }


}
