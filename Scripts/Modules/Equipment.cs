using System;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 装备类
    /// </summary>
    public class Equipment
    {
        // 装备类型枚举
        public enum EquipmentType
        {
            Armor,
            Helmet,
            Boots,
            Gloves,
            Accessory,
            Shield
        }

        /// <summary>
        /// 装备ID
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 装备名称Key（用于国际化）
        /// </summary>
        public string NameKey { get; set; } = "";

        /// <summary>
        /// 装备描述Key（用于国际化）
        /// </summary>
        public string DescriptionKey { get; set; } = "";

        /// <summary>
        /// 装备名称
        /// </summary>
        public string EquipmentName { get; set; } = "New Equipment";

        /// <summary>
        /// 装备描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 装备类型
        /// </summary>
        public EquipmentType EquipmentTypeValue { get; set; } = EquipmentType.Armor;

        /// <summary>
        /// 装备类型字符串（用于JSON序列化）
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// 装备子类型字符串（用于JSON序列化）
        /// </summary>
        public string SubType { get; set; } = "";

        /// <summary>
        /// 装备防御力
        /// </summary>
        public float Defense { get; set; } = 5f;

        /// <summary>
        /// 装备生命值加成
        /// </summary>
        public float Health { get; set; } = 0f;

        /// <summary>
        /// 装备魔法值加成
        /// </summary>
        public float Mana { get; set; } = 0f;

        /// <summary>
        /// 装备速度加成
        /// </summary>
        public float Speed { get; set; } = 0f;

        /// <summary>
        /// 装备攻击力加成
        /// </summary>
        public float Attack { get; set; } = 0f;

        /// <summary>
        /// 装备价格
        /// </summary>
        public int Price { get; set; } = 100;

        /// <summary>
        /// 装备是否已装备
        /// </summary>
        public bool IsEquipped { get; set; } = false;

        /// <summary>
        /// 装备特效Key（用于国际化）
        /// </summary>
        public string EffectKey { get; set; } = "";

        /// <summary>
        /// 装备特效名称
        /// </summary>
        public string Effect { get; set; } = "";

        /// <summary>
        /// 装备特效值
        /// </summary>
        public float EffectValue { get; set; } = 0f;

        /// <summary>
        /// 装备元素属性
        /// </summary>
        public string Element { get; set; } = "";

        /// <summary>
        /// 装备装备
        /// </summary>
        public void Equip()
        {
            IsEquipped = true;
            Log.Info($"Equipped equipment: {EquipmentName}");
        }

        /// <summary>
        /// 卸下装备
        /// </summary>
        public void Unequip()
        {
            IsEquipped = false;
            Log.Info($"Unequipped equipment: {EquipmentName}");
        }

        /// <summary>
        /// 获取装备类型名称
        /// </summary>
        /// <returns>装备类型名称</returns>
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
        /// <returns>装备信息</returns>
        public string GetInfo()
        {
            string effectLine = string.Empty;
            if (!string.IsNullOrEmpty(Effect))
            {
                effectLine = $"Effect: {Effect} (+{EffectValue})\n";
            }

            return $"{EquipmentName} ({GetTypeName()})\n" +
                   $"Description: {Description}\n" +
                   $"Defense: {Defense:F0} | Health: {Health:F0} | Mana: {Mana:F0}\n" +
                   $"Speed: {Speed:F0}\n" +
                   effectLine +
                   $"Price: {Price} Gold\n" +
                   $"Equipped: {IsEquipped}";
        }

        /// <summary>
        /// 获取装备属性加成描述
        /// </summary>
        /// <returns>属性加成描述</returns>
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