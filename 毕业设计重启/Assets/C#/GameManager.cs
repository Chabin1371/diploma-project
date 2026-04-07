using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : GameManager_Father
{
    public static GameManager Instance;

    [SerializeField] private Tower_keeper _keeper;
    public Tower_keeper Keeper => _keeper; 

    [Header("游戏货币")]
    [SerializeField] private LOOT loot; 
    public int exp => loot.EXP;
    public int coin => loot.Coin; 

    [Header("获取所有塔数据")]
    public List<Tower> towers_list = new List<Tower>();

    [Header("获取所有技能")]
    public List<Skill> skills_list = new List<Skill>();

    private Dictionary<int, Tower> towers_dict = new Dictionary<int, Tower>();
    private Dictionary<int, Skill> skills_dict = new Dictionary<int, Skill>();
    private Dictionary<int, Skill> skills_in_shop_dict = new Dictionary<int, Skill>();
    public Dictionary<int, Skill> Skills_in_shop_dict => skills_in_shop_dict;
    private List<Skill> skills_in_shop_Sold_list = new List<Skill>;
    public List<Skill> Skills_in_shop_Sold_list => skills_in_shop_Sold_list;
    public Dictionary<int, Skill> Skills_dict => skills_dict;
    private Dictionary<int, Skill> unlockSkills_dict = new Dictionary<int, Skill>();
    public Dictionary<int, Skill> UnlockSkills_dict => unlockSkills_dict;

    public Dictionary<int, Tower> Towers_dict => towers_dict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach(var tower in towers_list)
        {
            if (!towers_dict.ContainsKey(tower.id))
                towers_dict.Add(tower.id, tower);
        }
        foreach (var skill in skills_list)
        {
            if (!skills_dict.ContainsKey(skill.ID))
                skills_dict.Add(skill.ID, skill);

            if (skill.Need_to_be_bought)
                skills_in_shop_dict.Add(skill.ID, skill);
        }
        foreach (Tower tower in towers_dict.Values)
        {
            foreach (Skill skill in tower.SkillTree)
            {
                if (!unlockSkills_dict.ContainsKey(skill.ID))
                    unlockSkills_dict.Add(skill.ID, skill);
            }
        }
    }

    public void change_dict_for_shop(Skill skill)
    {
        if (skills_in_shop_dict.ContainsValue(skill))
        {
            foreach (var kvp in skills_in_shop_dict)
            {
                if (kvp.Value == skill)
                {
                    skills_in_shop_Sold_dict.Add(kvp.Value);
                    break;
                }
            }
        }
    }

    public Skill get_skill_from_dict(int ID)
    {
        if (!Skills_dict.ContainsKey(ID))
            return null;

        return skills_dict[ID]; 
    }

    public Tower get_tower_from_dict(int id)
    {
        if (!towers_dict.ContainsKey(id))
            return null;

        return towers_dict[id];
    }

    public void Change_Coin(int spend_Coin)
    {
        loot.spend_coin(spend_Coin);
    }
    public void Change_EXP(int spend_exp)
    {
        loot.spend_exp(spend_exp);
    }

    public void update_skill(int skillID)
    {
        Skill skill = unlockSkills_dict[skillID];

        if (skill == null)
        {
            Debug.Log("技能未解锁，无法升级");
            return;
        }

        skill.updateLevel();
    }

    public void update_unlockskill_dict(int id, Skill skill, int tower_id)
    {
        if (unlockSkills_dict.ContainsKey(id))
        {
            Debug.Log("该技能已存在，检查id是否重复或错误");
            return;
        }

        if (!towers_dict[tower_id].SkillTree.Contains(skill))
        {
            towers_dict[tower_id].SkillTree.Clear();
            towers_dict[tower_id].SkillTree.Add(skill);
        }

        unlockSkills_dict.Add(id, skill);
    }

    public void dict_debug()
    {
        foreach (var tower in towers_dict)
        {
            Debug.Log(tower.Key);
        }
    }
}
