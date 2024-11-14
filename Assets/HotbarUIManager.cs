using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;

public class HotbarUIManager : NetworkBehaviour
{
    [SerializeField] PlayerSkills playerSkills;
    [SerializeField] List<Image> hotbarIcons;
    [SerializeField] Sprite emptyIcon;
    [SerializeField] List<GameObject> hotbarSlots;


    public void AssignHotbarIcon()
    {
        for (int i = 0; i < hotbarIcons.Count; i++)
        {
            if (i < playerSkills.hotbarSkills.Count)
            {
                hotbarIcons[i].sprite = playerSkills.hotbarSkills[i].skillIcon;
            }
            else
            {
                hotbarIcons[i].sprite = emptyIcon;
            }
        }
    }


    void Update()
    {
        if (IsLocalPlayer)
        {
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    // Activate only the selected slot and deactivate the others
                    for (int j = 0; j < hotbarSlots.Count; j++)
                    {
                        hotbarSlots[j].SetActive(j == i);
                    }
                    break;
                }
            }
        }
    }


}
