using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    public partial class ResourcesManager: Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static ResourcesManager _instance;

        /// <summary>
        /// 资源管理器的单例实例
        /// </summary>
        /// <value>资源管理器的单例实例</value>
        public static ResourcesManager Instance => _instance;
        
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
        public static readonly Dictionary<string, NPC> NPCsCache = new();

        /// <summary>
        /// 怪物缓存字典
        /// </summary>
        public static readonly Dictionary<string, Monster> MonstersCache = new();

        /// <summary>
        /// 武器缓存字典
        /// </summary>
        public static readonly Dictionary<string, Weapon> WeaponsCache = new();

        /// <summary>
        /// 装备缓存字典
        /// </summary>
        public static readonly Dictionary<string, Equipment> EquipmentCache = new();

        /// <summary>
        /// 关卡缓存字典
        /// </summary>
        public static readonly Dictionary<string, Level> LevelsCache = new();

        /// <summary>
        /// 任务缓存字典
        /// </summary>
        public static readonly Dictionary<string, QuestData> QuestsCache = new();

        /// <summary>
        /// 任务线缓存字典
        /// </summary>
        public static readonly Dictionary<string, QuestLineData> QuestLinesCache = new();

        /// <summary>
        /// 视图注册缓存字典
        /// </summary>
        public static readonly Dictionary<string, string> ViewRegister = new();

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
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            // 忽略大小写
            PropertyNameCaseInsensitive = true,
            // 处理空值
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // 允许注释
            ReadCommentHandling = JsonCommentHandling.Skip,
            // 允许尾随逗号
            AllowTrailingCommas = true,
            // 使用驼峰命名
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // 自定义枚举转换器
            Converters = { new JsonStringEnumConverter() }
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
            
            // 初始化所有资源
            InitializeResources();
        }
        
        /// <summary>
        /// 初始化所有资源
        /// </summary>
        /// <remarks>
        /// 该方法负责加载所有静态资源、任务数据和任务线数据，并将它们缓存到对应的字典中。
        /// 如果加载过程中发生异常，会记录错误日志。
        /// </remarks>
        private void InitializeResources()
        {
            try
            {
                Log.Info("Starting resource initialization...");
                
                // 加载静态资源
                LoadResourceToCache("Skills.json", SkillsCache);
                LoadResourceToCache("Items.json", ItemsCache);
                LoadResourceToCache("NPCs.json", NPCsCache);
                LoadResourceToCache("Monsters.json", MonstersCache);
                LoadResourceToCache("Weapons.json", WeaponsCache);
                LoadResourceToCache("Equipment.json", EquipmentCache);
                LoadResourceToCache("Levels.json", LevelsCache);
                LoadViewRegisterToCache();
                
                // 加载任务数据
                LoadQuests();
                LoadQuestLines();
                
                // 加载示例资源（如果需要）
                // LoadExampleResources();

                
                Log.Info($"Resource initialization completed - Skills: {SkillsCache.Count}, Items: {ItemsCache.Count}, NPCs: {NPCsCache.Count}, Monsters: {MonstersCache.Count}, Weapons: {WeaponsCache.Count}, Equipment: {EquipmentCache.Count}, Levels: {LevelsCache.Count}, Quests: {QuestsCache.Count}, QuestLines: {QuestLinesCache.Count}");
            }
            catch (Exception ex)
            {
                Log.Error($"Resource initialization failed: {ex.Message}");
            }
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
            
            // 检查文件是否存在
            if (!FileAccess.FileExists(filePath))
            {
                Log.Warning($"JSON file not found: {filePath}");
                return default;
            }

            try
            {
                // 使用Godot的FileAccess API加载文件
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();
                
                if (string.IsNullOrEmpty(jsonContent))
                {
                    Log.Warning($"JSON file is empty: {filePath}");
                    return default;
                }

                // 反序列化为目标类型
                T result = JsonSerializer.Deserialize<T>(jsonContent, JsonOptions);
                Log.Info($"Successfully loaded JSON file: {filePath}, Type: {typeof(T).Name}");
                
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"JSON deserialization failed: {filePath}, Error: {ex.Message}");
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
            var loadedData = LoadResource<Dictionary<string, T>>(fileName);
            if (loadedData != null)
            {
                cache.Clear();
                foreach (var item in loadedData)
                {
                    cache[item.Key] = item.Value;
                }
            }
            else
            {
                cache.Clear();
            }
        }
        
        /// <summary>
        /// 加载ViewRegister到缓存
        /// </summary>
        /// <remarks>
        /// 该方法从ViewRegister.json文件加载视图注册信息，并将其转换为字典格式存储在ViewRegister缓存中。
        /// 如果文件不存在或加载失败，会清空ViewRegister缓存。
        /// </remarks>
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
        /// 该方法调用InitializeResources()重新加载所有资源，确保资源的最新状态。
        /// 如果资源管理器实例未初始化，会记录警告并返回。
        /// </remarks>
        public static void ReloadAllResources()
        {
            if (Instance == null)
            {
                Log.Warning("ResourcesManager instance not initialized, cannot reload resources");
                return;
            }
            
            Instance.InitializeResources();
        }
        


        /// <summary>
        /// ViewRegister项临时类
        /// </summary>
        private class ViewRegisterItem
        {
            /// <summary>
            /// 视图ID
            /// </summary>
            [JsonPropertyName("id")]
            public string Id { get; set; }

            /// <summary>
            /// 场景文件路径
            /// </summary>
            [JsonPropertyName("tscn")]
            public string Tscn { get; set; }
        }

        /// <summary>
        /// 本地化缓存，用于提高性能
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> _localizationCache = new();

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        /// <param name="key">字符串键</param>
        /// <param name="languageCode">语言代码（默认：zh）</param>
        /// <returns>本地化字符串，如果未找到则返回原始键</returns>
        /// <remarks>
        /// 该方法从本地化缓存中获取指定键的本地化字符串，如果缓存中不存在则加载对应语言的本地化文件。
        /// 如果加载失败或未找到对应键，会记录警告或错误日志并返回原始键。
        /// </remarks>
        public static string GetLocalizedString(string key, string languageCode = "zh")
        {
            try
            {
                // 检查本地化数据是否在缓存中
                if (!_localizationCache.TryGetValue(languageCode, out var localizationData))
                {
                    // 加载本地化数据并添加到缓存
                    string fileName = $"{languageCode}.json";
                    localizationData = LoadResource<Dictionary<string, string>>(fileName, LocalizationPath);
                    if (localizationData == null)
                    {
                        localizationData = new Dictionary<string, string>();
                    }
                    _localizationCache[languageCode] = localizationData;
                }
                
                if (localizationData.TryGetValue(key, out string value))
                {
                    return value;
                }
                
                Log.Warning($"Localized string not found: {key}, Language: {languageCode}");
                return key;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get localized string: {ex.Message}");
                return key;
            }
        }
        
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
                            string filePath = $"{questsDir}/{file}";
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
        /// 获取所有任务
        /// </summary>
        /// <returns>所有任务的列表</returns>
        /// <remarks>
        /// 该方法返回QuestsCache中所有任务数据的列表。
        /// </remarks>
        public static List<QuestData> GetAllQuests()
        {
            return new List<QuestData>(QuestsCache.Values);
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
            try
            {
                string questLinesDir = "res://Resources/Static/QuestLines";
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
                            string filePath = $"{questLinesDir}/{file}";
                            var fileAccess = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                            if (fileAccess != null)
                            {
                                try
                                {
                                    string json = fileAccess.GetAsText();
                                    QuestLineData questLineData = JsonSerializer.Deserialize<QuestLineData>(json, JsonOptions);
                                    if (questLineData != null && !string.IsNullOrEmpty(questLineData.Id))
                                    {
                                        QuestLinesCache[questLineData.Id] = questLineData;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Warning($"Failed to load quest line file {file}: {ex.Message}");
                                }
                                finally
                                {
                                    fileAccess.Close();
                                }
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
        /// 获取所有任务线
        /// </summary>
        /// <returns>所有任务线的列表</returns>
        /// <remarks>
        /// 该方法返回QuestLinesCache中所有任务线数据的列表。
        /// </remarks>
        public static List<QuestLineData> GetAllQuestLines()
        {
            return new List<QuestLineData>(QuestLinesCache.Values);
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
            ViewRegister.Clear();
            ClearLocalizationCache();
            Log.Info("All resource caches cleared");
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
    }
}
