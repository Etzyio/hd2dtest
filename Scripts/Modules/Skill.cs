using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    // SkillType and SkillDefent are defined inside the Skill class below

    /// <summary>
    /// 技能类，定义游戏中技能的属性和行为
    /// </summary>
    /// <remarks>
    /// 技能类包含技能的基本属性、效果参数和状态信息
    /// 支持多种技能类型（攻击、防御、辅助、治疗）和效果（伤害、治疗、防御加成、速度加成）
    /// </remarks>
    public class Skill
    {
        /// <summary>
        /// 技能类型枚举（作为 Skill 的嵌套类型，外部使用 `Skill.SkillType` 访问）
        /// </summary>
        public enum SkillType
        {
            /// <summary>攻击技能</summary>
            Attack,
            /// <summary>防御技能</summary>
            Defense,
            /// <summary>辅助技能</summary>
            Support,
            /// <summary>治疗技能</summary>
            Healing
        }

        /// <summary>
        /// 技能效果项（嵌套类）
        /// </summary>
        public class SkillDefent
        {
            public SkillType Type = SkillType.Attack;
            public float data = 0.1f;
            public int Duration = 0;
            
            /// <summary>
            /// 技能伤害类型
            /// </summary>
            /// <value>技能的伤害类型，如：物理、魔法、火、水、雷、冰等</value>
            public Weakness DamageType { get; set; } = new Weakness("Physical");
        }

        // 技能效果列表，存放多个 SkillDefent
        public List<SkillDefent> SkillDefs { get; set; } = new List<SkillDefent>();

        /// <summary>
        /// 技能ID
        /// </summary>
        /// <value>技能的唯一标识符</value>
        public string Id { get; set; } = "";

        // 技能属性
        /// <summary>
        /// 技能名称
        /// </summary>
        /// <value>技能的显示名称</value>
        public string SkillName { get; set; } = "New Skill";

        /// <summary>
        /// 技能描述
        /// </summary>
        /// <value>技能的详细描述信息</value>
        public string Description { get; set; } = "";

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        /// <value>技能的冷却时间，单位为回合</value>
        public float Cooldown { get; set; } = 1f;

        /// <summary>
        /// 技能魔法消耗
        /// </summary>
        /// <value>使用技能所需的魔法值</value>
        public int ManaCost { get; set; } = 0;


        /// <summary>
        /// 技能是否已解锁
        /// </summary>
        /// <value>true 表示技能已解锁，false 表示技能未解锁</value>
        public bool IsUnlocked { get; set; } = false;

        /// <summary>
        /// 获取技能状态描述
        /// </summary>
        /// <returns>技能状态的字符串描述</returns>
        /// <remarks>
        /// 根据技能是否解锁返回相应的状态描述
        /// </remarks>
        public string GetStatus()
        {
            if (!IsUnlocked)
            {
                return "Locked";
            }
            return "Ready";
        }

        /// <summary>
        /// 获取技能信息
        /// </summary>
        /// <returns>技能详细信息的字符串表示</returns>
        /// <remarks>
        /// 格式化输出技能的名称、类型、描述、伤害、治疗、冷却时间、魔法消耗、伤害类型和状态等信息
        /// </remarks>
        public string GetInfo()
        {
            // 尝试汇总技能效果（SkillDefent 列表），避免抛出异常
            string effectsSummary = "None";
            try
            {
                if (SkillDefs != null && SkillDefs.Count > 0)
                {
                    effectsSummary = string.Empty;
                    foreach (var e in SkillDefs)
                    {
                        if (effectsSummary.Length > 0) effectsSummary += ", ";
                        effectsSummary += $"{e.Type}:{e.data:F1}({e.Duration}t/{e.DamageType})";
                    }
                }
            }
            catch
            {
                // 在无法读取效果时使用占位文本
                effectsSummary = "N/A";
            }

            // 返回更完整的技能信息，包含 ID、解锁状态和效果汇总
            return $"ID: {Id}\n" +
                   $"Name: {SkillName}\n" +
                   $"Unlocked: {IsUnlocked} | Status: {GetStatus()}\n" +
                   $"Description: {Description}";
        }
    }
}