/*
 * File: SaveManager.Persistence.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose: SaveManager 的子系统持久化方法，负责保存/加载任务数据与 GameDataManager 数据。
 */

using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Quest;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    public partial class SaveManager
    {
        private void SaveQuestData(SaveData saveData)
        {
            try
            {
                if (QuestManager.Instance != null)
                {
                    var questStatuses = new Dictionary<string, int>();
                    var questProgress = new Dictionary<string, Dictionary<string, object>>();

                    try
                    {
                        var allQuests = QuestManager.Instance.GetAllQuests();
                        if (allQuests != null)
                        {
                            foreach (var quest in allQuests)
                            {
                                var status = QuestManager.Instance.GetQuestStatus(quest.Id);
                                questStatuses[quest.Id] = (int)status;

                                var progressDict = new Dictionary<string, object>();
                                if (quest.Objectives != null)
                                {
                                    foreach (var objective in quest.Objectives)
                                    {
                                        var progress = QuestManager.Instance.GetQuestProgress(quest.Id, objective.Id);
                                        if (progress != null)
                                        {
                                            progressDict[objective.Id] = progress;
                                        }
                                    }
                                }
                                if (progressDict.Count > 0)
                                {
                                    questProgress[quest.Id] = progressDict;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error saving quest data: {e.Message}");
                    }

                    saveData.QuestStatuses = questStatuses;
                    saveData.QuestProgress = questProgress;
                }

                if (QuestLineManager.Instance != null)
                {
                    var nodeStates = new Dictionary<string, int>();
                    try
                    {
                        var allQuestLines = ResourcesManager.GetAllQuestLines();
                        if (allQuestLines != null)
                        {
                            foreach (var questLine in allQuestLines)
                            {
                                if (questLine.Nodes != null)
                                {
                                    foreach (var node in questLine.Nodes)
                                    {
                                        var state = QuestLineManager.Instance.GetNodeState(node.Id);
                                        nodeStates[node.Id] = (int)state;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error saving quest line data: {e.Message}");
                    }
                    saveData.QuestLineNodeStates = nodeStates;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error in SaveQuestData: {e.Message}");
            }
        }

        private void SaveGameDataManagerData(SaveData saveData)
        {
            try
            {
                if (GameDataManager.Instance != null)
                {
                    saveData.ItemList = new Dictionary<string, int>(GameDataManager.Instance.ItemList);
                    saveData.EquipmentList = new Dictionary<string, int>(GameDataManager.Instance.EquipmentList);
                    saveData.NPCStatus = new Dictionary<string, int>(GameDataManager.Instance.NPCStatus);

                    saveData.UnlockedLevels = GameDataManager.Instance.LevelList
                        .Where(level => !level.Value.IsLocked)
                        .Select(level => level.Key)
                        .ToList();

                    saveData.UnlockedQuests = GameDataManager.Instance.QuestList
                        .Where(quest => !quest.Value.IsLocked)
                        .Select(quest => quest.Key)
                        .ToList();

                    if (GameDataManager.Instance.Teammates != null)
                    {
                        saveData.Teammates = new List<PlayerSaveData>();
                        var teammates = GameDataManager.Instance.Teammates.Get();
                        foreach (var tm in teammates)
                        {
                            saveData.Teammates.Add(new PlayerSaveData
                            {
                                PlayerId = saveData.Teammates.Count,
                                PlayerName = tm.CreatureName,
                                Level = tm.Level,
                                Experience = tm.Experience,
                                Health = tm.Health,
                                MaxHealth = tm.MaxHealth,
                                Mana = tm.CurrentMana,
                                MaxMana = tm.MaxMana,
                                Attack = (int)tm.Attack,
                                Defense = (int)tm.Defense,
                                Speed = tm.Speed,
                                Gold = tm.Gold,
                                Position = tm.Position,
                                Inventory = new Dictionary<string, int>(tm.Inventory),
                                LearnedSkills = [.. tm.Skills.Select(s => s.Id)],
                                EquippedWeapon = tm.CurrentWeapon?.WeaponName,
                                EquippedEquipment = tm.Equipments.ToDictionary(e => e.EquipmentTypeValue.ToString(), e => e.EquipmentName)
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error in SaveGameDataManagerData: {e.Message}");
            }
        }

        // private void SaveNewManagersData(SaveData saveData)
        // {
        //     ... (reserved for future managers)
        // }

        // private void LoadNewManagersData(SaveData saveData)
        // {
        //     ... (reserved for future managers)
        // }

        private void LoadQuestData(SaveData saveData)
        {
            try
            {
                if (QuestManager.Instance != null && saveData.QuestStatuses != null)
                {
                    try
                    {
                        foreach (var kvp in saveData.QuestStatuses)
                        {
                            var questId = kvp.Key;
                            var status = (QuestManager.QuestStatus)kvp.Value;
                            QuestManager.Instance.SetQuestStatus(questId, status);
                        }

                        if (saveData.QuestProgress != null)
                        {
                            foreach (var kvp in saveData.QuestProgress)
                            {
                                var questId = kvp.Key;
                                var progressDict = kvp.Value;
                                foreach (var progressKvp in progressDict)
                                {
                                    var objectiveId = progressKvp.Key;
                                    var progress = progressKvp.Value;
                                    QuestManager.Instance.UpdateQuestProgress(questId, objectiveId, progress);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error loading quest data: {e.Message}");
                    }
                }

                if (QuestLineManager.Instance != null && saveData.QuestLineNodeStates != null)
                {
                    try
                    {
                        QuestLineManager.Instance.LoadSaveData(saveData.QuestLineNodeStates);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error loading quest line data: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error in LoadQuestData: {e.Message}");
            }
        }

        private void LoadGameDataManagerData(SaveData saveData)
        {
            try
            {
                if (GameDataManager.Instance != null)
                {
                    if (saveData.ItemList != null)
                    {
                        GameDataManager.Instance.ItemList.Clear();
                        foreach (var kvp in saveData.ItemList)
                        {
                            GameDataManager.Instance.ItemList[kvp.Key] = kvp.Value;
                        }
                    }

                    if (saveData.EquipmentList != null)
                    {
                        GameDataManager.Instance.EquipmentList.Clear();
                        foreach (var kvp in saveData.EquipmentList)
                        {
                            GameDataManager.Instance.EquipmentList[kvp.Key] = kvp.Value;
                        }
                    }

                    if (saveData.NPCStatus != null)
                    {
                        GameDataManager.Instance.NPCStatus.Clear();
                        foreach (var kvp in saveData.NPCStatus)
                        {
                            GameDataManager.Instance.NPCStatus[kvp.Key] = kvp.Value;
                        }
                    }

                    if (saveData.UnlockedLevels != null)
                    {
                        foreach (var levelId in saveData.UnlockedLevels)
                        {
                            GameDataManager.Instance.UnlockLevel(levelId);
                        }
                    }

                    if (saveData.UnlockedQuests != null)
                    {
                        foreach (var questId in saveData.UnlockedQuests)
                        {
                            GameDataManager.Instance.UnlockQuest(questId);
                        }
                    }

                    if (saveData.Teammates != null && GameDataManager.Instance.Teammates != null)
                    {
                        var restoredTeammates = new List<Modules.Player>();
                        foreach (var psd in saveData.Teammates)
                        {
                            var tm = new Modules.Player();
                            ApplyPlayerSaveData(tm, psd);
                            restoredTeammates.Add(tm);
                        }
                        GameDataManager.Instance.Teammates.Set(restoredTeammates);
                    }

                    if (saveData.Players != null && saveData.Players.Count > 0)
                    {
                        var playerPsd = saveData.Players[0];
                        var player = GameDataManager.Instance.Teammates?.Player;
                        if (player == null)
                        {
                            player = new Modules.Player();
                            if (GameDataManager.Instance.Teammates != null)
                                GameDataManager.Instance.Teammates.Player = player;
                        }
                        ApplyPlayerSaveData(player, playerPsd);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error in LoadGameDataManagerData: {e.Message}");
            }
        }
    }
}
