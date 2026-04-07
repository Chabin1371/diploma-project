using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowCell
{
    private Vector3Int gridPos;
    private Vector3 worldPos;
    public float heatMapValue = 0;
    private Vector2 dir;
    private bool isWalkable = false;

    private bool isTarget;

    private bool is_placed = false;

    private bool is_working = false;

    private Tower tower;
    private Tower_Controller tower_Controler;

    private Dictionary<int, Skill> skills = new Dictionary<int, Skill>();

    public List<Enemy> enemiesInCell = new List<Enemy>();

    public FlowCell(Vector3Int tilePos, Vector3 worldPos, bool isWalkable, bool isTarget)
    {
        this.gridPos = tilePos;
        this.worldPos = worldPos;
        this.isWalkable = isWalkable;
        this.dir = Vector2.zero; // 初始方向设为零向量
        this.isTarget = isTarget;
    }

    public void Set_tower(Tower tower)
    {
        this.tower = tower; 

        foreach (Skill skill in tower.SkillTree)
        {
            if (!skills.ContainsValue(skill))
                skills.Add(skill.ID, skill);
        }
    }

    public void tower_dead()
    {
        Is_Placed = false;
        skills.Clear();
        tower = null;
        if (tower_Controler != null)
            tower_Controler.Dead();
        tower_Controler = null;
    }
    public void Set_is_working_(bool is_ = false)
    {
        is_working = is_;
        tower_Controler.is_working_Ui.SetActive(!is_working);
        tower_Controler.is_working = is_;
    }

    public Vector3Int GridPos { get => gridPos; }
    public Vector2 GridPos_Vector2 { get => new Vector2(gridPos.x, gridPos.y); }
    public Vector3 WorldPos { get => worldPos; }
    public Vector2 Dir { get => dir; set => dir = value; }
    public bool IsWalkable { get => isWalkable; }
    public bool IsTarget { get => isTarget; }
    public bool Is_Placed { get => is_placed; set => is_placed = value; }
    public bool Is_working { get => is_working; }
    public Tower_Controller Tower_Controler { get => tower_Controler; set => tower_Controler = value; }
    public Dictionary<int, Skill> Skills => skills;
    public Tower Tower { get => this.tower; }
}
