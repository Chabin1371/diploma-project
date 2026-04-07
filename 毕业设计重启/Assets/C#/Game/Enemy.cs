using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;


public class Enemy : Agent
{
    [HideInInspector] public Transform spawn;
    public bool is_dead { get; private set; }
    private enum BounceState
    {
        None,       // 正常移动
        Backward,   // 后退阶段
    }
    private BounceState bounceState = BounceState.None;

    [Header("Flow Field Settings")]
    public float gridSize = 1f;

    [Header("物体所在的格子")]
    private FlowCell currentCell;

    [Header("碰撞事件")]
    public float Collision_event_raylength;
    public float Collision_event_raystep;
    public float backwardStopThreshold = 0.1f;
    private Vector3 bounceDirection;      // 反弹方向（单位向量）
    private float bounceSpeed;            // 当前后退速度大小
    private const float bounceDeceleration = 1f;   // 后退减速度（可调）
    private const float stopThreshold = 0.01f;     // 停止速度阈值
    public float enemyRadius;
    public float push_force_scale;
    private Vector3 smoothedDir; // 类成员变量

    [Header("避障参数")]
    // 避障参数
    public float avoidanceRayLength;          // 射线最大检测距离
    public float avoidanceRayStep;          // 射线步长（格子大小的一部分）
    public float maxRepelForce;               // 最大斥力
    public float repelForceScale;              // 斥力系数（用于调节避障强度）

    [Header("路径点跟随参数")]
    public float pathStep;       // 每次步进的格子距离（世界单位）
    public int pathSteps;            // 步进次数
    public float targetReachDistance; // 到达目标点的距离阈值（可选）
    // 内部状态
    private Vector3 currentTarget;       // 当前路径目标点（世界坐标）
    private bool is_obstacled;

    [Header("子物体旋转")]
    [SerializeField] private GameObject icon;
    [SerializeField] private float rotationSpeed;

    private void Start()
    {
        currentCell = GameManager_in_game.Instance.flowField_System.Get_flowCell(transform.position);
        if (currentCell != null)
        {
            currentCell.enemiesInCell.Add(this);
        }
    }

    private void Update()
    {
        if (is_dead)
            return;

        InCellGrid();

/*        Debug.Log(currentCell.GridPos.x + "," + currentCell.GridPos.y);
        Debug.Log(currentCell.enemiesInCell);*/

        if (!Get_Situate_Grid(transform.position).IsWalkable)
        {
            Vector3 push_force = transform.position - Get_Situate_Grid(transform.position).WorldPos;
            // 加速度
            acceleration = (push_force * push_force_scale) / mass;

            // 更新速度
            velocity += acceleration * Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            // 更新位置
            transform.position += velocity * Time.deltaTime;

            return;
        }

        if (bounceState == BounceState.Backward)
        {
            // 减速
            bounceSpeed -= bounceDeceleration * Time.deltaTime;
            if (bounceSpeed < 0f) bounceSpeed = 0f;

            // 沿反弹方向移动
            transform.position += bounceDirection * bounceSpeed * Time.deltaTime;

            // 检查是否几乎停止
            if (bounceSpeed <= stopThreshold)
            {
                bounceState = BounceState.None;
                velocity = Vector3.zero;   // 清除速度，准备重新加速
            }
            return;
        }

        Collision_event(velocity);

        if (bounceState == BounceState.None)
        {
            UpdateTargetPoint();

            // 计算期望方向：从物体指向目标点
            Vector3 desiredDir = (currentTarget - transform.position).normalized;

            // 如果目标点就在脚下或距离极小，可认为已到达，保持当前速度或减速
            float distToTarget = Vector3.Distance(transform.position, currentTarget);
            if (distToTarget < 0.01f)
            {
                // 无期望方向，让物体缓慢停止
                velocity *= 0.95f;
                transform.position += velocity * Time.deltaTime;
                return;
            }

            // 期望速度
            Vector3 desiredVelocity = desiredDir * maxSpeed;

            // 转向力（PD控制）
            float kP = 0.8f;  // 比例系数
            float kD = 0.3f;  // 阻尼系数
            Vector3 steering = (desiredVelocity - velocity) * kP - velocity * kD;
            steering = Vector3.ClampMagnitude(steering, maxForce);

            Vector3 repulsion = get_avoidance_force(desiredDir);
            Vector3 separationForce = get_Separation_force();
            Vector3 totalForce = steering + repulsion + separationForce;

            Collision_event(velocity);
            // 加速度
            acceleration = totalForce / mass;

            // 更新速度
            velocity += acceleration * Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            // 更新位置
            transform.position += velocity * Time.deltaTime;
        }
    }
    private void LateUpdate()
    {
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            icon.transform.rotation = Quaternion.Slerp(icon.transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }
    public void Dead()
    {
        is_dead = true;
        GameManager_in_game.Instance.enemyDead();
        transform.position = spawn.transform.position;
        EnemyManager.Instance.enemyDead(this);
        FlowCell currentCell = GameManager_in_game.Instance.flowField_System.Get_flowCell(transform.position);
        currentCell.enemiesInCell.Remove(this);
    }
    public void InCellGrid()
    {
        FlowCell oldCell = currentCell;
        FlowCell newCell = GameManager_in_game.Instance.flowField_System.Get_flowCell(transform.position);
        if (oldCell != newCell)
        {
            oldCell?.enemiesInCell.Remove(this);
            newCell?.enemiesInCell.Add(this);
            currentCell = newCell;
        }
    }
    private void UpdateTargetPoint()
    {
        currentTarget = PredictFlowPathTarget();
    }

    #region  防御塔碰撞事件
    public void Collision_event(Vector3 desiredDir)
    {
        // 定义三条射线的方向：前、左前、右前（相对于期望方向）
        float angle = 15f; // 射线偏角（度）
        Quaternion leftRot = Quaternion.Euler(0, 0, angle);
        Quaternion rightRot = Quaternion.Euler(0, 0, -angle);

        Vector3[] rayDirs = new Vector3[]
        {
            desiredDir,                     // 正前方
            leftRot * desiredDir,            // 左前
            rightRot * desiredDir            // 右前
        };

        foreach (Vector3 rayDir in rayDirs)
        {
            // 从物体位置开始，沿射线方向步进检测
            float dist = 0f;
            while (dist < Collision_event_raylength)
            {
                dist += Collision_event_raystep;

                Vector3 checkPos = transform.position + rayDir * dist;

                FlowCell cell = Get_Situate_Grid(checkPos);

                if (cell.Is_Placed || cell.IsTarget)
                {
                    GameManager_in_game.Instance.flowField_System.attack_Tower(cell, ap);
                    // 设置反弹状态
                    bounceState = BounceState.Backward;
                    bounceDirection = -velocity.normalized;   // 当前速度的反方向
                    bounceSpeed = maxSpeed;
                    // 如果速度太小，设定一个最小初速度，确保能看到反弹效果
                    if (bounceSpeed < 0.2f)
                        bounceSpeed = 0.5f;
                    return;
                }
            }
        }
    }

    #endregion 

    #region 路径点跟随
    private Vector3 PredictFlowPathTarget()
    {
        Vector3 pos = transform.position;
        Vector3 dir;

        for (int i = 0; i < pathSteps; i++)
        {
            Vector2 dir2D = Get_Situate_Grid(transform.position).Dir;

            dir = new Vector3(dir2D.x, dir2D.y, 0).normalized;

            // 沿方向前进一个步长
            Vector3 nextPos = pos + dir * pathStep;

            // 可选：检查下一个位置是否可行走，防止走入障碍
            FlowCell nextCell = Get_Situate_Grid(nextPos);
            if (nextCell == null || !nextCell.IsWalkable)
                is_obstacled = true;

            pos = nextPos;
        }
        return pos;
    }
    #endregion

    #region 避障行为
    private Vector3 get_avoidance_force(Vector3 desiredDir)
    {
        // 如果没有期望方向，直接返回零
        if (desiredDir.sqrMagnitude < 0.01f)
            return Vector3.zero;

        // 定义三条射线的方向：前、左前、右前（相对于期望方向）
        float angle = 15f; // 射线偏角（度）
        Quaternion leftRot = Quaternion.Euler(0, 0, angle);
        Quaternion rightRot = Quaternion.Euler(0, 0, -angle);

        Vector3[] rayDirs = new Vector3[]
        {
            desiredDir,                     // 正前方
            leftRot * desiredDir,            // 左前
            rightRot * desiredDir            // 右前
        };

        // 四个基本逃离方向（上、下、左、右）
        Vector3[] escapeDirs = new Vector3[]
        {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right
        };

        // 大步长：用于从障碍点向周围检测，取格子大小（确保能到达相邻格子中心）
        float largeStep = avoidanceRayStep + 0.1f;

        Vector3 totalRepel = Vector3.zero;

        foreach (Vector3 rayDir in rayDirs)
        {
            // 从物体位置开始，沿射线方向步进检测
            float dist = 0f;
            while (dist < avoidanceRayLength)
            {
                dist += avoidanceRayStep;
                Vector3 checkPos = transform.position + rayDir * dist;

                // 检查该位置所在的格子是否可行走
                FlowCell cell = Get_Situate_Grid(checkPos);
                if (cell == null || !cell.IsWalkable)
                {
                    // 遇到障碍物
                    Vector3 obstaclePos = checkPos;

                    // 收集障碍点周围的可行走逃离方向
                    List<Vector3> validEscapeDirs = new List<Vector3>();
                    foreach (Vector3 escDir in escapeDirs)
                    {
                        Vector3 candidatePos = obstaclePos + escDir * largeStep;
                        FlowCell candidateCell = Get_Situate_Grid(candidatePos);
                        if (candidateCell != null && candidateCell.IsWalkable)
                        {
                            validEscapeDirs.Add(escDir); // escDir 已是归一化方向
                        }
                    }

                    // 如果有可行走方向，计算平均方向作为斥力方向
                    if (validEscapeDirs.Count > 0)
                    {
                        Vector3 avgDir = Vector3.zero;
                        foreach (Vector3 d in validEscapeDirs)
                            avgDir += d;
                        avgDir.Normalize();

                        // 计算斥力大小：与障碍物距离成反比
                        float distance = (obstaclePos - transform.position).magnitude;
                        float repelMagnitude = (avoidanceRayLength - distance) / avoidanceRayLength * maxRepelForce;
                        repelMagnitude = Mathf.Max(0, repelMagnitude);

                        totalRepel += avgDir * repelMagnitude * repelForceScale;

                        is_obstacled = true;

                    }
                    else if (validEscapeDirs.Count > 2)
                    {
                        Debug.Log("检测出现问题");
                        return Vector3.zero;
                    }
                    // 如果没有可行逃离方向，则忽略该射线（不产生斥力）

                    // 该射线检测到第一个障碍后即停止步进
                    break;
                }
            }
        }
        // 限制总斥力大小
        return Vector3.ClampMagnitude(totalRepel, maxRepelForce);
    }
    #endregion

    #region 分离行为
    private Vector3 get_Separation_force()
    {
        // 获取当前敌人组件
        Enemy currentEnemy = GetComponent<Enemy>();
        if (currentEnemy == null || GameManager_in_game.Instance == null || EnemyManager.Instance.activeEnemies == null)
            return Vector3.zero;

        Vector3 separation = Vector3.zero;
        Vector3 currentPos = transform.position;
        float currentRadius = currentEnemy.enemyRadius;  // 当前敌人的半径

        // 遍历所有活跃敌人
        foreach (Enemy other in EnemyManager.Instance.activeEnemies)
        {
            // 排除自身及已销毁对象
            if (other == currentEnemy || other == null) continue;

            float otherRadius = other.enemyRadius;       // 其他敌人的半径
            float threshold = currentRadius + otherRadius; // 期望保持的最小距离（半径之和）

            Vector3 diff = currentPos - other.transform.position;
            float dist = diff.magnitude;

            // 如果距离小于阈值，且不为零（避免除零）
            if (dist < threshold && dist > 0.001f)
            {
                // 强度：距离越近力越大，0距离时强度为1，阈值处强度为0
                float strength = (threshold - dist) / threshold;
                Vector3 force = diff.normalized * strength;
                separation += force;
            }
        }

        // 可根据需要乘以一个全局权重系数
        return separation;
    }
    #endregion

    #region 辅助方法
    private FlowCell Get_Situate_Grid(Vector3 Pos)
    {
        return EnemyManager.Instance._flowFieldSystem.Get_flowCell(Pos);
    }
    private Vector2 Get_Situate_Grid_Dir(FlowCell cell)
    {
        if (cell == null)
            return Vector2.zero;

        if (!cell.IsWalkable)
            return Vector2.zero;

        return cell.Dir;
    }
    #endregion

    #region 可视化
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 绘制期望方向（蓝色箭头）
        Vector3 desiredDir = (currentTarget - transform.position).normalized;
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, desiredDir * 1f);

        // 绘制速度向量（红色箭头）
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, velocity.normalized * 1f);

        // 绘制当前目标点（黄色球体）
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(currentTarget, 0.2f);
    }
    #endregion
}

#region 双线性插值（弃用）
/*/// <summary>
/// 获取当前所在格子内的局部坐标权重 (u, v)，范围 [0,1]
/// </summary>
private Vector2 Get_Weight()
{
    FlowCell cell = Get_Situate_Grid(transform.position);
    if (cell == null)
        return Vector2.zero; // 如果不在格子内，返回0

    // 格子左下角世界坐标（格子中心在 cell.WorldPos）
    Vector3 gridCorner = cell.WorldPos - new Vector3(gridSize * 0.5f, gridSize * 0.5f, 0);

    // 相对于格子左下角的局部坐标，并归一化到 [0,1]
    float u = (transform.position.x - gridCorner.x) / gridSize;
    float v = (transform.position.y - gridCorner.y) / gridSize;

    // 由于浮点误差，可能略小于0或大于1，钳位到 [0,1]
    u = Mathf.Clamp01(u);
    v = Mathf.Clamp01(v);

    return new Vector2(u, v);
}

/// <summary>
/// 获取当前位置的双线性插值方向向量
/// </summary>
private Vector2 Bilinear_interpolation_vector()
{
    // 获取当前格子（左下角格子）
    FlowCell cell00 = Get_Situate_Grid(transform.position);
    if (cell00 == null)
        return Vector2.zero; // 不在任何格子上

    // 当前格子的左下角世界坐标
    Vector3 corner00 = cell00.WorldPos - new Vector3(gridSize * 0.5f, gridSize * 0.5f, 0);

    // 计算三个邻居格子的左下角坐标
    Vector3 corner10 = corner00 + new Vector3(gridSize, 0, 0); // 右邻
    Vector3 corner01 = corner00 + new Vector3(0, gridSize, 0); // 上邻
    Vector3 corner11 = corner00 + new Vector3(gridSize, gridSize, 0); // 右上邻

    // 根据左下角坐标获取格子对象（邻居可能不存在，需判空）
    FlowCell cell10 = Get_Situate_Grid(corner10);
    FlowCell cell01 = Get_Situate_Grid(corner01);
    FlowCell cell11 = Get_Situate_Grid(corner11);

    // 获取方向（若格子不存在或不可行走，方向为零向量）
    Vector2 dir00 = Get_Situate_Grid_Dir(cell00);
    Vector2 dir10 = Get_Situate_Grid_Dir(cell10);
    Vector2 dir01 = Get_Situate_Grid_Dir(cell01);
    Vector2 dir11 = Get_Situate_Grid_Dir(cell11);

    // 获取权重
    Vector2 weight = Get_Weight();

    // 双线性插值
    Vector2 dirBottom = Vector2.Lerp(dir00, dir10, weight.x); // 下边插值
    Vector2 dirTop = Vector2.Lerp(dir01, dir11, weight.x); // 上边插值
    Vector2 result = Vector2.Lerp(dirBottom, dirTop, weight.y); // 垂直插值

    return result;
}*/
#endregion
