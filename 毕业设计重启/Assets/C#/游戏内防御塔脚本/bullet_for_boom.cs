using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet_for_boom : bullet
{
    public float boomer_rad = 1f;
    public override void attack_effect(Enemy enemy, List<Enemy> enemies)
    {
        foreach (Enemy enemy_ in enemies)
        {
            if (Vector3.Distance(enemy_.transform.position, transform.position) < boomer_rad)
            {
                enemy_.Hp -= attack_value;
            }
        }
    }
}
