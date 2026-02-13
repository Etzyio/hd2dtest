using System;
using System.Collections.Generic;
using System.Numerics;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 玩家状态数据结构，用于保存和加载玩家的状态信息
    /// </summary>
    /// <remarks>
    /// 存储玩家的基本属性、位置、物品、装备和技能等信息
    /// 用于存档系统中玩家数据的序列化和反序列化
    /// </remarks>
    public class PlayerSaveData
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        /// <value>玩家的唯一标识符</value>
        public int PlayerId { get; set; }
        
        /// <summary>
        /// 玩家名称
        /// </summary>
        /// <value>玩家的显示名称</value>
        public string PlayerName { get; set; }
        
        /// <summary>
        /// 玩家位置
        /// </summary>
        /// <value>玩家在游戏世界中的坐标位置</value>
        public Vector3 Position { get; set; }
        
        /// <summary>
        /// 玩家等级
        /// </summary>
        /// <value>玩家的当前等级</value>
        public int Level { get; set; }
        
        /// <summary>
        /// 玩家经验值
        /// </summary>
        /// <value>玩家当前积累的经验值</value>
        public int Experience { get; set; }
        
        /// <summary>
        /// 玩家当前生命值
        /// </summary>
        /// <value>玩家的当前生命值</value>
        public float Health { get; set; }
        
        /// <summary>
        /// 玩家最大生命值
        /// </summary>
        /// <value>玩家的最大生命值</value>
        public float MaxHealth { get; set; }
        
        /// <summary>
        /// 玩家当前魔法值
        /// </summary>
        /// <value>玩家的当前魔法值</value>
        public float Mana { get; set; }
        
        /// <summary>
        /// 玩家最大魔法值
        /// </summary>
        /// <value>玩家的最大魔法值</value>
        public float MaxMana { get; set; }
        
        /// <summary>
        /// 玩家攻击力
        /// </summary>
        /// <value>玩家的攻击力</value>
        public int Attack { get; set; }
        
        /// <summary>
        /// 玩家防御力
        /// </summary>
        /// <value>玩家的防御力</value>
        public int Defense { get; set; }
        
        /// <summary>
        /// 玩家移动速度
        /// </summary>
        /// <value>玩家的移动速度</value>
        public float Speed { get; set; }

        // 物品和装备
        /// <summary>
        /// 背包物品
        /// </summary>
        /// <value>玩家背包中的物品，键为物品ID，值为数量</value>
        public Dictionary<string, int> Inventory { get; set; } = [];
        
        /// <summary>
        /// 已装备武器
        /// </summary>
        /// <value>玩家当前装备的武器ID</value>
        public string EquippedWeapon { get; set; }
        
        /// <summary>
        /// 已装备装备
        /// </summary>
        /// <value>玩家当前装备的装备，键为装备位置，值为装备ID</value>
        public Dictionary<string, string> EquippedEquipment { get; set; } = [];

        // 技能
        /// <summary>
        /// 已学习技能列表
        /// </summary>
        /// <value>玩家已学习的技能ID集合</value>
        public List<string> LearnedSkills { get; set; } = [];
    }

    /// <summary>
    /// 存档数据结构，用于保存和加载游戏的完整状态
    /// </summary>
    /// <remarks>
    /// 存储游戏的基本信息、进度、任务数据、玩家状态和自定义数据
    /// 用于存档系统中游戏数据的序列化和反序列化
    /// </remarks>
    public class SaveData
    {
        // 基本存档信息
        /// <summary>
        /// 存档ID
        /// </summary>
        /// <value>存档的唯一标识符</value>
        public string SaveId { get; set; }
        
        /// <summary>
        /// 存档名称
        /// </summary>
        /// <value>存档的显示名称</value>
        public string SaveName { get; set; }
        
        /// <summary>
        /// 保存时间
        /// </summary>
        /// <value>存档的保存时间</value>
        public DateTime SaveTime { get; set; }
        
        /// <summary>
        /// 游戏版本号
        /// </summary>
        /// <value>创建存档时的游戏版本号</value>
        public string GameVersion { get; set; }
        
        /// <summary>
        /// 构建日期
        /// </summary>
        /// <value>游戏构建的日期</value>
        public string BuildDate { get; set; }
        
        /// <summary>
        /// Git提交哈希
        /// </summary>
        /// <value>游戏构建时的Git提交哈希值</value>
        public string GitCommit { get; set; }

        // 游戏进度
        /// <summary>
        /// 当前场景名称
        /// </summary>
        /// <value>存档时的当前场景名称</value>
        public string CurrentScene { get; set; }
        
        /// <summary>
        /// 游戏分数
        /// </summary>
        /// <value>玩家的游戏分数</value>
        public int GameScore { get; set; }
        
        /// <summary>
        /// 游戏时间（秒）
        /// </summary>
        /// <value>游戏的累计游玩时间，单位为秒</value>
        public int PlayTime { get; set; }
        
        /// <summary>
        /// 已完成任务列表
        /// </summary>
        /// <value>已完成的任务ID集合，值为true表示已完成</value>
        public Dictionary<string, bool> CompletedQuests { get; set; } = [];
        
        /// <summary>
        /// 已发现区域
        /// </summary>
        /// <value>已发现的区域ID集合，值为true表示已发现</value>
        public Dictionary<string, bool> DiscoveredAreas { get; set; } = [];

        // 任务数据
        /// <summary>
        /// 任务状态
        /// </summary>
        /// <value>任务的状态，键为任务ID，值为状态编码</value>
        public Dictionary<string, int> QuestStatuses { get; set; } = [];
        
        /// <summary>
        /// 任务进度
        /// </summary>
        /// <value>任务的详细进度数据，键为任务ID，值为进度数据字典</value>
        public Dictionary<string, Dictionary<string, object>> QuestProgress { get; set; } = [];
        
        /// <summary>
        /// 剧情线节点状态
        /// </summary>
        /// <value>剧情线节点的状态，键为节点ID，值为状态编码</value>
        public Dictionary<string, int> QuestLineNodeStates { get; set; } = [];

        // 多个玩家状态
        /// <summary>
        /// 所有玩家状态数据
        /// </summary>
        /// <value>游戏中的玩家列表</value>
        public List<Player> Players { get; set; } = [];

        // 自定义数据
        /// <summary>
        /// 自定义保存数据
        /// </summary>
        /// <value>自定义的存档数据，键为数据名称，值为数据对象</value>
        public Dictionary<string, object> CustomData { get; set; } = [];
    }

    /// <summary>
    /// 存档信息结构，用于显示存档列表
    /// </summary>
    /// <remarks>
    /// 存储存档的基本信息，用于在存档列表中显示
    /// 包含存档ID、名称、保存时间、游戏版本等信息
    /// </remarks>
    public class SaveInfo
    {
        /// <summary>
        /// 存档ID
        /// </summary>
        /// <value>存档的唯一标识符</value>
        public string SaveId { get; set; }
        
        /// <summary>
        /// 存档名称
        /// </summary>
        /// <value>存档的显示名称</value>
        public string SaveName { get; set; }
        
        /// <summary>
        /// 保存时间
        /// </summary>
        /// <value>存档的保存时间</value>
        public DateTime SaveTime { get; set; }
        
        /// <summary>
        /// 游戏版本号
        /// </summary>
        /// <value>创建存档时的游戏版本号</value>
        public string GameVersion { get; set; }
        
        /// <summary>
        /// 游戏分数
        /// </summary>
        /// <value>玩家的游戏分数</value>
        public int GameScore { get; set; }
        
        /// <summary>
        /// 玩家数量
        /// </summary>
        /// <value>存档中的玩家数量</value>
        public int PlayerCount { get; set; }
        
        /// <summary>
        /// 平均玩家等级
        /// </summary>
        /// <value>存档中所有玩家的平均等级</value>
        public int AveragePlayerLevel { get; set; }
        
        /// <summary>
        /// 游戏时间（秒）
        /// </summary>
        /// <value>游戏的累计游玩时间，单位为秒</value>
        public int PlayTime { get; set; }
        
        /// <summary>
        /// 当前场景
        /// </summary>
        /// <value>存档时的当前场景名称</value>
        public string CurrentScene { get; set; }
    }
}