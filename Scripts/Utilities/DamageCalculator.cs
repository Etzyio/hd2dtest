using hd2dtest.Scripts.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static List<int> CalculateDamage(Creature attacker, Creature defender, Skill skill)
        {
            List<int> list1 = [];
            // 计算基础伤害（考虑防御）
            float baseDamageAfterDefense = Math.Max(1f, attacker.Attack - defender.Defense * 0.1f);

            foreach (var skilldefent in skill.SkillDefs)
            {
                float weaknessMultiplier = defender.Weaknesses != null && defender.Weaknesses.Contains(skilldefent.DamageType) ? 1.3f : 1.0f;

                if (skilldefent.Type == Skill.SkillType.Attack)
                {
                    baseDamageAfterDefense += skilldefent.DamageCoefficient * weaknessMultiplier;
                    list1.Add((int)baseDamageAfterDefense);
                }
                else if (skilldefent.Type == Skill.SkillType.Defense)
                {
                    // 防御技能：临时提升防御力，不直接参与伤害计算
                    // 防御效果通过Buff系统实现
                    continue;
                }
                else if (skilldefent.Type == Skill.SkillType.Support)
                {
                    // 支持技能：提供增益效果，通过Buff系统实现
                    // 支持效果通过Buff系统实现
                    continue;
                }
                else if (skilldefent.Type == Skill.SkillType.Healing)
                {
                    // 治疗技能：不造成伤害，通过治疗系统实现
                    // 治疗技能不增加伤害，跳过伤害计算
                    continue;
                }
            }

            return list1;
        }

        /// <summary>
        /// 计算技能伤害（用于技能事件系统）
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="target">目标</param>
        /// <param name="baseDamage">基础伤害</param>
        /// <param name="damageType">伤害类型</param>
        /// <returns>最终伤害值</returns>
        public static float CalculateSkillDamage(Creature caster, Creature target, float baseDamage, string damageType)
        {
            if (caster == null || target == null) return 0f;

            // 计算基础伤害（考虑防御）
            float finalDamage = Math.Max(1f, baseDamage - target.Defense * 0.1f);

            // 检查弱点加成
            var weaknessMultiplier = 1.0f;
            if (target.Weaknesses != null)
            {
                var targetWeakness = target.Weaknesses.FirstOrDefault(w => w.Type.Equals(damageType, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(targetWeakness.Type))
                {
                    weaknessMultiplier = 1.3f; // 弱点伤害加成30%
                }
            }

            finalDamage *= weaknessMultiplier;

            // 添加随机波动（±10%）
            float randomMultiplier = RandRange(0.9f, 1.1f);
            finalDamage *= randomMultiplier;

            return Math.Max(0f, finalDamage);
        }
    }
}
