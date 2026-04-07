using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Update_UI : UIbase
{
    public GameObject Grid;
    public GameObject Grid_Parent;
    private List<GameObject> Grids = new List<GameObject>();

    private void Start()
    {
        foreach (var tower in GameManager.Instance.Towers_dict)
        {
            GameObject grid = Instantiate(Grid, Grid_Parent.transform);
            Grids.Add(grid);
            grid.GetComponent<Grid>().Set_name(tower.Value.TowerName);
            grid.GetComponent<Grid>().Set_icon(tower.Value.Icon);
        }
    }
}
