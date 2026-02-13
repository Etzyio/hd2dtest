using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using System;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Utilities
{
    /// <summary>
    /// 伤害计算器，用于处理所有伤害计算逻辑
    /// </summary>
    public static class DamageCalculator
    {
        private static readonly Random _random = new();

        /// <summary>
        /// 生成随机浮点数
        /// </summary>
        /// <returns>0到1之间的随机浮点数</returns>
        public static float Randf()
        {
            return (float)_random.NextDouble();
        }

        /// <summary>
        /// 生成随机整数
        /// </summary>
        /// <param name="max">最大值（不包含）</param>
        /// <returns>0到max-1之间的随机整数</returns>
        public static int Randi(int max)
        {
            return _random.Next(max);
        }

        /// <summary>
        /// 生成指定范围内的随机浮点数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>min到max之间的随机浮点数</returns>
        public static float RandRange(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }

        /// <summary>
        /// 计算实际伤害值
        /// </summary>
        /// <param name="attacker">攻击方生物</param>
        /// <param name="defender">防御方生物</param>
        /// <param name="skill">技能信息</param>
        /// <returns>实际伤害值</returns>
        public static float CalculateDamage(Creature attacker, Creature defender, Skill skill)
        {
            // 计算基础伤害（考虑防御）
            float baseDamageAfterDefense = Math.Max(1f, attacker.Attack - defender.Defense * 0.1f);

            // 计算弱点倍率
            float weaknessMultiplier = defender.Weaknesses != null && defender.Weaknesses.Contains(skill.DamageType) ? 1.3f : 1.0f; 

            // 计算最终伤害
            float finalDamage = baseDamageAfterDefense * weaknessMultiplier;

            return finalDamage;
        }
    }
}