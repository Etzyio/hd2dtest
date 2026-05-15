/*
 * File: AchievementManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 成就管理器，作为全局单例负责管理游戏中的成就系统。
 * 支持成就的解锁、进度跟踪、奖励发放和查询功能。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 支持多种成就类型（战斗、探索、任务、收集、故事）
 * - 成就进度跟踪和自动解锁
 * - 成就奖励系统（金币、物品等）
 * - 支持按类型查询成就
 * - 完整的异常处理和日志记录
 */

using Godot;
using hd2dtest.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hd2dtest.Scripts.Managers
{
    public enum AchievementType
    {
        Combat,
        Exploration,
        Quest,
        Collection,
        Story
    }

    public class AchievementData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public AchievementType Type { get; set; }
        public int RequiredProgress { get; set; } = 1;
        public bool IsUnlocked { get; set; } = false;
        public int CurrentProgress { get; set; } = 0;
        public string IconPath { get; set; } = "";
        public string Reward { get; set; } = "";
    }

    public partial class AchievementManager : Node
    {
        public static AchievementManager Instance { get; private set; }

        private Dictionary<string, AchievementData> _achievements = new Dictionary<string, AchievementData>();
        private List<string> _unlockedAchievements = new List<string>();

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeAchievements();
                Log.Info("AchievementManager initialized");
            }
            else
            {
                Log.Warning("AchievementManager instance already exists");
                QueueFree();
            }
        }

        private void InitializeAchievements()
        {
            _achievements.Add("first_blood", new AchievementData
            {
                Id = "first_blood",
                Name = "First Blood",
                Description = "Defeat your first enemy",
                Type = AchievementType.Combat,
                RequiredProgress = 1,
                IconPath = "res://Icons/Achievements/first_blood.png"
            });

            _achievements.Add("killer_10", new AchievementData
            {
                Id = "killer_10",
                Name = "Beginner Slayer",
                Description = "Defeat 10 enemies",
                Type = AchievementType.Combat,
                RequiredProgress = 10,
                IconPath = "res://Icons/Achievements/killer_10.png"
            });

            _achievements.Add("killer_100", new AchievementData
            {
                Id = "killer_100",
                Name = "Experienced Warrior",
                Description = "Defeat 100 enemies",
                Type = AchievementType.Combat,
                RequiredProgress = 100,
                IconPath = "res://Icons/Achievements/killer_100.png",
                Reward = "500 Gold"
            });

            _achievements.Add("explorer_5", new AchievementData
            {
                Id = "explorer_5",
                Name = "Novice Explorer",
                Description = "Discover 5 new areas",
                Type = AchievementType.Exploration,
                RequiredProgress = 5,
                IconPath = "res://Icons/Achievements/explorer_5.png"
            });

            _achievements.Add("quest_master", new AchievementData
            {
                Id = "quest_master",
                Name = "Quest Master",
                Description = "Complete 20 quests",
                Type = AchievementType.Quest,
                RequiredProgress = 20,
                IconPath = "res://Icons/Achievements/quest_master.png",
                Reward = "Special Title"
            });

            _achievements.Add("story_completed", new AchievementData
            {
                Id = "story_completed",
                Name = "Story Complete",
                Description = "Complete the main story",
                Type = AchievementType.Story,
                RequiredProgress = 1,
                IconPath = "res://Icons/Achievements/story_completed.png",
                Reward = "Legendary Weapon"
            });
        }

        public void UnlockAchievement(string achievementId)
        {
            if (_achievements.TryGetValue(achievementId, out AchievementData achievement))
            {
                if (!achievement.IsUnlocked)
                {
                    achievement.IsUnlocked = true;
                    _unlockedAchievements.Add(achievementId);
                    Log.Info($"Achievement unlocked: {achievement.Name}");
                    
                    if (!string.IsNullOrEmpty(achievement.Reward))
                    {
                        Log.Info($"Reward: {achievement.Reward}");
                        GrantReward(achievement);
                    }
                }
            }
            else
            {
                Log.Error($"Achievement not found: {achievementId}");
            }
        }

        public bool IsAchievementUnlocked(string achievementId)
        {
            return _unlockedAchievements.Contains(achievementId);
        }

        public void UpdateProgress(string achievementId, int progress)
        {
            if (_achievements.TryGetValue(achievementId, out AchievementData achievement))
            {
                achievement.CurrentProgress += progress;
                
                if (achievement.CurrentProgress >= achievement.RequiredProgress && !achievement.IsUnlocked)
                {
                    UnlockAchievement(achievementId);
                }
                
                Log.Info($"Progress updated for {achievement.Name}: {achievement.CurrentProgress}/{achievement.RequiredProgress}");
            }
            else
            {
                Log.Error($"Achievement not found: {achievementId}");
            }
        }

        public void UpdateProgressDirect(string achievementId, int newProgress)
        {
            if (_achievements.TryGetValue(achievementId, out AchievementData achievement))
            {
                achievement.CurrentProgress = newProgress;
                
                if (achievement.CurrentProgress >= achievement.RequiredProgress && !achievement.IsUnlocked)
                {
                    UnlockAchievement(achievementId);
                }
            }
        }

        public AchievementData GetAchievement(string achievementId)
        {
            if (_achievements.TryGetValue(achievementId, out AchievementData achievement))
            {
                return achievement;
            }
            return null;
        }

        public List<AchievementData> GetAllAchievements()
        {
            return new List<AchievementData>(_achievements.Values);
        }

        public List<AchievementData> GetAchievementsByType(AchievementType type)
        {
            return _achievements.Values.Where(a => a.Type == type).ToList();
        }

        public int GetTotalUnlockedCount()
        {
            return _unlockedAchievements.Count;
        }

        public int GetTotalAchievementCount()
        {
            return _achievements.Count;
        }

        private void GrantReward(AchievementData achievement)
        {
            switch (achievement.Id)
            {
                case "killer_100":
                    AddGoldToPlayer(500);
                    break;
                case "story_completed":
                    AddLegendaryWeapon();
                    break;
            }
        }

        private void AddGoldToPlayer(int amount)
        {
            if (GameManager.Instance != null && GameManager.Instance.Teammates != null && GameManager.Instance.Teammates.Player != null)
            {
                GameManager.Instance.Teammates.Player.Gold += amount;
            }
        }

        private void AddLegendaryWeapon()
        {
            if (GameManager.Instance != null && GameManager.Instance.Teammates != null && GameManager.Instance.Teammates.Player != null)
            {
                InventoryManager.Instance.AddItem("legendary_sword", 1);
            }
        }
    }
}