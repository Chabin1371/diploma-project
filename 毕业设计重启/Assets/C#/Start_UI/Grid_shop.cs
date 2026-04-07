using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Grid_shop : MonoBehaviour,IPointerClickHandler
{
    public TextMeshProUGUI Coin_spend_value;
    public Image icon;
    public GameObject Sold;
    public int coin_spend;
    public Skill Skill;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.coin > coin_spend)
        {
            Debug.Log("解锁成功！");
            GameManager.Instance.Change_Coin(coin_spend);
            GameManager.Instance.change_dict_for_shop(Skill);
            GameManager.Instance.UnlockSkills_dict.Add(Skill.ID, Skill);
            Sold.SetActive(true);
            UIManager.Instance.MainMenu_C.Shop_refresh();
        }
        else
        {
            Debug.Log("金币不足，无法解锁");
            return;
        }
    }
}
