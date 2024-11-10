using UnityEngine;

public class TooltipController : MonoBehaviour
{
    [SerializeField] Skill skill;
    [SerializeField] SkillTooltip tooltip;

    public void OnHover()
    {
        tooltip.ShowTooltip(skill, transform.position + new Vector3(0, 10f, 0));
    }

    public void OnExit()
    {
        tooltip.HideTooltip();
    }
}
