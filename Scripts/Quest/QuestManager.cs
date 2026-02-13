using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    /// <summary>
    /// 任务管理器
    /// 负责管理任务的加载、状态跟踪、进度更新和奖励发放
    /// </summary>
    public partial class QuestManager : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static QuestManager _instance;
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static QuestManager Instance => _instance;

        /// <summary>
        /// 任务状态枚举
        /// </summary>
        public enum QuestStatus
        {
            None,        // 无状态
            Available,   // 可接取
            InProgress,  // 进行中
            Completed,   // 已完成
            Failed       // 已失败
        }

        /// <summary>
        /// 任务存档数据类
        /// </summary>
        public class QuestSaveData
        {
            /// <summary>
            /// 任务状态字典
            /// 键：任务ID，值：任务状态
            /// </summary>
            public Dictionary<string, QuestStatus> QuestStatuses { get; set; } = new Dictionary<string, QuestStatus>();
            
            /// <summary>
            /// 任务进度字典
            /// 键：任务ID，值：目标进度字典
            /// </summary>
            public Dictionary<string, Dictionary<string, object>> QuestProgress { get; set; } = new Dictionary<string, Dictionary<string, object>>();
        }

        /// <summary>
        /// 所有任务数据
        /// </summary>
        private List<QuestData> _allQuests = new List<QuestData>();
        
        /// <summary>
        /// 任务状态字典
        /// 键：任务ID，值：任务状态
        /// </summary>
        private Dictionary<string, QuestStatus> _questStatuses = new Dictionary<string, QuestStatus>();
        
        /// <summary>
        /// 任务进度字典
        /// 键：任务ID，值：目标进度字典
        /// </summary>
        private Dictionary<string, Dictionary<string, object>> _questProgress = new Dictionary<string, Dictionary<string, object>>();

        /// <summary>
        /// 任务状态变更信号
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <param name="status">任务状态</param>
        [Signal]
        public delegate void QuestStatusChangedEventHandler(string questId, QuestStatus status);

        /// <summary>
        /// 任务完成信号
        /// </summary>
        /// <param name="questId">任务ID</param>
        [Signal]
        public delegate void QuestCompletedEventHandler(string questId);

        /// <summary>
        /// 任务失败信号
        /// </summary>
        /// <param name="questId">任务ID</param>
        [Signal]
        public delegate void QuestFailedEventHandler(string questId);

        /// <summary>
        /// 节点就绪时的回调
        /// 初始化单例实例，加载任务数据和存档数据
        /// </summary>
        public override void _Ready()
        {
            _instance = this;
            LoadQuests();
            LoadSaveData();
        }

        /// <summary>
        /// 加载任务数据
        /// 从ResourcesManager加载所有任务数据
        /// </summary>
        public void LoadQuests()
        {
            try
            {
                // 使用ResourcesManager加载所有任务数据
                if (hd2dtest.Scripts.Managers.ResourcesManager.Instance != null)
                {
                    _allQuests = hd2dtest.Scripts.Managers.ResourcesManager.GetAllQuests();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error loading quests: {e.Message}");
            }
        }

        /// <summary>
        /// 保存任务数据（通过SaveManager）
        /// </summary>
        public void SaveData()
        {
            try
            {
                if (hd2dtest.Scripts.Managers.SaveManager.Instance != null)
                {
                    // 任务数据会在SaveManager.SaveGame时自动保存
                    // 这里可以触发保存操作
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error saving quest data: {e.Message}");
            }
        }

        /// <summary>
        /// 加载任务数据（通过SaveManager）
        /// </summary>
        public void LoadSaveData()
        {
            try
            {
                if (hd2dtest.Scripts.Managers.SaveManager.Instance != null)
                {
                    // 任务数据会在SaveManager.LoadGame时自动加载
                    // 这里可以触发加载操作
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error loading quest save data: {e.Message}");
            }
        }

        /// <summary>
        /// 获取任务数据
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <returns>任务数据</returns>
        public QuestData GetQuest(string questId)
        {
            return _allQuests.Find(q => q.Id == questId);
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <returns>任务状态</returns>
        public QuestStatus GetQuestStatus(string questId)
        {
            if (_questStatuses.TryGetValue(questId, out QuestStatus status))
            {
                return status;
            }
            return QuestStatus.None;
        }

        /// <summary>
        /// 检查是否可以开始任务
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <returns>是否可以开始任务</returns>
        public bool CanStartQuest(string questId)
        {
            QuestData quest = GetQuest(questId);
            if (quest == null)
            {
                return false;
            }

            if (GetQuestStatus(questId) != QuestStatus.None)
            {
                return false;
            }

            if (quest.Dependencies != null)
            {
                foreach (var dependency in quest.Dependencies)
                {
                    if (!CheckDependency(dependency))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="questId">任务ID</param>
        public void StartQuest(string questId)
        {
            if (CanStartQuest(questId))
            {
                _questStatuses[questId] = QuestStatus.InProgress;
                _questProgress[questId] = new Dictionary<string, object>();
                EmitSignal(SignalName.QuestStatusChanged, questId, (int)QuestStatus.InProgress);
                SaveData();
            }
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        /// <param name="questId">任务ID</param>
        public void CompleteQuest(string questId)
        {
            QuestData quest = GetQuest(questId);
            if (quest != null && GetQuestStatus(questId) == QuestStatus.InProgress)
            {
                _questStatuses[questId] = QuestStatus.Completed;
                EmitSignal(SignalName.QuestStatusChanged, questId, (int)QuestStatus.Completed);
                EmitSignal(SignalName.QuestCompleted, questId);
                GrantRewards(quest);
                TriggerAchievements(quest);
                SaveData();
            }
        }

        /// <summary>
        /// 失败任务
        /// </summary>
        /// <param name="questId">任务ID</param>
        public void FailQuest(string questId)
        {
            if (GetQuestStatus(questId) == QuestStatus.InProgress)
            {
                _questStatuses[questId] = QuestStatus.Failed;
                EmitSignal(SignalName.QuestStatusChanged, questId, (int)QuestStatus.Failed);
                EmitSignal(SignalName.QuestFailed, questId);
                SaveData();
            }
        }

        /// <summary>
        /// 更新任务进度
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <param name="objectiveId">目标ID</param>
        /// <param name="progress">进度数据</param>
        public void UpdateQuestProgress(string questId, string objectiveId, object progress)
        {
            if (!_questProgress.ContainsKey(questId))
            {
                _questProgress[questId] = new Dictionary<string, object>();
            }
            _questProgress[questId][objectiveId] = progress;
            SaveData();

            // 检查任务是否完成
            CheckQuestCompletion(questId);
        }

        /// <summary>
        /// 获取任务进度
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <param name="objectiveId">目标ID</param>
        /// <returns>进度数据</returns>
        public object GetQuestProgress(string questId, string objectiveId)
        {
            if (_questProgress.TryGetValue(questId, out var progressDict))
            {
                if (progressDict.TryGetValue(objectiveId, out var progress))
                {
                    return progress;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查任务依赖
        /// </summary>
        /// <param name="dependency">依赖数据</param>
        /// <returns>依赖是否满足</returns>
        private bool CheckDependency(QuestDependency dependency)
        {
            switch (dependency.Type)
            {
                case "quest_completed":
                    return GetQuestStatus(dependency.QuestId) == QuestStatus.Completed;
                case "quest_in_progress":
                    return GetQuestStatus(dependency.QuestId) == QuestStatus.InProgress;
                case "level":
                    // 这里需要获取玩家等级
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 检查任务是否完成
        /// </summary>
        /// <param name="questId">任务ID</param>
        private void CheckQuestCompletion(string questId)
        {
            QuestData quest = GetQuest(questId);
            if (quest == null || quest.Objectives == null)
            {
                return;
            }

            bool allObjectivesCompleted = true;
            foreach (var objective in quest.Objectives)
            {
                object progress = GetQuestProgress(questId, objective.Id);
                if (!objective.IsCompleted(progress))
                {
                    allObjectivesCompleted = false;
                    break;
                }
            }

            if (allObjectivesCompleted)
            {
                CompleteQuest(questId);
            }
        }

        /// <summary>
        /// 发放任务奖励
        /// </summary>
        /// <param name="quest">任务数据</param>
        private void GrantRewards(QuestData quest)
        {
            if (quest.Rewards != null)
            {
                foreach (var reward in quest.Rewards)
                {
                    reward.Grant();
                }
            }
        }

        /// <summary>
        /// 触发成就
        /// </summary>
        /// <param name="quest">任务数据</param>
        private void TriggerAchievements(QuestData quest)
        {
            if (quest.Achievements != null)
            {
                foreach (var achievementId in quest.Achievements)
                {
                    // 这里需要触发成就系统
                    Log.Info($"Triggering achievement: {achievementId}");
                }
            }
        }

        /// <summary>
        /// 给予经验值
        /// </summary>
        /// <param name="amount">经验值数量</param>
        public void GrantExperience(int amount)
        {
            // 这里需要获取玩家并给予经验值
            Log.Info($"Granting {amount} experience");
        }

        /// <summary>
        /// 给予物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="count">物品数量</param>
        public void GrantItem(string itemId, int count)
        {
            // 这里需要获取玩家背包并给予物品
            Log.Info($"Granting {count} x {itemId}");
        }

        /// <summary>
        /// 给予货币
        /// </summary>
        /// <param name="currencyType">货币类型</param>
        /// <param name="amount">货币数量</param>
        public void GrantCurrency(string currencyType, int amount)
        {
            // 这里需要获取玩家货币并给予
            Log.Info($"Granting {amount} {currencyType}");
        }

        /// <summary>
        /// 给予装备
        /// </summary>
        /// <param name="equipmentId">装备ID</param>
        public void GrantEquipment(string equipmentId)
        {
            // 这里需要获取玩家装备栏并给予装备
            Log.Info($"Granting equipment: {equipmentId}");
        }

        /// <summary>
        /// 获取可接取的任务列表
        /// </summary>
        /// <returns>可接取的任务列表</returns>
        public List<QuestData> GetAvailableQuests()
        {
            List<QuestData> availableQuests = new List<QuestData>();
            foreach (var quest in _allQuests)
            {
                if (CanStartQuest(quest.Id))
                {
                    availableQuests.Add(quest);
                }
            }
            return availableQuests;
        }

        /// <summary>
        /// 获取进行中的任务列表
        /// </summary>
        /// <returns>进行中的任务列表</returns>
        public List<QuestData> GetActiveQuests()
        {
            List<QuestData> activeQuests = new List<QuestData>();
            foreach (var quest in _allQuests)
            {
                if (GetQuestStatus(quest.Id) == QuestStatus.InProgress)
                {
                    activeQuests.Add(quest);
                }
            }
            return activeQuests;
        }

        /// <summary>
        /// 获取已完成的任务列表
        /// </summary>
        /// <returns>已完成的任务列表</returns>
        public List<QuestData> GetCompletedQuests()
        {
            List<QuestData> completedQuests = new List<QuestData>();
            foreach (var quest in _allQuests)
            {
                if (GetQuestStatus(quest.Id) == QuestStatus.Completed)
                {
                    completedQuests.Add(quest);
                }
            }
            return completedQuests;
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns>所有任务列表</returns>
        public List<QuestData> GetAllQuests()
        {
            return _allQuests;
        }
    }
}
