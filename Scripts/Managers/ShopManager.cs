/*
 * File: ShopManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 商店管理器，作为全局单例负责管理游戏中的商店系统。
 * 支持商店数据管理、玩家交易（买入/卖出）、库存管理和声望折扣系统。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 支持多种商店类型（杂货店、武器店、防具店）
 * - 有限/无限库存管理
 * - 基于声望的折扣系统
 * - 完整的买入/卖出交易逻辑
 * - 背包空间检查和金币验证
 * - 完整的异常处理和日志记录
 */

using Godot;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 商店管理器，负责管理游戏中的商店系统
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为单例模式运行，负责管理多个商店的数据、
    /// 处理玩家与商店的交互（买入/卖出）、库存管理和交易逻辑。
    /// 支持多种商店类型、有限/无限库存、以及基于声望的折扣系统。
    /// </remarks>
    public partial class ShopManager : Node
    {
        /// <summary>
        /// 商店管理器的单例实例
        /// </summary>
        public static ShopManager Instance { get; private set; }

        /// <summary>
        /// 商店数据字典，存储商店ID与商店数据的映射
        /// </summary>
        private Dictionary<string, ShopData> _shops = new Dictionary<string, ShopData>();

        /// <summary>
        /// 当前打开的商店ID
        /// </summary>
        private string _currentShopId = string.Empty;

        /// <summary>
        /// 节点就绪时的初始化方法
        /// </summary>
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeShops();
                Log.Info("ShopManager initialized");
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// 初始化商店数据
        /// </summary>
        /// <remarks>
        /// 创建预设的商店数据，包括杂货店、武器店和防具店。
        /// 每个商店包含不同的商品和价格设置。
        /// </remarks>
        private void InitializeShops()
        {
            _shops.Add("general_store", new ShopData
            {
                Id = "general_store",
                Name = "General Store",
                Description = "A general store with basic supplies",
                ReputationDiscount = 0.0f,
                Items = new List<ShopItem>
                {
                    new ShopItem { ItemId = "health_potion", Price = 30, BuyPriceMultiplier = 1.0f, IsInfinite = true, Stock = 999 },
                    new ShopItem { ItemId = "mana_potion", Price = 40, BuyPriceMultiplier = 1.0f, IsInfinite = true, Stock = 999 },
                    new ShopItem { ItemId = "small_key", Price = 50, BuyPriceMultiplier = 1.0f, IsInfinite = false, Stock = 10 }
                }
            });

            _shops.Add("weapon_shop", new ShopData
            {
                Id = "weapon_shop",
                Name = "Weapon Shop",
                Description = "Buy powerful weapons",
                ReputationDiscount = 0.05f,
                Items = new List<ShopItem>
                {
                    new ShopItem { ItemId = "iron_sword", Price = 200, BuyPriceMultiplier = 1.0f, IsInfinite = true, Stock = 999 },
                    new ShopItem { ItemId = "steel_sword", Price = 500, BuyPriceMultiplier = 1.0f, IsInfinite = false, Stock = 5 },
                    new ShopItem { ItemId = "fire_blade", Price = 1000, BuyPriceMultiplier = 1.1f, IsInfinite = false, Stock = 2 }
                }
            });

            _shops.Add("armor_shop", new ShopData
            {
                Id = "armor_shop",
                Name = "Armor Shop",
                Description = "Protect yourself with armor",
                ReputationDiscount = 0.05f,
                Items = new List<ShopItem>
                {
                    new ShopItem { ItemId = "leather_armor", Price = 150, BuyPriceMultiplier = 1.0f, IsInfinite = true, Stock = 999 },
                    new ShopItem { ItemId = "chainmail", Price = 400, BuyPriceMultiplier = 1.0f, IsInfinite = false, Stock = 5 },
                    new ShopItem { ItemId = "plate_armor", Price = 800, BuyPriceMultiplier = 1.1f, IsInfinite = false, Stock = 3 }
                }
            });
        }

        /// <summary>
        /// 打开商店
        /// </summary>
        /// <param name="shopId">商店ID</param>
        /// <returns>打开成功返回true，失败返回false</returns>
        public bool OpenShop(string shopId)
        {
            if (!_shops.ContainsKey(shopId))
            {
                Log.Error($"Shop not found: {shopId}");
                return false;
            }

            _currentShopId = shopId;
            Log.Info($"Shop opened: {_shops[shopId].Name}");
            return true;
        }

        /// <summary>
        /// 关闭当前商店
        /// </summary>
        public void CloseShop()
        {
            _currentShopId = string.Empty;
            Log.Info("Shop closed");
        }

        /// <summary>
        /// 从商店购买物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="quantity">购买数量，默认为1</param>
        /// <returns>购买成功返回true，失败返回false</returns>
        public bool BuyItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(_currentShopId))
            {
                Log.Warning("No shop is open");
                return false;
            }

            ShopData shop = _shops[_currentShopId];
            ShopItem shopItem = shop.Items.FirstOrDefault(s => s.ItemId == itemId);

            if (shopItem == null)
            {
                Log.Error($"Item not found in shop: {itemId}");
                return false;
            }

            // 检查库存
            if (!shopItem.IsInfinite && shopItem.Stock < quantity)
            {
                Log.Warning($"Not enough stock for {itemId}. Available: {shopItem.Stock}");
                return false;
            }

            // 计算价格
            int basePrice = shopItem.Price;
            float discount = 1.0f - shop.ReputationDiscount;
            int totalCost = Mathf.FloorToInt(basePrice * quantity * shopItem.BuyPriceMultiplier * discount);

            // 获取玩家
            var player = GetPlayer();
            if (player == null) return false;

            // 检查金币
            if (player.Gold < totalCost)
            {
                Log.Warning($"Not enough gold. Need: {totalCost}, Have: {player.Gold}");
                return false;
            }

            // 检查背包空间
            if (!InventoryManager.Instance.HasEnoughSpace(quantity))
            {
                Log.Warning("Inventory is full");
                return false;
            }

            // 执行交易
            player.Gold -= totalCost;
            
            for (int i = 0; i < quantity; i++)
            {
                InventoryManager.Instance.AddItem(itemId);
            }

            // 更新库存
            if (!shopItem.IsInfinite)
            {
                shopItem.Stock -= quantity;
            }

            Log.Info($"Bought {itemId} x{quantity} for {totalCost} gold");
            return true;
        }

        /// <summary>
        /// 向商店卖出物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="quantity">卖出数量，默认为1</param>
        /// <returns>卖出成功返回true，失败返回false</returns>
        public bool SellItem(string itemId, int quantity = 1)
        {
            var player = GetPlayer();
            if (player == null) return false;

            // 检查玩家是否拥有足够物品
            if (!InventoryManager.Instance.HasItem(itemId, quantity))
            {
                Log.Warning($"Not enough {itemId} to sell");
                return false;
            }

            // 获取物品基础价格
            Item item = ResourcesManager.GetItem(itemId);
            if (item == null)
            {
                Log.Error($"Item not found: {itemId}");
                return false;
            }

            // 计算售价（通常是买入价的50%）
            int basePrice = item.Price;
            int sellPrice = Mathf.FloorToInt(basePrice * quantity * 0.5f);

            // 执行交易
            InventoryManager.Instance.RemoveItem(itemId, quantity);
            player.Gold += sellPrice;

            Log.Info($"Sold {itemId} x{quantity} for {sellPrice} gold");
            return true;
        }

        /// <summary>
        /// 获取当前打开的商店数据
        /// </summary>
        /// <returns>当前商店数据，如果未打开商店返回null</returns>
        public ShopData GetCurrentShop()
        {
            if (string.IsNullOrEmpty(_currentShopId))
            {
                return null;
            }
            return _shops[_currentShopId];
        }

        /// <summary>
        /// 获取指定商店数据
        /// </summary>
        /// <param name="shopId">商店ID</param>
        /// <returns>商店数据，如果不存在返回null</returns>
        public ShopData GetShop(string shopId)
        {
            if (_shops.TryGetValue(shopId, out ShopData shop))
            {
                return shop;
            }
            return null;
        }

        /// <summary>
        /// 获取所有商店列表
        /// </summary>
        /// <returns>所有商店数据列表</returns>
        public List<ShopData> GetAllShops()
        {
            return new List<ShopData>(_shops.Values);
        }

        /// <summary>
        /// 获取玩家对象
        /// </summary>
        /// <returns>玩家对象，如果未找到返回null</returns>
        private hd2dtest.Scripts.Modules.Player GetPlayer()
        {
            foreach (var node in GetTree().GetNodesInGroup("player"))
            {
                if (node is hd2dtest.Scripts.Modules.Player player)
                {
                    return player;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 商店数据类
    /// </summary>
    public class ShopData
    {
        /// <summary>
        /// 商店ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 商店名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 商店描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 声望折扣（0-1之间，例如0.1表示9折）
        /// </summary>
        public float ReputationDiscount { get; set; } = 0f;

        /// <summary>
        /// 商店物品列表
        /// </summary>
        public List<ShopItem> Items { get; set; } = new List<ShopItem>();
    }

    /// <summary>
    /// 商店物品类
    /// </summary>
    public class ShopItem
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 物品基础价格
        /// </summary>
        public int Price { get; set; } = 0;

        /// <summary>
        /// 买入价格倍率
        /// </summary>
        public float BuyPriceMultiplier { get; set; } = 1.0f;

        /// <summary>
        /// 是否无限库存
        /// </summary>
        public bool IsInfinite { get; set; } = true;

        /// <summary>
        /// 当前库存数量
        /// </summary>
        public int Stock { get; set; } = 0;
    }
}