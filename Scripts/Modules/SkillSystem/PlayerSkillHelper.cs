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
                BuffName = TranslationServer.Translate("buff_defense_boost_name"),
                Description = string.Format(TranslationServer.Translate("buff_defense_boost_desc"), defenseBoost * 100),
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
                Description = string.Format(TranslationServer.Translate("buff_effect_defense_desc"), defenseBoost * 100)
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
                BuffName = TranslationServer.Translate("buff_attack_boost_name"),
                Description = string.Format(TranslationServer.Translate("buff_attack_boost_desc"), attackBoost * 100),
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
                Description = string.Format(TranslationServer.Translate("buff_effect_attack_desc"), attackBoost * 100)
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
                BuffName = TranslationServer.Translate("buff_speed_boost_name"),
                Description = string.Format(TranslationServer.Translate("buff_speed_boost_desc"), speedBoost * 100),
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
                Description = string.Format(TranslationServer.Translate("buff_effect_speed_desc"), speedBoost * 100)
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
                BuffName = TranslationServer.Translate("buff_regeneration_name"),
                Description = string.Format(TranslationServer.Translate("buff_regeneration_desc"), healPerTurn),
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
                Description = string.Format(TranslationServer.Translate("buff_effect_regeneration_desc"), healPerTurn)
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
                BuffName = TranslationServer.Translate("buff_damage_reduction_name"),
                Description = string.Format(TranslationServer.Translate("buff_damage_reduction_desc"), reductionPercent),
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
                Description = string.Format(TranslationServer.Translate("buff_effect_damage_reduction_desc"), reductionPercent)
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
                BuffName = TranslationServer.Translate("buff_crit_rate_boost_name"),
                Description = string.Format(TranslationServer.Translate("buff_crit_rate_boost_desc"), critRateBoost * 100),
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
                Description = string.Format(TranslationServer.Translate("buff_effect_crit_rate_boost_desc"), critRateBoost * 100)
            };

            buff.Effects.Add(effect);
            return buff;
        }
    }
}