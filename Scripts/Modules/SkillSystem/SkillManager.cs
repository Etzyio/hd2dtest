/*
 * File: SkillManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 技能管理器，负责创建和管理游戏中所有技能的模板。
 * 支持6个职业各8个主动技能+1个大招+4个被动，主角额外拥有专属技能。
 *
 * Key Features:
 * - 6职业技能注册：剑客/治疗/法师/坦克/刺客/射手
 * - 每职业8主动技能+1大招+4被动
 * - 主角专属技能系统
 * - 技能模板跨职业兼容
 */

using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Modules.Battle;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    public static class SkillManager
    {
        private static readonly Dictionary<string, Skill> _skillTemplates = new();
        private static readonly Dictionary<string, PassiveSkill> _passiveTemplates = new();
        private static bool _initialized = false;

        // Class skill ID lists - each class has 8 active + 1 ultimate
        public static readonly List<string> SwordsmanSkills = new() {
            "swd_slash", "swd_charge", "swd_whirlwind", "swd_thrust",
            "swd_counter", "swd_dual_strike", "swd_war_cry", "swd_armor_break",
            "swd_ultimate_overlord" // 大招收尾
        };
        public static readonly List<string> SwordsmanPassives = new() {
            "swd_p_sword_mastery", "swd_p_battle_fury", "swd_p_iron_will", "swd_p_blade_dance"
        };

        public static readonly List<string> HealerSkills = new() {
            "hlr_heal", "hlr_greater_heal", "hlr_regen", "hlr_purify",
            "hlr_shield", "hlr_bless", "hlr_revive", "hlr_cleanse",
            "hlr_ultimate_holy_light"
        };
        public static readonly List<string> HealerPassives = new() {
            "hlr_p_divine_grace", "hlr_p_healing_mastery", "hlr_p_holy_protection", "hlr_p_mana_well"
        };

        public static readonly List<string> MageSkills = new() {
            "mg_fireball", "mg_ice_shard", "mg_lightning", "mg_arcane_blast",
            "mg_frost_nova", "mg_fire_wall", "mg_teleport", "mg_mana_shield",
            "mg_ultimate_storm"
        };
        public static readonly List<string> MagePassives = new() {
            "mg_p_fire_mastery", "mg_p_ice_mastery", "mg_p_arcane_power", "mg_p_mana_surge"
        };

        public static readonly List<string> TankSkills = new() {
            "tnk_shield_bash", "tnk_taunt", "tnk_fortify", "tnk_shield_wall",
            "tnk_ground_stomp", "tnk_provoke", "tnk_vengeance", "tnk_last_stand",
            "tnk_ultimate_immortal"
        };
        public static readonly List<string> TankPassives = new() {
            "tnk_p_toughness", "tnk_p_shield_mastery", "tnk_p_iron_skin", "tnk_p_unyielding"
        };

        public static readonly List<string> AssassinSkills = new() {
            "ast_backstab", "ast_shadow_step", "st_poison_blade", "ast_smoke_bomb",
            "ast_eviscerate", "ast_stealth", "ast_gouge", "ast_death_mark",
            "ast_ultimate_assassinate"
        };
        public static readonly List<string> AssassinPassives = new() {
            "ast_p_shadow_mastery", "ast_p_critical_edge", "ast_p_poison_mastery", "ast_p_fleet_footed"
        };

        public static readonly List<string> ArcherSkills = new() {
            "arc_power_shot", "arc_rapid_fire", "arc_aimed_shot", "arc_rain_of_arrows",
            "arc_snipe", "arc_trap", "arc_camouflage", "arc_multi_shot",
            "arc_ultimate_barrage"
        };
        public static readonly List<string> ArcherPassives = new() {
            "arc_p_bow_mastery", "arc_p_eagle_eye", "arc_p_swift_reload", "arc_p_ranged_prowess"
        };

        // Main character exclusive skills (only available to protagonist)
        public static readonly List<string> ExclusiveSkills = new() {
            "ex_sword_spirit", "ex_heavenly_strike", "ex_will_of_qingfeng",
            "ex_ultimate_annihilation"
        };

        public static void Initialize()
        {
            if (_initialized) return;

            RegisterSwordsmanSkills();
            RegisterHealerSkills();
            RegisterMageSkills();
            RegisterTankSkills();
            RegisterAssassinSkills();
            RegisterArcherSkills();
            RegisterExclusiveSkills();

            _initialized = true;
            Log.Info($"SkillManager initialized with {_skillTemplates.Count} skills and {_passiveTemplates.Count} passives");
        }

        #region Skill Registration Helpers

        private static Skill MakeActive(string id, string name, string desc, int manaCost, float cooldown,
            Skill.SkillType type, float coeff, string dmgType, int duration = 0)
        {
            var skill = new Skill
            {
                Id = id,
                SkillName = name,
                Description = desc,
                ManaCost = manaCost,
                Cooldown = cooldown,
                IsUnlocked = true
            };
            var def = new Skill.SkillDefent
            {
                Type = type,
                DamageCoefficient = coeff,
                Duration = duration,
                DamageTypeString = dmgType
            };
            skill.SkillDefs.Add(def);
            return skill;
        }

        private static void RegisterPassive(string id, string name, string desc,
            float hpBonus = 0, float atkBonus = 0, float defBonus = 0, float spdBonus = 0,
            float hpMult = 1f, float atkMult = 1f, float defMult = 1f)
        {
            _passiveTemplates[id] = new PassiveSkill
            {
                PassiveName = name,
                Description = desc,
                HealthBonus = hpBonus,
                AttackBonus = atkBonus,
                DefenseBonus = defBonus,
                SpeedBonus = spdBonus,
                HealthMultiplier = hpMult,
                AttackMultiplier = atkMult,
                DefenseMultiplier = defMult
            };
        }

        #endregion

        #region Swordsman (剑客)

        private static void RegisterSwordsmanSkills()
        {
            _skillTemplates["swd_slash"] = MakeActive("swd_slash", "Slash", "A swift sword strike.", 5, 0, Skill.SkillType.Attack, 1.0f, "Physical");
            _skillTemplates["swd_charge"] = MakeActive("swd_charge", "Charge", "A powerful charging strike.", 8, 1, Skill.SkillType.Attack, 1.3f, "Physical");
            _skillTemplates["swd_whirlwind"] = MakeActive("swd_whirlwind", "Whirlwind", "Spin and strike all enemies.", 12, 2, Skill.SkillType.Attack, 0.8f, "Physical");
            _skillTemplates["swd_thrust"] = MakeActive("swd_thrust", "Thrust", "A precise thrust attack.", 6, 0, Skill.SkillType.Attack, 1.2f, "Physical");
            _skillTemplates["swd_counter"] = MakeActive("swd_counter", "Counter Stance", "Enter a countering stance.", 7, 2, Skill.SkillType.Defense, 0.5f, "Physical", 2);
            _skillTemplates["swd_dual_strike"] = MakeActive("swd_dual_strike", "Dual Strike", "Strike twice in quick succession.", 14, 3, Skill.SkillType.Attack, 1.8f, "Physical");
            _skillTemplates["swd_war_cry"] = MakeActive("swd_war_cry", "War Cry", "Boost ally attack power.", 10, 3, Skill.SkillType.Support, 0.25f, "Physical", 3);
            _skillTemplates["swd_armor_break"] = MakeActive("swd_armor_break", "Armor Break", "Shatter enemy defenses.", 8, 2, Skill.SkillType.Attack, 0.9f, "Physical");
            _skillTemplates["swd_ultimate_overlord"] = MakeActive("swd_ultimate_overlord", "Overlord Slash", "The ultimate sword technique.", 30, 6, Skill.SkillType.Attack, 3.5f, "Physical");

            RegisterPassive("swd_p_sword_mastery", "Sword Mastery", "Mastery of the sword.", atkBonus:8, defMult:1.05f);
            RegisterPassive("swd_p_battle_fury", "Battle Fury", "Fury in battle.", atkBonus:5, spdBonus:5);
            RegisterPassive("swd_p_iron_will", "Iron Will", "Unbreakable will.", defBonus:10, hpBonus:50);
            RegisterPassive("swd_p_blade_dance", "Blade Dance", "Dance of the blade.", spdBonus:10, atkMult:1.08f);
        }

        #endregion

        #region Healer (治疗)

        private static void RegisterHealerSkills()
        {
            _skillTemplates["hlr_heal"] = MakeActive("hlr_heal", "Heal", "Restore ally HP.", 10, 1, Skill.SkillType.Healing, 30f, "Holy");
            _skillTemplates["hlr_greater_heal"] = MakeActive("hlr_greater_heal", "Greater Heal", "Massive HP restoration.", 20, 3, Skill.SkillType.Healing, 60f, "Holy");
            _skillTemplates["hlr_regen"] = MakeActive("hlr_regen", "Regeneration", "Heal over time.", 12, 2, Skill.SkillType.Healing, 10f, "Holy", 3);
            _skillTemplates["hlr_purify"] = MakeActive("hlr_purify", "Purify", "Remove debuffs from an ally.", 8, 1, Skill.SkillType.Support, 0f, "Holy");
            _skillTemplates["hlr_shield"] = MakeActive("hlr_shield", "Holy Shield", "Protect an ally with a barrier.", 15, 3, Skill.SkillType.Defense, 0.3f, "Holy", 3);
            _skillTemplates["hlr_bless"] = MakeActive("hlr_bless", "Blessing", "Bless an ally, boosting their power.", 10, 2, Skill.SkillType.Support, 0.2f, "Holy", 4);
            _skillTemplates["hlr_revive"] = MakeActive("hlr_revive", "Revive", "Return a fallen ally to battle.", 40, 8, Skill.SkillType.Healing, 50f, "Holy");
            _skillTemplates["hlr_cleanse"] = MakeActive("hlr_cleanse", "Cleanse", "Cleanse all negative effects.", 6, 1, Skill.SkillType.Support, 0f, "Holy");
            _skillTemplates["hlr_ultimate_holy_light"] = MakeActive("hlr_ultimate_holy_light", "Holy Radiance", "Bathe all allies in holy light.", 35, 6, Skill.SkillType.Healing, 100f, "Holy");

            RegisterPassive("hlr_p_divine_grace", "Divine Grace", "Grace of the divine.", hpBonus:30, defMult:1.1f);
            RegisterPassive("hlr_p_healing_mastery", "Healing Mastery", "Mastery of healing arts.", atkBonus:5);
            RegisterPassive("hlr_p_holy_protection", "Holy Protection", "Protected by holy power.", defBonus:8, hpMult:1.1f);
            RegisterPassive("hlr_p_mana_well", "Mana Well", "Deep well of mana.", spdBonus:5);
        }

        #endregion

        #region Mage (法师)

        private static void RegisterMageSkills()
        {
            _skillTemplates["mg_fireball"] = MakeActive("mg_fireball", "Fireball", "A blazing ball of fire.", 15, 1, Skill.SkillType.Attack, 1.5f, "Fire");
            _skillTemplates["mg_ice_shard"] = MakeActive("mg_ice_shard", "Ice Shard", "A sharp shard of ice.", 12, 1, Skill.SkillType.Attack, 1.2f, "Ice");
            _skillTemplates["mg_lightning"] = MakeActive("mg_lightning", "Lightning Bolt", "Call down lightning.", 18, 2, Skill.SkillType.Attack, 2.0f, "Thunder");
            _skillTemplates["mg_arcane_blast"] = MakeActive("mg_arcane_blast", "Arcane Blast", "Unleash raw arcane energy.", 22, 3, Skill.SkillType.Attack, 2.5f, "Magic");
            _skillTemplates["mg_frost_nova"] = MakeActive("mg_frost_nova", "Frost Nova", "Freeze all enemies in place.", 10, 2, Skill.SkillType.Attack, 0.7f, "Ice", 2);
            _skillTemplates["mg_fire_wall"] = MakeActive("mg_fire_wall", "Fire Wall", "A wall of protective fire.", 16, 3, Skill.SkillType.Defense, 0.4f, "Fire", 2);
            _skillTemplates["mg_teleport"] = MakeActive("mg_teleport", "Teleport", "Teleport to safety.", 8, 2, Skill.SkillType.Support, 0f, "Magic");
            _skillTemplates["mg_mana_shield"] = MakeActive("mg_mana_shield", "Mana Shield", "Convert mana to protection.", 10, 3, Skill.SkillType.Defense, 0.5f, "Magic", 3);
            _skillTemplates["mg_ultimate_storm"] = MakeActive("mg_ultimate_storm", "Elemental Storm", "Unleash a devastating elemental storm.", 35, 6, Skill.SkillType.Attack, 4.0f, "Thunder");

            RegisterPassive("mg_p_fire_mastery", "Fire Mastery", "Mastery over fire.", atkBonus:10);
            RegisterPassive("mg_p_ice_mastery", "Ice Mastery", "Mastery over ice.", atkBonus:10);
            RegisterPassive("mg_p_arcane_power", "Arcane Power", "Raw arcane might.", atkMult:1.1f);
            RegisterPassive("mg_p_mana_surge", "Mana Surge", "Surge of magical energy.", spdBonus:8);
        }

        #endregion

        #region Tank (坦克)

        private static void RegisterTankSkills()
        {
            _skillTemplates["tnk_shield_bash"] = MakeActive("tnk_shield_bash", "skill_tnk_shield_bash", "", 5, 0, Skill.SkillType.Attack, 0.8f, "Physical");
            _skillTemplates["tnk_taunt"] = MakeActive("tnk_taunt", "skill_tnk_taunt", "", 3, 1, Skill.SkillType.Support, 0f, "Physical");
            _skillTemplates["tnk_fortify"] = MakeActive("tnk_fortify", "skill_tnk_fortify", "", 8, 2, Skill.SkillType.Defense, 0.6f, "Physical", 3);
            _skillTemplates["tnk_shield_wall"] = MakeActive("tnk_shield_wall", "skill_tnk_shield_wall", "", 12, 4, Skill.SkillType.Defense, 0.8f, "Physical", 4);
            _skillTemplates["tnk_ground_stomp"] = MakeActive("tnk_ground_stomp", "skill_tnk_ground_stomp", "", 10, 2, Skill.SkillType.Attack, 1.0f, "Physical");
            _skillTemplates["tnk_provoke"] = MakeActive("tnk_provoke", "skill_tnk_provoke", "", 5, 1, Skill.SkillType.Support, 0f, "Physical", 2);
            _skillTemplates["tnk_vengeance"] = MakeActive("tnk_vengeance", "skill_tnk_vengeance", "", 15, 3, Skill.SkillType.Attack, 1.5f, "Physical");
            _skillTemplates["tnk_last_stand"] = MakeActive("tnk_last_stand", "skill_tnk_last_stand", "", 20, 5, Skill.SkillType.Defense, 0.9f, "Physical", 2);
            _skillTemplates["tnk_ultimate_immortal"] = MakeActive("tnk_ultimate_immortal", "skill_tnk_ultimate", "", 30, 6, Skill.SkillType.Defense, 0.95f, "Physical", 3);

            RegisterPassive("tnk_p_toughness", "passive_tnk_toughness", "", hpBonus:100, defBonus:10);
            RegisterPassive("tnk_p_shield_mastery", "passive_tnk_shield", "", defBonus:15, hpMult:1.1f);
            RegisterPassive("tnk_p_iron_skin", "passive_tnk_iron_skin", "", defMult:1.1f);
            RegisterPassive("tnk_p_unyielding", "passive_tnk_unyielding", "", hpBonus:50, defMult:1.05f);
        }

        #endregion

        #region Assassin (刺客)

        private static void RegisterAssassinSkills()
        {
            _skillTemplates["ast_backstab"] = MakeActive("ast_backstab", "skill_ast_backstab", "", 8, 1, Skill.SkillType.Attack, 1.8f, "Physical");
            _skillTemplates["ast_shadow_step"] = MakeActive("ast_shadow_step", "skill_ast_shadow_step", "", 6, 2, Skill.SkillType.Support, 0f, "Dark");
            _skillTemplates["st_poison_blade"] = MakeActive("st_poison_blade", "skill_ast_poison_blade", "", 10, 2, Skill.SkillType.Attack, 1.2f, "Poison", 3);
            _skillTemplates["ast_smoke_bomb"] = MakeActive("ast_smoke_bomb", "skill_ast_smoke_bomb", "", 5, 2, Skill.SkillType.Support, 0f, "Dark");
            _skillTemplates["ast_eviscerate"] = MakeActive("ast_eviscerate", "skill_ast_eviscerate", "", 16, 3, Skill.SkillType.Attack, 2.5f, "Physical");
            _skillTemplates["ast_stealth"] = MakeActive("ast_stealth", "skill_ast_stealth", "", 8, 3, Skill.SkillType.Defense, 0.3f, "Dark", 2);
            _skillTemplates["ast_gouge"] = MakeActive("ast_gouge", "skill_ast_gouge", "", 7, 1, Skill.SkillType.Attack, 1.0f, "Physical");
            _skillTemplates["ast_death_mark"] = MakeActive("ast_death_mark", "skill_ast_death_mark", "", 12, 3, Skill.SkillType.Attack, 1.5f, "Dark", 2);
            _skillTemplates["ast_ultimate_assassinate"] = MakeActive("ast_ultimate_assassinate", "skill_ast_ultimate", "", 28, 6, Skill.SkillType.Attack, 4.5f, "Dark");

            RegisterPassive("ast_p_shadow_mastery", "passive_ast_shadow", "", atkBonus:8, spdBonus:10);
            RegisterPassive("ast_p_critical_edge", "passive_ast_critical", "", atkMult:1.1f);
            RegisterPassive("ast_p_poison_mastery", "passive_ast_poison", "", atkBonus:5);
            RegisterPassive("ast_p_fleet_footed", "passive_ast_fleet", "", spdBonus:15);
        }

        #endregion

        #region Archer (射手)

        private static void RegisterArcherSkills()
        {
            _skillTemplates["arc_power_shot"] = MakeActive("arc_power_shot", "skill_arc_power_shot", "", 8, 1, Skill.SkillType.Attack, 1.3f, "Physical");
            _skillTemplates["arc_rapid_fire"] = MakeActive("arc_rapid_fire", "skill_arc_rapid_fire", "", 6, 0, Skill.SkillType.Attack, 0.6f, "Physical");
            _skillTemplates["arc_aimed_shot"] = MakeActive("arc_aimed_shot", "skill_arc_aimed_shot", "", 14, 2, Skill.SkillType.Attack, 2.0f, "Physical");
            _skillTemplates["arc_rain_of_arrows"] = MakeActive("arc_rain_of_arrows", "skill_arc_rain", "", 16, 3, Skill.SkillType.Attack, 1.2f, "Physical");
            _skillTemplates["arc_snipe"] = MakeActive("arc_snipe", "skill_arc_snipe", "", 20, 4, Skill.SkillType.Attack, 3.0f, "Physical");
            _skillTemplates["arc_trap"] = MakeActive("arc_trap", "skill_arc_trap", "", 8, 2, Skill.SkillType.Attack, 0.8f, "Physical", 2);
            _skillTemplates["arc_camouflage"] = MakeActive("arc_camouflage", "skill_arc_camouflage", "", 5, 3, Skill.SkillType.Defense, 0.2f, "Physical", 2);
            _skillTemplates["arc_multi_shot"] = MakeActive("arc_multi_shot", "skill_arc_multi_shot", "", 12, 2, Skill.SkillType.Attack, 0.9f, "Physical");
            _skillTemplates["arc_ultimate_barrage"] = MakeActive("arc_ultimate_barrage", "skill_arc_ultimate", "", 32, 6, Skill.SkillType.Attack, 3.8f, "Physical");

            RegisterPassive("arc_p_bow_mastery", "passive_arc_bow", "", atkBonus:8);
            RegisterPassive("arc_p_eagle_eye", "passive_arc_eagle", "", atkMult:1.08f, spdBonus:5);
            RegisterPassive("arc_p_swift_reload", "passive_arc_swift", "", spdBonus:10);
            RegisterPassive("arc_p_ranged_prowess", "passive_arc_ranged", "", atkBonus:5, defBonus:5);
        }

        #endregion

        #region Exclusive Skills (主角专属)

        private static void RegisterExclusiveSkills()
        {
            _skillTemplates["ex_sword_spirit"] = MakeActive("ex_sword_spirit", "skill_ex_sword_spirit", "", 15, 3, Skill.SkillType.Attack, 2.2f, "Physical");
            _skillTemplates["ex_heavenly_strike"] = MakeActive("ex_heavenly_strike", "skill_ex_heavenly_strike", "", 20, 4, Skill.SkillType.Attack, 2.8f, "Light");
            _skillTemplates["ex_will_of_qingfeng"] = MakeActive("ex_will_of_qingfeng", "skill_ex_will_qingfeng", "", 12, 3, Skill.SkillType.Support, 0.35f, "Light", 3);
            _skillTemplates["ex_ultimate_annihilation"] = MakeActive("ex_ultimate_annihilation", "skill_ex_ultimate", "", 40, 8, Skill.SkillType.Attack, 5.0f, "Light");
        }

        #endregion

        #region Public API

        public static Skill GetSkillTemplate(string skillId)
        {
            if (!_initialized) Initialize();
            return _skillTemplates.TryGetValue(skillId, out var skill) ? skill : null;
        }

        public static PassiveSkill GetPassiveTemplate(string passiveId)
        {
            if (!_initialized) Initialize();
            return _passiveTemplates.TryGetValue(passiveId, out var ps) ? ps : null;
        }

        public static Dictionary<string, Skill> GetAllSkillTemplates()
        {
            if (!_initialized) Initialize();
            return new Dictionary<string, Skill>(_skillTemplates);
        }

        public static Dictionary<string, PassiveSkill> GetAllPassiveTemplates()
        {
            if (!_initialized) Initialize();
            return new Dictionary<string, PassiveSkill>(_passiveTemplates);
        }

        public static Skill CreateSkillInstance(string skillId)
        {
            var template = GetSkillTemplate(skillId);
            if (template == null) return null;

            var instance = new Skill
            {
                Id = template.Id,
                SkillName = template.SkillName,
                Description = template.Description,
                Cooldown = template.Cooldown,
                ManaCost = template.ManaCost,
                IsUnlocked = template.IsUnlocked
            };

            foreach (var def in template.SkillDefs)
            {
                var newDef = new Skill.SkillDefent
                {
                    Type = def.Type,
                    DamageCoefficient = def.DamageCoefficient,
                    Duration = def.Duration,
                    DamageTypeString = def.DamageTypeString
                };
                instance.SkillDefs.Add(newDef);
            }

            return instance;
        }

        public static PassiveSkill CreatePassiveInstance(string passiveId)
        {
            var template = GetPassiveTemplate(passiveId);
            if (template == null) return null;

            return new PassiveSkill
            {
                PassiveName = template.PassiveName,
                Description = template.Description,
                HealthBonus = template.HealthBonus,
                AttackBonus = template.AttackBonus,
                DefenseBonus = template.DefenseBonus,
                SpeedBonus = template.SpeedBonus,
                HealthMultiplier = template.HealthMultiplier,
                AttackMultiplier = template.AttackMultiplier,
                DefenseMultiplier = template.DefenseMultiplier,
                EffectId = template.EffectId
            };
        }

        /// <summary>
        /// 为角色创建职业对应的完整技能集（8主动+1大招+4被动）
        /// </summary>
        public static (List<Skill> activeSkills, Skill ultimate, List<PassiveSkill> passives) CreateClassSkillSet(string classId)
        {
            if (!_initialized) Initialize();

            var activeSkills = new List<Skill>();
            Skill ultimate = null;
            var passives = new List<PassiveSkill>();

            // Get the skill ID lists
            var (activeIds, ultimateId, passiveIds) = GetClassSkillIds(classId);

            foreach (var sid in activeIds)
            {
                var s = CreateSkillInstance(sid);
                if (s != null) activeSkills.Add(s);
            }

            if (!string.IsNullOrEmpty(ultimateId))
            {
                ultimate = CreateSkillInstance(ultimateId);
            }

            foreach (var pid in passiveIds)
            {
                var p = CreatePassiveInstance(pid);
                if (p != null) passives.Add(p);
            }

            return (activeSkills, ultimate, passives);
        }

        public static (List<string> activeIds, string ultimateId, List<string> passiveIds) GetClassSkillIds(string classId)
        {
            var activeIds = new List<string>();
            string ultimateId = null;
            var passiveIds = new List<string>();

            switch (classId)
            {
                case "swordsman":
                    activeIds = SwordsmanSkills.GetRange(0, 8);
                    ultimateId = SwordsmanSkills[8];
                    passiveIds = SwordsmanPassives;
                    break;
                case "healer":
                    activeIds = HealerSkills.GetRange(0, 8);
                    ultimateId = HealerSkills[8];
                    passiveIds = HealerPassives;
                    break;
                case "mage":
                    activeIds = MageSkills.GetRange(0, 8);
                    ultimateId = MageSkills[8];
                    passiveIds = MagePassives;
                    break;
                case "tank":
                    activeIds = TankSkills.GetRange(0, 8);
                    ultimateId = TankSkills[8];
                    passiveIds = TankPassives;
                    break;
                case "assassin":
                    activeIds = AssassinSkills.GetRange(0, 8);
                    ultimateId = AssassinSkills[8];
                    passiveIds = AssassinPassives;
                    break;
                case "archer":
                    activeIds = ArcherSkills.GetRange(0, 8);
                    ultimateId = ArcherSkills[8];
                    passiveIds = ArcherPassives;
                    break;
            }

            return (activeIds, ultimateId, passiveIds);
        }

        public static List<Skill> CreatePlayerInitialSkills()
        {
            if (!_initialized) Initialize();

            var skills = new List<Skill>();

            // Add basic attack and heal
            var basicAttack = CreateSkillInstance("basic_attack");
            if (basicAttack != null) skills.Add(basicAttack);

            var basicHeal = CreateSkillInstance("basic_heal");
            if (basicHeal != null) skills.Add(basicHeal);

            var fireball = CreateSkillInstance("fireball");
            if (fireball != null) skills.Add(fireball);

            return skills;
        }

        public static void AddSkillTemplate(string skillId, Skill skill)
        {
            if (string.IsNullOrEmpty(skillId) || skill == null) return;
            _skillTemplates[skillId] = skill;
            Log.Info($"Added custom skill template: {skill.SkillName} ({skillId})");
        }

        #endregion
    }
}
