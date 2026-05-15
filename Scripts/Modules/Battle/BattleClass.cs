using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.Battle
{
    /// <summary>
    /// 战斗职业类，定义角色的职业属性和成长曲线
    /// </summary>
    [GlobalClass]
    public partial class BattleClass : Resource
    {
        /// <summary>
        /// 职业ID
        /// </summary>
        public string ClassId { get; set; } = "";

        /// <summary>
        /// 职业名称
        /// </summary>
        public string ClassName { get; set; } = "New Class";

        /// <summary>
        /// 职业描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 生命值成长系数
        /// </summary>
        public float HealthGrowth { get; set; } = 1.0f;

        /// <summary>
        /// 攻击力成长系数
        /// </summary>
        public float AttackGrowth { get; set; } = 1.0f;

        /// <summary>
        /// 防御力成长系数
        /// </summary>
        public float DefenseGrowth { get; set; } = 1.0f;

        /// <summary>
        /// 速度成长系数
        /// </summary>
        public float SpeedGrowth { get; set; } = 1.0f;

        /// <summary>
        /// 魔法值成长系数
        /// </summary>
        public float ManaGrowth { get; set; } = 1.0f;

        /// <summary>
        /// 可学习技能列表
        /// </summary>
        public Array<SkillEntry> SkillsToLearn { get; set; } = new Array<SkillEntry>();

        /// <summary>
        /// 可学习被动技能列表
        /// </summary>
        public Array<PassiveSkillEntry> PassivesToLearn { get; set; } = new Array<PassiveSkillEntry>();

        /// <summary>
        /// 技能JP消耗列表（key: skillId, value: jpCost）
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> SkillJpCosts { get; set; } = new();

        /// <summary>
        /// 被动技能解锁所需的主动技能数量
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> PassiveRequiredUnlocks { get; set; } = new();

        /// <summary>
        /// 大招ID
        /// </summary>
        public string UltimateId { get; set; } = "";

        /// <summary>
        /// 大招JP消耗
        /// </summary>
        public int UltimateJpCost { get; set; } = 0;

        #region Registry

        private static readonly System.Collections.Generic.Dictionary<string, BattleClass> _registry = new();

        public static void Register(BattleClass cls) { if (cls == null || string.IsNullOrEmpty(cls.ClassId)) return; _registry[cls.ClassId] = cls; }

        public static BattleClass Get(string classId) { return classId != null && _registry.TryGetValue(classId, out var cls) ? cls : null; }

        public static List<BattleClass> GetAll() { return _registry.Values.ToList(); }

        /// <summary>
        /// 获取技能的JP消耗
        /// </summary>
        public static int GetSkillJpCost(string classId, string skillId)
        {
            var cls = Get(classId);
            if (cls != null && cls.SkillJpCosts.TryGetValue(skillId, out int cost))
                return cost;
            return 0;
        }

        /// <summary>
        /// 获取被动解锁所需的主动技能数量
        /// </summary>
        public static int GetPassiveRequiredActiveUnlocks(string classId, string passiveId)
        {
            var cls = Get(classId);
            if (cls != null && cls.PassiveRequiredUnlocks.TryGetValue(passiveId, out int req))
                return req;
            return 999;
        }

        /// <summary>
        /// 获取大招JP消耗
        /// </summary>
        public static int GetUltimateJpCost(string classId)
        {
            var cls = Get(classId);
            return cls?.UltimateJpCost ?? 0;
        }

        /// <summary>
        /// 从JSON文件加载职业定义
        /// </summary>
        public static void LoadFromJson()
        {
            try
            {
                string filePath = "res://Resources/Static/Classes.json";
                if (!FileAccess.FileExists(filePath))
                {
                    Log.Warning("Classes.json not found, using hardcoded defaults");
                    RegisterDefaults();
                    return;
                }

                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string jsonContent = file.GetAsText();
                if (string.IsNullOrEmpty(jsonContent))
                {
                    Log.Warning("Classes.json is empty, using hardcoded defaults");
                    RegisterDefaults();
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };

                var wrapper = JsonSerializer.Deserialize<ClassDataWrapper>(jsonContent, options);
                if (wrapper?.Classes == null || wrapper.Classes.Count == 0)
                {
                    Log.Warning("No classes found in Classes.json, using hardcoded defaults");
                    RegisterDefaults();
                    return;
                }

                _registry.Clear();

                foreach (var cd in wrapper.Classes)
                {
                    var bc = new BattleClass
                    {
                        ClassId = cd.ClassId,
                        ClassName = cd.ClassName,
                        Description = cd.Description,
                        HealthGrowth = cd.HealthGrowth,
                        AttackGrowth = cd.AttackGrowth,
                        DefenseGrowth = cd.DefenseGrowth,
                        SpeedGrowth = cd.SpeedGrowth,
                        ManaGrowth = cd.ManaGrowth,
                        UltimateId = cd.UltimateId ?? "",
                        UltimateJpCost = cd.UltimateJpCost
                    };

                    if (cd.Skills != null)
                    {
                        foreach (var s in cd.Skills)
                        {
                            if (!string.IsNullOrEmpty(s.Id))
                                bc.SkillJpCosts[s.Id] = s.JpCost;
                        }
                    }

                    if (cd.Passives != null)
                    {
                        foreach (var p in cd.Passives)
                        {
                            if (!string.IsNullOrEmpty(p.Id))
                                bc.PassiveRequiredUnlocks[p.Id] = p.RequiredActiveUnlocks;
                        }
                    }

                    // Apply JP costs to skill templates in SkillManager
                    foreach (var kv in bc.SkillJpCosts)
                    {
                        var template = SkillSystem.SkillManager.GetSkillTemplate(kv.Key);
                        if (template != null)
                            template.JPCost = kv.Value;
                    }

                    Register(bc);
                }

                Log.Info($"Loaded {_registry.Count} classes from Classes.json");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to load Classes.json: {ex.Message}, using hardcoded defaults");
                RegisterDefaults();
            }
        }

        /// <summary>
        /// 注册默认职业（硬编码备用）
        /// </summary>
        public static void RegisterDefaults()
        {
            if (_registry.Count > 0) return;

            Register(new BattleClass { ClassId = "swordsman", ClassName = "Swordsman", Description = "Balanced melee fighter with versatile combat skills.", HealthGrowth = 1.1f, AttackGrowth = 1.2f, DefenseGrowth = 1.0f, SpeedGrowth = 1.0f, ManaGrowth = 0.8f });
            Register(new BattleClass { ClassId = "healer", ClassName = "Healer", Description = "Specialist in healing and support magic.", HealthGrowth = 0.9f, AttackGrowth = 0.6f, DefenseGrowth = 0.7f, SpeedGrowth = 1.0f, ManaGrowth = 1.5f });
            Register(new BattleClass { ClassId = "mage", ClassName = "Mage", Description = "Master of elemental destruction magic.", HealthGrowth = 0.7f, AttackGrowth = 1.5f, DefenseGrowth = 0.6f, SpeedGrowth = 0.8f, ManaGrowth = 1.8f });
            Register(new BattleClass { ClassId = "tank", ClassName = "Tank", Description = "Unyielding shield with immense durability.", HealthGrowth = 1.8f, AttackGrowth = 0.7f, DefenseGrowth = 1.8f, SpeedGrowth = 0.6f, ManaGrowth = 0.5f });
            Register(new BattleClass { ClassId = "assassin", ClassName = "Assassin", Description = "Quick and deadly, striking from the shadows.", HealthGrowth = 0.8f, AttackGrowth = 1.4f, DefenseGrowth = 0.5f, SpeedGrowth = 1.8f, ManaGrowth = 0.6f });
            Register(new BattleClass { ClassId = "archer", ClassName = "Archer", Description = "Precise ranged attacks and tactical support.", HealthGrowth = 0.9f, AttackGrowth = 1.2f, DefenseGrowth = 0.7f, SpeedGrowth = 1.2f, ManaGrowth = 0.8f });

            Log.Info("Registered 6 default battle classes (hardcoded)");
        }

        #endregion
    }

    #region JSON Data Models

    /// <summary>
    /// JSON根包装类
    /// </summary>
    public class ClassDataWrapper
    {
        public List<ClassJsonDefinition> Classes { get; set; } = new();
    }

    /// <summary>
    /// JSON职业定义
    /// </summary>
    public class ClassJsonDefinition
    {
        public string ClassId { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string Description { get; set; } = "";
        public float HealthGrowth { get; set; } = 1f;
        public float AttackGrowth { get; set; } = 1f;
        public float DefenseGrowth { get; set; } = 1f;
        public float SpeedGrowth { get; set; } = 1f;
        public float ManaGrowth { get; set; } = 1f;
        public List<ClassSkillEntryJson> Skills { get; set; } = new();
        public List<ClassPassiveEntryJson> Passives { get; set; } = new();
        public string UltimateId { get; set; } = "";
        public int UltimateJpCost { get; set; } = 0;
    }

    /// <summary>
    /// JSON技能条目
    /// </summary>
    public class ClassSkillEntryJson
    {
        public string Id { get; set; } = "";
        public int JpCost { get; set; } = 0;
    }

    /// <summary>
    /// JSON被动条目
    /// </summary>
    public class ClassPassiveEntryJson
    {
        public string Id { get; set; } = "";
        public int RequiredActiveUnlocks { get; set; } = 999;
    }

    #endregion

    /// <summary>
    /// 技能条目类
    /// </summary>
    [GlobalClass]
    public partial class SkillEntry : Resource
    {
        public int LevelRequired { get; set; } = 1;
        public Skill SkillResource { get; set; }
    }

    /// <summary>
    /// 被动技能条目类
    /// </summary>
    [GlobalClass]
    public partial class PassiveSkillEntry : Resource
    {
        public int LevelRequired { get; set; } = 1;
        public PassiveSkill PassiveResource { get; set; }
    }
}
