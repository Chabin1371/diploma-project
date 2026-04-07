using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shop_UI :UIbase
{
    public TextMeshProUGUI Coin;
    public GameObject grid;
    public GameObject grid_prefab;
    private void Start()
    {
        foreach (var skill in GameManager.Instance.Skills_in_shop_dict)
        {
            GameObject grid = Instantiate(grid_prefab);
            grid.transform.SetParent(grid.transform, true);
            Grid_shop grid_Shop = grid.GetComponent<Grid_shop>();
            grid_Shop.Coin_spend_value.text = skill.Value.Spend_Coin.ToString();
            grid_Shop.coin_spend = skill.Value.Spend_Coin;
            grid_Shop.Skill = skill.Value;
            if (skill.Value.Sprite != null)
                grid_Shop.icon.sprite = skill.Value.Sprite;
            else
                Debug.Log("图片设置出现问题！");
        }
    }
    public override void Refresh()
    {
        base.Refresh();

        Coin.text = GameManager.Instance.coin.ToString();
    }
}
