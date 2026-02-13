using System;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 武器类，定义游戏中武器的属性和行为
    /// </summary>
    /// <remarks>
    /// 武器类包含武器的基本属性、攻击参数、特效信息和装备状态
    /// 支持多种武器类型（剑、斧、弓、法杖、匕首、长矛）和攻击计算（基础伤害、暴击、攻击速度修正）
    /// </remarks>
    public class Weapon
    {
        /// <summary>
        /// 武器类型枚举
        /// </summary>
        public enum WeaponType
        {
            /// <summary>剑</summary>
            Sword,
            /// <summary>斧</summary>
            Axe,
            /// <summary>弓</summary>
            Bow,
            /// <summary>法杖</summary>
            Staff,
            /// <summary>匕首</summary>
            Dagger,
            /// <summary>长矛</summary>
            Spear
        }

        /// <summary>
        /// 武器ID
        /// </summary>
        /// <value>武器的唯一标识符</value>
        public string Id { get; set; } = "";

        /// <summary>
        /// 武器名称Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的武器名称键值</value>
        public string NameKey { get; set; } = "";

        /// <summary>
        /// 武器描述Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的武器描述键值</value>
        public string DescriptionKey { get; set; } = "";

        // 武器属性
        /// <summary>
        /// 武器名称
        /// </summary>
        /// <value>武器的显示名称</value>
        public string WeaponName { get; set; } = "New Weapon";

        /// <summary>
        /// 武器描述
        /// </summary>
        /// <value>武器的详细描述信息</value>
        public string Description { get; set; } = "";

        /// <summary>
        /// 武器类型
        /// </summary>
        /// <value>武器的类型枚举值</value>
        public WeaponType WeaponTypeValue { get; set; } = WeaponType.Sword;

        /// <summary>
        /// 武器类型字符串（用于JSON序列化）
        /// </summary>
        /// <value>武器类型的字符串表示，用于JSON序列化</value>
        public string Type { get; set; } = "";

        /// <summary>
        /// 武器子类型字符串（用于JSON序列化）
        /// </summary>
        /// <value>武器子类型的字符串表示，用于JSON序列化</value>
        public string SubType { get; set; } = "";

        /// <summary>
        /// 武器攻击力
        /// </summary>
        /// <value>武器的基础攻击力，默认值为10f</value>
        public float AttackPower { get; set; } = 10f;

        /// <summary>
        /// 武器攻击速度
        /// </summary>
        /// <value>武器的攻击速度倍率，默认值为1f</value>
        public float AttackSpeed { get; set; } = 1f;

        /// <summary>
        /// 武器攻击范围
        /// </summary>
        /// <value>武器的攻击范围，默认值为30f</value>
        public float Range { get; set; } = 30f;

        /// <summary>
        /// 武器暴击率
        /// </summary>
        /// <value>武器的暴击概率，默认值为0.05f（5%）</value>
        public float CriticalChance { get; set; } = 0.05f;

        /// <summary>
        /// 武器暴击伤害
        /// </summary>
        /// <value>武器的暴击伤害倍率，默认值为1.5f（150%）</value>
        public float CriticalDamage { get; set; } = 1.5f;

        /// <summary>
        /// 武器所需等级
        /// </summary>
        /// <value>使用该武器所需的玩家等级，默认值为1</value>
        public int RequiredLevel { get; set; } = 1;

        /// <summary>
        /// 武器价格
        /// </summary>
        /// <value>武器的购买价格，默认值为100</value>
        public int Price { get; set; } = 100;

        /// <summary>
        /// 武器是否已装备
        /// </summary>
        /// <value>true 表示武器已装备，false 表示武器未装备</value>
        public bool IsEquipped { get; set; } = false;

        // 武器特效
        /// <summary>
        /// 武器特效Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的武器特效键值</value>
        public string EffectKey { get; set; } = "";

        /// <summary>
        /// 武器特效名称
        /// </summary>
        /// <value>武器特效的显示名称</value>
        public string Effect { get; set; } = "";

        /// <summary>
        /// 武器特效触发几率
        /// </summary>
        /// <value>武器特效的触发概率，默认值为0f</value>
        public float EffectChance { get; set; } = 0f;

        /// <summary>
        /// 武器元素属性
        /// </summary>
        /// <value>武器的元素属性，如：火、水、雷、冰等</value>
        public string Element { get; set; } = "";

        /// <summary>
        /// 计算攻击伤害
        /// </summary>
        /// <param name="baseAttack">角色的基础攻击力</param>
        /// <returns>最终攻击力</returns>
        /// <remarks>
        /// 计算逻辑：基础攻击力 + 武器攻击力，然后应用攻击速度修正
        /// 攻击速度修正公式：1f + (攻击速度 - 1f) * 0.1f
        /// </remarks>
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
        /// <remarks>
        /// 生成一个随机数，与暴击率比较，判断是否触发暴击
        /// </remarks>
        public bool IsCriticalHit()
        {
            return DamageCalculator.Randf() <= CriticalChance;
        }

        /// <summary>
        /// 计算暴击伤害
        /// </summary>
        /// <param name="damage">基础伤害</param>
        /// <returns>暴击伤害</returns>
        /// <remarks>
        /// 暴击伤害 = 基础伤害 × 暴击伤害倍率
        /// </remarks>
        public float CalculateCriticalDamage(float damage)
        {
            return damage * CriticalDamage;
        }

        /// <summary>
        /// 装备武器
        /// </summary>
        /// <remarks>
        /// 将武器标记为已装备状态，并记录日志
        /// </remarks>
        public void Equip()
        {
            IsEquipped = true;
            Log.Info($"Equipped weapon: {WeaponName}");
        }

        /// <summary>
        /// 卸下武器
        /// </summary>
        /// <remarks>
        /// 将武器标记为未装备状态，并记录日志
        /// </remarks>
        public void Unequip()
        {
            IsEquipped = false;
            Log.Info($"Unequipped weapon: {WeaponName}");
        }

        /// <summary>
        /// 获取武器类型名称
        /// </summary>
        /// <returns>武器类型的字符串表示</returns>
        /// <remarks>
        /// 将武器类型枚举值转换为对应的字符串名称
        /// </remarks>
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
        /// <returns>包含武器详细信息的字符串</returns>
        /// <remarks>
        /// 格式化输出武器的名称、类型、描述、攻击力、攻击速度、攻击范围、暴击率、暴击伤害、所需等级、价格和装备状态
        /// </remarks>
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