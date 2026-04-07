using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float time;
    private float timer;
    public Vector3 targetPos;
    public float moveSpeed;
    public float bulletRadius = 0.2f;
    public int attack_value = 10;

    public void Set_target_Pos(Vector3 enemyPos)
    {
        targetPos = enemyPos;

        float angle = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;

        // 直接设置物体的 Z 轴旋转（2D 物体只需绕 Z 轴转）
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Start()
    {
        timer = time;
    }
    private void DetectHit()
    {
        List<Enemy> enemies = EnemyManager.Instance.activeEnemies;
        foreach (Enemy enemy in enemies)
        {
            if (enemy.is_dead)
                continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            float enemyRadius = enemy.enemyRadius; // 假设敌人也有半径字段
            if (distance <= bulletRadius + enemyRadius)
            {
                attack_effect(enemy, enemies);
                if (enemy.Hp <= 0)
                    enemy.Dead();

                GetComponentInParent<Shooter>().ReturnBullet(this.gameObject);
                break; // 一颗子弹只命中一个敌人
            }
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            GetComponentInParent<Shooter>().ReturnBullet(this.gameObject);
            timer = time;
        }

        if (targetPos == null && targetPos == Vector3.zero)
            return;

        transform.position += targetPos * moveSpeed * Time.deltaTime;
        DetectHit();
    }

    public virtual void attack_effect(Enemy enemy, List<Enemy> enemies)
    {
        enemy.Hp -= attack_value;
    }
}
