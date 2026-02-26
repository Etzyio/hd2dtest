using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules.SkillSystem;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 玩家技能辅助方法
    /// </summary>
    public static class PlayerSkillHelper
    {
        /// <summary>
        /// 创建防御Buff
        /// </summary>
        public static BuffData CreateDefenseBuff(string buffId, float defenseBoost, int duration)
        {
            var buff = new BuffData
            {
                BuffId = buffId,
                BuffName = "防御提升",
                Description = $"提升防御力 {defenseBoost * 100:F0}%",
                DurationType = BuffDurationType.Turns,
                TurnCount = duration,
                MaxStacks = 1,
                IsPositive = true,
                CanBeDispelled = true,
                RefreshDurationOnReapply = true
            };

            var effect = new BuffEffect
            {
                EffectType = BuffEffectType.DefenseBonus,
                Value = defenseBoost * 100f, // 转换为百分比
                IsPercentage = true,
                Description = $"防御力 +{defenseBoost * 100:F0}%"
            };

            buff.Effects.Add(effect);
            return buff;
        }

        /// <summary>
        /// 创建攻击Buff
        /// </summary>
        public static BuffData CreateAttackBuff(string buffId, float attackBoost, int duration)
        {
            var buff = new BuffData
            {
                BuffId = buffId,
                BuffName = "攻击提升",
                Description = $"提升攻击力 {attackBoost * 100:F0}%",
                DurationType = BuffDurationType.Turns,
                TurnCount = duration,
                MaxStacks = 1,
                IsPositive = true,
                CanBeDispelled = true,
                RefreshDurationOnReapply = true
            };

            var effect = new BuffEffect
            {
                EffectType = BuffEffectType.AttackBonus,
                Value = attackBoost * 100f, // 转换为百分比
                IsPercentage = true,
                Description = $"攻击力 +{attackBoost * 100:F0}%"
            };

            buff.Effects.Add(effect);
            return buff;
        }

        /// <summary>
        /// 创建速度Buff
        /// </summary>
        public static BuffData CreateSpeedBuff(string buffId, float speedBoost, int duration)
        {
            var buff = new BuffData
            {
                BuffId = buffId,
                BuffName = "速度提升",
                Description = $"提升速度 {speedBoost * 100:F0}%",
                DurationType = BuffDurationType.Turns,
                TurnCount = duration,
                MaxStacks = 1,
                IsPositive = true,
                CanBeDispelled = true,
                RefreshDurationOnReapply = true
            };

            var effect = new BuffEffect
            {
                EffectType = BuffEffectType.SpeedBonus,
                Value = speedBoost * 100f, // 转换为百分比
                IsPercentage = true,
                Description = $"速度 +{speedBoost * 100:F0}%"
            };

            buff.Effects.Add(effect);
            return buff;
        }

        /// <summary>
        /// 创建持续治疗Buff
        /// </summary>
        public static BuffData CreateRegenerationBuff(string buffId, float healPerTurn, int duration)
        {
            var buff = new BuffData
            {
                BuffId = buffId,
                BuffName = "持续恢复",
                Description = $"每回合恢复 {healPerTurn:F0} 点生命值",
                DurationType = BuffDurationType.Turns,
                TurnCount = duration,
                MaxStacks = 1,
                IsPositive = true,
                CanBeDispelled = true,
                RefreshDurationOnReapply = false
            };

            var effect = new BuffEffect
            {
                EffectType = BuffEffectType.HealthRegeneration,
                Value = healPerTurn,
                IsPercentage = false,
                Description = $"每回合恢复 {healPerTurn:F0} 生命值"
            };

            buff.Effects.Add(effect);
            return buff;
        }

        /// <summary>
        /// 创建伤害减免Buff
        /// </summary>
        public static BuffData CreateDamageReductionBuff(string buffId, float reductionPercent, int duration)
        {
            var buff = new BuffData
            {
                BuffId = buffId,
                BuffName = "伤害减免",
                Description = $"减少受到的伤害 {reductionPercent:F0}%",
                DurationType = BuffDurationType.Turns,
                TurnCount = duration,
                MaxStacks = 1,
                IsPositive = true,
                CanBeDispelled = true,
                RefreshDurationOnReapply = true
            };

            var effect = new BuffEffect
            {
                EffectType = BuffEffectType.DamageReduction,
                Value = reductionPercent,
                IsPercentage = true,
                Description = $"伤害减免 {reductionPercent:F0}%"
            };

            buff.Effects.Add(effect);
            return buff;
        }

        /// <summary>
        /// 创建暴击率提升Buff
        /// </summary>
        public static BuffData CreateCriticalRateBuff(string buffId, float critRateBoost, int duration)
        {
            var buff = new BuffData
            {
                BuffId = buffId,
                BuffName = "暴击提升",
                Description = $"提升暴击率 {critRateBoost * 100:F0}%",
                DurationType = BuffDurationType.Turns,
                TurnCount = duration,
                MaxStacks = 1,
                IsPositive = true,
                CanBeDispelled = true,
                RefreshDurationOnReapply = true
            };

            var effect = new BuffEffect
            {
                EffectType = BuffEffectType.CriticalRateBonus,
                Value = critRateBoost * 100f, // 转换为百分比
                IsPercentage = true,
                Description = $"暴击率 +{critRateBoost * 100:F0}%"
            };

            buff.Effects.Add(effect);
            return buff;
        }
    }
}