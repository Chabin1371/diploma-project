using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 单个波次的配置
[System.Serializable]
public class Wave
{
    public List<EnemyWaveEntry> enemies = new List<EnemyWaveEntry>();
}

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public FlowField _flowFieldSystem { get; private set; }

    [Header("敌人设置")]
    public List<Wave> waves = new List<Wave>();
    private List<Wave> finished_wave = new List<Wave>();
    private Wave current_wave;
    public Transform enemyContainer; // 所有敌人的父物体

    [Header("生成设置")]
    private List<Enemy> ActiveEnemies = new List<Enemy>();
    public List<Enemy> activeEnemies => ActiveEnemies;
    public GameObject spawnPoints;
    [SerializeField] float interval = 0.5f;

    [Header("对象池预设")]
    public int initialPoolSizePerType = 5;                // 每种敌人预先生成的数量（会根据波次需求自动扩容）
    private Dictionary<GameObject, Queue<Enemy>> enemyPools = new Dictionary<GameObject, Queue<Enemy>>();

    // 对象池
    private Queue<Enemy> enemyPool = new Queue<Enemy>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        InitializePool();
    }

    private void Start()
    {
        if (_flowFieldSystem == null)
        {
            _flowFieldSystem = FindObjectOfType<FlowField>();
            if (_flowFieldSystem == null)
            {
                Debug.LogError("EnemyManager: 找不到 FlowFieldSystem!");
            }
        }
    }

    #region 对象池
    void InitializePool()
    {
        if (waves.Count == 0)
        {
            Debug.LogWarning("EnemyManager: 没有配置任何波次，对象池将为空。");
            return;
        }

        // 统计每种敌人所需的最大数量
        Dictionary<GameObject, int> maxNeeded = new Dictionary<GameObject, int>();

        foreach (Wave wave in waves)
        {
            foreach (EnemyWaveEntry entry in wave.enemies)
            {
                foreach (GameObject gameObject in entry.enemyPrefab)
                {
                    if (gameObject == null)
                        continue;

                    if (!maxNeeded.ContainsKey(gameObject))
                        maxNeeded[gameObject] = 0;

                    // 累加所有波次中该类型的数量
                    maxNeeded[gameObject] += entry.count;
                }
            }
        }

        // 为每种敌人创建对象池
        foreach (var kv in maxNeeded)
        {
            GameObject prefab = kv.Key;
            int needed = kv.Value;

            // 如果设置了 initialPoolSizePerType，可以预先生成一些（取较大值）
            int preCreateCount = Mathf.Max(initialPoolSizePerType, needed);
            CreatePoolForPrefab(prefab, preCreateCount);
        }
    }
    private void CreatePoolForPrefab(GameObject prefab, int count)
    {
        if (enemyPools.ContainsKey(prefab))
        {
            Debug.LogWarning($"EnemyManager: 预制体 {prefab.name} 的池已存在，将补充至 {count} 个");
            // 补充到指定数量
            Queue<Enemy> pool = enemyPools[prefab];
            while (pool.Count < count)
            {
                CreateNewEnemyForPrefab(prefab, pool);
            }
        }
        else
        {
            Queue<Enemy> newPool = new Queue<Enemy>();
            for (int i = 0; i < count; i++)
            {
                CreateNewEnemyForPrefab(prefab, newPool);
            }
            enemyPools[prefab] = newPool;
        }
    }
    private void CreateNewEnemyForPrefab(GameObject prefab, Queue<Enemy> pool)
    {
        // 确保容器存在
        if (enemyContainer == null)
        {
            GameObject containerObj = new GameObject("EnemyContainer");
            enemyContainer = containerObj.transform;
            enemyContainer.SetParent(transform);
        }

        GameObject enemyObj = Instantiate(prefab, enemyContainer);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError($"EnemyManager: 预制体 {prefab.name} 没有 Enemy 组件！");
            Destroy(enemyObj);
            return;
        }
        enemy.spawn = enemyContainer.transform;
        enemyObj.SetActive(false);
        pool.Enqueue(enemy);
    }
    private Enemy GetEnemyFromPool(GameObject prefab)
    {
        if (!enemyPools.ContainsKey(prefab))
        {
            // 首次出现该类型，创建池（预生成 initialPoolSizePerType 个）
            CreatePoolForPrefab(prefab, initialPoolSizePerType);
        }

        Queue<Enemy> pool = enemyPools[prefab];
        if (pool.Count == 0)
        {
            // 池空，动态扩容
            CreateNewEnemyForPrefab(prefab, pool);
        }

        Enemy enemy = pool.Dequeue();
        return enemy;
    }
    public void ReturnEnemyToPool(Enemy enemy)
    {
        if (enemy == null) return;

        GameObject prefab = enemy.gameObject;
        if (prefab == null)
        {
            Debug.LogError("EnemyManager: 无法确定敌人所属预制体，无法正确回收！");
            Destroy(enemy.gameObject);
            return;
        }

        if (!enemyPools.ContainsKey(prefab))
        {
            // 如果池意外丢失，重新创建
            CreatePoolForPrefab(prefab, initialPoolSizePerType);
        }

        enemy.gameObject.SetActive(false);
        enemyPools[prefab].Enqueue(enemy);
    }
    #endregion

    #region 生成逻辑

    /// <summary>
    /// 初始化一个波次，创建生成任务列表
    /// </summary>
    private void StartWave(Wave wave)
    {
        current_wave = wave;
        StartCoroutine(SpawnWaveWithInterval(wave));
    }

    private IEnumerator SpawnWaveWithInterval(Wave wave)
    {
        foreach (var enemy in wave.enemies)
        {
            if (enemy.count != 0 && enemy.enemyPrefab.Count != 0)
            {
                foreach (var enemy_prefab in enemy.enemyPrefab)
                {
                    for (int i = 0; i < enemy.count; i++)
                    {
                        Enemy enemy_ = GetEnemyFromPool(enemy_prefab);
                        enemy_.transform.position = spawnPoints.transform.position;
                        enemy_.gameObject.SetActive(true);
                        activeEnemies.Add(enemy_);

                        // 每个敌人生成后等待 interval 秒
                        yield return new WaitForSeconds(interval);
                    }
                }
            }
            else
            {
                Debug.Log("该类型敌人没有设置数量");
            }
        }
    }
    private void TryToNextWave()
    {
        if (activeEnemies.Count != 0)
            return;

        finished_wave.Add(current_wave);

        if (finished_wave.Count == waves.Count)
        {
            GameManager_in_game.Instance.game_End(true);
            return;
        }

        foreach (Wave wave in waves)
        {
            if (finished_wave.Contains(wave))
                continue;

            StartWave(wave);
        }
    }

    #endregion

    #region 外部调用

    public void start_spawn()
    {
        StartCoroutine(start_spawn_());
    }
    private IEnumerator start_spawn_()
    {
        yield return new WaitForSeconds(5);
        StartWave(waves[0]);
    }
    public void enemyDead(Enemy enemy)
    {
        ReturnEnemyToPool(enemy);
        ActiveEnemies.Remove(enemy);
        TryToNextWave();
    }
    #endregion

}
