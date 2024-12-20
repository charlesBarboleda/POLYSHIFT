using System.Collections.Generic;
using UnityEngine;

public static class DebuffFactory
{
    private static Dictionary<string, Debuff> debuffRegistry = new Dictionary<string, Debuff>();

    // Call this method to preload all debuff ScriptableObjects
    public static void Initialize()
    {
        Debuff[] allDebuffs = Resources.LoadAll<Debuff>("Debuffs"); // Assumes all debuffs are in a "Resources/Debuffs" folder
        foreach (var debuff in allDebuffs)
        {
            if (!debuffRegistry.ContainsKey(debuff.debuffName))
            {
                debuffRegistry.Add(debuff.debuffName, debuff);
            }
        }
    }

    public static Debuff CreateDebuff(string debuffName, float duration)
    {
        if (debuffRegistry.TryGetValue(debuffName, out Debuff debuffTemplate))
        {
            Debuff newDebuff = ScriptableObject.Instantiate(debuffTemplate);
            newDebuff.duration = duration;
            return newDebuff;
        }

        Debug.LogWarning($"Debuff with name {debuffName} not found in registry.");
        return null;
    }
}
