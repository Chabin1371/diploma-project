using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseInteract : MonoBehaviour
{
    public event System.Action OnPlaced;

    [Header("Tilemap 设置")]
    private Tilemap targetTilemap;     // 要被高亮的Tilemap
    private Tilemap highlightTilemap;  // 用于显示高亮的Tilemap

    private Tilemap walkableTilemap;
    private Tilemap obstacleTilemap;

    public Tile highlightTile;

    [HideInInspector] public Tower current_Tower;

    private Vector3Int lastCellPos;

    private Tile primitive_tile;
    public bool is_selected_any_grid = false;

    public Tile pr_tile => primitive_tile;

    private void Start()
    {
        highlightTilemap = GameManager_in_game.Instance.highlightTilemap;

        primitive_tile = highlightTile;

        walkableTilemap = GameManager_in_game.Instance.walkableTilemap;
        obstacleTilemap = GameManager_in_game.Instance.obstacleTilemap;
        targetTilemap = GameManager_in_game.Instance.targetTilemap;
    }
    void Update()
    {
        // 获取鼠标位置对应的世界坐标
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // 确保z坐标为0

        if (GameManager_in_game.Instance.is_GameStarted)
        {
            FlowCell flowCell = GameManager_in_game.Instance.flowField_System.Get_flowCell(mouseWorldPos);

            if (flowCell == null)
                return;

            if (!flowCell.Is_working && flowCell.Is_Placed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (GameManager_in_game.Instance.current_energy >= flowCell.Tower.Energy)
                    {
                        flowCell.Set_is_working_(is_: true);
                        GameManager_in_game.Instance.current_energy -= flowCell.Tower.Energy;
                        GameManager_in_game.Instance.refreash_ui();
                    }
                    else
                    {
                        Debug.Log("能源不足，无法启动");
                    }
                }
            }

            if (flowCell.Is_working && flowCell.Is_Placed)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Debug.Log("停止工作");
                    flowCell.Set_is_working_(is_: false);
                    GameManager_in_game.Instance.current_energy += flowCell.Tower.Energy;
                    GameManager_in_game.Instance.refreash_ui();
                }
            }

            return;
        }

        if (!is_selected_any_grid)
            highlightTile = primitive_tile;

        // 将世界坐标转换为Tilemap的格子坐标
        Vector3Int cellPosition = targetTilemap.WorldToCell(mouseWorldPos);

        if (Input.GetMouseButtonDown(0))
        {
            if (!is_selected_any_grid)
                return;

            FlowCell flowCell = GameManager_in_game.Instance.flowField_System.Get_flowCell(mouseWorldPos);

            if (flowCell != null && !flowCell.Is_Placed && flowCell.IsWalkable)
            {
                if (GameManager_in_game.Instance.Max_placed_cell != GameManager_in_game.Instance.Current_placed_cell_)
                {
                    GameManager_in_game.Instance.Set_flowcell_tower(flowCell, current_Tower);
                    walkableTilemap.SetTile(cellPosition, highlightTile);

                    GameObject tower_control = GameManager_in_game.Instance.Get_towerControl(current_Tower);
                    flowCell.Tower_Controler = tower_control.GetComponent<Tower_Controller>();
                    if (tower_control != null)
                    {
                        tower_control.transform.position = flowCell.WorldPos;
                        tower_control.SetActive(true);
                        tower_control.GetComponent<Tower_Controller>().Start_();
                    }

                    OnPlaced?.Invoke();
                }
                else
                {
                    Debug.Log("可放置空位已满");
                }
            }
        }

        if (walkableTilemap.HasTile(cellPosition) && !targetTilemap.HasTile(cellPosition))
        {
            highlightTilemap.SetTile(cellPosition, highlightTile);

            if (cellPosition != lastCellPos)
                highlightTilemap.SetTile(lastCellPos, null);

            lastCellPos = cellPosition;
        }
        else
        {
            highlightTilemap.SetTile(lastCellPos, null);
        }
    }

    public void stop_highlight_tower()
    {
        highlightTilemap.SetTile(lastCellPos, null);
    }
}
