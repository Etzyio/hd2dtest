using System;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 道具类
    /// </summary>
    public class Item
    {
        // 道具类型枚举
        public enum ItemType
        {
            Consumable,     // 消耗品
            Material,       // 材料
            KeyItem,        // 关键物品
            QuestItem,      // 任务物品
            Miscellaneous   // 杂项
        }

        // 消耗品类型枚举
        public enum ConsumableType
        {
            HealthPotion,   // 生命药水
            ManaPotion,     // 魔法药水
            StatusPotion,   // 状态药水
            Food,           // 食物
            Scroll          // 卷轴
        }

        /// <summary>
        /// 道具ID
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 道具名称Key（用于国际化）
        /// </summary>
        public string NameKey { get; set; } = "";

        /// <summary>
        /// 道具描述Key（用于国际化）
        /// </summary>
        public string DescriptionKey { get; set; } = "";

        /// <summary>
        /// 道具名称
        /// </summary>
        public string ItemName { get; set; } = "New Item";

        /// <summary>
        /// 道具描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 道具类型
        /// </summary>
        public ItemType ItemTypeValue { get; set; } = ItemType.Miscellaneous;

        /// <summary>
        /// 消耗品类型（仅当ItemType为Consumable时有效）
        /// </summary>
        public ConsumableType ConsumableTypeValue { get; set; } = ConsumableType.HealthPotion;

        /// <summary>
        /// 道具类型字符串（用于JSON序列化）
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// 道具子类型字符串（用于JSON序列化）
        /// </summary>
        public string SubType { get; set; } = "";

        /// <summary>
        /// 道具价格
        /// </summary>
        public int Price { get; set; } = 10;

        /// <summary>
        /// 道具堆叠数量
        /// </summary>
        public int StackCount { get; set; } = 1;

        /// <summary>
        /// 最大堆叠数量
        /// </summary>
        public int MaxStackCount { get; set; } = 99;

        /// <summary>
        /// 道具效果值（如药水恢复量）
        /// </summary>
        public float EffectValue { get; set; } = 0f;

        /// <summary>
        /// 道具效果持续时间（秒）
        /// </summary>
        public float EffectDuration { get; set; } = 0f;

        /// <summary>
        /// 是否可使用
        /// </summary>
        public bool IsUsable { get; set; } = false;

        /// <summary>
        /// 是否可出售
        /// </summary>
        public bool IsSellable { get; set; } = true;

        /// <summary>
        /// 是否可丢弃
        /// </summary>
        public bool IsDroppable { get; set; } = true;

        /// <summary>
        /// 使用道具
        /// </summary>
        /// <param name="user">使用道具的生物</param>
        /// <returns>是否使用成功</returns>
        public bool Use(Creature user)
        {
            if (!IsUsable || user == null || !user.IsAlive)
            {
                return false;
            }

            bool result = false;

            switch (ItemTypeValue)
            {
                case ItemType.Consumable:
                    result = UseConsumable(user);
                    break;
                case ItemType.KeyItem:
                    result = UseKeyItem(user);
                    break;
                case ItemType.QuestItem:
                    result = UseQuestItem(user);
                    break;
                default:
                    Log.Info($"{ItemName} cannot be used.");
                    break;
            }

            if (result && ItemTypeValue == ItemType.Consumable)
            {
                // 消耗品使用后减少数量
                StackCount--;
            }

            return result;
        }

        /// <summary>
        /// 使用消耗品
        /// </summary>
        /// <param name="user">使用道具的生物</param>
        /// <returns>是否使用成功</returns>
        private bool UseConsumable(Creature user)
        {
            switch (ConsumableTypeValue)
            {
                case ConsumableType.HealthPotion:
                    float healAmount = EffectValue;
                    user.Health = Math.Min(user.Health + healAmount, user.MaxHealth);
                    Log.Info($"Used {ItemName}, recovered {healAmount:F0} HP.");
                    return true;
                case ConsumableType.ManaPotion:
                    Log.Info($"Used {ItemName}, but mana system not implemented yet.");
                    return true;
                case ConsumableType.StatusPotion:
                    Log.Info($"Used {ItemName}, but status system not implemented yet.");
                    return true;
                case ConsumableType.Food:
                    float foodHeal = EffectValue;
                    user.Health = Math.Min(user.Health + foodHeal, user.MaxHealth);
                    Log.Info($"Ate {ItemName}, recovered {foodHeal:F0} HP.");
                    return true;
                case ConsumableType.Scroll:
                    Log.Info($"Used {ItemName}, but scroll system not implemented yet.");
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 使用关键物品
        /// </summary>
        /// <param name="user">使用道具的生物</param>
        /// <returns>是否使用成功</returns>
        private bool UseKeyItem(Creature user)
        {
            Log.Info($"Used key item: {ItemName}, but key item system not implemented yet.");
            return true;
        }

        /// <summary>
        /// 使用任务物品
        /// </summary>
        /// <param name="user">使用道具的生物</param>
        /// <returns>是否使用成功</returns>
        private bool UseQuestItem(Creature user)
        {
            Log.Info($"Used quest item: {ItemName}, but quest system not implemented yet.");
            return true;
        }

        /// <summary>
        /// 堆叠物品
        /// </summary>
        /// <param name="other">要堆叠的物品</param>
        /// <returns>堆叠后剩余的数量</returns>
        public int Stack(Item other)
        {
            if (other == null || other.Id != this.Id || !other.IsStackable() || !this.IsStackable())
            {
                return other?.StackCount ?? 0;
            }

            int availableSpace = MaxStackCount - StackCount;
            int stackAmount = Math.Min(availableSpace, other.StackCount);

            StackCount += stackAmount;
            other.StackCount -= stackAmount;

            return other.StackCount;
        }

        /// <summary>
        /// 拆分物品
        /// </summary>
        /// <param name="amount">要拆分的数量</param>
        /// <returns>拆分出的新物品</returns>
        public Item Split(int amount)
        {
            if (amount <= 0 || amount >= StackCount)
            {
                return null;
            }

            Item newItem = new Item
            {
                Id = this.Id,
                NameKey = this.NameKey,
                DescriptionKey = this.DescriptionKey,
                ItemName = this.ItemName,
                Description = this.Description,
                ItemTypeValue = this.ItemTypeValue,
                ConsumableTypeValue = this.ConsumableTypeValue,
                Type = this.Type,
                SubType = this.SubType,
                Price = this.Price,
                StackCount = amount,
                MaxStackCount = this.MaxStackCount,
                EffectValue = this.EffectValue,
                EffectDuration = this.EffectDuration,
                IsUsable = this.IsUsable,
                IsSellable = this.IsSellable,
                IsDroppable = this.IsDroppable
            };

            this.StackCount -= amount;

            return newItem;
        }

        /// <summary>
        /// 检查物品是否可堆叠
        /// </summary>
        /// <returns>是否可堆叠</returns>
        public bool IsStackable()
        {
            return MaxStackCount > 1;
        }

        /// <summary>
        /// 获取道具类型名称
        /// </summary>
        /// <returns>道具类型名称</returns>
        public string GetTypeName()
        {
            return ItemTypeValue switch
            {
                ItemType.Consumable => "Consumable",
                ItemType.Material => "Material",
                ItemType.KeyItem => "Key Item",
                ItemType.QuestItem => "Quest Item",
                ItemType.Miscellaneous => "Miscellaneous",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 获取消耗品类型名称
        /// </summary>
        /// <returns>消耗品类型名称</returns>
        public string GetConsumableTypeName()
        {
            return ConsumableTypeValue switch
            {
                ConsumableType.HealthPotion => "Health Potion",
                ConsumableType.ManaPotion => "Mana Potion",
                ConsumableType.StatusPotion => "Status Potion",
                ConsumableType.Food => "Food",
                ConsumableType.Scroll => "Scroll",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 获取道具信息
        /// </summary>
        /// <returns>道具信息</returns>
        public string GetInfo()
        {
            string typeInfo = ItemTypeValue == ItemType.Consumable ? 
                $" ({GetTypeName()} - {GetConsumableTypeName()})" : 
                $" ({GetTypeName()})";

            string effectLine = string.Empty;
            if (EffectValue > 0)
            {
                effectLine = ItemTypeValue == ItemType.Consumable ? 
                    $"Effect: +{EffectValue:F0} " + 
                    (ConsumableTypeValue == ConsumableType.HealthPotion ? "HP" : 
                     ConsumableTypeValue == ConsumableType.ManaPotion ? "MP" : "") + "\n" : 
                    $"Effect: +{EffectValue:F0}\n";
            }

            string durationLine = string.Empty;
            if (EffectDuration > 0)
            {
                durationLine = $"Duration: {EffectDuration:F1}s\n";
            }

            return $"{ItemName}{typeInfo}\n" +
                   $"Description: {Description}\n" +
                   effectLine +
                   durationLine +
                   $"Price: {Price} Gold\n" +
                   $"Stack: {StackCount}/{MaxStackCount}\n" +
                   $"Usable: {IsUsable} | Sellable: {IsSellable} | Droppable: {IsDroppable}";
        }
    }
}