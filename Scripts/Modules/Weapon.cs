using Godot;
using System;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 武器类
    /// </summary>
    public class Weapon
    {
        // 武器类型枚举
        public enum WeaponType
        {
            Sword,
            Axe,
            Bow,
            Staff,
            Dagger,
            Spear
        }
        
        /// <summary>
        /// 武器ID
        /// </summary>
        public string Id { get; set; } = "";
        
        /// <summary>
        /// 武器名称Key（用于国际化）
        /// </summary>
        public string NameKey { get; set; } = "";
        
        /// <summary>
        /// 武器描述Key（用于国际化）
        /// </summary>
        public string DescriptionKey { get; set; } = "";
        
        // 武器属性
        /// <summary>
        /// 武器名称
        /// </summary>
        public string WeaponName { get; set; } = "New Weapon";
        
        /// <summary>
        /// 武器描述
        /// </summary>
        public string Description { get; set; } = "";
        
        /// <summary>
        /// 武器类型
        /// </summary>
        public WeaponType WeaponTypeValue { get; set; } = WeaponType.Sword;
        
        /// <summary>
        /// 武器类型字符串（用于JSON序列化）
        /// </summary>
        public string Type { get; set; } = "";
        
        /// <summary>
        /// 武器子类型字符串（用于JSON序列化）
        /// </summary>
        public string SubType { get; set; } = "";
        
        /// <summary>
        /// 武器攻击力
        /// </summary>
        public float AttackPower { get; set; } = 10f;
        
        /// <summary>
        /// 武器攻击速度
        /// </summary>
        public float AttackSpeed { get; set; } = 1f;
        
        /// <summary>
        /// 武器攻击范围
        /// </summary>
        public float Range { get; set; } = 30f;
        
        /// <summary>
        /// 武器暴击率（5%）
        /// </summary>
        public float CriticalChance { get; set; } = 0.05f;
        
        /// <summary>
        /// 武器暴击伤害（150%）
        /// </summary>
        public float CriticalDamage { get; set; } = 1.5f;
        
        /// <summary>
        /// 武器所需等级
        /// </summary>
        public int RequiredLevel { get; set; } = 1;
        
        /// <summary>
        /// 武器价格
        /// </summary>
        public int Price { get; set; } = 100;
        
        /// <summary>
        /// 武器是否已装备
        /// </summary>
        public bool IsEquipped { get; set; } = false;
        
        // 武器特效
        /// <summary>
        /// 武器特效Key（用于国际化）
        /// </summary>
        public string EffectKey { get; set; } = "";
        
        /// <summary>
        /// 武器特效名称
        /// </summary>
        public string Effect { get; set; } = "";
        
        /// <summary>
        /// 武器特效触发几率
        /// </summary>
        public float EffectChance { get; set; } = 0f;
        
        /// <summary>
        /// 武器元素属性
        /// </summary>
        public string Element { get; set; } = "";
        
        /// <summary>
        /// 计算攻击伤害
        /// </summary>
        /// <param name="baseAttack">基础攻击力</param>
        /// <returns>最终攻击力</returns>
        public float CalculateAttackDamage(float baseAttack)
        {
            // 基础攻击伤害 = 基础攻击力 + 武器攻击力
            float baseDamage = baseAttack + AttackPower;
            
            // 应用攻击速度修正（简化处理）
            float attackSpeedMultiplier = 1f + (AttackSpeed - 1f) * 0.1f;
            
            // 计算最终伤害
            float finalDamage = baseDamage * attackSpeedMultiplier;
            
            return finalDamage;
        }
        
        /// <summary>
        /// 检查是否暴击
        /// </summary>
        /// <returns>是否暴击</returns>
        public bool IsCriticalHit()
        {
            return GD.Randf() <= CriticalChance;
        }
        
        /// <summary>
        /// 计算暴击伤害
        /// </summary>
        /// <param name="damage">基础伤害</param>
        /// <returns>暴击伤害</returns>
        public float CalculateCriticalDamage(float damage)
        {
            return damage * CriticalDamage;
        }
        
        /// <summary>
        /// 装备武器
        /// </summary>
        public void Equip()
        {
            IsEquipped = true;
            Log.Info($"Equipped weapon: {WeaponName}");
        }
        
        /// <summary>
        /// 卸下武器
        /// </summary>
        public void Unequip()
        {
            IsEquipped = false;
            Log.Info($"Unequipped weapon: {WeaponName}");
        }
        
        /// <summary>
        /// 获取武器类型名称
        /// </summary>
        /// <returns>武器类型名称</returns>
        public string GetTypeName()
        {
            return WeaponTypeValue switch
            {
                WeaponType.Sword => "Sword",
                WeaponType.Axe => "Axe",
                WeaponType.Bow => "Bow",
                WeaponType.Staff => "Staff",
                WeaponType.Dagger => "Dagger",
                WeaponType.Spear => "Spear",
                _ => "Unknown"
            };
        }
        
        /// <summary>
        /// 获取武器信息
        /// </summary>
        /// <returns>武器信息</returns>
        public string GetInfo()
        {
            return $"{WeaponName} ({GetTypeName()})\n" +
                   $"Description: {Description}\n" +
                   $"Attack: {AttackPower:F0} | Speed: {AttackSpeed:F1} | Range: {Range:F0}\n" +
                   $"Crit Chance: {CriticalChance * 100:F0}% | Crit Damage: {CriticalDamage * 100:F0}%\n" +
                   $"Required Level: {RequiredLevel} | Price: {Price} Gold\n" +
                   $"Equipped: {IsEquipped}";
        }
    }
}