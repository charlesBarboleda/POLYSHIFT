using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    public static SkillCooldownManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private Dictionary<Skill, Coroutine> activeCooldowns = new Dictionary<Skill, Coroutine>();

    public void StartCooldown(Skill skill, float cooldownTime, Action onComplete)
    {
        if (activeCooldowns.ContainsKey(skill))
            return;

        Coroutine cooldownRoutine = StartCoroutine(CooldownCoroutine(skill, cooldownTime, onComplete));
        activeCooldowns[skill] = cooldownRoutine;
    }

    private IEnumerator CooldownCoroutine(Skill skill, float cooldownTime, Action onComplete)
    {
        yield return new WaitForSeconds(cooldownTime);
        activeCooldowns.Remove(skill);
        onComplete?.Invoke();
    }
}

