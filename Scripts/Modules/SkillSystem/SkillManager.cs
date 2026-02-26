using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Managers;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    /// <summary>
    /// 技能管理器，负责创建和管理默认技能
    /// </summary>
    public static class SkillManager
    {
        private static readonly Dictionary<string, Skill> _skillTemplates = new();
        private static bool _initialized = false;

        /// <summary>
        /// 初始化技能模板
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            CreateDefaultSkills();
            _initialized = true;
            // 使用Godot的日志系统
            GD.Print("SkillManager initialized with default skills");
        }

        /// <summary>
        /// 创建默认技能
        /// </summary>
        private static void CreateDefaultSkills()
        {
            // 基础攻击技能
            var basicAttack = CreateBasicAttack();
            _skillTemplates["basic_attack"] = basicAttack;

            // 基础治疗技能
            var basicHeal = CreateBasicHeal();
            _skillTemplates["basic_heal"] = basicHeal;

            // 火球术
            var fireball = CreateFireball();
            _skillTemplates["fireball"] = fireball;

            // 防御姿态
            var defenseStance = CreateDefenseStance();
            _skillTemplates["defense_stance"] = defenseStance;

            // 力量祝福
            var strengthBlessing = CreateStrengthBlessing();
            _skillTemplates["strength_blessing"] = strengthBlessing;
        }

        /// <summary>
        /// 创建基础攻击技能
        /// </summary>
        private static Skill CreateBasicAttack()
        {
            var skill = new Skill
            {
                Id = "basic_attack",
                SkillName = "基础攻击",
                Description = "对目标造成物理伤害",
                Cooldown = 0f,
                ManaCost = 0,
                IsUnlocked = true
            };

            var attackDef = new Skill.SkillDefent
            {
                Type = Skill.SkillType.Attack,
                DamageCoefficient = 1.0f,
                Duration = 0,
                DamageTypeString = "Physical"
            };

            skill.SkillDefs.Add(attackDef);
            return skill;
        }

        /// <summary>
        /// 创建基础治疗技能
        /// </summary>
        private static Skill CreateBasicHeal()
        {
            var skill = new Skill
            {
                Id = "basic_heal",
                SkillName = "基础治疗",
                Description = "恢复自身生命值",
                Cooldown = 2f,
                ManaCost = 10,
                IsUnlocked = true
            };

            var healDef = new Skill.SkillDefent
            {
                Type = Skill.SkillType.Healing,
                DamageCoefficient = 30f, // 治疗量
                Duration = 0,
                DamageTypeString = "Holy"
            };

            skill.SkillDefs.Add(healDef);
            return skill;
        }

        /// <summary>
        /// 创建火球术技能
        /// </summary>
        private static Skill CreateFireball()
        {
            var skill = new Skill
            {
                Id = "fireball",
                SkillName = "火球术",
                Description = "对目标造成火焰伤害",
                Cooldown = 1f,
                ManaCost = 15,
                IsUnlocked = true
            };

            var fireballDef = new Skill.SkillDefent
            {
                Type = Skill.SkillType.Attack,
                DamageCoefficient = 1.5f,
                Duration = 0,
                DamageTypeString = "Fire"
            };

            skill.SkillDefs.Add(fireballDef);
            return skill;
        }

        /// <summary>
        /// 创建防御姿态技能
        /// </summary>
        private static Skill CreateDefenseStance()
        {
            var skill = new Skill
            {
                Id = "defense_stance",
                SkillName = "防御姿态",
                Description = "提升自身防御力",
                Cooldown = 5f,
                ManaCost = 5,
                IsUnlocked = true
            };

            var defenseDef = new Skill.SkillDefent
            {
                Type = Skill.SkillType.Defense,
                DamageCoefficient = 0.5f, // 防御提升系数
                Duration = 3, // 持续3回合
                DamageTypeString = "Physical"
            };

            skill.SkillDefs.Add(defenseDef);
            return skill;
        }

        /// <summary>
        /// 创建力量祝福技能
        /// </summary>
        private static Skill CreateStrengthBlessing()
        {
            var skill = new Skill
            {
                Id = "strength_blessing",
                SkillName = "力量祝福",
                Description = "提升自身攻击力",
                Cooldown = 4f,
                ManaCost = 8,
                IsUnlocked = true
            };

            var blessingDef = new Skill.SkillDefent
            {
                Type = Skill.SkillType.Support,
                DamageCoefficient = 0.3f, // 攻击提升系数
                Duration = 4, // 持续4回合
                DamageTypeString = "Holy"
            };

            skill.SkillDefs.Add(blessingDef);
            return skill;
        }

        /// <summary>
        /// 获取技能模板
        /// </summary>
        public static Skill GetSkillTemplate(string skillId)
        {
            if (!_initialized) Initialize();
            return _skillTemplates.TryGetValue(skillId, out var skill) ? skill : null;
        }

        /// <summary>
        /// 获取所有技能模板
        /// </summary>
        public static Dictionary<string, Skill> GetAllSkillTemplates()
        {
            if (!_initialized) Initialize();
            return new Dictionary<string, Skill>(_skillTemplates);
        }

        /// <summary>
        /// 创建技能实例
        /// </summary>
        public static Skill CreateSkillInstance(string skillId)
        {
            var template = GetSkillTemplate(skillId);
            if (template == null) return null;

            // 创建技能副本
            var skillInstance = new Skill
            {
                Id = template.Id,
                SkillName = template.SkillName,
                Description = template.Description,
                Cooldown = template.Cooldown,
                ManaCost = template.ManaCost,
                IsUnlocked = template.IsUnlocked
            };

            // 复制技能定义
            foreach (var def in template.SkillDefs)
            {
                var newDef = new Skill.SkillDefent
                {
                    Type = def.Type,
                    DamageCoefficient = def.DamageCoefficient,
                    Duration = def.Duration,
                    DamageTypeString = def.DamageTypeString
                };
                skillInstance.SkillDefs.Add(newDef);
            }

            return skillInstance;
        }

        /// <summary>
        /// 创建玩家初始技能
        /// </summary>
        public static List<Skill> CreatePlayerInitialSkills()
        {
            if (!_initialized) Initialize();

            var initialSkills = new List<Skill>();

            // 添加基础攻击
            var basicAttack = CreateSkillInstance("basic_attack");
            if (basicAttack != null) initialSkills.Add(basicAttack);

            // 添加基础治疗
            var basicHeal = CreateSkillInstance("basic_heal");
            if (basicHeal != null) initialSkills.Add(basicHeal);

            // 添加火球术
            var fireball = CreateSkillInstance("fireball");
            if (fireball != null) initialSkills.Add(fireball);

            return initialSkills;
        }

        /// <summary>
        /// 添加自定义技能模板
        /// </summary>
        public static void AddSkillTemplate(string skillId, Skill skill)
        {
            if (string.IsNullOrEmpty(skillId) || skill == null) return;
            
            _skillTemplates[skillId] = skill;
            // 使用Godot的日志系统
            GD.Print($"Added custom skill template: {skill.SkillName} ({skillId})");
        }
    }
}