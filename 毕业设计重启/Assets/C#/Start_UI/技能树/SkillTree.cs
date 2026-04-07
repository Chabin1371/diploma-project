using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillTree : UIbase
{
    public int tower_id;
    public base_elements base_skill;
    public List<sub_elements> skills;
    public TextMeshProUGUI EXP_value;
    public TextMeshProUGUI Coin_value;
    public DesConnecter DesConnecter;
    public Tower current_Tower;

    private void Start()
    {
        base_skill.Initialized(DesConnecter);

        base_skill.Set_icon(GameManager.Instance.get_tower_from_dict(tower_id).Icon);
        base_skill.tower_id = tower_id;

        if (skills.Count != 0)
        {
            foreach (sub_elements skill in skills)
            {
                skill.Set_icon(GameManager.Instance.get_skill_from_dict(skill.skill_ID).Sprite);
                skill.tower_id = tower_id;
                skill.Initialized(DesConnecter);
            }
        }
    }
    public override void Refresh()
    {
        base.Refresh();

        EXP_value.text = GameManager.Instance.exp.ToString();
        Coin_value.text = GameManager.Instance.coin.ToString();
    }

    public void Reset_()
    {
        foreach (sub_elements skill in skills)
        {
            skill.Reset_this_Skill();
        }
    }

    public virtual void base_skill_Level_up()
    {
        current_Tower = GameManager.Instance.get_tower_from_dict(tower_id);

        current_Tower.LevelUp();
    }
}
