using System.Collections.Generic;
using UnityEngine;

public class DebuffManager : MonoBehaviour
{
    [SerializeField] List<Debuff> debuffs = new List<Debuff>();


    void Update()
    {
        List<Debuff> debuffsCopy = new List<Debuff>(debuffs);  // Create a copy of the list
        foreach (Debuff debuff in debuffsCopy)
        {
            debuff.UpdateEffect(gameObject);
        }
    }
    public void AddDebuff(Debuff debuff)
    {
        if (!debuffs.Contains(debuff))
        {
            debuffs.Add(debuff);
            debuff.Initialize(gameObject);
            return;
        }
        else
        {
            foreach (Debuff localDebuff in debuffs)
            {
                if (localDebuff == debuff)
                {
                    localDebuff.duration = debuff.duration;
                    return;
                }
            }
        }
    }

    public void RemoveDebuff(Debuff debuff)
    {
        if (debuffs.Contains(debuff))
            debuffs.Remove(debuff);
    }

    public void RemoveAllDebuffs()
    {
        debuffs.Clear();
    }



}
