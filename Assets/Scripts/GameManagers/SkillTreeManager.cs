using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public int skillPoints = 5;
    public List<Skill> availableSkills;  // List of all skills available for unlocking

    public void UnlockSkill(Skill skill, PlayerSkills playerSkills)
    {
        if (skillPoints > 0 && !playerSkills.unlockedSkills.Contains(skill))
        {
            skillPoints--;
            playerSkills.UnlockSkill(skill);
            Debug.Log($"{skill.skillName} unlocked!");
        }
    }
}
