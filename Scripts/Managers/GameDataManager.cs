using Godot;
using hd2dtest.Scripts.Modules;
using System.Collections.Generic;
using System.Linq;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 游戏数据管理器，负责管理游戏中的数据数据和状态
    /// </summary>
    /// <remarks>
    /// 该类负责处理游戏中的数据存储、加载、更新和查询，确保游戏数据的完整性和一致性。
    /// 它提供了一个中心位置来管理游戏数据，如玩家数据、团队数据、道具数据等，以及提供数据访问接口。
    /// </remarks>
    public class GameDataManager
    {

        private static GameDataManager _instance = new();
        public static GameDataManager Instance => _instance;

        public GameDataManager()
        {
            InitializeLevelCache();
        }

        /// <summary>
        /// 小队管理对象
        /// </summary>
        public Teammates Teammates = new();

        /// <summary>
        /// 道具列表
        /// </summary>
        public Dictionary<string, int> ItemList => _itemList;
        private Dictionary<string, int> _itemList = [];

        /// <summary>
        /// 等级列表
        /// </summary>
        public Dictionary<string, Level> LevelList => _levelList;
        private Dictionary<string, Level> _levelList;

        /// <summary>
        /// 任务列表
        /// </summary>
        public Dictionary<string, QuestData> QuestList => _questList;
        private Dictionary<string, QuestData> _questList = [];

        /// <summary>
        /// 装备列表
        /// </summary>
        public Dictionary<string, int> EquipmentList => _equipmentList;
        private Dictionary<string, int> _equipmentList = [];

        /// <summary>
        /// NPC 状态列表
        /// </summary>
        public Dictionary<string, int> NPCStatus => _NPCStatus;
        private Dictionary<string, int> _NPCStatus = [];

        public void InitializeLevelCache()
        {
            _questList = ResourcesManager.QuestsCache.ToDictionary(static quest => quest.Key, static quest => quest.Value);
            _levelList = ResourcesManager.LevelsCache.ToDictionary(static level => level.Key, static level => level.Value);
        }

        public void UnlockLevel(string levelId)
        {
            if (_levelList.TryGetValue(levelId, out Level level))
            {
                level.IsLocked = false;
            }
        }

        public void UnlockQuest(string questId)
        {
            if (_questList.TryGetValue(questId, out QuestData quest))
            {
                quest.IsLocked = false;
            }
        }

        public void AddEquipment(Equipment equipment)
        {
            if (_equipmentList.ContainsKey(equipment.Id))
            {
                _equipmentList[equipment.Id]++;
            }
            else
            {
                _equipmentList.Add(equipment.Id, 1);
            }
        }
        public void RemoveEquipment(Equipment equipment)
        {
            if (_equipmentList.ContainsKey(equipment.Id) && _equipmentList[equipment.Id] > 0)
            {
                _equipmentList[equipment.Id]--;
            }
        }

        public void RemoveEquipmentCount(Equipment equipment, int count)
        {
            if (_equipmentList.ContainsKey(equipment.Id) && _equipmentList[equipment.Id] >= count)
            {
                _equipmentList[equipment.Id] -= count;
            }
        }

        public void AddItem(Item item)
        {
            if (item != null && !_itemList.ContainsKey(item.Id))
            {
                _itemList.Add(item.Id, 1);
            }
            else if (item != null && _itemList.ContainsKey(item.Id))
            {
                _itemList[item.Id]++;
            }
        }

        /// <summary>
        /// 使用列表中道具
        /// </summary>
        /// <param name="item">要使用的道具</param>
        public void UseItem(Item item)
        {
            if (item != null && _itemList.ContainsKey(item.Id))
            {
                if (_itemList[item.Id] > 0)
                {
                    _itemList[item.Id]--;
                }
            }
        }

        /// <summary>
        /// 根据 ID 获取道具
        /// </summary>
        /// <param name="itemId">道具 ID</param>
        /// <returns>如果找到则返回道具，否则返回 null</returns>
        public Item GetItemById(string itemId)
        {
            return ResourcesManager.ItemsCache.TryGetValue(itemId, out Item item) ? item : null;
        }

        /// <summary>
        /// 获取所有道具
        /// </summary>
        /// <returns>道具列表</returns>
        public List<Item> GetAllItems()
        {
            return [.. _itemList.Keys.Select(key => GetItemById(key))];
        }

        public void RemoveItem(Item item)
        {
            if (_itemList.ContainsKey(item.Id) && _itemList[item.Id] > 0)
            {
                _itemList[item.Id]--;
            }
        }
    }
}