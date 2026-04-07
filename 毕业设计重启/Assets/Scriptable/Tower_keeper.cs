using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Keeper", menuName = "Game/Keeper")]
public class Tower_keeper : ScriptableObject
{
    public List<Tower> TowerList = new List<Tower>();
}
