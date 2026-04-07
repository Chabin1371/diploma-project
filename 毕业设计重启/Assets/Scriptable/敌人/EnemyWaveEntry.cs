using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWaveEntry", menuName = "Enemy/EnemyWaveEntry")]
[System.Serializable]
public class EnemyWaveEntry : ScriptableObject
{
    public List<GameObject> enemyPrefab;      // 敌人预制体（必须包含 Enemy 组件）
    public int count;                   // 该类型敌人在本波的数量
}
