using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GameManager_in_game : GameManager_Father
{
    public static GameManager_in_game Instance;
    public event Action OnGameReset;

    [Header("游戏胜利战利品")]
    public int EXP_get;
    public int Coin_get;

    [Header("初始系统生成")]
    public GameObject flowField_prefab;
    public GameObject MouseInteractSystem_prefab;

    [Header("选择防御塔UI设置")]
    [SerializeField] private GameObject UI_PREFAB;
    public TextMeshProUGUI Energy_UI_PREFAB;
    private GameObject UI;
    [Header("防御塔操控器")]
    public GameObject tower_control_container;
    private List<Tower_Controller> tower_control_list = new List<Tower_Controller>();
    public List<Tower_Controller> Tower_control_list => tower_control_list;

    [SerializeField] private int targetHp;
    public int TargetHp => targetHp;

    [Header("游戏设置")]
    public bool is_GameStarted = false;

    public float current_energy = 100;
    private float start_energy;
    public int Max_placed_cell;
    public int Current_placed_cell_;

    private FlowField flowField_system;
    public FlowField flowField_System => flowField_system;

    private MouseInteract Mouse;
    public MouseInteract mouse => Mouse;

    [Header("tilemap设置")]
    [SerializeField] private Tilemap walkable;
    public Tilemap walkableTilemap => walkable;
    [SerializeField] private Tilemap obstacle;
    public Tilemap obstacleTilemap => walkable;
    [SerializeField] private Tilemap target;
    public Tilemap targetTilemap => target;
    [SerializeField] private Tilemap highlight;
    public Tilemap highlightTilemap => highlight;

    [SerializeField] private Tower_keeper tower_Keeper;
    public Tower_keeper towers_ => tower_Keeper;
    [SerializeField] private Dictionary<int, Tower> towers = new Dictionary<int, Tower>();
    public Dictionary<int, Tower> Towers => towers;

    private int Destroyed_enemy_count = 0;
    public void enemyDead()
    {
        Destroyed_enemy_count++;
    }
    public void game_Start()
    {
        mouse.stop_highlight_tower();
        flowField_System.start_flow_field();
        EnemyManager.Instance.start_spawn();
        is_GameStarted = true;
        UI.SetActive(false);
    }

    public void game_End(bool victory)
    {
        if (victory)
        {
            Time.timeScale = 0f;
            Debug.Log("任务成功");
        }
        else
        {
            Time.timeScale = 0f;
            Debug.Log("任务失败");
        }
    }
    
    public void Set_flowcell_tower(FlowCell flowCell, Tower tower)
    {
        foreach (Skill skill in tower.SkillTree)
        {
            flowCell.Skills.Add(skill.ID, skill);
        }
        flowCell.Set_tower(tower);
        flowCell.Is_Placed = true;
        Current_placed_cell_++;
    }

    public GameObject Get_towerControl(Tower tower)
    {
        foreach (var tower_ in tower_control_list)
        {
            if (tower_.ID == tower.id && !tower_.isActiveAndEnabled)
                return tower_.gameObject;
        }

        Debug.Log("没有找到合适的控制器");
        return null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        start_energy = current_energy;

        foreach (Tower tower in tower_Keeper.TowerList)
        {
            towers.Add(tower.id, tower);
        }

        UI = Instantiate(UI_PREFAB);

        set_tower_controler_pool();

        flowField_system = Instantiate(flowField_prefab).GetComponent<FlowField>();

        Mouse = Instantiate(MouseInteractSystem_prefab).GetComponent<MouseInteract>();
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R) && !is_GameStarted)
        {
            game_Start();
        }
        else if (Input.GetKeyUp(KeyCode.S) && !is_GameStarted)
        {
            game_reSet();  
        }
    }

    public void refreash_ui()
    {
        Energy_UI_PREFAB.text = current_energy.ToString();
    }
    public void game_reSet()
    {
        foreach (FlowCell flowCell in flowField_System.cellGrid)
        {
            flowCell.tower_dead();
        }
        Current_placed_cell_ = 0;
        current_energy = start_energy;
        OnGameReset?.Invoke();
    }

    public void targetHp_reduce(int Hp)
    {
        targetHp -= Hp;
    }
    public void set_tower_controler_pool()
    {
        foreach (var tower in tower_Keeper.TowerList)
        {
            for (int i = 0; i < Max_placed_cell; i++)
            {
                if (tower.TowerPrefab == null)
                {
                    Debug.Log("未设置防御塔！");
                    continue;
                }
                GameObject towerobj = Instantiate(tower.TowerPrefab, tower_control_container.transform);
                tower_control_list.Add(towerobj.GetComponent<Tower_Controller>());
                towerobj.GetComponent<Tower_Controller>().ID = tower.id;
                towerobj.SetActive(false);
            }
        }
    }
}
