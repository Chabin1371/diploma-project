using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class level_value
{
    public float value;
    public float growth_scale;
}

[CreateAssetMenu(fileName = "Tower", menuName = "Game/Tower")]
public class Tower : ScriptableObject
{
    [SerializeField] private int tower_ID;
    [SerializeField] private Sprite icon;
    [SerializeField] private string Name;

    [Header("经验值相关")]
    [SerializeField] private int level = 1;
    [SerializeField] private int maxLevel;
    [SerializeField] private int required_EXP_to_update;
    [SerializeField] private float required_EXP_growth_scale;

    [Header("局内游戏相关")]
    [SerializeField] private Tile tile;
    [SerializeField] private GameObject tower;
    [SerializeField] private float energy;
    [SerializeField] private float energy_growth_scale;
    [SerializeField] private float energy_produce;
    [SerializeField] private float energy_produce_growth_scale;

    [Header("属性值相关")]
    [SerializeField] private float base_Hp;
    [SerializeField] private float base_Hp_scale;
    [SerializeField] private float base_AP;
    [SerializeField] private float base_AP_scale;
    [SerializeField] private float attack_range;
    [SerializeField] private float attack_speed;

    [Header("技能树相关")]
    [SerializeField] private List<Skill> active_skill = new List<Skill>();

    public void LevelUp()
    {
        if (level > maxLevel)
        {
            Debug.Log("防御塔已达最大等级");
            return;
        }

        level++;
        base_Hp += base_Hp_scale;
        base_AP += base_AP_scale;
        energy += energy_growth_scale;
        energy_produce += energy_produce_growth_scale;
    }
    public Sprite Icon => icon;
    public string TowerName => Name;
    public Tile Tile => tile;
    public GameObject TowerPrefab => tower;
    public float Energy => energy;
    public float EnergyProduce => energy_produce;
    public float BaseHp => base_Hp;
    public float BaseAP => base_AP;
    public float AttackRange => attack_range;
    public List<Skill> SkillTree => active_skill;
    public int Level => level;
    public int id => tower_ID;
    public int required_exp => required_EXP_to_update;
    public float Base_Hp_scale => base_Hp_scale;
    public float Base_Ap_scale => base_AP_scale;
    public float Energy_growth_scale => energy_growth_scale;
    public float Energy_produce_growth_scale => energy_produce_growth_scale;
}
