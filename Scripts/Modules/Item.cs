using System;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 道具类，定义游戏中所有道具的属性和行为
    /// </summary>
    /// <remarks>
    /// 道具类包含基础属性、类型信息和使用行为，支持道具使用、堆叠、拆分等功能
    /// </remarks>
    public class Item
    {
        /// <summary>
        /// 道具类型枚举
        /// </summary>
        public enum ItemType
        {
            /// <summary>消耗品</summary>
            Consumable,
            /// <summary>材料</summary>
            Material,
            /// <summary>关键物品</summary>
            KeyItem,
            /// <summary>任务物品</summary>
            QuestItem,
            /// <summary>杂项</summary>
            Miscellaneous
        }

        /// <summary>
        /// 消耗品类型枚举
        /// </summary>
        public enum ConsumableType
        {
            /// <summary>生命药水</summary>
            HealthPotion,
            /// <summary>魔法药水</summary>
            ManaPotion,
            /// <summary>状态药水</summary>
            StatusPotion,
            /// <summary>食物</summary>
            Food,
            /// <summary>卷轴</summary>
            Scroll
        }

        /// <summary>
        /// 道具ID
        /// </summary>
        /// <value>道具的唯一标识符</value>
        public string Id { get; set; } = "";

        /// <summary>
        /// 道具名称Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的道具名称键值</value>
        public string NameKey { get; set; } = "";

        /// <summary>
        /// 道具描述Key（用于国际化）
        /// </summary>
        /// <value>用于国际化的道具描述键值</value>
        public string DescriptionKey { get; set; } = "";

        /// <summary>
        /// 道具名称
        /// </summary>
        /// <value>道具的显示名称</value>
        public string ItemName { get; set; } = "New Item";

        /// <summary>
        /// 道具描述
        /// </summary>
        /// <value>道具的详细描述信息</value>
        public string Description { get; set; } = "";

        /// <summary>
        /// 道具类型
        /// </summary>
        /// <value>道具的类型枚举值</value>
        public ItemType ItemTypeValue { get; set; } = ItemType.Miscellaneous;

        /// <summary>
        /// 消耗品类型（仅当ItemType为Consumable时有效）
        /// </summary>
        /// <value>消耗品的具体类型枚举值</value>
        public ConsumableType ConsumableTypeValue { get; set; } = ConsumableType.HealthPotion;

        /// <summary>
        /// 道具类型字符串（用于JSON序列化）
        /// </summary>
        /// <value>道具类型的字符串表示，用于JSON序列化</value>
        public string Type { get; set; } = "";

        /// <summary>
        /// 道具子类型字符串（用于JSON序列化）
        /// </summary>
        /// <value>道具子类型的字符串表示，用于JSON序列化</value>
        public string SubType { get; set; } = "";

        /// <summary>
        /// 道具价格
        /// </summary>
        /// <value>道具的购买价格，默认值为10</value>
        public int Price { get; set; } = 10;

        /// <summary>
        /// 道具堆叠数量
        /// </summary>
        /// <value>当前道具的堆叠数量，默认值为1</value>
        public int StackCount { get; set; } = 1;

        /// <summary>
        /// 最大堆叠数量
        /// </summary>
        /// <value>道具的最大堆叠数量，默认值为99</value>
        public int MaxStackCount { get; set; } = 99;

        /// <summary>
        /// 道具效果值（如药水恢复量）
        /// </summary>
        /// <value>道具的效果数值，如生命药水的恢复量</value>
        public float EffectValue { get; set; } = 0f;

        /// <summary>
        /// 道具效果持续时间（秒）
        /// </summary>
        /// <value>道具效果的持续时间，单位为秒，默认值为0</value>
        public float EffectDuration { get; set; } = 0f;

        /// <summary>
        /// 是否可使用
        /// </summary>
        /// <value>true 表示道具可以使用，false 表示道具不可使用</value>
        public bool IsUsable { get; set; } = false;

        /// <summary>
        /// 是否可出售
        /// </summary>
        /// <value>true 表示道具可以出售，false 表示道具不可出售</value>
        public bool IsSellable { get; set; } = true;

        /// <summary>
        /// 是否可丢弃
        /// </summary>
        /// <value>true 表示道具可以丢弃，false 表示道具不可丢弃</value>
        public bool IsDroppable { get; set; } = true;

        /// <summary>
        /// 使用道具
        /// </summary>
        /// <param name="user">使用道具的生物</param>
        /// <returns>是否使用成功</returns>
        /// <remarks>
        /// 根据道具类型执行相应的使用逻辑，消耗品使用后会减少数量
        /// 使用条件：道具可使用、用户不为空、用户存活
        /// </remarks>
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
        /// <remarks>
        /// 根据消耗品类型执行相应的使用逻辑，如生命药水恢复生命值
        /// </remarks>
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
        /// <remarks>
        /// 关键物品使用逻辑，目前仅记录日志
        /// </remarks>
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
        /// <remarks>
        /// 任务物品使用逻辑，目前仅记录日志
        /// </remarks>
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
        /// <remarks>
        /// 将另一个物品堆叠到当前物品中，返回堆叠后剩余的数量
        /// 堆叠条件：两个物品ID相同且都可堆叠
        /// </remarks>
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
        /// <remarks>
        /// 从当前物品中拆分出指定数量的物品，返回新创建的物品
        /// 拆分条件：拆分数量大于0且小于当前堆叠数量
        /// </remarks>
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
        /// <remarks>
        /// 如果最大堆叠数量大于1，则物品可堆叠
        /// </remarks>
        public bool IsStackable()
        {
            return MaxStackCount > 1;
        }

        /// <summary>
        /// 获取道具类型名称
        /// </summary>
        /// <returns>道具类型的字符串表示</returns>
        /// <remarks>
        /// 将道具类型枚举值转换为对应的字符串名称
        /// </remarks>
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
        /// <returns>消耗品类型的字符串表示</returns>
        /// <remarks>
        /// 将消耗品类型枚举值转换为对应的字符串名称
        /// </remarks>
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
        /// <returns>包含道具详细信息的字符串</returns>
        /// <remarks>
        /// 格式化输出道具的名称、类型、描述、效果、价格等信息
        /// </remarks>
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