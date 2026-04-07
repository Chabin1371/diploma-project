using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Skill", menuName = "Producer Skill/Money")]
public class Producer_Money : Skill_Effect<Producer>
{
    private int skill_id = 2001;
    public override void apply_skill_effect_attack(Producer tower_Controller)
    {
        if (GameManager_in_game.Instance != null)
        {
            tower_Controller.skill_2001 = true;
            tower_Controller.Coin_get_for_skill_2001 += 5;
            tower_Controller.Coin_get_for_skill_2001_every_time -= 0.2f;
        }
    }
}
