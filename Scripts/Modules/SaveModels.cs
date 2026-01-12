using System;
using System.Collections.Generic;
using System.Numerics;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 玩家状态数据结构
    /// </summary>
    public class PlayerSaveData
    {
        public int PlayerId { get; set; }  // 玩家ID
        public string PlayerName { get; set; }  // 玩家名称
        public Vector2 Position { get; set; }  // 玩家位置
        public int Level { get; set; }  // 玩家等级
        public int Experience { get; set; }  // 玩家经验值
        public float Health { get; set; }  // 玩家当前生命值
        public float MaxHealth { get; set; }  // 玩家最大生命值
        public float Mana { get; set; }  // 玩家当前魔法值
        public float MaxMana { get; set; }  // 玩家最大魔法值
        public int Attack { get; set; }  // 玩家攻击力
        public int Defense { get; set; }  // 玩家防御力
        public float Speed { get; set; }  // 玩家移动速度
        
        // 物品和装备
        public Dictionary<string, int> Inventory { get; set; } = [];  // 背包物品（物品ID:数量）
        public string EquippedWeapon { get; set; }  // 已装备武器
        public Dictionary<string, string> EquippedEquipment { get; set; } = [];  // 已装备装备（装备位置:装备ID）
        
        // 技能
        public List<string> LearnedSkills { get; set; } = [];  // 已学习技能列表
    }

    /// <summary>
    /// 存档数据结构
    /// </summary>
    public class SaveData
    {
        // 基本存档信息
        public string SaveId { get; set; }  // 存档ID
        public string SaveName { get; set; }  // 存档名称
        public DateTime SaveTime { get; set; }  // 保存时间

        // 游戏进度
        public string CurrentScene { get; set; }  // 当前场景名称
        public int GameScore { get; set; }  // 游戏分数
        public int PlayTime { get; set; }  // 游戏时间（秒）
        public Dictionary<string, bool> CompletedQuests { get; set; } = [];  // 已完成任务列表
        public Dictionary<string, bool> DiscoveredAreas { get; set; } = [];  // 已发现区域

        // 多个玩家状态
        public List<PlayerSaveData> Players { get; set; } = [];  // 所有玩家状态数据

        // 自定义数据
        public Dictionary<string, object> CustomData { get; set; } = [];  // 自定义保存数据
    }

    /// <summary>
    /// 存档信息结构（用于显示存档列表）
    /// </summary>
    public class SaveInfo
    {
        public string SaveId { get; set; }  // 存档ID
        public string SaveName { get; set; }  // 存档名称
        public DateTime SaveTime { get; set; }  // 保存时间
        public int GameScore { get; set; }  // 游戏分数
        public int PlayerCount { get; set; }  // 玩家数量
        public int AveragePlayerLevel { get; set; }  // 平均玩家等级
        public int PlayTime { get; set; }  // 游戏时间（秒）
        public string CurrentScene { get; set; }  // 当前场景
    }
}