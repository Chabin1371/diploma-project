using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Skill", menuName = "Game/Shooter_skilleffect")]
public class Shooter_skilleffect : SkillEffect
{
    public override void Apply(Tower_Controller tower_Controller)
    {
        tower_Controller.attackCooldown += ap_speed_change;
        tower_Controller.cell_Hp += Hp_change;
        tower_Controller.cell_Ap += Ap_change;
        tower_Controller.attackRange += Range_change;
    }

    public override void levelup()
    {
        Hp_change += 1;
        Ap_change += 2;
    }
}
