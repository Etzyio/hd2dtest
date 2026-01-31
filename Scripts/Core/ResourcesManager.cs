using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// Resource manager for loading and parsing JSON files
    /// </summary>
    public partial class ResourcesManager: Node
    {
        private static ResourcesManager _instance;
        public static ResourcesManager Instance => _instance;
        
        // Resource cache dictionaries
        public static Dictionary<string, Skill> SkillsCache = new Dictionary<string, Skill>();
        public static Dictionary<string, Item> ItemsCache = new Dictionary<string, Item>();
        public static Dictionary<string, NPC> NPCsCache = new Dictionary<string, NPC>();
        public static Dictionary<string, Monster> MonsterCache = new Dictionary<string, Monster>();
        public static Dictionary<string, Weapon> WeaponsCache = new Dictionary<string, Weapon>();
        public static Dictionary<string, Equipment> EquipmentCache = new Dictionary<string, Equipment>();
        public static Dictionary<string, Level> LevelCache = new Dictionary<string, Level>();
        public Dictionary<string, NPC> NpcCache = new Dictionary<string, NPC>();
        public Dictionary<string, string> ViewRegister = new Dictionary<string, string>();

        // Default resource paths
        private const string DefaultResourcesPath = "res://Resources/Static/";
        private const string LocalizationPath = "res://Resources/Localization/";
        
        // JSON serialization options
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            // Ignore case
            PropertyNameCaseInsensitive = true,
            // Handle null values
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // Allow comments
            ReadCommentHandling = JsonCommentHandling.Skip,
            // Allow trailing commas
            AllowTrailingCommas = true,
            // Use camel case
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Custom enum converter
            Converters = { new JsonStringEnumConverter() }
        };

        public override void _Ready()
        {
            _instance = this;
            
            // Initialize all resources
            InitializeResources();
        }
        
        /// <summary>
        /// Initialize all resources
        /// </summary>
        private void InitializeResources()
        {
            try
            {
                Log.Info("Starting resource initialization...");
                
                // Load static resources
                SkillsCache = LoadResource<Dictionary<string, Skill>>("Skills.json") ?? new Dictionary<string, Skill>();
                ItemsCache = LoadResource<Dictionary<string, Item>>("Items.json") ?? new Dictionary<string, Item>();
                NPCsCache = LoadResource<Dictionary<string, NPC>>("NPCs.json") ?? new Dictionary<string, NPC>();
                MonsterCache = LoadResource<Dictionary<string, Monster>>("Monsters.json") ?? new Dictionary<string, Monster>();
                WeaponsCache = LoadResource<Dictionary<string, Weapon>>("Weapons.json") ?? new Dictionary<string, Weapon>();
                EquipmentCache = LoadResource<Dictionary<string, Equipment>>("Equipment.json") ?? new Dictionary<string, Equipment>();
                NpcCache = LoadResource<Dictionary<string, NPC>>("NPCs.json") ?? new Dictionary<string, NPC>();
                ViewRegister = LoadViewRegister();
                
                // Load example resources (if needed)
                // LoadExampleResources();

                
                Log.Info($"Resource initialization completed - Skills: {SkillsCache.Count}, Items: {ItemsCache.Count}, NPCs: {NPCsCache.Count}, Monsters: {MonsterCache.Count}, Weapons: {WeaponsCache.Count}, Equipment: {EquipmentCache.Count}");
            }
            catch (Exception ex)
            {
                Log.Error($"Resource initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Load data from a JSON file and deserialize to object
        /// </summary>
        /// <typeparam name="T">Target object type</typeparam>
        /// <param name="fileName">JSON file name (with extension)</param>
        /// <param name="customPath">Custom resource path (optional)</param>
        /// <returns>Deserialized object, or default if failed</returns>
        public static T LoadResource<T>(string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
            
            // Check if file exists
            if (!FileAccess.FileExists(filePath))
            {
                Log.Warning($"JSON file not found: {filePath}");
                return default;
            }

            try
            {
                // Load file using Godot's FileAccess API
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();
                
                if (string.IsNullOrEmpty(jsonContent))
                {
                    Log.Warning($"JSON file is empty: {filePath}");
                    return default;
                }

                // Deserialize to target type
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
        /// Serialize object to JSON and save to file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="data">Data to save</param>
        /// <param name="fileName">File name to save (with extension)</param>
        /// <param name="customPath">Custom resource path (optional)</param>
        /// <returns>Whether save was successful</returns>
        public static bool SaveResource<T>(T data, string fileName, string customPath = null)
        {
            string filePath = string.IsNullOrEmpty(customPath) ? DefaultResourcesPath + fileName : customPath + fileName;
            
            try
            {
                // Serialize object to JSON
                string jsonContent = JsonSerializer.Serialize(data, JsonOptions);
                
                // Save file using Godot's FileAccess API
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
        /// Reload all resources
        /// </summary>
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
        /// Load ViewRegister.json file and convert to Dictionary<string, string>
        /// </summary>
        /// <returns>Converted Dictionary, key is id, value is tscn</returns>
        private Dictionary<string, string> LoadViewRegister()
        {
            try
            {
                string filePath = DefaultResourcesPath + "ViewRegister.json";
                
                // Check if file exists
                if (!FileAccess.FileExists(filePath))
                {
                    Log.Warning($"ViewRegister.json file not found: {filePath}");
                    return new Dictionary<string, string>();
                }

                // Load file using Godot's FileAccess API
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();
                
                if (string.IsNullOrEmpty(jsonContent))
                {
                    Log.Warning($"ViewRegister.json file is empty: {filePath}");
                    return new Dictionary<string, string>();
                }

                // Parse JSON array
                var viewRegisterArray = JsonSerializer.Deserialize<List<ViewRegisterItem>>(jsonContent, JsonOptions);
                if (viewRegisterArray == null)
                {
                    Log.Warning($"ViewRegister.json parsing failed: {filePath}");
                    return new Dictionary<string, string>();
                }

                // Convert to Dictionary
                var viewRegisterDict = new Dictionary<string, string>();
                foreach (var item in viewRegisterArray)
                {
                    if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.Tscn))
                    {
                        viewRegisterDict[item.Id] = item.Tscn;
                    }
                }

                Log.Info($"Successfully loaded ViewRegister.json: {filePath}, Items: {viewRegisterDict.Count}");
                return viewRegisterDict;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load ViewRegister.json: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// ViewRegister item temporary class
        /// </summary>
        private class ViewRegisterItem
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("tscn")]
            public string Tscn { get; set; }
        }

        /// <summary>
        /// Get localized string
        /// </summary>
        /// <param name="key">String key</param>
        /// <param name="languageCode">Language code (default: zh)</param>
        /// <returns>Localized string, or original key if not found</returns>
        public static string GetLocalizedString(string key, string languageCode = "zh")
        {
            try
            {
                string fileName = $"{languageCode}.json";
                var localizationData = LoadResource<Dictionary<string, string>>(fileName, LocalizationPath);
                
                if (localizationData != null && localizationData.TryGetValue(key, out string value))
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
    }
}
