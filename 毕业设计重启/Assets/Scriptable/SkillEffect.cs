using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    public int Hp_change;
    public int ap_speed_change;
    public int Ap_change;
    public int Range_change;
    public int energy_change;
    public int energy_produce_change;

    public abstract void Apply(Tower_Controller tower_Controller);
    public abstract void levelup();
}
