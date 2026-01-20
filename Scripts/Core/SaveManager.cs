using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using hd2dtest.Scripts.Modules;

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
                
                // 设置版本信息
                if (VersionManager.Instance != null)
                {
                    saveData.GameVersion = VersionManager.Instance.GameVersion;
                    saveData.BuildDate = VersionManager.Instance.BuildDate;
                    saveData.GitCommit = VersionManager.Instance.GitCommit;
                }
                
                // 如果VersionManager实例不可用，使用默认版本号
                else
                {
                    saveData.GameVersion = "0.0.1";
                    saveData.BuildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    saveData.GitCommit = "unknown";
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
            // 创建包含单个玩家的默认存档数据
            var defaultPlayer = new hd2dtest.Scripts.Modules.Player();
            
            // 获取版本信息
            string gameVersion = "0.0.1";
            string buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string gitCommit = "unknown";
            
            if (VersionManager.Instance != null)
            {
                gameVersion = VersionManager.Instance.GameVersion;
                buildDate = VersionManager.Instance.BuildDate;
                gitCommit = VersionManager.Instance.GitCommit;
            }

            return new SaveData
            {
                // 基本存档信息
                SaveId = saveId,
                SaveName = saveName ?? $"Save {saveId}",
                SaveTime = DateTime.Now,
                GameVersion = gameVersion,
                BuildDate = buildDate,
                GitCommit = gitCommit,

                // 游戏进度
                CurrentScene = "main",
                GameScore = 0,
                PlayTime = 0,
                CompletedQuests = [],
                DiscoveredAreas = [],

                // 多个玩家状态
                Players = [defaultPlayer],

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

                                    // 计算平均玩家等级
                                    int totalLevel = 0;
                                    int playerCount = saveData.Players.Count;
                                    foreach (var player in saveData.Players)
                                    {
                                        totalLevel += player.Level;
                                    }
                                    int averageLevel = playerCount > 0 ? totalLevel / playerCount : 0;
                                    
                                    // 创建存档信息
                                    SaveInfo saveInfo = new()
                                    {
                                        SaveId = saveId,
                                        SaveName = saveData.SaveName,
                                        SaveTime = saveData.SaveTime,
                                        GameVersion = saveData.GameVersion,
                                        GameScore = saveData.GameScore,
                                        PlayerCount = playerCount,
                                        AveragePlayerLevel = averageLevel,
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