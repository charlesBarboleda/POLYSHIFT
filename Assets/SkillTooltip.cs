using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTooltip : MonoBehaviour
{
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;
    public Image skillIcon;

    private RectTransform tooltipTransform;

    private void Awake()
    {
        tooltipTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);  // Start hidden
    }

    public void ShowTooltip(Skill skill, Vector3 position)
    {
        skillNameText.text = skill.skillName;
        skillDescriptionText.text = skill.skillDescription;
        skillIcon.sprite = skill.skillIcon;

        tooltipTransform.position = position;
        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
