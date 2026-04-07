using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Skill", menuName = "Shooter Skill/base_skill")]
public class Shooter_base_skill : Skill_Effect<Shooter>
{
    private int skill_id = 1001;
    public override void apply_skill_effect_attack(Shooter tower_Controller)
    {
        int Level = 0;
        foreach (Skill skill in tower_Controller.Skills.Values)
        {
            if (skill_id == skill.ID)
                Level = skill.Level;
            else
                return;
        }
        tower_Controller.cell_Ap += 5 * Level;
        tower_Controller.cell_Hp += 10 * Level;
    }
}
