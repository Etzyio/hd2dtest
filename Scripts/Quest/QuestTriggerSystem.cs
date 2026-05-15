/*
 * File: QuestTriggerSystem.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 任务触发系统，负责管理任务触发条件的检测和执行。
 * 支持多种触发类型：自动触发、任务完成触发、NPC交互触发、位置触发等。
 * 
 * Key Features:
 * - 单例模式，全局访问
 * - 支持多种触发类型（自动、任务完成、NPC交互、位置、物品收集、敌人击杀）
 * - 支持多条件组合触发
 * - 触发时自动启动任务、显示通知、播放过场动画
 */
using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    /// <summary>
    /// 任务触发系统
    /// 负责管理任务触发条件的检测和执行
    /// </summary>
    public partial class QuestTriggerSystem : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static QuestTriggerSystem Instance { get; private set; }

        /// <summary>
        /// 所有触发器列表
        /// </summary>
        private List<QuestTrigger> _triggers = new List<QuestTrigger>();

        /// <summary>
        /// 是否正在处理触发器（防止重复处理）
        /// </summary>
        private bool _isProcessing = false;

        /// <summary>
        /// 节点就绪时的回调
        /// 初始化单例实例并加载触发器数据
        /// </summary>
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeTriggers();
                Log.Info("QuestTriggerSystem initialized");
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// 初始化触发器数据
        /// 加载所有内置的触发器定义
        /// </summary>
        private void InitializeTriggers()
        {
            _triggers.Add(new QuestTrigger
            {
                Id = "trigger_intro",
                TriggerType = TriggerType.Auto,
                QuestId = "main_001",
                Conditions = new List<TriggerCondition>
                {
                    new TriggerCondition { Type = "game_start", Value = "true" }
                },
                Description = "游戏开始自动触发"
            });

            _triggers.Add(new QuestTrigger
            {
                Id = "trigger_main_002",
                TriggerType = TriggerType.QuestComplete,
                QuestId = "main_002",
                Conditions = new List<TriggerCondition>
                {
                    new TriggerCondition { Type = "quest_completed", Value = "main_001" }
                },
                Description = "完成主线任务1后触发"
            });

            _triggers.Add(new QuestTrigger
            {
                Id = "trigger_side_001",
                TriggerType = TriggerType.NPCInteraction,
                QuestId = "side_001",
                Conditions = new List<TriggerCondition>
                {
                    new TriggerCondition { Type = "npc_interaction", Value = "npc_village_elder" },
                    new TriggerCondition { Type = "level", Value = "5" }
                },
                Description = "与村长对话触发支线任务"
            });

            _triggers.Add(new QuestTrigger
            {
                Id = "trigger_side_002",
                TriggerType = TriggerType.Location,
                QuestId = "side_002",
                Conditions = new List<TriggerCondition>
                {
                    new TriggerCondition { Type = "location", Value = "forest_entrance" }
                },
                Description = "到达森林入口触发"
            });

            _triggers.Add(new QuestTrigger
            {
                Id = "trigger_main_003",
                TriggerType = TriggerType.QuestComplete,
                QuestId = "main_003",
                Conditions = new List<TriggerCondition>
                {
                    new TriggerCondition { Type = "quest_completed", Value = "main_002" },
                    new TriggerCondition { Type = "level", Value = "8" }
                },
                Description = "完成主线任务2且等级达到8级触发"
            });
        }

        /// <summary>
        /// 每帧更新方法
        /// 检测所有触发器是否满足触发条件
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        public override void _Process(double delta)
        {
            // 防止重复处理
            if (_isProcessing) return;
            
            _isProcessing = true;
            CheckTriggers();
            _isProcessing = false;
        }

        /// <summary>
        /// 检查所有触发器
        /// </summary>
        private void CheckTriggers()
        {
            foreach (var trigger in _triggers)
            {
                if (IsTriggerActive(trigger))
                {
                    ProcessTrigger(trigger);
                }
            }
        }

        /// <summary>
        /// 检查触发器是否激活
        /// </summary>
        /// <param name="trigger">触发器数据</param>
        /// <returns>是否满足触发条件</returns>
        private bool IsTriggerActive(QuestTrigger trigger)
        {
            // 检查任务是否已经处于非初始状态（已接取或完成）
            if (QuestManager.Instance.GetQuestStatus(trigger.QuestId) != QuestManager.QuestStatus.None)
            {
                return false;
            }

            // 检查所有条件是否满足
            foreach (var condition in trigger.Conditions)
            {
                if (!CheckCondition(condition))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查单个条件是否满足
        /// </summary>
        /// <param name="condition">条件数据</param>
        /// <returns>条件是否满足</returns>
        private bool CheckCondition(TriggerCondition condition)
        {
            switch (condition.Type)
            {
                case "game_start":
                    return GameManager.Instance != null;

                case "quest_completed":
                    return QuestManager.Instance.GetQuestStatus(condition.Value) == QuestManager.QuestStatus.Completed;

                case "npc_interaction":
                    return _activeNPC == condition.Value;

                case "location":
                    return _currentLocation == condition.Value;

                case "level":
                    if (GameManager.Instance?.Teammates?.Player != null)
                    {
                        return GameManager.Instance.Teammates.Player.Level >= int.Parse(condition.Value);
                    }
                    return false;

                case "item_collected":
                    return InventoryManager.Instance.HasItem(condition.Value);

                case "enemy_killed":
                    return CheckEnemyKilled(condition.Value);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 检查敌人是否被击杀
        /// </summary>
        /// <param name="enemyId">敌人ID</param>
        /// <returns>是否击杀了该敌人</returns>
        private bool CheckEnemyKilled(string enemyId)
        {
            if (GameManager.Instance?.Teammates?.Player != null)
            {
                return GameManager.Instance.Teammates.Player.KillCount > 0;
            }
            return false;
        }

        /// <summary>
        /// 处理触发器激活
        /// </summary>
        /// <param name="trigger">触发的触发器</param>
        private void ProcessTrigger(QuestTrigger trigger)
        {
            Log.Info($"Trigger activated: {trigger.Id} -> Quest: {trigger.QuestId}");
            
            // 启动任务
            QuestManager.Instance.StartQuest(trigger.QuestId);
            
            // 显示通知（如果设置）
            if (trigger.ShowNotification)
            {
                ShowQuestNotification(trigger.QuestId);
            }

            // 播放过场动画（如果设置）
            if (!string.IsNullOrEmpty(trigger.Cutscene))
            {
                StartCutscene(trigger.Cutscene);
            }
        }

        /// <summary>
        /// NPC交互事件处理
        /// </summary>
        /// <param name="npcId">NPC ID</param>
        public void OnNPCInteraction(string npcId)
        {
            _activeNPC = npcId;
            CheckTriggers();
            _activeNPC = null;
        }

        /// <summary>
        /// 进入位置事件处理
        /// </summary>
        /// <param name="locationId">位置ID</param>
        public void OnLocationEntered(string locationId)
        {
            _currentLocation = locationId;
            CheckTriggers();
        }

        /// <summary>
        /// 敌人击杀事件处理
        /// </summary>
        /// <param name="enemyId">敌人ID</param>
        public void OnEnemyKilled(string enemyId)
        {
            _lastKilledEnemy = enemyId;
            CheckTriggers();
        }

        /// <summary>
        /// 物品收集事件处理
        /// </summary>
        /// <param name="itemId">物品ID</param>
        public void OnItemCollected(string itemId)
        {
            _lastCollectedItem = itemId;
            CheckTriggers();
        }

        /// <summary>
        /// 显示任务通知
        /// </summary>
        /// <param name="questId">任务ID</param>
        private void ShowQuestNotification(string questId)
        {
            QuestData quest = QuestManager.Instance.GetQuest(questId);
            if (quest != null)
            {
                NotificationManager.Instance.ShowNotification(
                    $"新任务可用！\n{quest.Name}\n{quest.Description}",
                    5.0f
                );
            }
        }

        /// <summary>
        /// 开始过场动画
        /// </summary>
        /// <param name="cutsceneId">过场动画ID</param>
        private void StartCutscene(string cutsceneId)
        {
            Log.Info($"Starting cutscene: {cutsceneId}");
            CutsceneManager.Instance.PlayCutscene(cutsceneId);
        }

        /// <summary>
        /// 当前交互的NPC
        /// </summary>
        private string _activeNPC;

        /// <summary>
        /// 当前位置
        /// </summary>
        private string _currentLocation;

        /// <summary>
        /// 最后击杀的敌人
        /// </summary>
        private string _lastKilledEnemy;

        /// <summary>
        /// 最后收集的物品
        /// </summary>
        private string _lastCollectedItem;
    }

    /// <summary>
    /// 任务触发器数据类
    /// </summary>
    public class QuestTrigger
    {
        /// <summary>
        /// 触发器ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 触发器类型
        /// </summary>
        public TriggerType TriggerType { get; set; }

        /// <summary>
        /// 关联的任务ID
        /// </summary>
        public string QuestId { get; set; }

        /// <summary>
        /// 触发条件列表
        /// </summary>
        public List<TriggerCondition> Conditions { get; set; } = new List<TriggerCondition>();

        /// <summary>
        /// 触发器描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否显示通知
        /// </summary>
        public bool ShowNotification { get; set; } = true;

        /// <summary>
        /// 关联的过场动画ID
        /// </summary>
        public string Cutscene { get; set; }
    }

    /// <summary>
    /// 触发条件数据类
    /// </summary>
    public class TriggerCondition
    {
        /// <summary>
        /// 条件类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 条件值
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// 触发类型枚举
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        /// 自动触发
        /// </summary>
        Auto,

        /// <summary>
        /// 任务完成触发
        /// </summary>
        QuestComplete,

        /// <summary>
        /// NPC交互触发
        /// </summary>
        NPCInteraction,

        /// <summary>
        /// 位置触发
        /// </summary>
        Location,

        /// <summary>
        /// 物品收集触发
        /// </summary>
        ItemCollection,

        /// <summary>
        /// 敌人击杀触发
        /// </summary>
        EnemyKill,

        /// <summary>
        /// 时间触发
        /// </summary>
        TimeBased
    }
}
