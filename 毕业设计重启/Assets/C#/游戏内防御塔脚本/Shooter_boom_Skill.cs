using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Shooter Skill/boom_skill")]
public class Shooter_boom_Skill : Skill_Effect<Shooter>
{
    private int skill_id = 1002;

    [SerializeField] private GameObject boom_bullet_for_shooter;

    public override void apply_skill_effect_attack(Shooter tower_Controller)
    {
        tower_Controller.bullet = boom_bullet_for_shooter;
    }
}
