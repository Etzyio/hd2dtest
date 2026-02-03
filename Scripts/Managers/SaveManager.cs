using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Quest;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 存档管理器，负责游戏存档的保存和加载
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为自动加载的单例使用，负责管理游戏中的存档系统。
    /// 它提供了存档的保存、加载、复制、自定义数据管理等功能，支持多个存档槽位和存档信息查询。
    /// </remarks>
    public partial class SaveManager : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static SaveManager _instance;

        /// <summary>
        /// 存档管理器的单例实例
        /// </summary>
        /// <value>存档管理器的单例实例</value>
        public static SaveManager Instance => _instance;

        /// <summary>
        /// 缓存的JsonSerializerOptions实例，用于序列化和反序列化
        /// </summary>
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// 存档目录路径
        /// </summary>
        [Export]
        public string SaveDirectory = "user://saves";

        /// <summary>
        /// 存档文件扩展名
        /// </summary>
        [Export]
        public string SaveFileExtension = ".json";

        /// <summary>
        /// 私有构造函数，防止外部实例化
        /// </summary>
        private SaveManager() { }

        /// <summary>
        /// 公共静态方法，用于获取或创建实例
        /// </summary>
        /// <returns>存档管理器的单例实例</returns>
        /// <remarks>
        /// 该方法使用懒加载模式创建存档管理器实例，并确保存档目录存在。
        /// 如果实例不存在，则创建新实例并初始化必要的设置。
        /// </remarks>
        public static SaveManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SaveManager();
                // 立即初始化必要的设置
                string globalizedPath = ProjectSettings.GlobalizePath(_instance.SaveDirectory);
                DirAccess dirAccess = DirAccess.Open(globalizedPath);
                if (dirAccess == null)
                {
                    DirAccess.MakeDirRecursiveAbsolute(globalizedPath);
                }
                Log.Info($"SaveManager initialized. Save directory: {globalizedPath}");
            }
            return _instance;
        }

        /// <summary>
        /// 节点准备就绪时调用的方法
        /// </summary>
        /// <remarks>
        /// 该方法在节点进入场景树时自动调用，用于确保单例实例指向当前节点并初始化存档目录。
        /// </remarks>
        public override void _Ready()
        {
            // 确保单例实例指向当前节点
            if (_instance != this)
            {
                _instance = this;
            }

            // 解析Godot用户路径
            string globalizedPath = ProjectSettings.GlobalizePath(SaveDirectory);

            // 确保存档目录存在
            DirAccess dirAccess = DirAccess.Open(globalizedPath);
            if (dirAccess == null)
            {
                DirAccess.MakeDirRecursiveAbsolute(globalizedPath);
            }

            Log.Info($"SaveManager _Ready() called. Save directory: {globalizedPath}");
        }

        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <returns>存档文件的完整路径</returns>
        /// <remarks>
        /// 该方法根据存档ID构建存档文件的完整路径，使用ProjectSettings.GlobalizePath解析用户路径。
        /// </remarks>
        private string GetSaveFilePath(string saveId)
        {
            return ProjectSettings.GlobalizePath($"{SaveDirectory}/save_{saveId}{SaveFileExtension}");
        }

        /// <summary>
        /// 保存游戏到指定存档槽位
        /// </summary>
        /// <param name="saveData">存档数据</param>
        /// <param name="saveId">存档ID，默认为"1"</param>
        /// <returns>保存成功返回true，失败返回false</returns>
        /// <remarks>
        /// 该方法将游戏数据保存到指定的存档槽位，包括设置保存时间、存档ID、版本信息等。
        /// 它还会保存任务数据和剧情线节点状态。
        /// 如果存档数量达到上限（20个），会拒绝新的存档请求。
        /// </remarks>
        /// <exception cref="System.Exception">保存过程中可能发生的异常</exception>
        public bool SaveGame(SaveData saveData, string saveId = "1")
        {
            try
            {
                // 检查存档数量上限
                int currentSaveCount = GetMaxSaveSlots();
                if (currentSaveCount >= 20 && !SaveExists(saveId))
                {
                    Log.Error("Save limit reached: maximum 20 saves allowed");
                    return false;
                }

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

                // 保存任务数据
                SaveQuestData(saveData);

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

        /// <summary>
        /// 保存任务数据到SaveData
        /// </summary>
        /// <param name="saveData">存档数据</param>
        /// <remarks>
        /// 该方法负责保存任务状态、任务进度和剧情线节点状态到存档数据中。
        /// 如果QuestManager或QuestLineManager实例不可用，会记录错误并跳过相应的数据保存。
        /// </remarks>
        private void SaveQuestData(SaveData saveData)
        {
            try
            {
                // 保存任务状态
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

                                // 保存任务进度
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

                // 保存剧情线节点状态
                if (QuestLineManager.Instance != null)
                {
                    var nodeStates = new Dictionary<string, int>();
                    try
                    {
                        // 获取所有剧情线节点状态
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

        /// <summary>
        /// 加载任务数据从SaveData
        /// </summary>
        /// <param name="saveData">存档数据</param>
        /// <remarks>
        /// 该方法负责从存档数据中加载任务状态、任务进度和剧情线节点状态。
        /// 如果QuestManager或QuestLineManager实例不可用，会记录错误并跳过相应的数据加载。
        /// </remarks>
        private void LoadQuestData(SaveData saveData)
        {
            try
            {
                // 加载任务状态
                if (QuestManager.Instance != null && saveData.QuestStatuses != null)
                {
                    try
                    {
                        foreach (var kvp in saveData.QuestStatuses)
                        {
                            var questId = kvp.Key;
                            var status = (QuestManager.QuestStatus)kvp.Value;
                            // 这里需要设置任务状态
                        }

                        // 加载任务进度
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

                // 加载剧情线节点状态
                if (QuestLineManager.Instance != null && saveData.QuestLineNodeStates != null)
                {
                    try
                    {
                        foreach (var kvp in saveData.QuestLineNodeStates)
                        {
                            var nodeId = kvp.Key;
                            var state = (QuestLineManager.QuestLineNodeState)kvp.Value;
                            // 设置剧情线节点状态
                            QuestLineManager.Instance.SetNodeState(nodeId, state);
                        }
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

        /// <summary>
        /// 加载指定存档槽位的游戏
        /// </summary>
        /// <param name="saveId">存档ID，默认为"1"</param>
        /// <returns>加载的存档数据，如果加载失败则返回null</returns>
        /// <remarks>
        /// 该方法从指定的存档槽位加载游戏数据，包括反序列化存档文件和加载任务数据。
        /// 如果存档文件不存在或加载失败，会记录错误并返回null。
        /// </remarks>
        /// <exception cref="System.Exception">加载过程中可能发生的异常</exception>
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

                // 加载任务数据
                LoadQuestData(saveData);

                Log.Info($"Game loaded successfully from slot {saveId} saved at {saveData.SaveTime}");
                return saveData;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load game from slot {saveId}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 检查指定存档槽位是否存在
        /// </summary>
        /// <param name="saveId">存档ID，默认为"1"</param>
        /// <returns>如果存档存在则返回true，否则返回false</returns>
        /// <remarks>
        /// 该方法检查指定ID的存档是否存在，使用Godot的FileAccess.FileExists方法。
        /// </remarks>
        public bool SaveExists(string saveId = "1")
        {
            string savePath = GetSaveFilePath(saveId);
            return Godot.FileAccess.FileExists(savePath);
        }

        /// <summary>
        /// 获取指定存档槽位的保存时间
        /// </summary>
        /// <param name="saveId">存档ID，默认为"1"</param>
        /// <returns>存档的保存时间，如果存档不存在则返回null</returns>
        /// <remarks>
        /// 该方法加载指定ID的存档并返回其保存时间。
        /// 如果存档不存在或加载失败，会返回null。
        /// </remarks>
        public DateTime? GetSaveTime(string saveId = "1")
        {
            SaveData saveData = LoadGame(saveId);
            return saveData?.SaveTime;
        }

        /// <summary>
        /// 创建默认存档数据
        /// </summary>
        /// <param name="saveId">存档ID，默认为"1"</param>
        /// <param name="saveName">存档名称，默认为null</param>
        /// <returns>创建的默认存档数据，如果创建失败则返回null</returns>
        /// <remarks>
        /// 该方法创建一个包含默认玩家数据的存档数据对象，设置基本存档信息、游戏进度和版本信息。
        /// 如果VersionManager实例可用，会使用其提供的版本信息。
        /// </remarks>
        public static SaveData CreateDefaultSaveData(string saveId = "1", string saveName = null)
        {
            try
            {
                // 创建包含单个玩家的默认存档数据
                var defaultPlayer = new hd2dtest.Scripts.Modules.Player();
                
                // 初始化玩家
                defaultPlayer.Initialize();
                
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
            catch (Exception e)
            {
                Log.Error($"Error in CreateDefaultSaveData: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有存档信息列表
        /// </summary>
        /// <returns>所有存档的信息列表</returns>
        /// <remarks>
        /// 该方法扫描存档目录，读取所有存档文件的信息，并返回一个包含存档信息的列表。
        /// 列表按存档ID的数值顺序排序，非整数ID的存档会放在最后。
        /// </remarks>
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

        /// <summary>
        /// 保存特定数据到自定义字段
        /// </summary>
        /// <param name="key">自定义数据键</param>
        /// <param name="value">自定义数据值</param>
        /// <param name="saveId">存档ID，默认为"1"</param>
        /// <remarks>
        /// 该方法将特定的自定义数据保存到指定存档槽位的CustomData字段中。
        /// 如果存档不存在，会创建一个默认存档。
        /// </remarks>
        public void SaveCustomData(string key, object value, string saveId = "1")
        {
            SaveData saveData = LoadGame(saveId) ?? CreateDefaultSaveData(saveId);
            saveData.CustomData[key] = value;
            SaveGame(saveData, saveId);
        }

        /// <summary>
        /// 加载特定自定义数据
        /// </summary>
        /// <typeparam name="T">自定义数据类型</typeparam>
        /// <param name="key">自定义数据键</param>
        /// <param name="saveId">存档ID，默认为"1"</param>
        /// <param name="defaultValue">默认值，默认为default(T)</param>
        /// <returns>加载的自定义数据，如果加载失败则返回默认值</returns>
        /// <remarks>
        /// 该方法从指定存档槽位的CustomData字段中加载特定的自定义数据，并尝试转换为指定类型。
        /// 如果存档不存在或转换失败，会返回默认值。
        /// </remarks>
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

        /// <summary>
        /// 获取最大存档槽位数
        /// </summary>
        /// <returns>当前存档数量</returns>
        /// <remarks>
        /// 该方法扫描存档目录，统计当前的存档文件数量。
        /// </remarks>
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

        /// <summary>
        /// 复制存档
        /// </summary>
        /// <param name="sourceSaveId">源存档ID</param>
        /// <param name="targetSaveId">目标存档ID</param>
        /// <returns>复制成功返回true，失败返回false</returns>
        /// <remarks>
        /// 该方法将一个存档槽位的内容复制到另一个存档槽位。
        /// 如果源存档不存在，会记录错误并返回false。
        /// </remarks>
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