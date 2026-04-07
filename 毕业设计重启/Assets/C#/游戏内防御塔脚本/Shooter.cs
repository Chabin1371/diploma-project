using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Shooter : Tower_Controller
{
    private float lastAttackTime;
    public float max_bullet_num;
    public GameObject bullet;
    private List<GameObject> bullets = new List<GameObject>();

    public override void Start_()
    {
        base.Start_();

        is_working_Ui.SetActive(true);

        FlowCell flowCell = GameManager_in_game.Instance.flowField_System.Get_flowCell(transform.position);

        if (flowCell.Skills.Count != 0)
        {
            foreach (Skill skill in flowCell.Skills.Values)
            {
                Debug.Log("开始切换");
                Debug.Log(skill.Skill_Effect);
                skill.Skill_Effect.apply_skill_effect_attack(this);
            }
        }

        Instantiate_bullet_pool(bullet);
    }

    public void Instantiate_bullet_pool(GameObject bullet_prefab)
    {
        for (int i = 0; i < max_bullet_num; i++)
        {
            GameObject bullet_ = Instantiate(bullet_prefab, transform);
            bullet_.transform.position = transform.position;
            bullet_.SetActive(false);
            bullets.Add(bullet_);
        }
    }
    public GameObject GetBulletFromPool()
    {
        // 遍历所有子弹，找到第一个非激活的返回
        foreach (GameObject bullet in bullets)
        {
            if (!bullet.activeInHierarchy)
            {
                bullet.SetActive(true);
                return bullet;
            }
        }

        // 如果超过最大限制且没有可用子弹，返回 null（或根据需要处理）
        Debug.LogWarning("子弹池已满，没有可用子弹！");
        return null;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.position = this.transform.position;
    }

    public void Update()
    {
/*        Debug.Log(is_working);*/
        if (!is_working)
            return;

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        List<Enemy> enemiesInRange = new List<Enemy>();

        List<FlowCell> flowCells = GameManager_in_game.Instance.flowField_System.get_all_walkable_flowCell();

        foreach (FlowCell cell in flowCells)
        {
            if (cell == null) continue;

            // 检查该网格内的每个敌人
            foreach (Enemy enemy in cell.enemiesInCell)
            {
                float distSqr = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distSqr <= attackRange * attackRange)
                {
                    if (!enemy.is_dead)
                        enemiesInRange.Add(enemy);
                }
            }
        }

        if (enemiesInRange.Count > 0)
        {
            // 选择最近的目标进行攻击
            Enemy target = GetClosestEnemy(enemiesInRange);
            Attack(target);
            lastAttackTime = Time.time;
        }
    }
    Enemy GetClosestEnemy(List<Enemy> enemies)
    {
        Enemy closest = null;
        float minDistSqr = float.MaxValue;
        foreach (Enemy e in enemies)
        {
            float dsqr = (e.transform.position - transform.position).sqrMagnitude;
            if (dsqr < minDistSqr)
            {
                minDistSqr = dsqr;
                closest = e;
            }
        }
        return closest;
    }
    void Attack(Enemy target)
    {
        GameObject bullet = GetBulletFromPool();
        bullet.SetActive(true);
        bullet_for_Shooter bullet_For_Shooter = bullet.GetComponent<bullet_for_Shooter>();
        bullet_For_Shooter.attack_value += cell_Ap;
        bullet_For_Shooter.Set_target_Pos(CalculateShootDirection(target, bullet_For_Shooter));
    }

    public Vector3 CalculateShootDirection(Enemy target, bullet_for_Shooter bullet)
    {
        float time = 0;
        float min_distance = float.MaxValue;
        Vector3 current_Dir = Vector3.zero;

        for (int i = 0; i < 10f; i++)
        {
            Vector3 predict_enemyPos = target.transform.position + target.velocity * time;
            Vector3 predict_bulletPos = bullet.transform.position + (predict_enemyPos - bullet.transform.position) * bullet.moveSpeed * time;
            float distance = Vector3.Distance(predict_enemyPos, predict_enemyPos);

            if (distance < min_distance)
            {
                min_distance = distance;
                current_Dir = predict_enemyPos - bullet.transform.position;
            }
        }

        return current_Dir;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
