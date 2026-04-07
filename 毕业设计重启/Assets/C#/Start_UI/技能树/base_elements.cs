using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class base_elements : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    private DesConnecter connecter;

    [HideInInspector] public int tower_id;

    public TextMeshProUGUI EXP_value;
    public TextMeshProUGUI level;

    private void Start()
    {
        level.text = GameManager.Instance.get_tower_from_dict(tower_id).Level.ToString();
    }
    public void Set_icon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
    public void Initialized(DesConnecter desConnecter)
    {
        connecter = desConnecter;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.exp >= GameManager.Instance.get_tower_from_dict(tower_id).required_exp)
        {
            Debug.Log("升级成功");
            GameManager.Instance.get_tower_from_dict(tower_id).LevelUp();
            GameManager.Instance.Change_EXP(GameManager.Instance.get_tower_from_dict(tower_id).required_exp);
            UIManager.Instance.MainMenu_C.all_refresh();
        }
        else
        {
            Debug.Log("经验不足");
        }

        refresh();

        level.text = GameManager.Instance.get_tower_from_dict(tower_id).Level.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EXP_value.transform.parent.gameObject.SetActive(true);

        refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EXP_value.transform.parent.gameObject.SetActive(false);
        connecter.gameObject.SetActive(false);
    }

    void refresh()
    {
        Tower tower = GameManager.Instance.get_tower_from_dict(tower_id);
        if (tower != null)
        {
            connecter.Name.text = tower.name;
            connecter.HP.text = tower.BaseHp.ToString();
            connecter.AP.text = tower.BaseAP.ToString();
            connecter.energy.text = tower.Energy.ToString();
            connecter.energy_produce.text = tower.EnergyProduce.ToString();
            connecter.gameObject.SetActive(true);
        }

        refresh_for_connecter(tower);

        EXP_value.text = GameManager.Instance.get_tower_from_dict(tower_id).required_exp.ToString();
    }
    void refresh_for_connecter(Tower tower)
    {
        connecter.HP_effect.color = Color.green;
        connecter.AP_effect.color = Color.green;
        connecter.energy_effect.color = Color.green;
        connecter.energy_produce_effect.color = Color.green;
        connecter.HP_effect.text = "(" + "+" + tower.Base_Hp_scale.ToString() +")";
        connecter.AP_effect.text = "(" + "+" + tower.Base_Ap_scale.ToString() + ")";
        connecter.energy_effect.text = "(" + "+" + tower.Energy_growth_scale.ToString() + ")";
        connecter.energy_produce_effect.text = "(" + "+" + tower.Energy_produce_growth_scale.ToString() + ")";
    }
}
