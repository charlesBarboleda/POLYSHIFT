using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [SerializeField] Skill skill;
    [SerializeField] SkillTreeManager skillTreeManager;
    Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            skillTreeManager.SetCurrentSkill(skill);
            skillTreeManager.OnNodePressed();
        });
    }


}
