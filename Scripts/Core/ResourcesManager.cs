using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        public static Dictionary<string, Skill> SkillsCache = new Dictionary<string, Skill>();
        public static Dictionary<string, Item> ItemsCache = new Dictionary<string, Item>();
        public static Dictionary<string, NPC> NPCsCache = new Dictionary<string, NPC>();
        public static Dictionary<string, Monster> MonsterCache = new Dictionary<string, Monster>();
        public static Dictionary<string, Weapon> WeaponsCache = new Dictionary<string, Weapon>();
        public static Dictionary<string, Equipment> EquipmentCache = new Dictionary<string, Equipment>();


        public override void _Ready()
        {
            _instance = this;
            SkillsCache = LoadFromJson<Dictionary<string, Skill>>("Skills.json");
            ItemsCache = LoadFromJson<Dictionary<string, Item>>("Items.json");
            NPCsCache = LoadFromJson<Dictionary<string, NPC>>("NPCs.json");
            MonsterCache = LoadFromJson<Dictionary<string, Monster>>("Monsters.json");
            WeaponsCache = LoadFromJson<Dictionary<string, Weapon>>("Weapons.json");
            EquipmentCache = LoadFromJson<Dictionary<string, Equipment>>("Equipment.json");
        }
        private const string ResourcesPath = "res://Resources/Static/";

        /// <summary>
        /// 从指定的 JSON 文件加载数据并反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="fileName">JSON 文件名（包含扩展名）</param>
        /// <returns>反序列化后的对象</returns>
        public static T LoadFromJson<T>(string fileName)
        {
            string filePath = ResourcesPath + fileName;

            // 加载 JSON 文件
            using StreamReader reader = new(filePath);
            string jsonContent = reader.ReadToEnd();

            // 反序列化为目标类型
            try
            {
                return JsonSerializer.Deserialize<T>(jsonContent);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"JSON 反序列化失败: {ex.Message}");
                return default;
            }
        }
    }
}
