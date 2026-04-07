using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Pick_tower_ui_game : MonoBehaviour
{
    private List<GameObject> Grid = new List<GameObject>();

    [SerializeField] private GameObject Grid_Prefab;
    [SerializeField] private GameObject Grid_parent;
    [SerializeField] private TextMeshProUGUI Placed_Count;
    public void Start()
    {
        if (GameManager_in_game.Instance.mouse != null)
        {
            GameManager_in_game.Instance.mouse.OnPlaced += refreash_UI;
        }
        GameManager_in_game.Instance.OnGameReset += refreash_UI;

        foreach (var tower in GameManager_in_game.Instance.towers_.TowerList)
        {
            GameObject tower_UI = Instantiate(Grid_Prefab, Grid_parent.transform);
            Grid.Add(tower_UI);

            tower_UI.GetComponent<Image>().sprite = tower.Icon;
            Pick_to_place pick_To_Place = tower_UI.GetComponent<Pick_to_place>();
            pick_To_Place.Tower = tower;
        }

        refreash_UI();
    }

    public void refreash_UI()
    {
        Placed_Count.text 
            = GameManager_in_game.Instance.Current_placed_cell_.ToString() + "/" + GameManager_in_game.Instance.Max_placed_cell.ToString();
        GameManager_in_game.Instance.Energy_UI_PREFAB.text
            = GameManager_in_game.Instance.current_energy.ToString();
    }
    private void OnDestroy()
    {
        if (GameManager_in_game.Instance.mouse != null)
            GameManager_in_game.Instance.mouse.OnPlaced -= refreash_UI;

        GameManager_in_game.Instance.OnGameReset -= refreash_UI;
    }
}
