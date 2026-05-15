using Godot;
using hd2dtest.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts.Managers
{
    public class ShopItem
    {
        public string ItemId { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
        public int MaxStock { get; set; }
        public bool IsInfinite { get; set; } = true;
        public float BuyPriceMultiplier { get; set; } = 1.0f;
        public float SellPriceMultiplier { get; set; } = 0.5f;
    }

    public class ShopData
    {
        public string ShopId { get; set; }
        public string ShopName { get; set; }
        public List<ShopItem> Items { get; set; } = new List<ShopItem>();
        public float ReputationDiscount { get; set; } = 0f;
    }

    public partial class ShopManager : Node
    {
        public static ShopManager Instance { get; private set; }

        private Dictionary<string, ShopData> _shops = new Dictionary<string, ShopData>();
        private string _currentShopId = string.Empty;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeDefaultShops();
                Log.Info("ShopManager initialized");
            }
            else
            {
                Log.Warning("ShopManager instance already exists");
                QueueFree();
            }
        }

        private void InitializeDefaultShops()
        {
            // 创建默认商店
            var generalStore = new ShopData
            {
                ShopId = "general_store",
                ShopName = "General Store",
                Items = new List<ShopItem>
                {
                    new ShopItem { ItemId = "potion_health_small", Price = 10, MaxStock = 999, IsInfinite = true },
                    new ShopItem { ItemId = "potion_mana_small", Price = 15, MaxStock = 999, IsInfinite = true },
                    new ShopItem { ItemId = "potion_health_medium", Price = 25, MaxStock = 999, IsInfinite = true },
                    new ShopItem { ItemId = "potion_mana_medium", Price = 35, MaxStock = 999, IsInfinite = true },
                }
            };

            var weaponShop = new ShopData
            {
                ShopId = "weapon_shop",
                ShopName = "Weapon Shop",
                Items = new List<ShopItem>
                {
                    new ShopItem { ItemId = "weapon_iron_sword", Price = 50, MaxStock = 10 },
                    new ShopItem { ItemId = "weapon_steel_sword", Price = 100, MaxStock = 5 },
                    new ShopItem { ItemId = "weapon_iron_dagger", Price = 30, MaxStock = 15 },
                }
            };

            var armorShop = new ShopData
            {
                ShopId = "armor_shop",
                ShopName = "Armor Shop",
                Items = new List<ShopItem>
                {
                    new ShopItem { ItemId = "armor_leather", Price = 40, MaxStock = 10 },
                    new ShopItem { ItemId = "armor_chainmail", Price = 80, MaxStock = 5 },
                    new ShopItem { ItemId = "armor_helmet", Price = 30, MaxStock = 10 },
                }
            };

            _shops.Add(generalStore.ShopId, generalStore);
            _shops.Add(weaponShop.ShopId, weaponShop);
            _shops.Add(armorShop.ShopId, armorShop);

            Log.Info($"Initialized {_shops.Count} shops");
        }

        public void OpenShop(string shopId)
        {
            if (!_shops.ContainsKey(shopId))
            {
                Log.Error($"Shop not found: {shopId}");
                return;
            }

            _currentShopId = shopId;
            Log.Info($"Opening shop: {_shops[shopId].ShopName}");
        }

        public void CloseShop()
        {
            _currentShopId = string.Empty;
            Log.Info("Shop closed");
        }

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

            // 计算价格（考虑声望折扣）
            int basePrice = shopItem.Price;
            float discount = 1.0f - shop.ReputationDiscount;
            int totalCost = Mathf.FloorToInt(basePrice * quantity * shopItem.BuyPriceMultiplier * discount);

            // 检查玩家金币
            var player = GetPlayer();
            if (player == null) return false;

            if (player.Gold < totalCost)
            {
                Log.Warning($"Not enough gold. Need: {totalCost}, Have: {player.Gold}");
                return false;
            }

            // 检查背包容量
            if (!InventoryManager.Instance.HasEnoughSpace(quantity))
            {
                Log.Warning("Inventory is full");
                return false;
            }

            // 执行购买
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

        public bool SellItem(string itemId, int quantity = 1)
        {
            var player = GetPlayer();
            if (player == null) return false;

            // 检查背包是否有足够物品
            if (!InventoryManager.Instance.HasItem(itemId, quantity))
            {
                Log.Warning($"Not enough {itemId} in inventory");
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

            // 执行出售
            InventoryManager.Instance.RemoveItem(itemId, quantity);
            player.Gold += sellPrice;

            Log.Info($"Sold {itemId} x{quantity} for {sellPrice} gold");
            return true;
        }

        public ShopData GetCurrentShop()
        {
            if (string.IsNullOrEmpty(_currentShopId)) return null;
            return _shops.TryGetValue(_currentShopId, out var shop) ? shop : null;
        }

        public ShopData GetShop(string shopId)
        {
            return _shops.TryGetValue(shopId, out var shop) ? shop : null;
        }

        public List<string> GetAllShopIds()
        {
            return _shops.Keys.ToList();
        }

        public void AddShop(ShopData shop)
        {
            if (!_shops.ContainsKey(shop.ShopId))
            {
                _shops.Add(shop.ShopId, shop);
                Log.Info($"Added shop: {shop.ShopId}");
            }
            else
            {
                Log.Warning($"Shop already exists: {shop.ShopId}");
            }
        }

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
}