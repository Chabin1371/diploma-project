using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tower_Controller : MonoBehaviour
{
    [HideInInspector] public int ID;
    [HideInInspector] public bool is_working = false;

    public float attackRange = 5f;          // 攻击半径（世界单位）
    public float attackCooldown = 1f;       // 攻击间隔
    public int cell_Ap;
    public int cell_Hp;

    private Dictionary<int, Skill> skills = new Dictionary<int, Skill>();
    public Dictionary<int, Skill> Skills => skills; 

    public GameObject is_working_Ui;

    public virtual void Start_()
    {
        FlowCell flowCell = GameManager_in_game.Instance.flowField_System.Get_flowCell(transform.position);
        flowCell.Tower_Controler = this;
        is_working = false;

        cell_Ap = (int)flowCell.Tower.BaseAP;
        cell_Hp = (int)flowCell.Tower.BaseHp;

        if (flowCell.Skills.Count != 0)
        {
            foreach (Skill skill in flowCell.Skills.Values)
            {
                if (!skills.ContainsValue(skill))
                    skills.Add(skill.ID, skill);

                deal_with_skill_effect(skill);
            }
        }
    }

    public void deal_with_skill_effect(Skill skill)
    {
        foreach (var effect in skill.SkillEffects)
        {
            effect.Apply(this);
        }
    }

    public virtual void Dead()
    {
        Vector3Int cellPos = GameManager_in_game.Instance.walkableTilemap.WorldToCell(transform.position);
        GameManager_in_game.Instance.walkableTilemap.SetTile(cellPos, null);
        gameObject.SetActive(false);
    }
}
