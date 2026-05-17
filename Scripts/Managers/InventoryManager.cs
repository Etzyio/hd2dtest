/*
 * File: InventoryManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 背包管理器，作为全局单例负责管理玩家背包中的物品。
 * 支持物品的添加、移除、使用、分类查询和容量管理。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 物品堆叠机制（支持最大堆叠数限制）
 * - 背包容量管理和扩展
 * - 物品分类查询（按类型筛选）
 * - 消耗品使用功能（药水等）
 * - 完整的异常处理和日志记录
 */

using Godot;
using hd2dtest.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 背包管理器，负责管理玩家背包中的物品
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为单例模式运行，负责管理玩家背包的物品存储、
    /// 物品添加/移除、物品使用、容量管理等功能。支持物品堆叠、分类查询和容量扩展。
    /// </remarks>
    public partial class InventoryManager : Node
    {
        /// <summary>
        /// 背包管理器的单例实例
        /// </summary>
        public static InventoryManager Instance { get; private set; }

        /// <summary>
        /// 物品字典，存储物品ID与(物品对象, 数量)的映射
        /// </summary>
        private Dictionary<string, (Item, int)> _items = new Dictionary<string, (Item, int)>();

        /// <summary>
        /// 背包最大容量
        /// </summary>
        private int _maxCapacity = 50;

        /// <summary>
        /// 当前物品总数
        /// </summary>
        public int CurrentCount => _items.Sum(kv => kv.Value.Item2);

        /// <summary>
        /// 获取背包最大容量
        /// </summary>
        public int MaxCapacity => _maxCapacity;

        /// <summary>
        /// 判断背包是否已满
        /// </summary>
        public bool IsFull => CurrentCount >= _maxCapacity;

        /// <summary>
        /// 节点就绪时的初始化方法
        /// </summary>
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                Log.Info("InventoryManager initialized");
            }
            else
            {
                Log.Warning("InventoryManager instance already exists");
                QueueFree();
            }
        }

        /// <summary>
        /// 初始化背包系统
        /// </summary>
        public void Initialize()
        {
            Log.Info("Initializing inventory system...");
        }

        /// <summary>
        /// 向背包中添加物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="quantity">添加数量，默认为1</param>
        /// <returns>添加成功返回true，失败返回false</returns>
        /// <remarks>
        /// 添加物品前会检查：数量是否合法、背包是否已满、物品是否存在、堆叠是否溢出。
        /// 如果物品已存在且未达到堆叠上限，则增加数量；否则创建新条目。
        /// </remarks>
        public bool AddItem(string itemId, int quantity = 1)
        {
            // 检查数量是否合法
            if (quantity <= 0) return false;
            
            // 检查背包是否已满
            if (IsFull)
            {
                Log.Warning("Inventory is full");
                return false;
            }

            // 获取物品数据
            Item item = ResourcesManager.GetItem(itemId);
            if (item == null)
            {
                Log.Error($"Item not found: {itemId}");
                return false;
            }

            // 如果物品已存在，检查堆叠
            if (_items.ContainsKey(itemId))
            {
                var (existingItem, existingQty) = _items[itemId];
                int newQty = existingQty + quantity;
                
                // 检查堆叠上限
                if (newQty > item.MaxStackCount)
                {
                    Log.Warning($"Item {itemId} stack overflow. Max: {item.MaxStackCount}");
                    return false;
                }
                
                _items[itemId] = (existingItem, newQty);
            }
            else
            {
                _items[itemId] = (item, quantity);
            }

            Log.Info($"Added item: {itemId} x{quantity}");
            return true;
        }

        /// <summary>
        /// 从背包中移除物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="quantity">移除数量，默认为1</param>
        /// <returns>移除成功返回true，失败返回false</returns>
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0) return false;
            
            if (!_items.ContainsKey(itemId))
            {
                Log.Warning($"Item not in inventory: {itemId}");
                return false;
            }

            var (item, qty) = _items[itemId];
            if (qty < quantity)
            {
                Log.Warning($"Not enough {itemId} in inventory. Have: {qty}, Need: {quantity}");
                return false;
            }

            if (qty == quantity)
            {
                _items.Remove(itemId);
            }
            else
            {
                _items[itemId] = (item, qty - quantity);
            }

            Log.Info($"Removed item: {itemId} x{quantity}");
            return true;
        }

        /// <summary>
        /// 获取物品数量
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <returns>物品数量，不存在返回0</returns>
        public int GetItemQuantity(string itemId)
        {
            if (_items.TryGetValue(itemId, out var value))
            {
                return value.Item2;
            }
            return 0;
        }

        /// <summary>
        /// 检查背包是否有足够空间
        /// </summary>
        /// <param name="quantity">需要的空间数量</param>
        /// <returns>有足够空间返回true</returns>
        public bool HasEnoughSpace(int quantity = 1)
        {
            return CurrentCount + quantity <= _maxCapacity;
        }

        /// <summary>
        /// 检查是否拥有指定数量的物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="quantity">需要的数量</param>
        /// <returns>拥有足够数量返回true</returns>
        public bool HasItem(string itemId, int quantity = 1)
        {
            return GetItemQuantity(itemId) >= quantity;
        }

        /// <summary>
        /// 使用物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="player">使用物品的玩家</param>
        /// <remarks>
        /// 目前仅支持消耗品类型物品的使用，包括生命药水和魔法药水。
        /// 使用后会扣除物品并应用相应效果。
        /// </remarks>
        public void UseItem(string itemId, hd2dtest.Scripts.Modules.Player player)
        {
            if (!HasItem(itemId)) return;

            Item item = ResourcesManager.GetItem(itemId);
            if (item == null) return;

            if (item.ItemTypeValue == Item.ItemType.Consumable)
            {
                if (item.ConsumableTypeValue == Item.ConsumableType.HealthPotion)
                {
                    float healAmount = Mathf.Min(item.EffectValue, player.MaxHealth - player.Health);
                    player.Health += healAmount;
                    Log.Info($"Used {itemId}, healed {healAmount} HP");
                }
                else if (item.ConsumableTypeValue == Item.ConsumableType.ManaPotion)
                {
                    float manaAmount = Mathf.Min(item.EffectValue, player.MaxMana - player.CurrentMana);
                    player.CurrentMana += manaAmount;
                    Log.Info($"Used {itemId}, restored {manaAmount} MP");
                }

                RemoveItem(itemId, 1);
            }
        }

        /// <summary>
        /// 根据物品类型获取物品列表
        /// </summary>
        /// <param name="itemType">物品类型</param>
        /// <returns>该类型的物品字典</returns>
        public Dictionary<string, (Item, int)> GetItemsByCategory(Item.ItemType itemType)
        {
            return _items.Where(kv => kv.Value.Item1.ItemTypeValue == itemType)
                         .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// 扩展背包容量
        /// </summary>
        /// <param name="amount">增加的容量数量</param>
        public void ExpandCapacity(int amount)
        {
            _maxCapacity += amount;
            Log.Info($"Inventory capacity expanded to {_maxCapacity}");
        }

        /// <summary>
        /// 清空背包
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            Log.Info("Inventory cleared");
        }

        /// <summary>
        /// 获取所有物品列表
        /// </summary>
        /// <returns>包含物品ID、物品对象和数量的列表</returns>
        public List<(string ItemId, Item Item, int Quantity)> GetAllItems()
        {
            return _items.Select(kv => (kv.Key, kv.Value.Item1, kv.Value.Item2)).ToList();
        }
    }
}