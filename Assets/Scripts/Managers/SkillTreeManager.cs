using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : NetworkBehaviour
{
    public int skillPoints = 0;
    [SerializeField] GameObject confirmationBox;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;
    public Skill currentSkill;
    [SerializeField] List<RectTransform> skillNodes;
    [SerializeField] List<Button> ultimateSkills;
    [SerializeField] List<Skill> unlockedSkills;
    [SerializeField] float unlockRadius = 200f; // Radius for unlocking nearby skills
    Dictionary<Skill, RectTransform> skillNodeMap = new Dictionary<Skill, RectTransform>();
    [SerializeField] PlayerNetworkLevel playerLevel;
    [SerializeField] GameObject linePrefab;
    [SerializeField] Transform lineParent;
    [SerializeField] Image newBeginningsButton;
    [SerializeField] Image newBeginningsContainer;
    List<GameObject> nodeLines = new List<GameObject>();
    PlayerSkills playerSkills;

    void Start()
    {
        confirmationBox.SetActive(false);
        noButton.onClick.AddListener(() => confirmationBox.SetActive(false));
        InitializeSkillNodeMap();
        playerLevel.Level.OnValueChanged += OnLevelUp;

    }

    void UnlockSkill(Skill skill)
    {
        if (playerSkills == null)
        {
            Debug.LogError("playerSkills is null in UnlockSkill.");
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

        // Disable all ultimate skills if an ultimate skill is unlocked
        if (skill is IUltimateSkill)
        {
            foreach (var ultimateSkill in ultimateSkills)
            {
                ultimateSkill.interactable = false;
            }
        }
        // Find the RectTransform of the unlocked node
        if (skillNodeMap.TryGetValue(skill, out RectTransform newNode))
        {
            // Find the closest already unlocked node
            RectTransform closestNode = FindClosestUnlockedNode(newNode);

            // Draw a line to the closest unlocked node
            if (closestNode != null)
            {
                DrawLineBetweenNodes(closestNode, newNode);
            }
            else
            {
                Debug.LogWarning("No closest unlocked node found to connect.");
            }

            // Unlock nearby nodes
            UnlockNearbySkills(newNode);
        }
        else
        {
            Debug.LogError("No UI node found for this skill.");
        }
    }

    RectTransform FindClosestUnlockedNode(RectTransform targetNode)
    {
        float closestDistance = float.MaxValue;
        RectTransform closestNode = null;

        foreach (Skill unlockedSkill in playerSkills.unlockedSkills)
        {
            if (skillNodeMap.TryGetValue(unlockedSkill, out RectTransform unlockedNode))
            {
                // Skip the targetNode itself
                if (unlockedNode == targetNode)
                    continue;

                // Calculate the distance
                float distance = Vector2.Distance(targetNode.localPosition, unlockedNode.localPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNode = unlockedNode;
                }
            }
        }

        return closestNode;
    }

    void DrawLineBetweenNodes(RectTransform node1, RectTransform node2)
    {
        // Instantiate the line and parent it to the lineParent
        GameObject line = Instantiate(linePrefab, lineParent);

        nodeLines.Add(line);
        // Get the RectTransform of the line
        RectTransform lineRect = line.GetComponent<RectTransform>();

        // Get the local positions of the nodes
        Vector3 startLocalPos = node1.localPosition;
        Vector3 endLocalPos = node2.localPosition;

        // Calculate the midpoint to position the line
        Vector3 midpoint = (startLocalPos + endLocalPos) / 2f;
        lineRect.localPosition = midpoint;

        // Calculate the direction and angle for rotation
        Vector3 direction = endLocalPos - startLocalPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);

        // Set the width of the line to be half of the distance between nodes
        float distance = Vector3.Distance(startLocalPos, endLocalPos);
        lineRect.sizeDelta = new Vector2(distance * 3, 60f); // Half the distance as width, height fixed at 60

        // Debug logs for verification
        Debug.Log($"Branch created: Start {node1.name}, End {node2.name}, Width {distance / 2f}, Height 60, Angle {angle}");
    }


    void OnLevelUp(int prev, int current)
    {

        if (current % 20 == 0)
        {
            foreach (var ultimateSkill in ultimateSkills)
            {
                ultimateSkill.interactable = true;
            }
        }
    }

    void InitializeSkillNodeMap()
    {
        for (int i = 0; i < skillNodes.Count; i++)
        {
            Skill skill = skillNodes[i].GetComponent<SkillNodeController>().skill; // Assuming each node has a Skill component or reference
            Image image = skillNodes[i].GetComponent<Image>();
            Image button = skillNodes[i].GetComponentInChildren<Image>();

            if (button != null)
            {
                button.color = new Color(1, 1, 1, 0.0f);
            }

            if (image != null)
            {
                image.color = new Color(1, 1, 1, 0.0f);
            }



            if (skill != null)
            {
                skillNodeMap[skill] = skillNodes[i];
            }
        }
        newBeginningsButton.color = new Color(1, 1, 1, 1);
        newBeginningsContainer.color = new Color(1, 1, 1, 1);
    }

    void UnlockNearbySkills(RectTransform unlockedNode)
    {
        Vector2 unlockedNodePos = unlockedNode.localPosition;

        foreach (var node in skillNodes)
        {
            Skill nodeSkill = node.GetComponent<SkillNodeController>().skill;
            if (nodeSkill == null || playerSkills.unlockedSkills.Contains(nodeSkill))
                continue;

            // Calculate distance in local space
            Vector2 nodePos = node.localPosition;
            float distance = Vector2.Distance(unlockedNodePos, nodePos);


            // Unlock if within specified radius
            if (distance <= unlockRadius)
            {
                node.GetComponentInChildren<Button>().interactable = true;
                node.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                node.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1);
            }
        }
    }

    public void ToggleSkillTree()
    {
        Cursor.visible = !Cursor.visible;
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void OnNodePressed()
    {
        confirmationBox.SetActive(true);
        if (currentSkill.skillName == "Bond Of The Colossus" || currentSkill.skillName == "Devil Slam")
        {
            description.text = "Unlocking this skill will disable all other ultimate skills until the next level threshold. Continue?";
        }
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

    public void ResetSkillTree()
    {
        foreach (var node in skillNodes)
        {
            node.GetComponentInChildren<Button>().interactable = false;
            node.GetComponent<Image>().color = new Color(1, 1, 1, 0.0f);
            node.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.0f);
        }

        newBeginningsButton.GetComponent<Button>().interactable = true;
        newBeginningsButton.color = new Color(1, 1, 1, 1);
        newBeginningsContainer.color = new Color(1, 1, 1, 1);

        foreach (var line in nodeLines)
        {
            Destroy(line);
        }

        foreach (var ultimateSkill in ultimateSkills)
        {
            ultimateSkill.interactable = false;
        }


        skillPoints = 0;
        playerSkills.unlockedSkills.Clear();

    }
}
