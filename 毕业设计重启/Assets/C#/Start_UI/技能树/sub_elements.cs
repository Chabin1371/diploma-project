using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class sub_elements : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Sprite skill_icon;
    private DesConnecter DesConnecter;
    public int skill_ID;
    public Image icon;

    public int tower_id;

    public TextMeshProUGUI EXP_value;
    public TextMeshProUGUI Coin_value;
    public TextMeshProUGUI Conflict_Skill;

    public TextMeshProUGUI level;

    public bool is_unlocked = false;

    private int spend_Coin_value = 0;
    private int spend_EXP_value = 0;

    public GameObject lock_image;

    public void Set_icon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
    public void Initialized(DesConnecter desConnecter)
    {
        DesConnecter = desConnecter;
    }

    public void Start()
    {
        if (GameManager.Instance.UnlockSkills_dict.ContainsKey(skill_ID))
            is_unlocked = true;
        else 
            is_unlocked = false;

        if (is_unlocked)
            lock_image.SetActive(false);

        level.text = GameManager.Instance.get_skill_from_dict(skill_ID).Level.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        refresh();

        if (is_unlocked)
        {
            if (GameManager.Instance.exp >= GameManager.Instance.UnlockSkills_dict[skill_ID].required_EXP)
            {
                Debug.Log("升级成功！");
                GameManager.Instance.Change_EXP((int)GameManager.Instance.UnlockSkills_dict[skill_ID].required_EXP);
                spend_EXP_value += (int)GameManager.Instance.UnlockSkills_dict[skill_ID].required_EXP;
                GameManager.Instance.update_skill(skill_ID);
                UIManager.Instance.MainMenu_C.all_refresh();
            }
            else
            {
                Debug.Log("经验值不足");
            }
        }
        else
        {
            if (GameManager.Instance.Skills_dict[skill_ID].RequiredSkill.Count != 0)
            {
                foreach (Skill skill in GameManager.Instance.Skills_dict[skill_ID].RequiredSkill)
                {
                    if (!GameManager.Instance.UnlockSkills_dict.ContainsValue(skill))
                    {
                        Debug.Log(skill + "该前置技能未解锁，无法解锁此技能");
                        return;
                    }
                }
            }

            if (GameManager.Instance.Skills_dict[skill_ID].ConflictingSkills.Count != 0)
            {
                foreach (Skill skill in GameManager.Instance.Skills_dict[skill_ID].ConflictingSkills)
                {
                    if (GameManager.Instance.UnlockSkills_dict.ContainsValue(skill))
                    {
                        Debug.Log(skill + "已解锁冲突技能，无法解锁此技能");
                        return;
                    }
                }
            }

            if (GameManager.Instance.coin >= GameManager.Instance.Skills_dict[skill_ID].required_Coin)
            {
                Debug.Log("解锁成功!");
                is_unlocked = true;
                GameManager.Instance.Change_Coin((int)GameManager.Instance.Skills_dict[skill_ID].required_Coin);
                spend_Coin_value += (int)GameManager.Instance.Skills_dict[skill_ID].required_Coin;
                GameManager.Instance.update_unlockskill_dict(skill_ID, GameManager.Instance.Skills_dict[skill_ID], tower_id);
                UIManager.Instance.MainMenu_C.all_refresh();
                lock_image.SetActive(false);
                Coin_value.transform.parent.gameObject.SetActive(false);
                GameManager.Instance.Skills_dict[skill_ID].updateLevel();
                EXP_value.text = GameManager.Instance.Skills_dict[skill_ID].required_EXP.ToString();
                EXP_value.transform.parent.gameObject.SetActive(true);
            }
        }

        level.text = GameManager.Instance.get_skill_from_dict(skill_ID).Level.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!is_unlocked)
        {
            Coin_value.transform.parent.gameObject.SetActive(true);
            Coin_value.text = GameManager.Instance.Skills_dict[skill_ID].required_Coin.ToString();

            if (GameManager.Instance.Skills_dict[skill_ID].ConflictingSkills.Count != 0)
            {
                Skill conflict_skill = null;
                foreach (Skill skill in GameManager.Instance.Skills_dict[skill_ID].ConflictingSkills)
                {
                    if (GameManager.Instance.UnlockSkills_dict.ContainsValue(skill))
                    {
                        conflict_skill = skill;
                        Coin_value.transform.parent.gameObject.SetActive(false);
                        Conflict_Skill.transform.parent.gameObject.SetActive(true);
                        break;
                    }
                }
                if (conflict_skill != null)
                {
                    Conflict_Skill.color = Color.red;
                    Conflict_Skill.text = conflict_skill.name.ToString();
                }
            }
        }
        else
        {
            refresh();
            EXP_value.transform.parent.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DesConnecter.gameObject.SetActive(false);
        Coin_value.transform.parent.gameObject.SetActive(false);
        EXP_value.transform.parent.gameObject.SetActive(false);
        if (Conflict_Skill.transform.parent.gameObject)
            Conflict_Skill.transform.parent.gameObject.SetActive(false);
    }

    public void Reset_this_Skill()
    {
        Debug.Log("重置");

        is_unlocked = false;
        lock_image.SetActive(true);

        //返还经验值和硬币
        GameManager.Instance.Change_Coin(-spend_Coin_value);
        GameManager.Instance.Change_EXP(-spend_EXP_value);
        spend_Coin_value = 0;
        spend_EXP_value = 0;
        //重置技能等级
        Skill skill = GameManager.Instance.Skills_dict[skill_ID];
        skill.updateLevel(is_reset: true);
        //解除字典里对应的技能
        if (GameManager.Instance.UnlockSkills_dict.ContainsKey(skill_ID))
            GameManager.Instance.UnlockSkills_dict.Remove(skill_ID);

        if (GameManager.Instance.Towers_dict[tower_id].SkillTree.Contains(skill))
            GameManager.Instance.Towers_dict[tower_id].SkillTree.Remove(skill);

        level.text = GameManager.Instance.get_skill_from_dict(skill_ID).Level.ToString();

        UIManager.Instance.MainMenu_C.all_refresh();
    }

    void refresh()
    {
        Tower tower = GameManager.Instance.get_tower_from_dict(tower_id);
        Skill skill = GameManager.Instance.Skills_dict[skill_ID];
        if (tower != null)
        {
            DesConnecter.Name.text = skill.name;
            DesConnecter.HP.text = tower.BaseHp.ToString();
            DesConnecter.AP.text = tower.BaseAP.ToString();
            DesConnecter.energy.text = tower.Energy.ToString();
            DesConnecter.energy_produce.text = tower.EnergyProduce.ToString();
            DesConnecter.gameObject.SetActive(true);
        }

        if (skill.SkillEffects.Count != 0)
        {
            int HP_effect = 0;
            int AP_effect = 0;
            int energy_effect = 0;
            int energy_produce_effect = 0;
            foreach (var effect in skill.SkillEffects)
            {
                if (effect.Hp_change != 0)
                    HP_effect += effect.Hp_change;
                if (effect.Ap_change != 0)
                    AP_effect += effect.Ap_change;
                if (effect.energy_change != 0)
                    energy_effect += effect.energy_change;
                if (effect.energy_produce_change != 0)
                    energy_produce_effect += effect.energy_produce_change;
            }

            SkillEffects_UI(HP_effect, DesConnecter.HP_effect);
            SkillEffects_UI(AP_effect, DesConnecter.AP_effect);
            SkillEffects_UI(energy_effect, DesConnecter.energy_effect);
            SkillEffects_UI(energy_produce_effect, DesConnecter.energy_produce_effect);
        }
        else
        {
            SkillEffects_UI(0, DesConnecter.HP_effect);
            SkillEffects_UI(0, DesConnecter.AP_effect);
            SkillEffects_UI(0, DesConnecter.energy_effect);
            SkillEffects_UI(0, DesConnecter.energy_produce_effect);
        }

        EXP_value.text = GameManager.Instance.Skills_dict[skill_ID].required_EXP.ToString();
    }

    public void SkillEffects_UI(int var, TextMeshProUGUI textMeshPro)
    {
        if (var >= 0)
        {
            textMeshPro.text = "(" + "+" + var.ToString() + ")";
            textMeshPro.color = Color.green;
        }
        else
        {
            textMeshPro.text = "(" + var.ToString() + ")";
            textMeshPro.color = Color.red;
        }

    }
}
