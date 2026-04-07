using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Skill", menuName = "Game/Skill")]
public class Skill : ScriptableObject
{
    [Header("基础属性")]
    [SerializeField] private int Skill_ID;
    [SerializeField] private string SkillName;
    [SerializeField] private Sprite Skill_icon;
    [SerializeField] private int required_EXP_to_update;
    [SerializeField] private int required_Coin_to_unlocked;
    [Header("技能前置解锁条件")]
    [SerializeField] private List<Skill> requiredSkill = new List<Skill>();
    [SerializeField] private List<Skill> conflictingSkills = new List<Skill>();
    [Header("等级变化")]
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private int SkillLevel = 0;
    [SerializeField] private float SkillLevel_growth_scale = 0.2f;
    [Header("属性变更")]
    [SerializeField] private List<SkillEffect> skillEffects = new List<SkillEffect>();
    [SerializeField] private SE skill_Effect;//行为
    [Header("是否需要商店解锁")]
    [SerializeField] private bool need_to_be_bought = false;
    [SerializeField] private int speed;

    // runtime copies of the ScriptableObject templates in `skillEffects`.
    // These are created with `Instantiate` so modifying them at runtime
    // does not change the asset files.
    private List<SkillEffect> skillEffectsList = new List<SkillEffect>();

    public void updateLevel(bool is_reset = false)
    {
        // ensure we have runtime instances available
        EnsureRuntimeEffects();
        if (is_reset)
        {
            // recreate runtime copies from templates
            skillEffectsList.Clear();
            foreach (SkillEffect effect in skillEffects)
            {
                skillEffectsList.Add(UnityEngine.Object.Instantiate(effect));
            }
            SkillLevel = 0;
            required_EXP_to_update = 100;
            return;
        }

        if (SkillLevel == 0)
        {
            SkillLevel++;
            return;
        }

        if (SkillLevel >= maxLevel)
        {
            Debug.Log("技能已到最大等级！");
            return;
        }

        SkillLevel++;
        foreach (var effect in skillEffectsList)
        {
            effect.levelup();
        }
        required_EXP_to_update += Mathf.CeilToInt(required_EXP_to_update * SkillLevel_growth_scale);
    }

    public int ID => Skill_ID;
    public bool Need_to_be_bought => need_to_be_bought;
    public int Level => SkillLevel;
    public List<Skill> RequiredSkill => requiredSkill;
    public List<Skill> ConflictingSkills => conflictingSkills;
    public List<SkillEffect> SkillEffects => skillEffectsList;
    public Sprite Sprite => Skill_icon;
    public float required_EXP => required_EXP_to_update;
    public float required_Coin => required_Coin_to_unlocked;
    public SE Skill_Effect => skill_Effect;
    public int Spend_Coin => speed;

    // create runtime instances from the ScriptableObject templates if not already created
    private void EnsureRuntimeEffects()
    {
        if (skillEffectsList == null)
            skillEffectsList = new List<SkillEffect>();
        if (skillEffectsList.Count > 0)
            return;
        if (skillEffects == null)
            return;

        foreach (var template in skillEffects)
        {
            if (template != null)
                skillEffectsList.Add(UnityEngine.Object.Instantiate(template));
        }
    }
}
