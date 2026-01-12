using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 资源管理器，用于加载和解析 JSON 文件
    /// </summary>
    public partial class ResourcesManager: Node
    {
        private static ResourcesManager _instance;
        public static ResourcesManager Instance => _instance;
        
        // 资源缓存字典
        public static Dictionary<string, Skill> SkillsCache = new Dictionary<string, Skill>();
        public static Dictionary<string, Item> ItemsCache = new Dictionary<string, Item>();
        public static Dictionary<string, NPC> NPCsCache = new Dictionary<string, NPC>();
        public static Dictionary<string, Monster> MonsterCache = new Dictionary<string, Monster>();
        public static Dictionary<string, Weapon> WeaponsCache = new Dictionary<string, Weapon>();
        public static Dictionary<string, Equipment> EquipmentCache = new Dictionary<string, Equipment>();

        // 默认资源路径
        private const string DefaultResourcesPath = "res://Resources/Static/";
        private const string ExamplesPath = "res://Resources/Examples/";
        private const string LocalizationPath = "res://Resources/Localization/";
        
        // JSON 序列化选项
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

        public override void _Ready()
        {
            _instance = this;
            
            // 初始化所有资源
            InitializeResources();
        }
        
        /// <summary>
        /// 初始化所有资源
        /// </summary>
        private void InitializeResources()
        {
            try
            {
                Log.Info("开始初始化资源...");
                
                // 加载静态资源
                SkillsCache = LoadFromJson<Dictionary<string, Skill>>("Skills.json") ?? new Dictionary<string, Skill>();
                ItemsCache = LoadFromJson<Dictionary<string, Item>>("Items.json") ?? new Dictionary<string, Item>();
                NPCsCache = LoadFromJson<Dictionary<string, NPC>>("NPCs.json") ?? new Dictionary<string, NPC>();
                MonsterCache = LoadFromJson<Dictionary<string, Monster>>("Monsters.json") ?? new Dictionary<string, Monster>();
                WeaponsCache = LoadFromJson<Dictionary<string, Weapon>>("Weapons.json") ?? new Dictionary<string, Weapon>();
                EquipmentCache = LoadFromJson<Dictionary<string, Equipment>>("Equipment.json") ?? new Dictionary<string, Equipment>();
                
                // 加载示例资源（如果需要）
                // LoadExampleResources();
                
                Log.Info($"资源初始化完成 - 技能: {SkillsCache.Count}, 物品: {ItemsCache.Count}, NPC: {NPCsCache.Count}, 怪物: {MonsterCache.Count}, 武器: {WeaponsCache.Count}, 装备: {EquipmentCache.Count}");
            }
            catch (Exception ex)
            {
                Log.Error($"资源初始化失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 加载示例资源（用于开发和测试）
        /// </summary>
        private void LoadExampleResources()
        {
            try
            {
                // 加载示例资源
                var characterExample = LoadFromJson<Dictionary<string, object>>("CharacterExample.json", ExamplesPath);
                var equipmentExample = LoadFromJson<Dictionary<string, object>>("EquipmentExample.json", ExamplesPath);
                var itemExample = LoadFromJson<Dictionary<string, object>>("ItemExample.json", ExamplesPath);
                var monsterExample = LoadFromJson<Dictionary<string, object>>("MonsterExample.json", ExamplesPath);
                var skillExample = LoadFromJson<Dictionary<string, object>>("SkillExample.json", ExamplesPath);
                
                Log.Info("示例资源加载完成");
            }
            catch (Exception ex)
            {
                Log.Warning($"示例资源加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从指定的 JSON 文件加载数据并反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="fileName">JSON 文件名（包含扩展名）</param>
        /// <param name="customPath">自定义资源路径（可选）</param>
        /// <returns>反序列化后的对象，如果失败则返回默认值</returns>
        public static T LoadFromJson<T>(string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
            
            // 检查文件是否存在
            if (!FileAccess.FileExists(filePath))
            {
                Log.Warning($"JSON 文件不存在: {filePath}");
                return default;
            }

            try
            {
                // 使用 Godot 的 FileAccess API 加载文件
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();
                
                if (string.IsNullOrEmpty(jsonContent))
                {
                    Log.Warning($"JSON 文件内容为空: {filePath}");
                    return default;
                }

                // 反序列化为目标类型
                T result = JsonSerializer.Deserialize<T>(jsonContent, JsonOptions);
                Log.Debug($"成功加载 JSON 文件: {filePath}, 类型: {typeof(T).Name}");
                
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"JSON 反序列化失败: {filePath}, 错误: {ex.Message}");
                return default;
            }
        }
        
        /// <summary>
        /// 将对象序列化为 JSON 并保存到文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="data">要保存的数据</param>
        /// <param name="fileName">保存的文件名（包含扩展名）</param>
        /// <param name="customPath">自定义资源路径（可选）</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveToJson<T>(T data, string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
            
            try
            {
                // 序列化对象为 JSON
                string jsonContent = JsonSerializer.Serialize(data, JsonOptions);
                
                // 使用 Godot 的 FileAccess API 保存文件
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
                file.StoreString(jsonContent);
                
                Log.Debug($"成功保存 JSON 文件: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"JSON 序列化失败: {filePath}, 错误: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 重新加载所有资源
        /// </summary>
        public static void ReloadAllResources()
        {
            if (Instance == null)
            {
                Log.Warning("ResourcesManager 实例未初始化，无法重新加载资源");
                return;
            }
            
            Instance.InitializeResources();
        }
        
        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        /// <param name="key">字符串键</param>
        /// <param name="languageCode">语言代码（默认中文）</param>
        /// <returns>本地化字符串，如果找不到则返回原始键</returns>
        public static string GetLocalizedString(string key, string languageCode = "zh")
        {
            try
            {
                string fileName = $"{languageCode}.json";
                var localizationData = LoadFromJson<Dictionary<string, string>>(fileName, LocalizationPath);
                
                if (localizationData != null && localizationData.TryGetValue(key, out string value))
                {
                    return value;
                }
                
                Log.Warning($"本地化字符串未找到: {key}, 语言: {languageCode}");
                return key;
            }
            catch (Exception ex)
            {
                Log.Error($"获取本地化字符串失败: {ex.Message}");
                return key;
            }
        }
    }
}
