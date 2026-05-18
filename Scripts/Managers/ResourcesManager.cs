/*
 * File: ResourcesManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 资源管理器，作为全局单例负责管理游戏中的所有静态资源。
 * 支持异步加载、优先级排序、缓存管理和资源查询。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 异步资源加载，支持优先级排序（Critical/High/Medium/Low）
 * - 多级缓存机制（技能、物品、NPC、怪物、武器、装备、关卡、任务等）
 * - ViewRegister场景注册管理
 * - 资源加载进度事件和统计
 * - JSON序列化/反序列化支持
 * - 资源重新加载和缓存清理功能
 * - 完整的异常处理和性能监控
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Quest;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 资源管理器，负责加载和解析JSON文件
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为自动加载的单例使用，负责管理游戏中的所有资源，包括技能、物品、NPC、怪物、武器、装备、关卡、任务等。
    /// 它提供了资源的加载、保存、缓存管理和本地化等功能，确保资源的高效访问和管理。
    /// </remarks>
    public partial class ResourcesManager : Node
    {
        /// 单例实例
        /// </summary>
        private static ResourcesManager _instance;

        /// <summary>
        /// 资源管理器的单例实例
        /// </summary>
        /// <value>资源管理器的单例实例</value>
        public static ResourcesManager Instance => _instance;

        /// <summary>
        /// 获取资源管理器实例
        /// </summary>
        /// <returns>资源管理器实例</returns>
        public static ResourcesManager GetInstance()
        {
            if (_instance == null)
            {
                Log.Warning("ResourcesManager instance not initialized, creating new instance");
                _instance = new ResourcesManager();
            }
            return _instance;
        }

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
        /// 地图缓存字典
        /// </summary>
        public static readonly Dictionary<string, MapData> MapsCache = [];

        /// <summary>
        /// 视图注册缓存字典
        /// </summary>
        public static readonly Dictionary<string, string> ViewRegister = [];

        /// <summary>
        /// 资源加载信息字典
        /// </summary>
        private static readonly Dictionary<string, ResourceLoadInfo> _loadInfoCache = [];

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
            { "Maps.json", ResourceLoadPriority.Medium },
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
        /// 默认资源路径
        /// </summary>
        private const string DefaultResourcesPath = "res://Resources/Static/";
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
            Converters = {
                new JsonStringEnumConverter(),
                new QuestObjectiveConverter(),
                new QuestRewardConverter(),
                new Vector3JsonConverter()
            }
        };

        /// <summary>
        /// 节点准备就绪时调用的方法
        /// </summary>
        /// <remarks>
        /// 该方法在节点进入场景树时自动调用，用于设置单例实例并初始化所有资源。
        /// </remarks>
        public override void _Ready()
        {
            _instance = this;

            // 同步加载ViewRegister.json（必须先加载完成）
            LoadViewRegisterSync();

            // 然后异步初始化其他资源
            _ = InitializeResourcesAsync();
        }

        /// <summary>
        /// 同步加载ViewRegister.json
        /// </summary>
        private static void LoadViewRegisterSync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                string filePath = DefaultResourcesPath + "ViewRegister.json";

                Log.Info("Sync loading ViewRegister.json...");

                // 检查文件是否存在
                if (!FileAccess.FileExists(filePath))
                {
                    stopwatch.Stop();
                    Log.Error($"ViewRegister.json file not found: {filePath}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                    ViewRegister.Clear();
                    return;
                }

                // 使用Godot的FileAccess API加载文件
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();

                if (string.IsNullOrEmpty(jsonContent))
                {
                    stopwatch.Stop();
                    Log.Error($"ViewRegister.json file is empty: {filePath}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                    ViewRegister.Clear();
                    return;
                }

                // 解析JSON数组
                var viewRegisterArray = JsonSerializer.Deserialize<List<ViewRegisterItem>>(jsonContent, JsonOptions);
                if (viewRegisterArray == null)
                {
                    stopwatch.Stop();
                    Log.Error($"ViewRegister.json parsing failed: {filePath}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                    ViewRegister.Clear();
                    return;
                }

                // 转换为字典
                ViewRegister.Clear();
                foreach (var item in viewRegisterArray)
                {
                    if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.Tscn))
                    {
                        ViewRegister[item.Id] = item.Tscn;
                    }
                }

                stopwatch.Stop();
                Log.Info($"Successfully loaded ViewRegister.json: {filePath}, Items: {ViewRegister.Count}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error($"Failed to load ViewRegister.json: {ex.Message}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                ViewRegister.Clear();
            }
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
                loadTasks.Add(LoadLowPriorityResourcesAsync());
                await Task.WhenAll(loadTasks);

                LoadProgress?.Invoke(1.0f, "Resource loading completed!");

                _loadTimer.Stop();

                Log.Info($"Resource initialization completed - Skills: {SkillsCache.Count}, Items: {ItemsCache.Count}, NPCs: {NPCsCache.Count}, Monsters: {MonstersCache.Count}, Weapons: {WeaponsCache.Count}, Equipment: {EquipmentCache.Count}, Levels: {LevelsCache.Count}, Maps: {MapsCache.Count}, Quests: {QuestsCache.Count}, QuestLines: {QuestLinesCache.Count}, Dialogues: {DialoguesCache.Count}");
                Log.Info($"Total load time: {_loadTimer.ElapsedMilliseconds}ms");

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
        private static async Task LoadCriticalResourcesAsync()
        {
            // ViewRegister.json已经在LoadViewRegisterSync中同步加载完成
            // 这里只需要标记为已加载
            var loadInfo = new ResourceLoadInfo
            {
                ResourceName = "ViewRegister.json",
                Priority = ResourceLoadPriority.High,
                Status = ResourceLoadStatus.Loaded,
                LoadTimeMs = 0,
                FileSizeBytes = 0
            };
            _loadInfoCache["ViewRegister.json"] = loadInfo;
            Log.Info("ViewRegister.json already loaded synchronously");

            // 加载其他关键资源
            await Task.Run(() =>
            {
                // 其他关键资源加载
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
                LoadSkillsToCacheWithTracking("Skills.json", SkillsCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("Items.json", ItemsCache, ResourceLoadPriority.Medium);
                LoadNPCsToCacheWithTracking("NPCs.json", NPCsCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("Monsters.json", MonstersCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("Weapons.json", WeaponsCache, ResourceLoadPriority.Medium);
                LoadResourceToCacheWithTracking("Equipment.json", EquipmentCache, ResourceLoadPriority.Medium);
                LoadMaps();
            });
        }

        /// <summary>
        /// 加载低优先级资源
        /// </summary>
        private static async Task LoadLowPriorityResourcesAsync()
        {
            await Task.Run(() =>
            {
                LoadQuests();
                LoadQuestLines();
                LoadDialogues();
            });
        }

        /// <summary>
        /// 从JSON文件加载数据并反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="fileName">JSON文件名（带扩展名）</param>
        /// <param name="customPath">自定义资源路径（可选）</param>
        /// <returns>反序列化后的对象，如果失败则返回默认值</returns>
        /// <remarks>
        /// 该方法使用Godot的FileAccess API加载JSON文件，然后使用JsonSerializer反序列化为指定类型的对象。
        /// 如果文件不存在或反序列化失败，会记录警告或错误日志并返回默认值。
        /// </remarks>
        public static T LoadResource<T>(string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
            var stopwatch = Stopwatch.StartNew();

            // 检查文件是否存在
            if (!FileAccess.FileExists(filePath))
            {
                stopwatch.Stop();
                Log.Warning($"JSON file not found: {filePath}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                return default;
            }

            try
            {
                // 使用Godot的FileAccess API加载文件
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();

                if (string.IsNullOrEmpty(jsonContent))
                {
                    stopwatch.Stop();
                    Log.Warning($"JSON file is empty: {filePath}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                    return default;
                }

                // 反序列化为目标类型
                T result = JsonSerializer.Deserialize<T>(jsonContent, JsonOptions);
                stopwatch.Stop();
                Log.Info($"Successfully loaded JSON file: {filePath}, Type: {typeof(T).Name}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error($"JSON deserialization failed: {filePath}, Error: {ex.Message}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                return default;
            }
        }

        /// <summary>
        /// 将对象序列化为JSON并保存到文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="data">要保存的数据</param>
        /// <param name="fileName">要保存的文件名（带扩展名）</param>
        /// <param name="customPath">自定义资源路径（可选）</param>
        /// <returns>保存是否成功</returns>
        /// <remarks>
        /// 该方法使用JsonSerializer将对象序列化为JSON字符串，然后使用Godot的FileAccess API保存到文件。
        /// 如果保存过程中发生异常，会记录错误日志并返回false。
        /// </remarks>
        public static bool SaveResource<T>(T data, string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;

            try
            {
                // 将对象序列化为JSON
                string jsonContent = JsonSerializer.Serialize(data, JsonOptions);

                // 使用Godot的FileAccess API保存文件
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
                file.StoreString(jsonContent);

                Log.Debug($"Successfully saved JSON file: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"JSON serialization failed: {filePath}, Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载资源到缓存字典
        /// </summary>
        /// <typeparam name="T">字典的值类型</typeparam>
        /// <param name="fileName">JSON文件名（带扩展名）</param>
        /// <param name="cache">目标缓存字典</param>
        /// <remarks>
        /// 该方法从JSON文件加载数据并填充到指定的缓存字典中。
        /// 如果加载失败，会清空缓存字典。
        /// </remarks>
        private static void LoadResourceToCache<T>(string fileName, Dictionary<string, T> cache)
        {
            var stopwatch = Stopwatch.StartNew();
            var loadedData = LoadResource<Dictionary<string, T>>(fileName);
            stopwatch.Stop();

            if (loadedData != null)
            {
                cache.Clear();
                foreach (var item in loadedData)
                {
                    cache[item.Key] = item.Value;
                }
                Log.Info($"Successfully loaded resource to cache: {fileName}, Items: {cache.Count}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
            }
            else
            {
                cache.Clear();
                Log.Warning($"Failed to load resource to cache: {fileName}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
            }
        }

        /// <summary>
        /// 带跟踪的资源加载
        /// </summary>
        /// <typeparam name="T">字典的值类型</typeparam>
        /// <param name="fileName">JSON文件名（带扩展名）</param>
        /// <param name="cache">目标缓存字典</param>
        /// <param name="priority">加载优先级</param>
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

        private void LoadSkillsToCacheWithTracking(string fileName, Dictionary<string, Skill> cache, ResourceLoadPriority priority)
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

                cache.Clear();

                using var doc = JsonDocument.Parse(jsonContent);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("Skills.json root must be an object");
                }

                var root = doc.RootElement;
                bool loadedAny = false;

                if (root.TryGetProperty("active_skills", out var activeSkills) && activeSkills.ValueKind == JsonValueKind.Object)
                {
                    LoadSkillsSection(activeSkills, cache);
                    loadedAny = loadedAny || cache.Count > 0;
                }

                if (root.TryGetProperty("passive_skills", out var passiveSkills) && passiveSkills.ValueKind == JsonValueKind.Object)
                {
                    int before = cache.Count;
                    LoadSkillsSection(passiveSkills, cache);
                    loadedAny = loadedAny || cache.Count > before;
                }

                if (!loadedAny)
                {
                    var fallback = JsonSerializer.Deserialize<Dictionary<string, Skill>>(jsonContent, JsonOptions);
                    if (fallback != null)
                    {
                        foreach (var kv in fallback)
                        {
                            cache[kv.Key] = kv.Value;
                        }
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

        private void LoadNPCsToCacheWithTracking(string fileName, Dictionary<string, NPC> cache, ResourceLoadPriority priority)
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

                cache.Clear();

                var wrapper = JsonSerializer.Deserialize<NpcStaticDataWrapper>(jsonContent, JsonOptions);
                if (wrapper?.Npcs != null && wrapper.Npcs.Count > 0)
                {
                    foreach (var data in wrapper.Npcs)
                    {
                        if (data == null || string.IsNullOrWhiteSpace(data.Id))
                        {
                            continue;
                        }

                        var npc = new NPC
                        {
                            Id = data.Id,
                            CreatureName = data.Name ?? string.Empty,
                            Dialogue = string.IsNullOrWhiteSpace(data.Dialogue)
                                ? TranslationServer.Translate("npc_default_dialogue")
                                : data.Dialogue,
                            DialogueGraphId = data.DialogueGraphId,
                            IsInteractive = data.IsInteractive ?? true,
                            DetectionRadius = data.DetectionRadius ?? 50f
                        };

                        if (data.Type.HasValue)
                        {
                            npc.Type = data.Type.Value;
                        }

                        if (data.AvailableInteractions != null && data.AvailableInteractions.Count > 0)
                        {
                            npc.AvailableInteractions = data.AvailableInteractions;
                        }

                        cache[npc.Id] = npc;
                    }
                }
                else
                {
                    var fallback = JsonSerializer.Deserialize<Dictionary<string, NPC>>(jsonContent, JsonOptions);
                    if (fallback != null)
                    {
                        foreach (var kv in fallback)
                        {
                            cache[kv.Key] = kv.Value;
                        }
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

        private static void LoadSkillsSection(JsonElement section, Dictionary<string, Skill> cache)
        {
            foreach (var prop in section.EnumerateObject())
            {
                if (prop.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var skill = ConvertSkillsJsonEntryToSkill(prop.Name, prop.Value);
                if (skill == null)
                {
                    continue;
                }

                cache[skill.Id] = skill;
            }
        }

        private static Skill ConvertSkillsJsonEntryToSkill(string fallbackId, JsonElement entry)
        {
            string id = TryGetString(entry, "id") ?? fallbackId;
            string skillNameKey = TryGetString(entry, "skill_name_key");
            string descriptionKey = TryGetString(entry, "description_key");

            float cooldown = (float)(TryGetDouble(entry, "cooldown") ?? 0d);
            string attackType = TryGetString(entry, "attack_type") ?? string.Empty;
            string typeStr = TryGetString(entry, "type") ?? string.Empty;

            float damageCoefficient = (float)(TryGetDouble(entry, "damage_coefficient") ?? 0d);
            int buffDuration = TryGetInt(entry, "buff_duration") ?? 0;
            int debuffDuration = TryGetInt(entry, "debuff_duration") ?? 0;
            int duration = Math.Max(buffDuration, debuffDuration);

            var skill = new Skill
            {
                Id = id,
                SkillName = string.IsNullOrEmpty(skillNameKey) ? id : TranslationServer.Translate(skillNameKey),
                Description = string.IsNullOrEmpty(descriptionKey) ? string.Empty : TranslationServer.Translate(descriptionKey),
                Cooldown = cooldown,
                ManaCost = 0,
                IsUnlocked = false
            };

            var def = new Skill.SkillDefent
            {
                Type = MapSkillType(attackType, typeStr),
                DamageCoefficient = damageCoefficient,
                Duration = duration,
                DamageTypeString = MapDamageTypeString(attackType)
            };
            skill.SkillDefs.Add(def);

            return skill;
        }

        private static Skill.SkillType MapSkillType(string attackType, string typeStr)
        {
            if (!string.IsNullOrEmpty(attackType))
            {
                if (attackType.Contains("治疗", StringComparison.Ordinal))
                {
                    return Skill.SkillType.Healing;
                }
                if (attackType.Contains("增益", StringComparison.Ordinal))
                {
                    return Skill.SkillType.Support;
                }
                if (attackType.Contains("防御", StringComparison.Ordinal))
                {
                    return Skill.SkillType.Defense;
                }
                return Skill.SkillType.Attack;
            }

            if (!string.IsNullOrEmpty(typeStr) && typeStr.Contains("被动", StringComparison.Ordinal))
            {
                return Skill.SkillType.Support;
            }

            return Skill.SkillType.Attack;
        }

        private static string MapDamageTypeString(string attackType)
        {
            if (string.IsNullOrEmpty(attackType))
            {
                return "Physical";
            }

            if (attackType.Contains("物", StringComparison.Ordinal))
            {
                return "Physical";
            }
            if (attackType.Contains("法", StringComparison.Ordinal))
            {
                return "Magic";
            }
            if (attackType.Contains("治疗", StringComparison.Ordinal) || attackType.Contains("增益", StringComparison.Ordinal))
            {
                return "Holy";
            }

            return "Physical";
        }

        private static string TryGetString(JsonElement obj, string name)
        {
            if (!obj.TryGetProperty(name, out var el))
            {
                return null;
            }
            return el.ValueKind == JsonValueKind.String ? el.GetString() : null;
        }

        private static int? TryGetInt(JsonElement obj, string name)
        {
            if (!obj.TryGetProperty(name, out var el))
            {
                return null;
            }
            return el.ValueKind == JsonValueKind.Number ? el.GetInt32() : null;
        }

        private static double? TryGetDouble(JsonElement obj, string name)
        {
            if (!obj.TryGetProperty(name, out var el))
            {
                return null;
            }
            return el.ValueKind == JsonValueKind.Number ? el.GetDouble() : null;
        }

        private sealed class Vector3JsonConverter : JsonConverter<Vector3>
        {
            public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    reader.Read();
                    float x = reader.TokenType == JsonTokenType.Number ? (float)reader.GetDouble() : 0f;

                    reader.Read();
                    float y = reader.TokenType == JsonTokenType.Number ? (float)reader.GetDouble() : 0f;

                    reader.Read();
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        return new Vector3(x, y, 0f);
                    }

                    float z = reader.TokenType == JsonTokenType.Number ? (float)reader.GetDouble() : 0f;
                    reader.Read();

                    if (reader.TokenType != JsonTokenType.EndArray)
                    {
                        throw new JsonException("Vector3 array must have 2 or 3 elements");
                    }

                    return new Vector3(x, y, z);
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    float x = 0f;
                    float y = 0f;
                    float z = 0f;

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }

                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new JsonException("Unexpected token while reading Vector3 object");
                        }

                        string propName = reader.GetString();
                        reader.Read();

                        float value = reader.TokenType == JsonTokenType.Number ? (float)reader.GetDouble() : 0f;
                        switch (propName?.ToLowerInvariant())
                        {
                            case "x":
                                x = value;
                                break;
                            case "y":
                                y = value;
                                break;
                            case "z":
                                z = value;
                                break;
                        }
                    }

                    return new Vector3(x, y, z);
                }

                throw new JsonException($"Unsupported token {reader.TokenType} for Vector3");
            }

            public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                writer.WriteNumberValue(value.X);
                writer.WriteNumberValue(value.Y);
                writer.WriteNumberValue(value.Z);
                writer.WriteEndArray();
            }
        }

        /// <summary>
        /// 加载ViewRegister到缓存
        /// </summary>
        /// <remarks>
        /// 该方法从ViewRegister.json文件加载视图注册信息，并将其转换为字典格式存储在ViewRegister缓存中。
        /// 如果文件不存在或加载失败，会清空ViewRegister缓存。
        /// </remarks>
        private void LoadViewRegisterToCacheWithTracking()
        {
            var loadInfo = new ResourceLoadInfo
            {
                ResourceName = "ViewRegister.json",
                Priority = ResourceLoadPriority.High,
                Status = ResourceLoadStatus.Loading
            };

            _loadInfoCache["ViewRegister.json"] = loadInfo;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                string filePath = DefaultResourcesPath + "ViewRegister.json";

                // 检查文件是否存在
                if (!FileAccess.FileExists(filePath))
                {
                    loadInfo.Status = ResourceLoadStatus.Failed;
                    loadInfo.ErrorMessage = "File not found";
                    Log.Warning($"ViewRegister.json file not found: {filePath}");
                    ViewRegister.Clear();
                    ResourceLoaded?.Invoke(loadInfo);
                    return;
                }

                // 使用Godot的FileAccess API加载文件
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                loadInfo.FileSizeBytes = (long)file.GetLength();
                string jsonContent = file.GetAsText();

                if (string.IsNullOrEmpty(jsonContent))
                {
                    loadInfo.Status = ResourceLoadStatus.Failed;
                    loadInfo.ErrorMessage = "File is empty";
                    Log.Warning($"ViewRegister.json file is empty: {filePath}");
                    ViewRegister.Clear();
                    ResourceLoaded?.Invoke(loadInfo);
                    return;
                }

                // 解析JSON数组
                var viewRegisterArray = JsonSerializer.Deserialize<List<ViewRegisterItem>>(jsonContent, JsonOptions);
                if (viewRegisterArray == null)
                {
                    loadInfo.Status = ResourceLoadStatus.Failed;
                    loadInfo.ErrorMessage = "Parsing failed";
                    Log.Warning($"ViewRegister.json parsing failed: {filePath}");
                    ViewRegister.Clear();
                    ResourceLoaded?.Invoke(loadInfo);
                    return;
                }

                // 转换为字典
                ViewRegister.Clear();
                foreach (var item in viewRegisterArray)
                {
                    if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.Tscn))
                    {
                        ViewRegister[item.Id] = item.Tscn;
                    }
                }

                stopwatch.Stop();
                loadInfo.Status = ResourceLoadStatus.Loaded;
                loadInfo.LoadTimeMs = stopwatch.ElapsedMilliseconds;
                Log.Info($"Successfully loaded ViewRegister.json: {filePath}, Items: {ViewRegister.Count}, Time: {loadInfo.LoadTimeMs:F2}ms");
                ResourceLoaded?.Invoke(loadInfo);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                loadInfo.Status = ResourceLoadStatus.Failed;
                loadInfo.ErrorMessage = ex.Message;
                loadInfo.LoadTimeMs = stopwatch.ElapsedMilliseconds;
                Log.Error($"Failed to load ViewRegister.json: {ex.Message}");
                ViewRegister.Clear();
                ResourceLoaded?.Invoke(loadInfo);
            }
        }

        /// <summary>
        /// 加载ViewRegister到缓存（向后兼容）
        /// </summary>
        private static void LoadViewRegisterToCache()
        {
            try
            {
                string filePath = DefaultResourcesPath + "ViewRegister.json";

                // 检查文件是否存在
                if (!FileAccess.FileExists(filePath))
                {
                    Log.Warning($"ViewRegister.json file not found: {filePath}");
                    ViewRegister.Clear();
                    return;
                }

                // 使用Godot的FileAccess API加载文件
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();

                if (string.IsNullOrEmpty(jsonContent))
                {
                    Log.Warning($"ViewRegister.json file is empty: {filePath}");
                    ViewRegister.Clear();
                    return;
                }

                // 解析JSON数组
                var viewRegisterArray = JsonSerializer.Deserialize<List<ViewRegisterItem>>(jsonContent, JsonOptions);
                if (viewRegisterArray == null)
                {
                    Log.Warning($"ViewRegister.json parsing failed: {filePath}");
                    ViewRegister.Clear();
                    return;
                }

                // 转换为字典
                ViewRegister.Clear();
                foreach (var item in viewRegisterArray)
                {
                    if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.Tscn))
                    {
                        ViewRegister[item.Id] = item.Tscn;
                    }
                }

                Log.Info($"Successfully loaded ViewRegister.json: {filePath}, Items: {ViewRegister.Count}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load ViewRegister.json: {ex.Message}");
                ViewRegister.Clear();
            }
        }

        /// <summary>
        /// 重新加载所有资源
        /// </summary>
        /// <remarks>
        /// 该方法调用InitializeResourcesAsync()重新加载所有资源，确保资源的最新状态。
        /// 如果资源管理器实例未初始化，会记录警告并返回。
        /// </remarks>
        public void ReloadAllResources()
        {
            if (Instance == null)
            {
                Log.Warning("ResourcesManager instance not initialized, cannot reload resources");
                return;
            }

            _ = InitializeResourcesAsync();
        }

        /// <summary>
        /// 重新加载所有资源（异步）
        /// </summary>
        /// <returns>异步任务</returns>
        public async Task ReloadAllResourcesAsync()
        {
            if (Instance == null)
            {
                Log.Warning("ResourcesManager instance not initialized, cannot reload resources");
                return;
            }

            await InitializeResourcesAsync();
        }

        /// <summary>
        /// 对话章节缓存（按章节ID索引）
        /// </summary>
        public static readonly Dictionary<string, DialogueChapterData> DialoguesCache = [];

        /// <summary>
        /// 本地化缓存，用于提高性能
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> _localizationCache = [];

        /// <summary>
        /// 检查资源文件是否存在
        /// </summary>
        /// <param name="fileName">资源文件名</param>
        /// <param name="customPath">自定义资源路径（可选）</param>
        /// <returns>如果文件存在则返回true，否则返回false</returns>
        /// <remarks>
        /// 该方法使用Godot的FileAccess API检查指定的资源文件是否存在。
        /// </remarks>
        public static bool ResourceExists(string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
            return FileAccess.FileExists(filePath);
        }

        /// <summary>
        /// 获取资源文件路径
        /// </summary>
        /// <param name="fileName">资源文件名</param>
        /// <param name="customPath">自定义资源路径（可选）</param>
        /// <returns>完整的资源文件路径</returns>
        /// <remarks>
        /// 该方法根据指定的文件名和自定义路径（如果提供）构建完整的资源文件路径。
        /// </remarks>
        public static string GetResourcePath(string fileName, string customPath = null)
        {
            return string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
        }

        /// <summary>
        /// 清除本地化缓存
        /// </summary>
        /// <remarks>
        /// 该方法清除本地化缓存，释放内存并确保下次获取本地化字符串时重新加载最新数据。
        /// </remarks>
        public static void ClearLocalizationCache()
        {
            _localizationCache.Clear();
            Log.Info("Localization cache cleared");
        }

        /// <summary>
        /// 加载所有任务数据
        /// </summary>
        /// <remarks>
        /// 该方法从Quests目录加载所有任务数据文件，并将其缓存到QuestsCache字典中。
        /// 如果加载过程中发生异常，会记录错误日志并清空QuestsCache。
        /// </remarks>
        private static void LoadQuests()
        {
            var stopwatch = Stopwatch.StartNew();
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
                        if (!(file.Length > 0 && file[0] == '.') && file.EndsWith(".json") && file != "dialogues.json")
                        {
                            var fileStopwatch = Stopwatch.StartNew();
                            string filePath = $"{questsDir}/{file}";
                            try
                            {
                                var questDataList = LoadQuestDataList(file, questsDir + "/");
                                if (questDataList != null && questDataList.Quests != null)
                                {
                                    foreach (var questData in questDataList.Quests)
                                    {
                                        if (questData != null && !string.IsNullOrEmpty(questData.Id))
                                        {
                                            QuestsCache[questData.Id] = questData;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Failed to load quest file {file}: {ex.Message}");
                            }
                            finally
                            {
                                fileStopwatch.Stop();
                                Log.Info($"Loaded quest file: {file}, Time: {fileStopwatch.ElapsedMilliseconds:F2}ms");
                            }
                        }
                    }
                    dir.ListDirEnd();
                }
                stopwatch.Stop();
                Log.Info($"Successfully loaded {QuestsCache.Count} quests, Total Time: {stopwatch.ElapsedMilliseconds:F2}ms");
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Log.Error($"Error loading quests: {e.Message}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                QuestsCache.Clear();
            }
        }

        /// <summary>
        /// 加载任务数据列表（支持新的JSON结构）
        /// </summary>
        /// <param name="fileName">JSON文件名</param>
        /// <param name="customPath">自定义路径</param>
        /// <returns>任务数据包装类</returns>
        private static QuestDataWrapper LoadQuestDataList(string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
            var stopwatch = Stopwatch.StartNew();

            if (!FileAccess.FileExists(filePath))
            {
                stopwatch.Stop();
                Log.Warning($"JSON file not found: {filePath}");
                return null;
            }

            try
            {
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();

                if (string.IsNullOrEmpty(jsonContent))
                {
                    stopwatch.Stop();
                    Log.Warning($"JSON file is empty: {filePath}");
                    return null;
                }

                var result = JsonSerializer.Deserialize<QuestDataWrapper>(jsonContent, JsonOptions);
                stopwatch.Stop();
                Log.Info($"Successfully loaded quest data list: {filePath}, Type: QuestDataWrapper, Time: {stopwatch.ElapsedMilliseconds:F2}ms");

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error($"Quest data list deserialization failed: {filePath}, Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns>所有任务的列表</returns>
        /// <remarks>
        /// 该方法返回QuestsCache中所有任务数据的列表。
        /// </remarks>
        public static List<QuestData> GetAllQuests()
        {
            return [.. QuestsCache.Values];
        }

        /// <summary>
        /// 根据ID获取任务
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <returns>如果找到则返回任务数据，否则返回null</returns>
        /// <remarks>
        /// 该方法从QuestsCache中根据任务ID查找并返回对应的任务数据。
        /// </remarks>
        public static QuestData GetQuest(string questId)
        {
            if (QuestsCache.TryGetValue(questId, out QuestData questData))
            {
                return questData;
            }
            return null;
        }

        /// <summary>
        /// 加载所有任务线数据
        /// </summary>
        /// <remarks>
        /// 该方法从QuestLines目录加载所有任务线数据文件，并将其缓存到QuestLinesCache字典中。
        /// 如果加载过程中发生异常，会记录错误日志并清空QuestLinesCache。
        /// </remarks>
        private static void LoadQuestLines()
        {
            var stopwatch = Stopwatch.StartNew();
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
                        if (!(file.Length > 0 && file[0] == '.') && file.EndsWith(".json") && file.Contains("quest_lines"))
                        {
                            var fileStopwatch = Stopwatch.StartNew();
                            string filePath = $"{questLinesDir}/{file}";
                            try
                            {
                                var questLineList = LoadQuestLineDataList(file, questLinesDir + "/");
                                if (questLineList != null && questLineList.QuestLines != null)
                                {
                                    foreach (var questLineData in questLineList.QuestLines)
                                    {
                                        if (questLineData != null && !string.IsNullOrEmpty(questLineData.Id))
                                        {
                                            QuestLinesCache[questLineData.Id] = questLineData;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Failed to load quest line file {file}: {ex.Message}");
                            }
                            finally
                            {
                                fileStopwatch.Stop();
                                Log.Info($"Loaded quest line file: {file}, Time: {fileStopwatch.ElapsedMilliseconds:F2}ms");
                            }
                        }
                    }
                    dir.ListDirEnd();
                }
                stopwatch.Stop();
                Log.Info($"Successfully loaded {QuestLinesCache.Count} quest lines, Total Time: {stopwatch.ElapsedMilliseconds:F2}ms");
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Log.Error($"Error loading quest lines: {e.Message}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                QuestLinesCache.Clear();
            }
        }

        /// <summary>
        /// 加载所有地图数据
        /// </summary>
        private static void LoadMaps()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var wrapper = LoadResource<MapDataWrapper>("Maps.json");
                MapsCache.Clear();
                if (wrapper?.Maps != null)
                {
                    foreach (var map in wrapper.Maps)
                    {
                        if (map != null && !string.IsNullOrEmpty(map.Id))
                        {
                            MapsCache[map.Id] = map;
                        }
                    }
                }
                stopwatch.Stop();
                Log.Info($"Loaded {MapsCache.Count} maps from Maps.json, Time: {stopwatch.ElapsedMilliseconds:F2}ms");

                // Notify MapManager to reload from cache
                if (MapManager.Instance != null)
                {
                    MapManager.Instance.LoadMapsFromCache();
                }
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Log.Error($"Error loading maps: {e.Message}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                MapsCache.Clear();
            }
        }

        /// <summary>
        /// 获取所有地图数据
        /// </summary>
        public static List<MapData> GetAllMaps()
        {
            return [.. MapsCache.Values];
        }

        /// <summary>
        /// 加载所有对话数据
        /// </summary>
        private static void LoadDialogues()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                string questsDir = "res://Resources/Static/Quests/";
                var wrapper = LoadResource<DialogueChapterWrapper>("dialogues.json", questsDir);
                DialoguesCache.Clear();
                if (wrapper?.Chapters != null)
                {
                    foreach (var chapter in wrapper.Chapters)
                    {
                        if (chapter != null && !string.IsNullOrEmpty(chapter.ChapterId))
                        {
                            DialoguesCache[chapter.ChapterId] = chapter;
                        }
                    }
                }
                stopwatch.Stop();
                Log.Info($"Loaded {DialoguesCache.Count} dialogue chapters from dialogues.json, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Log.Error($"Error loading dialogues: {e.Message}, Time: {stopwatch.ElapsedMilliseconds:F2}ms");
                DialoguesCache.Clear();
            }
        }

        /// <summary>
        /// 根据任务ID获取对话列表
        /// </summary>
        public static List<DialogueEntryData> GetDialoguesByQuest(string questId)
        {
            var result = new List<DialogueEntryData>();
            foreach (var chapter in DialoguesCache.Values)
            {
                if (chapter.Dialogues != null)
                {
                    foreach (var dlg in chapter.Dialogues)
                    {
                        if (dlg.QuestId == questId)
                            result.Add(dlg);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 加载任务线数据列表（支持新的JSON结构）
        /// </summary>
        /// <param name="fileName">JSON文件名</param>
        /// <param name="customPath">自定义路径</param>
        /// <returns>任务线数据包装类</returns>
        private static QuestLineWrapper LoadQuestLineDataList(string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
            var stopwatch = Stopwatch.StartNew();

            if (!FileAccess.FileExists(filePath))
            {
                stopwatch.Stop();
                Log.Warning($"JSON file not found: {filePath}");
                return null;
            }

            try
            {
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();

                if (string.IsNullOrEmpty(jsonContent))
                {
                    stopwatch.Stop();
                    Log.Warning($"JSON file is empty: {filePath}");
                    return null;
                }

                var result = JsonSerializer.Deserialize<QuestLineWrapper>(jsonContent, JsonOptions);
                stopwatch.Stop();
                Log.Info($"Successfully loaded quest line data list: {filePath}, Type: QuestLineWrapper, Time: {stopwatch.ElapsedMilliseconds:F2}ms");

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error($"Quest line data list deserialization failed: {filePath}, Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有任务线
        /// </summary>
        /// <returns>所有任务线的列表</returns>
        /// <remarks>
        /// 该方法返回QuestLinesCache中所有任务线数据的列表。
        /// </remarks>
        public static List<QuestLineData> GetAllQuestLines()
        {
            return [.. QuestLinesCache.Values];
        }

        /// <summary>
        /// 根据ID获取任务线
        /// </summary>
        /// <param name="questLineId">任务线ID</param>
        /// <returns>如果找到则返回任务线数据，否则返回null</returns>
        /// <remarks>
        /// 该方法从QuestLinesCache中根据任务线ID查找并返回对应的任务线数据。
        /// </remarks>
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
        /// <param name="skillId">技能ID</param>
        /// <returns>如果找到则返回技能，否则返回null</returns>
        /// <remarks>
        /// 该方法从SkillsCache中根据技能ID查找并返回对应的技能。
        /// </remarks>
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
        /// <param name="itemId">物品ID</param>
        /// <returns>如果找到则返回物品，否则返回null</returns>
        /// <remarks>
        /// 该方法从ItemsCache中根据物品ID查找并返回对应的物品。
        /// </remarks>
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
        /// <param name="npcId">NPC ID</param>
        /// <returns>如果找到则返回NPC，否则返回null</returns>
        /// <remarks>
        /// 该方法从NPCsCache中根据NPC ID查找并返回对应的NPC。
        /// </remarks>
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
        /// <param name="monsterId">怪物ID</param>
        /// <returns>如果找到则返回怪物，否则返回null</returns>
        /// <remarks>
        /// 该方法从MonstersCache中根据怪物ID查找并返回对应的怪物。
        /// </remarks>
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
        /// <param name="weaponId">武器ID</param>
        /// <returns>如果找到则返回武器，否则返回null</returns>
        /// <remarks>
        /// 该方法从WeaponsCache中根据武器ID查找并返回对应的武器。
        /// </remarks>
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
        /// <param name="equipmentId">装备ID</param>
        /// <returns>如果找到则返回装备，否则返回null</returns>
        /// <remarks>
        /// 该方法从EquipmentCache中根据装备ID查找并返回对应的装备。
        /// </remarks>
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
        /// <param name="levelId">关卡ID</param>
        /// <returns>如果找到则返回关卡，否则返回null</returns>
        /// <remarks>
        /// 该方法从LevelsCache中根据关卡ID查找并返回对应的关卡。
        /// </remarks>
        public static Level GetLevel(string levelId)
        {
            if (LevelsCache.TryGetValue(levelId, out Level level))
            {
                return level;
            }
            return null;
        }

        /// <summary>
        /// 清除所有资源缓存
        /// </summary>
        /// <remarks>
        /// 该方法清除所有资源缓存，包括技能、物品、NPC、怪物、武器、装备、关卡、任务、任务线、视图注册和本地化缓存。
        /// 这会释放内存并确保下次访问资源时重新加载最新数据。
        /// </remarks>
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
            MapsCache.Clear();
            DialoguesCache.Clear();
            ViewRegister.Clear();
            ClearLocalizationCache();
            Log.Info("All resource caches cleared");
        }

        /// <summary>
        /// 清理资源管理器
        /// </summary>
        /// <remarks>
        /// 该方法在游戏退出时调用，用于清理所有资源缓存和释放内存。
        /// </remarks>
        public void Cleanup()
        {
            ClearAllCaches();
            _loadInfoCache.Clear();
            Log.Info("ResourcesManager cleaned up");
        }

        /// <summary>
        /// 获取资源数量信息（用于调试）
        /// </summary>
        /// <returns>资源数量信息字符串</returns>
        /// <remarks>
        /// 该方法返回所有资源缓存的数量信息，用于调试和监控资源加载情况。
        /// </remarks>
        public static string GetResourceCountInfo()
        {
            return $"Resources: Skills={SkillsCache.Count}, Items={ItemsCache.Count}, NPCs={NPCsCache.Count}, Monsters={MonstersCache.Count}, Weapons={WeaponsCache.Count}, Equipment={EquipmentCache.Count}, Levels={LevelsCache.Count}, Quests={QuestsCache.Count}, QuestLines={QuestLinesCache.Count}";
        }

        /// <summary>
        /// 获取资源加载信息
        /// </summary>
        /// <returns>资源加载信息字典</returns>
        public static Dictionary<string, ResourceLoadInfo> GetLoadInfo()
        {
            return new Dictionary<string, ResourceLoadInfo>(_loadInfoCache);
        }

        /// <summary>
        /// 获取加载性能统计
        /// </summary>
        /// <returns>性能统计字符串</returns>
        public static string GetPerformanceStats()
        {
            var stats = new StringBuilder();
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

        internal static string GetLocalizedString(string v)
        {
            throw new NotImplementedException();
        }
    }
}
