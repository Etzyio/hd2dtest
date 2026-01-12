using Godot;
using hd2dtest.Scripts.Modules;
using System;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 伤害计算器，用于处理所有伤害计算逻辑
    /// </summary>
    public partial class DamageCalculator : Node
    {
        private static DamageCalculator _instance;

        public static DamageCalculator Instance => _instance;

        public override void _Ready()
        {
            _instance = this;
        }
        /// <summary>
        /// 计算实际伤害值
        /// </summary>
        /// <param name="baseDamage">基础伤害值</param>
        /// <param name="defense">目标防御值</param>
        /// <param name="weaknesses">目标弱点列表</param>
        /// <param name="damageType">伤害类型</param>
        /// <returns>实际伤害值</returns>
        public static float CalculateDamage(Creature player, Creature monster, Skill skill)
        {
            // 计算基础伤害（考虑防御）
            float baseDamageAfterDefense = Mathf.Max(1f, player.Attack - monster.Defense * 0.1f);

            // 计算弱点倍率
            float weaknessMultiplier = monster.weaknesses.Contains(skill.DamageType) ? 1.3f : 1.0f;

            // float CalculateCritDamage = GD.Randf() < player.CritRate ? 1.5f : 1.0f;

            // 计算最终伤害
            float finalDamage = baseDamageAfterDefense * weaknessMultiplier;

            return finalDamage;
        }
    }
}