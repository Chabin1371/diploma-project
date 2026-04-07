using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Skill_Effect<T> : SE where T : Tower_Controller
{
    public abstract void apply_skill_effect_attack(T tower_Controller);
    public override void apply_skill_effect_attack(Tower_Controller tower_Controller)
    {
        // ========== targetController 核心代码 ==========
        // 1. 类型校验 + 转换：把 Tower_Controller 转为 T 类型（比如 Shooter）
        if (tower_Controller is not T targetController)
        {
            // 类型不匹配时打日志，方便调试
            Debug.LogError($"[Skill_Effect] 类型错误！需要 {typeof(T).Name}，实际传入 {tower_Controller.GetType().Name}");
            return;
        }
        // ========== targetController 核心代码 ==========

        // 2. 调用子类实现的泛型方法，传入转换后的 targetController
        apply_skill_effect_attack(targetController);
    }
}
