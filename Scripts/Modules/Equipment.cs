using System;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 装备类，定义游戏中所有装备的属性和行为
    /// </summary>
    /// <remarks>
    /// 装备类包含基础属性、特效属性和装备行为，支持装备/卸下、特效触发等功能
    /// </remarks>
    public class Equipment
    {
        /// <summary>
        /// 装备类型枚举
        /// </summary>
        public enum EquipmentType
        {
            /// <summary>盔甲</summary>
            Armor,
            /// <summary>头盔</summary>
            Helmet,
            /// <summary>靴子</summary>
            Boots,
            /// <summary>手套</summary>
            Gloves,
            /// <summary>饰品</summary>
            Accessory,
            /// <summary>盾牌</summary>
            Shield
        }

        /// <summary>
        /// 装备ID
        /// </summary>
        /// <value>装备的唯一标识符</value>
        public string Id { get; set; } = "";

        /// <summary>
        /// 装备名称Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的装备名称键值</value>
        public string NameKey { get; set; } = "";

        /// <summary>
        /// 装备描述Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的装备描述键值</value>
        public string DescriptionKey { get; set; } = "";

        /// <summary>
        /// 装备名称
        /// </summary>
        /// <value>装备的显示名称</value>
        public string EquipmentName { get; set; } = "New Equipment";

        /// <summary>
        /// 装备描述
        /// </summary>
        /// <value>装备的详细描述信息</value>
        public string Description { get; set; } = "";

        /// <summary>
        /// 装备类型
        /// </summary>
        /// <value>装备的类型枚举值</value>
        public EquipmentType EquipmentTypeValue { get; set; } = EquipmentType.Armor;

        /// <summary>
        /// 装备类型字符串（用于JSON序列化）
        /// </summary>
        /// <value>装备类型的字符串表示，用于JSON序列化</value>
        public string Type { get; set; } = "";

        /// <summary>
        /// 装备子类型字符串（用于JSON序列化）
        /// </summary>
        /// <value>装备子类型的字符串表示，用于JSON序列化</value>
        public string SubType { get; set; } = "";

        /// <summary>
        /// 装备防御力
        /// </summary>
        /// <value>装备提供的防御力加成，默认值为5</value>
        public float Defense { get; set; } = 5f;

        /// <summary>
        /// 装备生命值加成
        /// </summary>
        /// <value>装备提供的生命值加成，默认值为0</value>
        public float Health { get; set; } = 0f;

        /// <summary>
        /// 装备魔法值加成
        /// </summary>
        /// <value>装备提供的魔法值加成，默认值为0</value>
        public float Mana { get; set; } = 0f;

        /// <summary>
        /// 装备速度加成
        /// </summary>
        /// <value>装备提供的速度加成，默认值为0</value>
        public float Speed { get; set; } = 0f;

        /// <summary>
        /// 装备攻击力加成
        /// </summary>
        /// <value>装备提供的攻击力加成，默认值为0</value>
        public float Attack { get; set; } = 0f;

        /// <summary>
        /// 装备价格
        /// </summary>
        /// <value>装备的购买价格，默认值为100</value>
        public int Price { get; set; } = 100;

        /// <summary>
        /// 装备是否已装备
        /// </summary>
        /// <value>true 表示装备已装备，false 表示装备未装备</value>
        public bool IsEquipped { get; set; } = false;

        /// <summary>
        /// 装备特效Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的装备特效键值</value>
        public string EffectKey { get; set; } = "";

        /// <summary>
        /// 装备特效名称
        /// </summary>
        /// <value>装备特效的显示名称</value>
        public string Effect { get; set; } = "";

        /// <summary>
        /// 装备特效值
        /// </summary>
        /// <value>装备特效的具体数值</value>
        public float EffectValue { get; set; } = 0f;

        /// <summary>
        /// 装备元素属性
        /// </summary>
        /// <value>装备的元素属性，如：火、水、雷、冰等</value>
        public string Element { get; set; } = "";

        /// <summary>
        /// 装备特效类型
        /// </summary>
        /// <value>装备特效的类型，如：WeaponRestriction、DamageBoost、EnemyInterference等</value>
        public string EffectType { get; set; } = "";

        /// <summary>
        /// 装备特效持续时间（秒）
        /// </summary>
        /// <value>装备特效的持续时间，单位为秒，默认值为0</value>
        public float EffectDuration { get; set; } = 0f;

        /// <summary>
        /// 装备特效触发概率（0-1）
        /// </summary>
        /// <value>装备特效的触发概率，范围为0到1，默认值为1</value>
        public float EffectChance { get; set; } = 1f;

        /// <summary>
        /// 限制武器类型
        /// </summary>
        /// <value>当装备特效类型为WeaponRestriction时，限制使用的武器类型</value>
        public string RestrictedWeaponType { get; set; } = "";

        /// <summary>
        /// 伤害增强类型
        /// </summary>
        /// <value>当装备特效类型为DamageBoost时，增强的伤害类型</value>
        public string DamageBoostType { get; set; } = "";

        /// <summary>
        /// 敌对干扰目标装备类型
        /// </summary>
        /// <value>当装备特效类型为EnemyInterference时，干扰的敌人装备类型</value>
        public string EnemyInterferenceType { get; set; } = "";

        /// <summary>
        /// 装备装备
        /// </summary>
        /// <remarks>
        /// 将装备标记为已装备状态，记录日志，并触发装备特效
        /// </remarks>
        public void Equip()
        {
            IsEquipped = true;
            Log.Info($"Equipped equipment: {EquipmentName}");
            
            // 触发装备特效
            TriggerEquipEffect();
        }

        /// <summary>
        /// 卸下装备
        /// </summary>
        /// <remarks>
        /// 将装备标记为未装备状态，记录日志，并移除装备特效
        /// </remarks>
        public void Unequip()
        {
            IsEquipped = false;
            Log.Info($"Unequipped equipment: {EquipmentName}");
            
            // 移除装备特效
            RemoveEquipEffect();
        }

        /// <summary>
        /// 触发装备特效
        /// </summary>
        /// <remarks>
        /// 根据装备特效类型触发相应的特效，并记录日志
        /// 支持的特效类型：WeaponRestriction、DamageBoost、EnemyInterference
        /// </remarks>
        private void TriggerEquipEffect()
        {
            switch (EffectType)
            {
                case "WeaponRestriction":
                    Log.Info($"Weapon restriction effect triggered: {RestrictedWeaponType}");
                    break;
                case "DamageBoost":
                    Log.Info($"Damage boost effect triggered: +{EffectValue}% {DamageBoostType} damage");
                    break;
                case "EnemyInterference":
                    Log.Info($"Enemy interference effect triggered: {EnemyInterferenceType}");
                    break;
            }
        }

        /// <summary>
        /// 移除装备特效
        /// </summary>
        /// <remarks>
        /// 根据装备特效类型移除相应的特效，并记录日志
        /// 支持的特效类型：WeaponRestriction、DamageBoost、EnemyInterference
        /// </remarks>
        private void RemoveEquipEffect()
        {
            switch (EffectType)
            {
                case "WeaponRestriction":
                    Log.Info($"Weapon restriction effect removed: {RestrictedWeaponType}");
                    break;
                case "DamageBoost":
                    Log.Info($"Damage boost effect removed: +{EffectValue}% {DamageBoostType} damage");
                    break;
                case "EnemyInterference":
                    Log.Info($"Enemy interference effect removed: {EnemyInterferenceType}");
                    break;
            }
        }

        /// <summary>
        /// 检查是否可以使用特定武器
        /// </summary>
        /// <param name="weaponType">武器类型</param>
        /// <returns>是否可以使用该武器</returns>
        /// <remarks>
        /// 如果装备有武器限制特效且已装备，则只能使用指定类型的武器
        /// 否则可以使用任何武器
        /// </remarks>
        public bool CanUseWeapon(string weaponType)
        {
            if (EffectType == "WeaponRestriction" && IsEquipped)
            {
                return weaponType == RestrictedWeaponType;
            }
            return true;
        }

        /// <summary>
        /// 计算伤害增强
        /// </summary>
        /// <param name="baseDamage">基础伤害</param>
        /// <param name="damageType">伤害类型</param>
        /// <returns>增强后的伤害值</returns>
        /// <remarks>
        /// 如果装备有伤害增强特效且已装备，并且伤害类型匹配，则根据特效值增强伤害
        /// 增强公式：baseDamage * (1 + EffectValue / 100)
        /// </remarks>
        public float CalculateEnhancedDamage(float baseDamage, string damageType)
        {
            if (EffectType == "DamageBoost" && IsEquipped && damageType == DamageBoostType)
            {
                float boostFactor = 1f + (EffectValue / 100f);
                return baseDamage * boostFactor;
            }
            return baseDamage;
        }

        /// <summary>
        /// 检查是否可以触发敌对干扰效果
        /// </summary>
        /// <param name="enemyEquipmentType">敌人装备类型</param>
        /// <returns>是否可以触发敌对干扰效果</returns>
        /// <remarks>
        /// 如果装备有敌对干扰特效且已装备，并且敌人装备类型匹配，则可以触发干扰效果
        /// </remarks>
        public bool CanTriggerEnemyInterference(string enemyEquipmentType)
        {
            if (EffectType == "EnemyInterference" && IsEquipped)
            {
                return enemyEquipmentType == EnemyInterferenceType;
            }
            return false;
        }

        /// <summary>
        /// 获取装备类型名称
        /// </summary>
        /// <returns>装备类型的字符串表示</returns>
        /// <remarks>
        /// 将装备类型枚举值转换为对应的字符串名称
        /// </remarks>
        public string GetTypeName()
        {
            return EquipmentTypeValue switch
            {
                EquipmentType.Armor => "Armor",
                EquipmentType.Helmet => "Helmet",
                EquipmentType.Boots => "Boots",
                EquipmentType.Gloves => "Gloves",
                EquipmentType.Accessory => "Accessory",
                EquipmentType.Shield => "Shield",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 获取装备信息
        /// </summary>
        /// <returns>包含装备详细信息的字符串</returns>
        /// <remarks>
        /// 格式化输出装备的名称、类型、描述、属性、特效等信息
        /// </remarks>
        public string GetInfo()
        {
            string effectLine = string.Empty;
            if (!string.IsNullOrEmpty(Effect))
            {
                effectLine = $"Effect: {Effect} (+{EffectValue})\n";
            }

            string specialEffectLine = string.Empty;
            if (!string.IsNullOrEmpty(EffectType))
            {
                switch (EffectType)
                {
                    case "WeaponRestriction":
                        specialEffectLine = $"Special Effect: Weapon Restriction ({RestrictedWeaponType})\n";
                        break;
                    case "DamageBoost":
                        specialEffectLine = $"Special Effect: Damage Boost (+{EffectValue}% {DamageBoostType})\n";
                        break;
                    case "EnemyInterference":
                        specialEffectLine = $"Special Effect: Enemy Interference ({EnemyInterferenceType})\n";
                        break;
                }
            }

            return $"{EquipmentName} ({GetTypeName()})\n" +
                   $"Description: {Description}\n" +
                   $"Defense: {Defense:F0} | Health: {Health:F0} | Mana: {Mana:F0}\n" +
                   $"Speed: {Speed:F0}\n" +
                   effectLine +
                   specialEffectLine +
                   $"Price: {Price} Gold\n" +
                   $"Equipped: {IsEquipped}";
        }

        /// <summary>
        /// 获取装备属性加成描述
        /// </summary>
        /// <returns>装备属性加成的字符串描述</returns>
        /// <remarks>
        /// 生成装备所有正属性加成的格式化描述字符串
        /// </remarks>
        public string GetBonusDescription()
        {
            string bonuses = string.Empty;

            if (Defense > 0)
            {
                bonuses += $"+{Defense:F0} Defense ";
            }

            if (Health > 0)
            {
                bonuses += $"+{Health:F0} Health ";
            }

            if (Mana > 0)
            {
                bonuses += $"+{Mana:F0} Mana ";
            }

            if (Speed > 0)
            {
                bonuses += $"+{Speed:F0} Speed ";
            }

            return bonuses.Trim();
        }
    }
}