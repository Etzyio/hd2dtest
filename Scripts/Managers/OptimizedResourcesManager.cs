using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Quest;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 资源加载优先级
    /// </summary>
    public enum ResourceLoadPriority
    {
        Critical = 0,
        High = 1,
        Medium = 2,
        Low = 3
    }

    /// <summary>
    /// 资源加载状态
    /// </summary>
    public enum ResourceLoadStatus
    {
        NotLoaded,
        Loading,
        Loaded,
        Failed
    }

    /// <summary>
    /// 资源加载信息
    /// </summary>
    public class ResourceLoadInfo
    {
        public string ResourceName { get; set; }
        public ResourceLoadPriority Priority { get; set; }
        public ResourceLoadStatus Status { get; set; }
        public double LoadTimeMs { get; set; }
        public long FileSizeBytes { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 优化的资源管理器，支持异步加载和性能优化
    /// </summary>
    public partial class OptimizedResourcesManager: Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static OptimizedResourcesManager _instance;

        /// <summary>
        /// 资源管理器的单例实例
        /// </summary>
        public static OptimizedResourcesManager Instance => _instance;

        /// <summary>
        /// 资源加载完成事件
        /// </summary>
        public event Action<ResourceLoadInfo> ResourceLoaded;

        /// <summary>
        /// 所有资源加载完成事件
        /// </summary>
        public event Action AllResourcesLoaded;

        /// <summary>
        /// 资源加载进度事件
        /// </summary>
        public event Action<float, string> LoadProgress;

        /// <summary>
        /// 技能缓存字典
        /// </summary>
        public static readonly Dictionary<string, Skill> SkillsCache = [];

        /// <summary>
        /// 物品缓存字典
        /// </summary>
        public static readonly Dictionary<string, Item> ItemsCache = [];

        /// <summary>
        /// NPC缓存字典
        /// </summary>
        public static readonly Dictionary<string, NPC> NPCsCache = [];

        /// <summary>
        /// 怪物缓存字典
        /// </summary>
        public static readonly Dictionary<string, Monster> MonstersCache = [];

        /// <summary>
        /// 武器缓存字典
        /// </summary>
        public static readonly Dictionary<string, Weapon> WeaponsCache = [];

        /// <summary>
        /// 装备缓存字典
        /// </summary>
        public static readonly Dictionary<string, Equipment> EquipmentCache = [];

        /// <summary>
        /// 关卡缓存字典
        /// </summary>
        public static readonly Dictionary<string, Level> LevelsCache = [];

        /// <summary>
        /// 任务缓存字典
        /// </summary>
        public static readonly Dictionary<string, QuestData> QuestsCache = [];

        /// <summary>
        /// 任务线缓存字典
        /// </summary>
        public static readonly Dictionary<string, QuestLineData> QuestLinesCache = [];

        /// <summary>
        /// 视图注册缓存字典
        /// </summary>
        public static readonly Dictionary<string, string> ViewRegister = [];

        /// <summary>
        /// 资源加载信息字典
        /// </summary>
        private static readonly Dictionary<string, ResourceLoadInfo> _loadInfoCache = [];

        /// <summary>
        /// 默认资源路径
        /// </summary>
        private const string DefaultResourcesPath = "res://Resources/Static/";

        /// <summary>
        /// 本地化资源路径
        /// </summary>
        private const string LocalizationPath = "res://Resources/Localization/";

        /// <summary>
        /// JSON序列化选项
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// 资源加载配置
        /// </summary>
        private static readonly Dictionary<string, ResourceLoadPriority> ResourcePriorities = new()
        {
            { "Skills.json", ResourceLoadPriority.Medium },
            { "Items.json", ResourceLoadPriority.Medium },
            { "NPCs.json", ResourceLoadPriority.Medium },
            { "Monsters.json", ResourceLoadPriority.Medium },
            { "Weapons.json", ResourceLoadPriority.Medium },
            { "Equipment.json", ResourceLoadPriority.Medium },
            { "Levels.json", ResourceLoadPriority.High },
            { "ViewRegister.json", ResourceLoadPriority.High },
            { "Quests", ResourceLoadPriority.Low },
            { "QuestLines", ResourceLoadPriority.Low }
        };

        /// <summary>
        /// 是否正在加载资源
        /// </summary>
        private static bool _isLoading = false;

        /// <summary>
        /// 加载计时器
        /// </summary>
        private static readonly Stopwatch _loadTimer = new();

        /// <summary>
        /// 节点准备就绪时调用的方法
        /// </summary>
        public override void _Ready()
        {
            _instance = this;
        }

        /// <summary>
        /// 异步初始化所有资源
        /// </summary>
        /// <returns>异步任务</returns>
        public async Task InitializeResourcesAsync()
        {
            if (_isLoading)
            {
                Log.Warning("Resource loading already in progress");
                return;
            }

            _isLoading = true;
            _loadTimer.Restart();

            try
            {
                Log.Info("Starting asynchronous resource initialization...");

                var loadTasks = new List<Task>();

                LoadProgress?.Invoke(0.1f, "Starting resource loading...");

                await Task.Delay(100);

                loadTasks.Add(LoadCriticalResourcesAsync());
                await Task.WhenAll(loadTasks);

                LoadProgress?.Invoke(0.3f, "Loading high priority resources...");
                await Task.Delay(50);

                loadTasks.Clear();
                loadTasks.Add(LoadHighPriorityResourcesAsync());
                await Task.WhenAll(loadTasks);

                LoadProgress?.Invoke(0.6f, "Loading medium priority resources...");
                await Task.Delay(50);

                loadTasks.Clear();
                loadTasks.Add(LoadMediumPriorityResourcesAsync());
                await Task.WhenAll(loadTasks);

                LoadProgress?.Invoke(0.8f, "Loading low priority resources...");
                await Task.Delay(50);

                loadTasks.Clear();
                loadTasks.Add(LowPriorityResourcesAsync());
                await Task.WhenAll(loadTasks);

                LoadProgress?.Invoke(1.0f, "Resource loading completed!");

                _loadTimer.Stop();

                Log.Info($"Resource initialization completed in {_loadTimer.ElapsedMilliseconds}ms");
                Log.Info($"Skills: {SkillsCache.Count}, Items: {ItemsCache.Count}, NPCs: {NPCsCache.Count}, " +
                        $"Monsters: {MonstersCache.Count}, Weapons: {WeaponsCache.Count}, " +
                        $"Equipment: {EquipmentCache.Count}, Levels: {LevelsCache.Count}, " +
                        $"Quests: {QuestsCache.Count}, QuestLines: {QuestLinesCache.Count}");

                AllResourcesLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error($"Resource initialization failed: {ex.Message}");
                LoadProgress?.Invoke(1.0f, $"Resource loading failed: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// 加载关键资源
        /// </summary>
        private async Task LoadCriticalResourcesAsync()
        {
            await Task.Run(() =>
            {
                LoadResourceToCacheWithTracking("ViewRegister.json", ViewRegister, ResourceLoadPriority.High);
            });
        }

        /// <summary>
        /// 加载高优先级资源
        /// </summary>
        private async Task LoadHighPriorityResourcesAsync()
        {
            await Task.Run(() =>
            {
                LoadResourceToCacheWithTracking("Levels.json", LevelsCache, ResourceLoadPriority.High);
            });
        }

        /// <summary>
        /// 加载中优先级资源
        /// </summary>
        private async Task LoadMediumPriorityResourcesAsync()
        {
            await Task.Run(() =>
            {
                LoadResourceToCacheWithTracking("Skills.json", SkillsCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("Items.json", ItemsCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("NPCs.json", NPCsCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("Monsters.json", MonstersCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("Weapons.json", WeaponsCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("Equipment.json", EquipmentCache, ResourceLoadPriority.Medium);
            });
        }

        /// <summary>
        /// 加载低优先级资源
        /// </summary>
        private static async Task LowPriorityResourcesAsync()
        {
            await Task.Run(() =>
            {
                LoadQuestsAsync();
                LoadQuestLinesAsync();
            });
        }

        /// <summary>
        /// 带跟踪的资源加载
        /// </summary>
        private void LoadResourceToCacheWithTracking<T>(string fileName, Dictionary<string, T> cache, ResourceLoadPriority priority)
        {
            var loadInfo = new ResourceLoadInfo
            {
                ResourceName = fileName,
                Priority = priority,
                Status = ResourceLoadStatus.Loading
            };

            _loadInfoCache[fileName] = loadInfo;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                string filePath = DefaultResourcesPath + fileName;

                if (!FileAccess.FileExists(filePath))
                {
                    loadInfo.Status = ResourceLoadStatus.Failed;
                    loadInfo.ErrorMessage = "File not found";
                    Log.Warning($"JSON file not found: {filePath}");
                    ResourceLoaded?.Invoke(loadInfo);
                    return;
                }

                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                loadInfo.FileSizeBytes = (long)file.GetLength();

                string jsonContent = file.GetAsText();

                if (string.IsNullOrEmpty(jsonContent))
                {
                    loadInfo.Status = ResourceLoadStatus.Failed;
                    loadInfo.ErrorMessage = "File is empty";
                    Log.Warning($"JSON file is empty: {filePath}");
                    ResourceLoaded?.Invoke(loadInfo);
                    return;
                }

                var loadedData = JsonSerializer.Deserialize<Dictionary<string, T>>(jsonContent, JsonOptions);

                cache.Clear();
                if (loadedData != null)
                {
                    foreach (var item in loadedData)
                    {
                        cache[item.Key] = item.Value;
                    }
                }

                stopwatch.Stop();
                loadInfo.Status = ResourceLoadStatus.Loaded;
                loadInfo.LoadTimeMs = stopwatch.ElapsedMilliseconds;

                Log.Info($"Successfully loaded {fileName} in {loadInfo.LoadTimeMs:F2}ms ({loadInfo.FileSizeBytes / 1024:F2}KB)");
                ResourceLoaded?.Invoke(loadInfo);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                loadInfo.Status = ResourceLoadStatus.Failed;
                loadInfo.ErrorMessage = ex.Message;
                loadInfo.LoadTimeMs = stopwatch.ElapsedMilliseconds;
                Log.Error($"Failed to load {fileName}: {ex.Message}");
                ResourceLoaded?.Invoke(loadInfo);
            }
        }

        /// <summary>
        /// 异步加载任务数据
        /// </summary>
        private static void LoadQuestsAsync()
        {
            try
            {
                string questsDir = "res://Resources/Static/Quests";
                var dir = DirAccess.Open(questsDir);
                if (dir != null)
                {
                    dir.ListDirBegin();
                    string file;
                    QuestsCache.Clear();

                    while ((file = dir.GetNext()) != "")
                    {
                        if (!(file.Length > 0 && file[0] == '.') && file.EndsWith(".json"))
                        {
                            try
                            {
                                var questData = LoadResource<QuestData>(file, questsDir + "/");
                                if (questData != null && !string.IsNullOrEmpty(questData.Id))
                                {
                                    QuestsCache[questData.Id] = questData;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Failed to load quest file {file}: {ex.Message}");
                            }
                        }
                    }

                    dir.ListDirEnd();
                }

                Log.Info($"Successfully loaded {QuestsCache.Count} quests");
            }
            catch (Exception e)
            {
                Log.Error($"Error loading quests: {e.Message}");
                QuestsCache.Clear();
            }
        }

        /// <summary>
        /// 异步加载任务线数据
        /// </summary>
        private static void LoadQuestLinesAsync()
        {
            try
            {
                string questLinesDir = "res://Resources/Static/Quests";
                var dir = DirAccess.Open(questLinesDir);
                if (dir != null)
                {
                    dir.ListDirBegin();
                    string file;
                    QuestLinesCache.Clear();

                    while ((file = dir.GetNext()) != "")
                    {
                        if (!(file.Length > 0 && file[0] == '.') && file.EndsWith(".json"))
                        {
                            try
                            {
                                var questLineData = LoadResource<QuestLineData>(file, questLinesDir + "/");
                                if (questLineData != null && !string.IsNullOrEmpty(questLineData.Id))
                                {
                                    QuestLinesCache[questLineData.Id] = questLineData;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Failed to load quest line file {file}: {ex.Message}");
                            }
                        }
                    }

                    dir.ListDirEnd();
                }

                Log.Info($"Successfully loaded {QuestLinesCache.Count} quest lines");
            }
            catch (Exception e)
            {
                Log.Error($"Error loading quest lines: {e.Message}");
                QuestLinesCache.Clear();
            }
        }

        /// <summary>
        /// 从JSON文件加载数据并反序列化为对象
        /// </summary>
        public static T LoadResource<T>(string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;

            if (!FileAccess.FileExists(filePath))
            {
                Log.Warning($"JSON file not found: {filePath}");
                return default;
            }

            try
            {
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();

                if (string.IsNullOrEmpty(jsonContent))
                {
                    Log.Warning($"JSON file is empty: {filePath}");
                    return default;
                }

                T result = JsonSerializer.Deserialize<T>(jsonContent, JsonOptions);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"JSON deserialization failed: {filePath}, Error: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        public static List<QuestData> GetAllQuests()
        {
            return [.. QuestsCache.Values];
        }

        /// <summary>
        /// 根据ID获取任务
        /// </summary>
        public static QuestData GetQuest(string questId)
        {
            if (QuestsCache.TryGetValue(questId, out QuestData questData))
            {
                return questData;
            }
            return null;
        }

        /// <summary>
        /// 获取所有任务线
        /// </summary>
        public static List<QuestLineData> GetAllQuestLines()
        {
            return [.. QuestLinesCache.Values];
        }

        /// <summary>
        /// 根据ID获取任务线
        /// </summary>
        public static QuestLineData GetQuestLine(string questLineId)
        {
            if (QuestLinesCache.TryGetValue(questLineId, out QuestLineData questLineData))
            {
                return questLineData;
            }
            return null;
        }

        /// <summary>
        /// 根据ID获取技能
        /// </summary>
        public static Skill GetSkill(string skillId)
        {
            if (SkillsCache.TryGetValue(skillId, out Skill skill))
            {
                return skill;
            }
            return null;
        }

        /// <summary>
        /// 根据ID获取物品
        /// </summary>
        public static Item GetItem(string itemId)
        {
            if (ItemsCache.TryGetValue(itemId, out Item item))
            {
                return item;
            }
            return null;
        }

        /// <summary>
        /// 根据ID获取NPC
        /// </summary>
        public static NPC GetNPC(string npcId)
        {
            if (NPCsCache.TryGetValue(npcId, out NPC npc))
            {
                return npc;
            }
            return null;
        }

        /// <summary>
        /// 根据ID获取怪物
        /// </summary>
        public static Monster GetMonster(string monsterId)
        {
            if (MonstersCache.TryGetValue(monsterId, out Monster monster))
            {
                return monster;
            }
            return null;
        }

        /// <summary>
        /// 根据ID获取武器
        /// </summary>
        public static Weapon GetWeapon(string weaponId)
        {
            if (WeaponsCache.TryGetValue(weaponId, out Weapon weapon))
            {
                return weapon;
            }
            return null;
        }

        /// <summary>
        /// 根据ID获取装备
        /// </summary>
        public static Equipment GetEquipment(string equipmentId)
        {
            if (EquipmentCache.TryGetValue(equipmentId, out Equipment equipment))
            {
                return equipment;
            }
            return null;
        }

        /// <summary>
        /// 根据ID获取关卡
        /// </summary>
        public static Level GetLevel(string levelId)
        {
            if (LevelsCache.TryGetValue(levelId, out Level level))
            {
                return level;
            }
            return null;
        }

        /// <summary>
        /// 获取资源加载信息
        /// </summary>
        public static Dictionary<string, ResourceLoadInfo> GetLoadInfo()
        {
            return new Dictionary<string, ResourceLoadInfo>(_loadInfoCache);
        }

        /// <summary>
        /// 获取加载性能统计
        /// </summary>
        public static string GetPerformanceStats()
        {
            var stats = new System.Text.StringBuilder();
            stats.AppendLine("=== Resource Loading Performance Stats ===");
            stats.AppendLine($"Total Load Time: {_loadTimer.ElapsedMilliseconds}ms");
            stats.AppendLine();

            foreach (var info in _loadInfoCache.Values)
            {
                stats.AppendLine($"{info.ResourceName}:");
                stats.AppendLine($"  Status: {info.Status}");
                stats.AppendLine($"  Priority: {info.Priority}");
                stats.AppendLine($"  Load Time: {info.LoadTimeMs:F2}ms");
                stats.AppendLine($"  File Size: {info.FileSizeBytes / 1024:F2}KB");
                if (!string.IsNullOrEmpty(info.ErrorMessage))
                {
                    stats.AppendLine($"  Error: {info.ErrorMessage}");
                }
                stats.AppendLine();
            }

            return stats.ToString();
        }

        /// <summary>
        /// 清除所有资源缓存
        /// </summary>
        public static void ClearAllCaches()
        {
            SkillsCache.Clear();
            ItemsCache.Clear();
            NPCsCache.Clear();
            MonstersCache.Clear();
            WeaponsCache.Clear();
            EquipmentCache.Clear();
            LevelsCache.Clear();
            QuestsCache.Clear();
            QuestLinesCache.Clear();
            ViewRegister.Clear();
            _loadInfoCache.Clear();
            Log.Info("All resource caches cleared");
        }
    }
}