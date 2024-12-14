using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTooltip : MonoBehaviour
{
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;
    public Image skillIcon;

    private RectTransform tooltipTransform;
    private Canvas parentCanvas;

    private void Start()
    {
        tooltipTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        gameObject.SetActive(false);  // Start hidden
    }

    public void ShowTooltip(Skill skill, Vector3 position)
    {
        skillNameText.text = skill.skillName;
        skillDescriptionText.text = skill.skillDescription;
        skillIcon.sprite = skill.skillIcon;

        // Convert the world position to canvas space (screen point)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(),
            position,
            parentCanvas.worldCamera,
            out Vector2 localPoint
        );

        // Set the position and adjust
        tooltipTransform.anchoredPosition = localPoint + new Vector2(0, -75);
        AdjustTooltipPosition();

        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    private void AdjustTooltipPosition()
    {
        // Get the parent canvas RectTransform
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        // Get the tooltip's RectTransform position relative to the canvas
        Vector2 anchoredPosition = tooltipTransform.anchoredPosition;

        // Get the size of the tooltip and the canvas
        Vector2 tooltipSize = tooltipTransform.sizeDelta;
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Adjust the tooltip's position to fit within canvas bounds
        // Left edge
        if (anchoredPosition.x < -canvasSize.x / 2 + tooltipSize.x / 2)
        {
            anchoredPosition.x = -canvasSize.x / 2 + tooltipSize.x / 2;
        }

        // Right edge
        if (anchoredPosition.x > canvasSize.x / 2 - tooltipSize.x / 2)
        {
            anchoredPosition.x = canvasSize.x / 2 - tooltipSize.x / 2;
        }

        // Bottom edge
        if (anchoredPosition.y < -canvasSize.y / 2 + tooltipSize.y / 2)
        {
            anchoredPosition.y = -canvasSize.y / 2 + tooltipSize.y / 2;
        }

        // Top edge
        if (anchoredPosition.y > canvasSize.y / 2 - tooltipSize.y / 2)
        {
            anchoredPosition.y = canvasSize.y / 2 - tooltipSize.y / 2;
        }

        // Apply the adjusted position
        tooltipTransform.anchoredPosition = anchoredPosition;
    }


}
