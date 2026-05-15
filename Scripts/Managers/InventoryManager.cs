using Godot;
using hd2dtest.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts.Managers
{
    public partial class InventoryManager : Node
    {
        public static InventoryManager Instance { get; private set; }

        private Dictionary<string, (Item, int)> _items = new Dictionary<string, (Item, int)>();
        private int _maxCapacity = 50;

        public int CurrentCount => _items.Sum(kv => kv.Value.Item2);
        public int MaxCapacity => _maxCapacity;
        public bool IsFull => CurrentCount >= _maxCapacity;

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

        public void Initialize()
        {
            Log.Info("Initializing inventory system...");
        }

        public bool AddItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0) return false;
            
            if (IsFull)
            {
                Log.Warning("Inventory is full");
                return false;
            }

            Item item = ResourcesManager.GetItem(itemId);
            if (item == null)
            {
                Log.Error($"Item not found: {itemId}");
                return false;
            }

            if (_items.ContainsKey(itemId))
            {
                var (existingItem, existingQty) = _items[itemId];
                int newQty = existingQty + quantity;
                
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

        public int GetItemQuantity(string itemId)
        {
            if (_items.TryGetValue(itemId, out var value))
            {
                return value.Item2;
            }
            return 0;
        }

        public bool HasEnoughSpace(int quantity = 1)
        {
            return CurrentCount + quantity <= _maxCapacity;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            return GetItemQuantity(itemId) >= quantity;
        }

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

        public Dictionary<string, (Item, int)> GetItemsByCategory(Item.ItemType itemType)
        {
            return _items.Where(kv => kv.Value.Item1.ItemTypeValue == itemType)
                         .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public void ExpandCapacity(int amount)
        {
            _maxCapacity += amount;
            Log.Info($"Inventory capacity expanded to {_maxCapacity}");
        }

        public void Clear()
        {
            _items.Clear();
            Log.Info("Inventory cleared");
        }

        public List<(string ItemId, Item Item, int Quantity)> GetAllItems()
        {
            return _items.Select(kv => (kv.Key, kv.Value.Item1, kv.Value.Item2)).ToList();
        }
    }
}