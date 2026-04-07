using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FlowField : MonoBehaviour
{
    private FlowCell[,] CellGrid;
    public FlowCell[,] cellGrid => CellGrid;
    private int minGridX;
    private int minGridY;
    [Header("Tilemap")]
    private Tilemap walkableTilemap;
    private Tilemap obstacleTilemap;
    private Tilemap targetTilemap;

    // 网格边界
    private Vector3Int gridMin;
    private Vector3Int gridMax;

    private void Start()
    {
        GetTilemap();
        InitializeCellGrid();
        // 假设你已经生成了格子，并且知道格子坐标的范围
        // 例如格子坐标从 -10 到 10，那么 minGridX = -10，minGridY = -10
        // 或者从 CellGrid[0,0].WorldPos 反推：
        Vector3 firstCellWorldPos = CellGrid[0, 0].WorldPos;
        minGridX = Mathf.FloorToInt(firstCellWorldPos.x - 0.5f);
        minGridY = Mathf.FloorToInt(firstCellWorldPos.y - 0.5f);
    }
    public void start_flow_field()
    {
        FlowCell target_cell = GetTargetPos();
        if (target_cell == null)
            return;
        BFS_for_calculate_heat_map(target_cell);
        Vector_calculation(target_cell);
    }

    #region 向量场计算
    private void Vector_calculation(FlowCell targetCell)
    {
        foreach (var cell in CellGrid)
        {
            // 只处理可行走的格子
            if (cell == null || !cell.IsWalkable)
                continue;

            // 目标格子本身：停留（无方向）
            if (cell.GridPos == targetCell.GridPos)
            {
                cell.Dir = Vector2.zero;
                continue;
            }

            // 找出最佳邻居
            FlowCell best = Find_the_best_neighbor(cell, targetCell);
            if (best != null)
            {
                // 计算方向向量（从当前格子指向最佳邻居）
                Vector2 dir = (best.GridPos_Vector2 - cell.GridPos_Vector2).normalized;
                cell.Dir = dir;
            }
            else
            {
                // 无可通行邻居（比如被障碍包围），保持静止
                cell.Dir = Vector2.zero;
            }
        }

        Debug.Log("流场向量计算完成！");
    }
    private FlowCell Find_the_best_neighbor(FlowCell flowCell, FlowCell targetCell)
    {
        // 1. 获取所有可通行的邻居（包含目标本身，不包含障碍）
        List<FlowCell> neighbors = GetWalkableNeighbors(flowCell);

        // 2. 优先检查：如果邻居中包含目标，直接指向目标
        foreach (var neighbor in neighbors)
        {
            if (neighbor.GridPos == targetCell.GridPos)
                return targetCell;
        }

        // 3. 寻找最小热力值，并收集所有达到该最小值的候选邻居
        float minHeat = float.MaxValue;
        List<FlowCell> candidates = new List<FlowCell>();

        foreach (var neighbor in neighbors)
        {
            if (neighbor.heatMapValue < minHeat - 0.0001f)
            {
                minHeat = neighbor.heatMapValue;
                candidates.Clear();
                candidates.Add(neighbor);
            }
            else if (Mathf.Abs(neighbor.heatMapValue - minHeat) < 0.0001f)
            {
                candidates.Add(neighbor);
            }
        }

        // 4. 打破平局：在多个最短路径中选择“与目标方向最一致”的那个
        //    （点积越大，方向越直）
        FlowCell bestNeighbor = null;
        float bestDot = -float.MaxValue;

        Vector2 currentToTarget = (targetCell.GridPos_Vector2 - flowCell.GridPos_Vector2).normalized;

        foreach (var candidate in candidates)
        {
            Vector2 candidateDir = (candidate.GridPos_Vector2 - flowCell.GridPos_Vector2).normalized;
            float dot = Vector2.Dot(currentToTarget, candidateDir);

            if (dot > bestDot)
            {
                bestDot = dot;
                bestNeighbor = candidate;
            }
        }

        return bestNeighbor;
    }
    private List<FlowCell> GetWalkableNeighbors(FlowCell flowCell)
    {
        List<FlowCell> neighbors = new List<FlowCell>();

        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),    // 上
        new Vector2Int(0, -1),   // 下
        new Vector2Int(-1, 0),   // 左
        new Vector2Int(1, 0),    // 右
        new Vector2Int(-1, 1),   // 左上
        new Vector2Int(1, 1),    // 右上
        new Vector2Int(-1, -1),  // 左下
        new Vector2Int(1, -1)    // 右下
        };

        int width = CellGrid.GetLength(0);
        int height = CellGrid.GetLength(1);

        foreach (var dir in directions)
        {
            int x = flowCell.GridPos.x + dir.x;
            int y = flowCell.GridPos.y + dir.y;

            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            FlowCell neighbor = CellGrid[x, y];
            if (neighbor == null || !neighbor.IsWalkable)
                continue;

            // ---------- 对角线阻塞检测（关键优化）----------
            // 如果是对角方向，检查相邻的两个正交格子是否都不可通行
            if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
            {
                FlowCell horizontal = CellGrid[flowCell.GridPos.x + dir.x, flowCell.GridPos.y];
                FlowCell vertical = CellGrid[flowCell.GridPos.x, flowCell.GridPos.y + dir.y];

                bool horizontalBlocked = (horizontal == null || !horizontal.IsWalkable);
                bool verticalBlocked = (vertical == null || !vertical.IsWalkable);

                if (horizontalBlocked && verticalBlocked)
                    continue; // 不允许从“墙角”滑过
            }
            // ---------------------------------------------

            neighbors.Add(neighbor);
        }

        return neighbors;
    }
    #endregion

    #region BFS路径计算
    private void BFS_for_calculate_heat_map(FlowCell target_cell)
    {
        target_cell.heatMapValue = 0;

        Queue<FlowCell> queue = new Queue<FlowCell>();
        List<FlowCell> heatMap = new List<FlowCell>();

        queue.Enqueue(target_cell);
        while (queue.Count > 0)
        {
            FlowCell current = queue.Dequeue();

            List<FlowCell> neighbors = GetNeighbors(current);

            foreach (FlowCell neighbor in neighbors)
            {
                if (Vector3.Distance(current.WorldPos, neighbor.WorldPos) > 1)
                    neighbor.heatMapValue = current.heatMapValue + 1.4f;
                else
                    neighbor.heatMapValue = current.heatMapValue + 1;

                // 添加到队列继续扩散
                queue.Enqueue(neighbor);
            }
        }
    }
    private List<FlowCell> GetNeighbors(FlowCell flowCell)
    {
        List<FlowCell> neighbors = new List<FlowCell>();

        // 定义所有四个方向的偏移量
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),    // 上
            new Vector2Int(0, -1),   // 下
            new Vector2Int(-1, 0),   // 左
            new Vector2Int(1, 0),    // 右
            new Vector2Int(-1, 1),   // 左上
            new Vector2Int(1, 1),    // 右上
            new Vector2Int(-1, -1),  // 左下
            new Vector2Int(1, -1)    // 右下
        };

        foreach (Vector2Int dir in directions)
        {
            int index_x = flowCell.GridPos.x + dir.x;
            int index_y = flowCell.GridPos.y + dir.y;

            if (index_x >= 0 && index_x < CellGrid.GetLength(0) &&
                index_y >= 0 && index_y < CellGrid.GetLength(1))
            {
                FlowCell neighbor = CellGrid[index_x, index_y];
                if (neighbor.IsWalkable && !neighbor.IsTarget && neighbor.heatMapValue == 0)
                    neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    private FlowCell GetTargetPos()
    {
        for (int x = 0; x < CellGrid.GetLength(0); x++)
        {
            for (int y = 0; y < CellGrid.GetLength(1); y++)
            {
                if (CellGrid[x, y].IsTarget)
                    return CellGrid[x, y];
            }
        }

        Debug.Log("没有设置目标点");
        return null;
    }
    #endregion

    #region 初始化
    private void GetTilemap()
    {
        walkableTilemap = GameManager_in_game.Instance.walkableTilemap;
        obstacleTilemap = GameManager_in_game.Instance.obstacleTilemap;
        targetTilemap = GameManager_in_game.Instance.targetTilemap;
    }
    private void InitializeCellGrid()
    {
        SearchThePathRange();

        int width = gridMax.x - gridMin.x + 1;
        int height = gridMax.y - gridMin.y + 1;

        CellGrid = new FlowCell[width, height];

        for (int x = gridMin.x; x <= gridMax.x; x++)
        {
            for (int y = gridMin.y; y <= gridMax.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                Vector3 worldPos = walkableTilemap.GetCellCenterWorld(tilePos);

                // 将Tilemap坐标转换为数组索引
                int arrayX = x - gridMin.x;
                int arrayY = y - gridMin.y;

                bool hasGroundTile = walkableTilemap.HasTile(tilePos);
                bool isTarget = targetTilemap.HasTile(tilePos);

                CellGrid[arrayX, arrayY] = new FlowCell(new Vector3Int(arrayX, arrayY), worldPos, hasGroundTile, isTarget);
            }
        }
    }
    private void SearchThePathRange()
    {
        UpdateBoundsFromTilemap(walkableTilemap);
        UpdateBoundsFromTilemap(obstacleTilemap);
        UpdateBoundsFromTilemap(targetTilemap);
    }
    private void UpdateBoundsFromTilemap(Tilemap tilemap)
    {
        if (tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        if (bounds.size.x > 0 && bounds.size.y > 0)
        {
            gridMin.x = Mathf.Min(gridMin.x, bounds.xMin);
            gridMin.y = Mathf.Min(gridMin.y, bounds.yMin);
            gridMax.x = Mathf.Max(gridMax.x, bounds.xMax - 1);
            gridMax.y = Mathf.Max(gridMax.y, bounds.yMax - 1);
        }
    }
    #endregion

    #region 外部接口
    public void attack_Tower(FlowCell flowCell, int ap)
    {
        if (flowCell.IsTarget)
        {
            GameManager_in_game.Instance.targetHp_reduce(ap);
            if (GameManager_in_game.Instance.TargetHp <= 0)
                GameManager_in_game.Instance.game_End(false);
        }
        else if(flowCell.Is_Placed)
        {
            flowCell.Tower_Controler.cell_Hp -= ap;
        }
    }

    public List<FlowCell> get_all_walkable_flowCell()
    {
        List<FlowCell> flowCells = new List<FlowCell>();

        for (int x = 0; x < CellGrid.GetLength(0); x++)
        {
            for (int y = 0; y < CellGrid.GetLength(1); y++)
            {
                if (CellGrid[x, y].IsWalkable)
                {
                    flowCells.Add(CellGrid[x, y]);
                }
            }
        }
        return flowCells;
    }

    public FlowCell Get_flowCell(Vector3 Pos)
    {
        // 计算格子坐标
        int gridX = Mathf.FloorToInt(Pos.x);
        int gridY = Mathf.FloorToInt(Pos.y);

        // 转换为数组索引
        int arrayX = gridX - minGridX;
        int arrayY = gridY - minGridY;

/*        Debug.Log(Pos.x + "," + Pos.y);
        Debug.Log(gridX + "," + gridY);
        Debug.Log(arrayX + "," + arrayY);*/

        // 检查索引是否有效
        if (arrayX < 0 || arrayX >= CellGrid.GetLength(0) || arrayY < 0 || arrayY >= CellGrid.GetLength(1))
            return null;

        return CellGrid[arrayX, arrayY];
    }
    public FlowCell index_get_flowcell(int x, int y)
    {
        Debug.Log(x + "," + y + ":" + CellGrid[x, y]);
        return CellGrid[x, y];
    }

    #endregion
    private void OnDrawGizmos()
    {
        if (CellGrid == null) return;

        for (int x = 0; x < CellGrid.GetLength(0); x++)
        {
            for (int y = 0; y < CellGrid.GetLength(1); y++)
            {
                FlowCell cell = CellGrid[x, y];
                if (cell != null && cell.IsWalkable)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(cell.WorldPos, Vector3.one * 0.9f);
                    // 绘制方向箭头
                    if (cell.Dir != Vector2.zero)
                    {
                        Vector3 start = cell.WorldPos;
                        Vector3 end = start + (Vector3)cell.Dir * 0.4f;

                        Gizmos.DrawLine(start, end);

                        // 绘制箭头头部
                        Vector3 right = Quaternion.Euler(0, 0, 30) * -cell.Dir * 0.1f;
                        Vector3 left = Quaternion.Euler(0, 0, -30) * -cell.Dir * 0.1f;
                        Gizmos.DrawLine(end, end + right);
                        Gizmos.DrawLine(end, end + left);
                    }

                    // 文字样式设置
                    GUIStyle textStyle = new GUIStyle();
                    textStyle.normal.textColor = Color.white;
                    textStyle.fontSize = 15;
                    textStyle.alignment = TextAnchor.MiddleCenter;
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(cell.WorldPos, $"{cell.heatMapValue}\n✗", textStyle);
                    UnityEditor.Handles.Label(cell.WorldPos + new Vector3(0, (float)-0.2, 0), $"({cell.GridPos.x} , {cell.GridPos.y})\n✗", textStyle);
                    UnityEditor.Handles.Label(cell.WorldPos + new Vector3(0, (float)-0.4, 0), $"({cell.WorldPos.x} , {cell.WorldPos.y})\n✗", textStyle);
#endif

                }
            }
        }
    }
}

