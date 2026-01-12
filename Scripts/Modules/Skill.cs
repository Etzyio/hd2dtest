using Godot;
using System;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 技能类
    /// </summary>
    public class Skill
    {
        // 技能类型枚举
        public enum SkillType
        {
            Attack,
            Defense,
            Support,
            Healing
        }

        /// <summary>
        /// 技能ID
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 技能名称Key（用于国际化）
        /// </summary>
        public string NameKey { get; set; } = "";

        /// <summary>
        /// 技能描述Key（用于国际化）
        /// </summary>
        public string DescriptionKey { get; set; } = "";

        /// <summary>
        /// 技能效果Key（用于国际化）
        /// </summary>
        public string EffectKey { get; set; } = "";

        // 技能属性
        /// <summary>
        /// 技能名称
        /// </summary>
        public string SkillName { get; set; } = "New Skill";

        /// <summary>
        /// 技能描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 技能类型
        /// </summary>
        public SkillType SkillTypeValue { get; set; } = SkillType.Attack;

        /// <summary>
        /// 技能类型字符串（用于JSON序列化）
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// 技能伤害
        /// </summary>
        public float Damage { get; set; } = 0f;

        /// <summary>
        /// 技能治疗量
        /// </summary>
        public float Healing { get; set; } = 0f;

        /// <summary>
        /// 技能防御加成
        /// </summary>
        public float DefenseBoost { get; set; } = 0f;

        /// <summary>
        /// 技能速度加成
        /// </summary>
        public float SpeedBoost { get; set; } = 0f;

        /// <summary>
        /// 技能持续时间
        /// </summary>
        public float Duration { get; set; } = 0f;

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        public float Cooldown { get; set; } = 1f;

        /// <summary>
        /// 技能魔法消耗
        /// </summary>
        public int ManaCost { get; set; } = 0;

        /// <summary>
        /// 技能所需等级
        /// </summary>
        public int RequiredLevel { get; set; } = 1;

        /// <summary>
        /// 技能伤害类型
        /// </summary>
        public string DamageType { get; set; } = "";

        /// <summary>
        /// 技能是否已解锁
        /// </summary>
        public bool IsUnlocked { get; set; } = false;

        // 技能状态
        /// <summary>
        /// 当前冷却时间
        /// </summary>
        private float _currentCooldown = 0f;

        /// <summary>
        /// 技能是否可用
        /// </summary>
        public bool IsAvailable => _currentCooldown <= 0f;

        /// <summary>
        /// 获取技能状态描述
        /// </summary>
        /// <returns>技能状态描述</returns>
        public string GetStatus()
        {
            if (!IsUnlocked)
            {
                return "Locked";
            }

            if (!IsAvailable)
            {
                return $"Cooldown: {_currentCooldown:F1}s";
            }

            return "Ready";
        }

        /// <summary>
        /// 获取技能信息
        /// </summary>
        /// <returns>技能信息</returns>
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
                   $"Required Level: {RequiredLevel} | Status: {GetStatus()}";
        }
    }
}