using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Modules.SkillSystem;

namespace hd2dtest.Scripts.Tools
{
    /// <summary>
    /// DPS（每秒伤害）计算器，用于模拟和计算技能的伤害输出
    /// </summary>
    public class DPSCalculator
    {
        /// <summary>
        /// 模拟结果结构体，包含总伤害、DPS 和持续时间
        /// </summary>
        public struct SimulationResult
        {
            /// <summary>
            /// 模拟期间的总伤害值
            /// </summary>
            public float TotalDamage;
            
            /// <summary>
            /// 每秒伤害（Damage Per Second）
            /// </summary>
            public float DPS;
            
            /// <summary>
            /// 模拟持续时间（秒）
            /// </summary>
            public float Duration;
        }

        /// <summary>
        /// 计算指定技能在给定时间内的 DPS
        /// </summary>
        /// <param name="attacker">攻击者生物，提供攻击力等属性</param>
        /// <param name="dummyTarget">目标生物，用于计算防御减免</param>
        /// <param name="skill">要模拟的技能数据</param>
        /// <param name="duration">模拟持续时间，默认为 60 秒</param>
        /// <returns>包含总伤害、DPS 和持续时间的 SimulationResult 结构体</returns>
        /// <remarks>
        /// 该方法通过循环模拟技能释放过程：
        /// 1. 遍历技能的所有阶段，计算每个阶段的伤害
        /// 2. 考虑防御减免：mitigation = Defense / (Defense + 100)
        /// 3. 累加所有伤害并计算平均 DPS
        /// 
        /// 注意：当前实现假设技能释放是瞬发的，未考虑技能动画时间
        /// </remarks>
        public static SimulationResult CalculateDPS(Creature attacker, Creature dummyTarget, SkillData skill, float duration = 60f)
        {
            float totalDamage = 0f;
            float currentTime = 0f;
            float skillCooldownTimer = 0f;

            // 简化模拟循环
            while (currentTime < duration)
            {
                // 处理冷却时间
                if (skillCooldownTimer <= 0)
                {
                    // 施放技能
                    // 假设技能释放是瞬发的，用于 DPS 计算或占用阶段持续时间
                    float skillExecutionTime = 0f;
                    foreach(var phase in skill.Phases)
                    {
                        skillExecutionTime += phase.Duration;
                        foreach(var evt in phase.Events)
                        {
                            if (evt is DamageSkillEvent dmgEvt)
                            {
                                float damage = attacker.Attack * dmgEvt.DamageMultiplier;
                                // 基础防御减免
                                float mitigation = dummyTarget.Defense / (dummyTarget.Defense + 100); 
                                damage *= 1.0f - mitigation;
                                totalDamage += damage;
                            }
                        }
                    }

                    // 添加执行时间
                    currentTime += skillExecutionTime;
                    // 开始冷却时间
                    skillCooldownTimer = skill.Cooldown;
                }
                else
                {
                    // 推进时间
                    float step = 0.1f;
                    currentTime += step;
                    skillCooldownTimer -= step;
                }
            }

            return new SimulationResult
            {
                TotalDamage = totalDamage,
                Duration = duration,
                DPS = totalDamage / duration
            };
        }
    }
}
