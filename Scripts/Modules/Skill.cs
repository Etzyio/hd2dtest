using System;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
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
        /// 技能类型枚举
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
        /// 技能ID
        /// </summary>
        /// <value>技能的唯一标识符</value>
        public string Id { get; set; } = "";

        /// <summary>
        /// 技能名称Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的技能名称键值</value>
        public string NameKey { get; set; } = "";

        /// <summary>
        /// 技能描述Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的技能描述键值</value>
        public string DescriptionKey { get; set; } = "";

        /// <summary>
        /// 技能效果Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的技能效果键值</value>
        public string EffectKey { get; set; } = "";

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
        /// 技能类型
        /// </summary>
        /// <value>技能的类型枚举值</value>
        public SkillType SkillTypeValue { get; set; } = SkillType.Attack;

        /// <summary>
        /// 技能类型字符串（用于JSON序列化）
        /// </summary>
        /// <value>技能类型的字符串表示，用于JSON序列化</value>
        public string Type { get; set; } = "";

        /// <summary>
        /// 技能伤害
        /// </summary>
        /// <value>技能的伤害值</value>
        public float Damage { get; set; } = 0f;

        /// <summary>
        /// 技能治疗量
        /// </summary>
        /// <value>技能的治疗值</value>
        public float Healing { get; set; } = 0f;

        /// <summary>
        /// 技能防御加成
        /// </summary>
        /// <value>技能提供的防御加成值</value>
        public float DefenseBoost { get; set; } = 0f;

        /// <summary>
        /// 技能速度加成
        /// </summary>
        /// <value>技能提供的速度加成值</value>
        public float SpeedBoost { get; set; } = 0f;

        /// <summary>
        /// 技能持续时间
        /// </summary>
        /// <value>技能效果的持续时间，单位为秒</value>
        public float Duration { get; set; } = 0f;

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        /// <value>技能的冷却时间，单位为秒</value>
        public float Cooldown { get; set; } = 1f;

        /// <summary>
        /// 技能魔法消耗
        /// </summary>
        /// <value>使用技能所需的魔法值</value>
        public int ManaCost { get; set; } = 0;

        /// <summary>
        /// 技能伤害类型
        /// </summary>
        /// <value>技能的伤害类型，如：物理、魔法、火、水、雷、冰等</value>
        public Weakness DamageType { get; set; } = new Weakness("Physical");

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
            string typeStr = SkillTypeValue switch
            {
                SkillType.Attack => "Attack",
                SkillType.Defense => "Defense",
                SkillType.Support => "Support",
                SkillType.Healing => "Healing",
                _ => "Unknown"
            };

            return $"{SkillName} ({typeStr})\n" +
                   $"Description: {Description}\n" +
                   $"Damage: {Damage:F1} | Healing: {Healing:F1}\n" +
                   $"Cooldown: {Cooldown:F1}s | Mana Cost: {ManaCost}\n" +
                   $"Damage Type: {DamageType} | Status: {GetStatus()}";
        }
    }
}