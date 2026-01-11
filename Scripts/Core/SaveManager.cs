using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace hd2dtest.Scripts.Core
{
    public partial class SaveManager : Node
    {
        // 单例实例
        private static SaveManager _instance;
        public static SaveManager Instance => _instance;

        // 缓存的JsonSerializerOptions实例，用于序列化和反序列化
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [Export]
        public string SaveDirectory = "user://saves";

        [Export]
        public string SaveFileExtension = ".json";

        // 存档数据结构
        public class SaveData
        {
            // 基本存档信息
            public string SaveId { get; set; }  // 存档ID
            public string SaveName { get; set; }  // 存档名称
            public DateTime SaveTime { get; set; }  // 保存时间

            // 玩家状态
            public Vector2 PlayerPosition { get; set; }  // 玩家位置
            public int PlayerLevel { get; set; }  // 玩家等级
            public int PlayerExperience { get; set; }  // 玩家经验值
            public float PlayerHealth { get; set; }  // 玩家当前生命值
            public float PlayerMaxHealth { get; set; }  // 玩家最大生命值
            public float PlayerMana { get; set; }  // 玩家当前魔法值
            public float PlayerMaxMana { get; set; }  // 玩家最大魔法值
            public int PlayerAttack { get; set; }  // 玩家攻击力
            public int PlayerDefense { get; set; }  // 玩家防御力
            public float PlayerSpeed { get; set; }  // 玩家移动速度

            // 游戏进度
            public string CurrentScene { get; set; }  // 当前场景名称
            public int GameScore { get; set; }  // 游戏分数
            public int PlayTime { get; set; }  // 游戏时间（秒）
            public Dictionary<string, bool> CompletedQuests { get; set; } = [];  // 已完成任务列表
            public Dictionary<string, bool> DiscoveredAreas { get; set; } = [];  // 已发现区域

            // 物品和装备
            public Dictionary<string, int> Inventory { get; set; } = [];  // 背包物品（物品ID:数量）
            public string EquippedWeapon { get; set; }  // 已装备武器
            public Dictionary<string, string> EquippedEquipment { get; set; } = [];  // 已装备装备（装备位置:装备ID）

            // 技能
            public List<string> LearnedSkills { get; set; } = [];  // 已学习技能列表
            public Dictionary<string, int> SkillLevels { get; set; } = [];  // 技能等级（技能ID:等级）

            // 自定义数据
            public Dictionary<string, object> CustomData { get; set; } = [];  // 自定义保存数据
        }

        // 存档信息结构（用于显示存档列表）
        public class SaveInfo
        {
            public string SaveId { get; set; }  // 存档ID
            public string SaveName { get; set; }  // 存档名称
            public DateTime SaveTime { get; set; }  // 保存时间
            public int GameScore { get; set; }  // 游戏分数
            public int PlayerLevel { get; set; }  // 玩家等级
            public int PlayTime { get; set; }  // 游戏时间（秒）
            public string CurrentScene { get; set; }  // 当前场景
        }

        // 私有构造函数，防止外部实例化
        private SaveManager() { }

        // 公共静态方法，用于获取或创建实例
        public static SaveManager GetInstance()
        {
            _instance ??= new SaveManager();
            return _instance;
        }

        public override void _Ready()
        {
            // 设置单例实例
            _instance = this;

            // 解析Godot用户路径
            string globalizedPath = ProjectSettings.GlobalizePath(SaveDirectory);

            // 确保存档目录存在
            DirAccess dirAccess = DirAccess.Open(globalizedPath);
            if (dirAccess == null)
            {
                DirAccess.MakeDirRecursiveAbsolute(globalizedPath);
            }

            Log.Info($"SaveManager initialized. Save directory: {globalizedPath}");
        }

        // 获取存档文件路径
        private string GetSaveFilePath(string saveId)
        {
            return ProjectSettings.GlobalizePath($"{SaveDirectory}/save_{saveId}{SaveFileExtension}");
        }

        // 保存游戏到指定存档槽位
        public bool SaveGame(SaveData saveData, string saveId = "1")
        {
            try
            {
                // 设置保存时间和存档ID
                saveData.SaveTime = DateTime.Now;
                saveData.SaveId = saveId;

                // 如果没有设置存档名称，使用默认名称
                if (string.IsNullOrEmpty(saveData.SaveName))
                {
                    saveData.SaveName = $"Save {saveId}";
                }

                // 序列化存档数据
                string json = JsonSerializer.Serialize(saveData, _jsonSerializerOptions);

                // 获取存档路径
                string savePath = GetSaveFilePath(saveId);

                // 使用Godot FileAccess写入文件
                using (var file = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Write))
                {
                    if (file == null)
                    {
                        Log.Error($"Failed to open save file for writing: {savePath}");
                        return false;
                    }

                    file.StoreString(json);
                }

                Log.Info($"Game saved successfully to slot {saveId} at {saveData.SaveTime}");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to save game to slot {saveId}: {e.Message}");
                return false;
            }
        }

        // 加载指定存档槽位的游戏
        public SaveData LoadGame(string saveId = "1")
        {
            try
            {
                string savePath = GetSaveFilePath(saveId);

                if (!Godot.FileAccess.FileExists(savePath))
                {
                    Log.Info($"No save file found for slot {saveId}");
                    return null;
                }

                // 使用Godot FileAccess读取文件
                using var file = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    Log.Error($"Failed to open save file for reading: {savePath}");
                    return null;
                }

                string json = file.GetAsText();

                // 反序列化存档数据
                SaveData saveData = JsonSerializer.Deserialize<SaveData>(json, _jsonSerializerOptions);

                Log.Info($"Game loaded successfully from slot {saveId} saved at {saveData.SaveTime}");
                return saveData;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load game from slot {saveId}: {e.Message}");
                return null;
            }
        }

        // 删除指定存档槽位的存档
        public bool DeleteSave(string saveId = "1")
        {
            try
            {
                string savePath = GetSaveFilePath(saveId);

                if (Godot.FileAccess.FileExists(savePath))
                {
                    DirAccess.RemoveAbsolute(savePath);
                    Log.Info($"Save file deleted successfully from slot {saveId}");
                    return true;
                }

                Log.Info($"No save file to delete for slot {saveId}");
                return false;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to delete save file from slot {saveId}: {e.Message}");
                return false;
            }
        }

        // 检查指定存档槽位是否存在
        public bool SaveExists(string saveId = "1")
        {
            string savePath = GetSaveFilePath(saveId);
            return Godot.FileAccess.FileExists(savePath);
        }

        // 获取指定存档槽位的保存时间
        public DateTime? GetSaveTime(string saveId = "1")
        {
            SaveData saveData = LoadGame(saveId);
            return saveData?.SaveTime;
        }

        // 创建默认存档数据
        public static SaveData CreateDefaultSaveData(string saveId = "1", string saveName = null)
        {
            return new SaveData
            {
                // 基本存档信息
                SaveId = saveId,
                SaveName = saveName ?? $"Save {saveId}",
                SaveTime = DateTime.Now,

                // 玩家状态
                PlayerPosition = new Vector2(0, 0),
                PlayerLevel = 1,
                PlayerExperience = 0,
                PlayerHealth = 100f,
                PlayerMaxHealth = 100f,
                PlayerMana = 50f,
                PlayerMaxMana = 50f,
                PlayerAttack = 10,
                PlayerDefense = 5,
                PlayerSpeed = 50f,

                // 游戏进度
                CurrentScene = "main",
                GameScore = 0,
                PlayTime = 0,
                CompletedQuests = [],
                DiscoveredAreas = [],

                // 物品和装备
                Inventory = [],
                EquippedWeapon = "",
                EquippedEquipment = [],

                // 技能
                LearnedSkills = [],
                SkillLevels = [],

                // 自定义数据
                CustomData = []
            };
        }

        // 获取所有存档信息列表
        public List<SaveInfo> GetAllSaveInfos()
        {
            List<SaveInfo> saveInfos = [];

            try
            {
                // 解析存档目录路径
                string globalizedPath = ProjectSettings.GlobalizePath(SaveDirectory);

                // 获取所有存档文件
                DirAccess dirAccess = DirAccess.Open(globalizedPath);
                if (dirAccess != null)
                {
                    dirAccess.ListDirBegin();
                    string fileName;

                    while ((fileName = dirAccess.GetNext()) != "")
                    {
                        if (!dirAccess.CurrentIsDir() && fileName.EndsWith(SaveFileExtension))
                        {
                            try
                            {
                                // 完整文件路径
                                string filePath = $"{globalizedPath}/{fileName}";

                                // 使用Godot FileAccess读取文件
                                using var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
                                if (file != null)
                                {
                                    string json = file.GetAsText();

                                    // 反序列化存档数据
                                    SaveData saveData = JsonSerializer.Deserialize<SaveData>(json, _jsonSerializerOptions);

                                    // 提取存档ID（从文件名中提取）
                                    string baseName = fileName[..fileName.LastIndexOf(SaveFileExtension)];
                                    string saveId = baseName.Replace("save_", "");

                                    // 创建存档信息
                                    SaveInfo saveInfo = new()
                                    {
                                        SaveId = saveId,
                                        SaveName = saveData.SaveName,
                                        SaveTime = saveData.SaveTime,
                                        GameScore = saveData.GameScore,
                                        PlayerLevel = saveData.PlayerLevel,
                                        PlayTime = saveData.PlayTime,
                                        CurrentScene = saveData.CurrentScene
                                    };

                                    saveInfos.Add(saveInfo);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error($"Failed to read save file {fileName}: {e.Message}");
                            }
                        }
                    }

                    dirAccess.ListDirEnd();
                }

                // 按存档ID数值顺序排序，使用安全转换
                saveInfos = [.. saveInfos.OrderBy(s => {
                if (int.TryParse(s.SaveId, out int id))
                {
                    return id;
                }
                // 非整数ID放在最后
                return int.MaxValue;
            })];
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get save infos: {e.Message}");
            }

            return saveInfos;
        }

        // 保存特定数据到自定义字段
        public void SaveCustomData(string key, object value, string saveId = "1")
        {
            SaveData saveData = LoadGame(saveId) ?? CreateDefaultSaveData(saveId);
            saveData.CustomData[key] = value;
            SaveGame(saveData, saveId);
        }

        // 加载特定自定义数据
        public T LoadCustomData<T>(string key, string saveId = "1", T defaultValue = default)
        {
            SaveData saveData = LoadGame(saveId);
            if (saveData != null && saveData.CustomData.TryGetValue(key, out object value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to convert custom data for slot {saveId}: {e.Message}");
                }
            }
            return defaultValue;
        }

        // 获取最大存档槽位数
        public int GetMaxSaveSlots()
        {
            int count = 0;

            try
            {
                // 解析存档目录路径
                string globalizedPath = ProjectSettings.GlobalizePath(SaveDirectory);

                // 获取所有存档文件
                DirAccess dirAccess = DirAccess.Open(globalizedPath);
                if (dirAccess != null)
                {
                    dirAccess.ListDirBegin();
                    string fileName;

                    while ((fileName = dirAccess.GetNext()) != "")
                    {
                        if (!dirAccess.CurrentIsDir() && fileName.EndsWith(SaveFileExtension))
                        {
                            count++;
                        }
                    }

                    dirAccess.ListDirEnd();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get save slots count: {e.Message}");
            }

            return count;
        }

        // 删除所有存档
        public bool DeleteAllSaves()
        {
            try
            {
                // 解析存档目录路径
                string globalizedPath = ProjectSettings.GlobalizePath(SaveDirectory);

                // 获取所有存档文件
                DirAccess dirAccess = DirAccess.Open(globalizedPath);
                if (dirAccess != null)
                {
                    dirAccess.ListDirBegin();
                    string fileName;

                    while ((fileName = dirAccess.GetNext()) != "")
                    {
                        if (!dirAccess.CurrentIsDir() && fileName.EndsWith(SaveFileExtension))
                        {
                            string filePath = $"{globalizedPath}/{fileName}";
                            DirAccess.RemoveAbsolute(filePath);
                        }
                    }

                    dirAccess.ListDirEnd();
                }

                Log.Info("All save files deleted successfully");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to delete all save files: {e.Message}");
                return false;
            }
        }

        // 复制存档
        public bool CopySave(string sourceSaveId, string targetSaveId)
        {
            try
            {
                // 加载源存档
                SaveData sourceData = LoadGame(sourceSaveId);
                if (sourceData == null)
                {
                    Log.Error($"Source save {sourceSaveId} not found");
                    return false;
                }

                // 保存到目标存档槽位
                return SaveGame(sourceData, targetSaveId);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to copy save from {sourceSaveId} to {targetSaveId}: {e.Message}");
                return false;
            }
        }
    }
}