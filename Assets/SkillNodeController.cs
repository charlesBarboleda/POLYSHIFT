using UnityEngine;

public class SkillNodeController : MonoBehaviour
{
    public Skill skill;
    [SerializeField] SkillTooltip tooltip;

    public void OnHover()
    {
        tooltip.ShowTooltip(skill, transform.position + new Vector3(0, 40f, 0));
    }

    public void OnExit()
    {
        tooltip.HideTooltip();
    }
}
