using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour
{
    public int skillPoints = 0;
    [SerializeField] GameObject confirmationBox;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;
    public Skill currentSkill;
    [SerializeField] List<RectTransform> skillNodes;
    [SerializeField] float unlockRadius = 200f; // Radius for unlocking nearby skills
    Dictionary<Skill, RectTransform> skillNodeMap = new Dictionary<Skill, RectTransform>();
    PlayerSkills playerSkills;

    void Start()
    {
        confirmationBox.SetActive(false);
        noButton.onClick.AddListener(() => confirmationBox.SetActive(false));

        for (int i = 0; i < skillNodes.Count; i++)
        {
            Skill skill = skillNodes[i].GetComponent<SkillNodeController>().skill; // Assuming each node has a Skill component or reference
            if (skill != null)
            {
                skillNodeMap[skill] = skillNodes[i];
            }
        }
    }

    void UnlockSkill(Skill skill)
    {
        if (playerSkills == null)
        {
            Debug.LogError("playerSkills is null in UnlockSkill. It may not have been assigned correctly.");
            return;
        }

        if (skillPoints <= 0)
        {
            description.text = "Not enough skill points!";
            return;
        }
        if (playerSkills.unlockedSkills.Contains(skill))
        {
            description.text = "Skill already unlocked!";
            return;
        }

        skillPoints--;
        playerSkills.UnlockSkill(skill);
        confirmationBox.SetActive(false);
        Debug.Log($"{skill.skillName} unlocked!");

        // Use dictionary to retrieve RectTransform for the unlocked skill and unlock nearby nodes
        if (skillNodeMap.TryGetValue(skill, out RectTransform unlockedNode))
        {
            UnlockNearbySkills(unlockedNode);
        }
        else
        {
            Debug.LogError("No UI node found for this skill.");
        }
    }

    void UnlockNearbySkills(RectTransform unlockedNode)
    {
        Vector2 unlockedNodePos = unlockedNode.localPosition;
        Debug.Log($"Unlocked Node Local Position: {unlockedNodePos}");

        foreach (var node in skillNodes)
        {
            Skill nodeSkill = node.GetComponent<SkillNodeController>().skill;
            if (nodeSkill == null || playerSkills.unlockedSkills.Contains(nodeSkill))
                continue;

            // Calculate distance in local space
            Vector2 nodePos = node.localPosition;
            float distance = Vector2.Distance(unlockedNodePos, nodePos);

            Debug.Log($"Node: {nodeSkill.skillName} Local Position: {nodePos}, Distance: {distance}");

            // Unlock if within specified radius
            if (distance <= unlockRadius)
            {
                node.GetComponentInChildren<Button>().interactable = true;
                Debug.Log($"Node {nodeSkill.skillName} unlocked within radius.");
            }
        }
    }




    public void AddSkillPoint()
    {
        skillPoints++;
    }

    public void ToggleSkillTree()
    {
        Cursor.visible = !Cursor.visible;
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void OnNodePressed()
    {
        confirmationBox.SetActive(true);
        description.text = "Unlock " + currentSkill.skillName + "?";
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            UnlockSkill(currentSkill);
        });
    }

    public void SetPlayerSkills(PlayerSkills playerSkills)
    {
        this.playerSkills = playerSkills;
        Debug.Log("PlayerSkills set in SkillTreeManager.");
    }

    public void SetCurrentSkill(Skill skill)
    {
        currentSkill = skill;
    }
}
