using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    public partial class QuestTriggerSystem : Node
    {
        public static QuestTriggerSystem Instance { get; private set; }

        private List<QuestTrigger> _triggers = new List<QuestTrigger>();
        private bool _isProcessing = false;

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

        public override void _Process(double delta)
        {
            if (_isProcessing) return;
            
            _isProcessing = true;
            CheckTriggers();
            _isProcessing = false;
        }

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

        private bool IsTriggerActive(QuestTrigger trigger)
        {
            if (QuestManager.Instance.GetQuestStatus(trigger.QuestId) != QuestManager.QuestStatus.None)
            {
                return false;
            }

            foreach (var condition in trigger.Conditions)
            {
                if (!CheckCondition(condition))
                {
                    return false;
                }
            }

            return true;
        }

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

        private bool CheckEnemyKilled(string enemyId)
        {
            if (GameManager.Instance?.Teammates?.Player != null)
            {
                return GameManager.Instance.Teammates.Player.KillCount > 0;
            }
            return false;
        }

        private void ProcessTrigger(QuestTrigger trigger)
        {
            Log.Info($"Trigger activated: {trigger.Id} -> Quest: {trigger.QuestId}");
            
            QuestManager.Instance.StartQuest(trigger.QuestId);
            
            if (trigger.ShowNotification)
            {
                ShowQuestNotification(trigger.QuestId);
            }

            if (!string.IsNullOrEmpty(trigger.Cutscene))
            {
                StartCutscene(trigger.Cutscene);
            }
        }

        public void OnNPCInteraction(string npcId)
        {
            _activeNPC = npcId;
            CheckTriggers();
            _activeNPC = null;
        }

        public void OnLocationEntered(string locationId)
        {
            _currentLocation = locationId;
            CheckTriggers();
        }

        public void OnEnemyKilled(string enemyId)
        {
            _lastKilledEnemy = enemyId;
            CheckTriggers();
        }

        public void OnItemCollected(string itemId)
        {
            _lastCollectedItem = itemId;
            CheckTriggers();
        }

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

        private void StartCutscene(string cutsceneId)
        {
            Log.Info($"Starting cutscene: {cutsceneId}");
            CutsceneManager.Instance.PlayCutscene(cutsceneId);
        }

        private string _activeNPC;
        private string _currentLocation;
        private string _lastKilledEnemy;
        private string _lastCollectedItem;
    }

    public class QuestTrigger
    {
        public string Id { get; set; }
        public TriggerType TriggerType { get; set; }
        public string QuestId { get; set; }
        public List<TriggerCondition> Conditions { get; set; } = new List<TriggerCondition>();
        public string Description { get; set; }
        public bool ShowNotification { get; set; } = true;
        public string Cutscene { get; set; }
    }

    public class TriggerCondition
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public enum TriggerType
    {
        Auto,
        QuestComplete,
        NPCInteraction,
        Location,
        ItemCollection,
        EnemyKill,
        TimeBased
    }
}
