using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;

public class HotbarUIManager : NetworkBehaviour
{
    [SerializeField] PlayerSkills playerSkills;
    [SerializeField] List<Image> hotbarIcons; // Skill icons
    [SerializeField] Sprite emptyIcon;
    [SerializeField] List<GameObject> hotbarSlots;
    [SerializeField] List<Image> cooldownOverlays; // Cooldown overlay images for each hotbar slot

    void Update()
    {
        if (IsLocalPlayer)
        {
            HandleHotbarInput();
        }

        UpdateCooldownOverlays();
    }
    public void AssignHotbarIcon()
    {
        for (int i = 0; i < hotbarIcons.Count; i++)
        {
            if (i < playerSkills.hotbarSkills.Count)
            {
                hotbarIcons[i].sprite = playerSkills.hotbarSkills[i].skillIcon;

                cooldownOverlays[i].gameObject.SetActive(true); // Enable cooldown overlay
                cooldownOverlays[i].fillAmount = 0; // Reset fill amount
                cooldownOverlays[i].sprite = playerSkills.hotbarSkills[i].skillIcon; // Set overlay icon
            }
            else
            {
                hotbarIcons[i].sprite = emptyIcon;
                cooldownOverlays[i].sprite = emptyIcon; // Set overlay icon to empty
                cooldownOverlays[i].gameObject.SetActive(false); // Disable cooldown overlay

            }
        }
    }


    private void HandleHotbarInput()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            // Handle keys 1-9
            if (i < 9 && Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Activate only the selected slot and deactivate the others
                for (int j = 0; j < hotbarSlots.Count; j++)
                {
                    hotbarSlots[j].SetActive(j == i);
                }
                break;
            }
            // Special case for key 0 (slot 10)
            if (i == 9 && Input.GetKeyDown(KeyCode.Alpha0))
            {
                for (int j = 0; j < hotbarSlots.Count; j++)
                {
                    hotbarSlots[j].SetActive(j == i);
                }
                break;
            }
        }
    }

    private void UpdateCooldownOverlays()
    {
        for (int i = 0; i < playerSkills.hotbarSkills.Count; i++)
        {
            var skill = playerSkills.hotbarSkills[i];
            if (skill.OnCooldown)
            {
                // Calculate cooldown progress (remaining time / total cooldown)
                float cooldownProgress = skill.cooldownTimer / skill.Cooldown;

                // Update the overlay's fill amount
                cooldownOverlays[i].fillAmount = cooldownProgress;
            }
        }
    }
}
