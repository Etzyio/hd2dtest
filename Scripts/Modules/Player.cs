/*
 * File: Player.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 玩家类，继承自Creature类，定义玩家角色的属性和行为。
 * 包含装备系统、技能系统、职业系统、被动技能系统和统计数据。
 * 
 * Key Features:
 * - 装备系统：武器和防具装备管理
 * - 技能系统：技能学习、使用和魔法消耗
 * - 职业系统：主职业和副职业支持
 * - 被动技能系统：最多4个被动技能槽位
 * - 属性计算：基于等级、职业、装备和被动技能的综合计算
 * - 经验和等级系统
 * - 金币和击杀统计
 * - 完整的异常处理和日志记录
 * - 支持多语言翻译
 */

using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;
using System.Linq;
using Godot;
using hd2dtest.Scripts.Modules.SkillSystem;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 玩家类，继承自Creature类，定义玩家角色的属性和行为
    /// </summary>
    /// <remarks>
    /// 玩家类包含玩家的移动属性、装备系统、技能系统、职业系统和统计数据
    /// 支持武器装备、防具装备、技能使用、职业切换和被动技能装备等功能
    /// </remarks>
    public partial class Player : Creature
    {
        /// <summary>
        /// 移动速度
        /// </summary>
        /// <value>玩家正常移动时的速度，默认为5.0f</value>
        [Export] public float MoveSpeed = 5.0f;

        /// <summary>
        /// 冲刺速度倍率
        /// </summary>
        /// <value>冲刺时移动速度的倍率，默认为1.5f</value>
        [Export] public float SprintMultiplier = 1.5f;

        /// <summary>
        /// 是否正在冲刺
        /// </summary>
        /// <value>true 表示玩家正在冲刺，false 表示正常移动</value>
        public bool IsSprinting { get; private set; } = false;

        /// <summary>
        /// 玩家拥有的武器列表
        /// </summary>
        /// <value>玩家已收集的所有武器集合</value>
        public List<Weapon> Weapons { get; } = [];

        /// <summary>
        /// 玩家拥有的装备列表
        /// </summary>
        /// <value>玩家已装备的所有防具和饰品集合</value>
        public List<Equipment> Equipments { get; } = [];

        /// <summary>
        /// 当前装备的武器
        /// </summary>
        /// <value>玩家当前正在使用的武器，可为null</value>
        public Weapon CurrentWeapon { get; private set; } = null;

        /// <summary>
        /// 玩家拥有的技能列表
        /// </summary>
        /// <value>玩家已解锁的所有技能集合</value>
        public List<Skill> Skills { get; } = [];

        /// <summary>
        /// 是否正在移动
        /// </summary>
        /// <value>true 表示玩家正在移动中</value>
        public bool IsMoving { get; set; } = false;

        /// <summary>
        /// 是否正在攻击
        /// </summary>
        /// <value>true 表示玩家正在执行攻击动作</value>
        public bool IsAttacking { get; set; } = false;

        /// <summary>
        /// 是否正在防御
        /// </summary>
        /// <value>true 表示玩家处于防御状态</value>
        public bool IsDefending { get; set; } = false;

        /// <summary>
        /// 移动方向
        /// </summary>
        /// <value>玩家当前的移动方向向量</value>
        public Vector2 Direction { get; set; } = Vector2.Zero;

        /// <summary>
        /// 金币数量
        /// </summary>
        /// <value>玩家当前拥有的金币数量</value>
        public int Gold { get; set; } = 0;

        /// <summary>
        /// 击杀计数
        /// </summary>
        /// <value>玩家累计击杀的敌人数量</value>
        public int KillCount { get; set; } = 0;

        /// <summary>
        /// 死亡计数
        /// </summary>
        /// <value>玩家累计死亡的次数</value>
        public int DeathCount { get; set; } = 0;

        /// <summary>
        /// 背包物品
        /// </summary>
        /// <value>物品ID与数量的映射字典</value>
        public Dictionary<string, int> Inventory { get; set; } = [];

        /// <summary>
        /// 当前魔法值
        /// </summary>
        /// <value>玩家当前的魔法值，范围：0 到 MaxMana</value>
        public float Mana { get; set; } = 50f;

        /// <summary>
        /// 最大魔法值
        /// </summary>
        /// <value>玩家的魔法值上限，默认为50f</value>
        public float MaxMana { get; set; } = 50f;

        /// <summary>
        /// 主职业
        /// </summary>
        /// <value>玩家当前选择的主职业</value>
        public Battle.BattleClass MainClass { get; private set; }

        /// <summary>
        /// 副职业
        /// </summary>
        /// <value>玩家当前选择的副职业</value>
        public Battle.BattleClass SubClass { get; private set; }

        /// <summary>
        /// 已装备的被动技能列表
        /// </summary>
        /// <value>玩家当前装备的被动技能集合</value>
        public List<Battle.PassiveSkill> EquippedPassives { get; } = [];

        /// <summary>
        /// 最大被动技能槽位数量
        /// </summary>
        public const int MaxPassiveSlots = 4;

        /// <summary>
        /// 当前魔法值（带范围限制）
        /// </summary>
        /// <value>自动将魔法值限制在 0 到 MaxMana 范围内</value>
        public float CurrentMana
        {
            get => Mana;
            set => Mana = Mathf.Clamp(value, 0, MaxMana);
        }

        /// <summary>
        /// 设置主职业
        /// </summary>
        /// <param name="mainClass">要设置的主职业</param>
        /// <remarks>
        /// 设置玩家的主职业并重新计算属性
        /// </remarks>
        public void SetMainClass(Battle.BattleClass mainClass)
        {
            MainClass = mainClass;
            CalculateStats();
        }

        /// <summary>
        /// 设置副职业
        /// </summary>
        /// <param name="subClass">要设置的副职业</param>
        /// <remarks>
        /// 设置玩家的副职业并重新计算属性
        /// </remarks>
        public void SetSubClass(Battle.BattleClass subClass)
        {
            SubClass = subClass;
            CalculateStats();
        }

        /// <summary>
        /// 装备被动技能
        /// </summary>
        /// <param name="passive">要装备的被动技能</param>
        /// <returns>装备成功返回true，否则返回false</returns>
        /// <remarks>
        /// 被动技能槽位有限，最多装备4个被动技能
        /// </remarks>
        public bool EquipPassive(Battle.PassiveSkill passive)
        {
            if (EquippedPassives.Count >= MaxPassiveSlots) return false;
            if (EquippedPassives.Contains(passive)) return false;

            EquippedPassives.Add(passive);
            CalculateStats();
            return true;
        }

        /// <summary>
        /// 卸下被动技能
        /// </summary>
        /// <param name="passive">要卸下的被动技能</param>
        /// <remarks>
        /// 从已装备的被动技能列表中移除指定技能并重新计算属性
        /// </remarks>
        public void UnequipPassive(Battle.PassiveSkill passive)
        {
            if (EquippedPassives.Remove(passive))
            {
                CalculateStats();
            }
        }

        /// <summary>
        /// 计算玩家属性
        /// </summary>
        /// <remarks>
        /// 根据等级、职业、装备和被动技能计算玩家的最终属性
        /// 属性计算公式：基础值 × 职业倍率 + 装备加成 + 被动加成 × 被动倍率
        /// </remarks>
        private void CalculateStats()
        {
            float baseHealth = 100f + (Level - 1) * 10f;
            float baseAttack = 15f + (Level - 1) * 2f;
            float baseDefense = 8f + (Level - 1) * 1f;
            float baseSpeed = 75f;

            float classHealthMult = 1f;
            float classAttackMult = 1f;
            float classDefenseMult = 1f;
            float classSpeedMult = 1f;

            if (MainClass != null)
            {
                classHealthMult *= MainClass.HealthGrowth;
                classAttackMult *= MainClass.AttackGrowth;
                classDefenseMult *= MainClass.DefenseGrowth;
                classSpeedMult *= MainClass.SpeedGrowth;
            }

            float passiveHealthAdd = 0f;
            float passiveAttackAdd = 0f;
            float passiveDefenseAdd = 0f;
            float passiveSpeedAdd = 0f;

            float passiveHealthMult = 1f;
            float passiveAttackMult = 1f;
            float passiveDefenseMult = 1f;

            foreach (var p in EquippedPassives)
            {
                passiveHealthAdd += p.HealthBonus;
                passiveAttackAdd += p.AttackBonus;
                passiveDefenseAdd += p.DefenseBonus;
                passiveSpeedAdd += p.SpeedBonus;

                passiveHealthMult *= p.HealthMultiplier;
                passiveAttackMult *= p.AttackMultiplier;
                passiveDefenseMult *= p.DefenseMultiplier;
            }

            float equipmentAttack = CurrentWeapon?.AttackPower ?? 0f;
            float equipmentDefense = 0f;
            float equipmentHealth = 0f;

            foreach (var eq in Equipments)
            {
                equipmentDefense += eq.Defense;
                equipmentHealth += eq.Health;
            }

            MaxHealth = (baseHealth * classHealthMult + equipmentHealth + passiveHealthAdd) * passiveHealthMult;
            Attack = (baseAttack * classAttackMult + equipmentAttack + passiveAttackAdd) * passiveAttackMult;
            Defense = (baseDefense * classDefenseMult + equipmentDefense + passiveDefenseAdd) * passiveDefenseMult;
            Speed = (baseSpeed * classSpeedMult + passiveSpeedAdd);

            if (Health > MaxHealth) Health = MaxHealth;
        }

        /// <summary>
        /// 初始化玩家
        /// </summary>
        /// <remarks>
        /// 调用父类Initialize方法，设置玩家默认名称、等级、魔法值
        /// 初始化武器、装备和技能，并计算属性
        /// </remarks>
        public override void Initialize()
        {
            base.Initialize();

            CreatureName = TranslationServer.Translate("player_default_name");
            Level = 1;
            Mana = 50f;
            MaxMana = 50f;

            InitializeWeapons();
            InitializeEquipments();
            InitializeSkills();

            CalculateStats();
            Health = MaxHealth;
        }

        /// <summary>
        /// 初始化武器
        /// </summary>
        /// <remarks>
        /// 创建默认武器（长剑）并添加到武器列表，设置为当前装备
        /// </remarks>
        private void InitializeWeapons()
        {
            Weapon defaultWeapon = new()
            {
                WeaponName = TranslationServer.Translate("weapon_default_sword_name"),
                WeaponTypeValue = Weapon.WeaponType.Sword,
                AttackPower = 5f
            };

            Weapons.Add(defaultWeapon);
            CurrentWeapon = defaultWeapon;
        }

        /// <summary>
        /// 初始化装备
        /// </summary>
        /// <remarks>
        /// 创建默认护甲并添加到装备列表
        /// </remarks>
        private void InitializeEquipments()
        {
            Equipment defaultArmor = new()
            {
                EquipmentName = TranslationServer.Translate("equipment_default_armor_name"),
                EquipmentTypeValue = Equipment.EquipmentType.Armor,
                Defense = 3f,
                Health = 20f
            };

            Equipments.Add(defaultArmor);
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        /// <remarks>
        /// 初始化技能管理器并获取玩家初始技能列表
        /// </remarks>
        private void InitializeSkills()
        {
            SkillManager.Initialize();

            var initialSkills = SkillManager.CreatePlayerInitialSkills();
            foreach (var skill in initialSkills)
            {
                Skills.Add(skill);
            }

            Log.Info($"Initialized player with {Skills.Count} skills");
        }

        public void EquipWeapon(Weapon weapon)
        {
            try
            {
                if (weapon == null)
                {
                    Log.Error("Cannot equip null weapon");
                    return;
                }

                if (!Weapons.Contains(weapon))
                {
                    Weapons.Add(weapon);
                }

                CurrentWeapon = weapon;
                CalculateStats();
                Log.Info(string.Format(TranslationServer.Translate("log_equipped_weapon"), weapon.WeaponName));
            }
            catch (Exception ex)
            {
                Log.Error($"Error equipping weapon: {ex.Message}");
            }
        }

        public void EquipEquipment(Equipment equipment)
        {
            try
            {
                if (equipment == null)
                {
                    Log.Error("Cannot equip null equipment");
                    return;
                }

                if (!Equipments.Contains(equipment))
                {
                    Equipments.Add(equipment);
                }

                CalculateStats();
                Log.Info(string.Format(TranslationServer.Translate("log_equipped_equipment"), equipment.EquipmentName));
            }
            catch (Exception ex)
            {
                Log.Error($"Error equipping equipment: {ex.Message}");
            }
        }

        public bool UseSkill(int skillIndex, Creature target)
        {
            try
            {
                if (!IsAlive)
                {
                    Log.Error("Cannot use skill: Player is not alive");
                    return false;
                }

                if (skillIndex < 0 || skillIndex >= Skills.Count)
                {
                    Log.Error($"Invalid skill index: {skillIndex}. Valid range: 0-{Skills.Count - 1}");
                    return false;
                }

                if (target == null)
                {
                    Log.Error("Cannot use skill on null target");
                    return false;
                }

                if (!target.IsAlive)
                {
                    Log.Warning($"Cannot use skill on dead target: {target.CreatureName}");
                    return false;
                }

                Skill skill = Skills[skillIndex];
                if (skill == null)
                {
                    Log.Error($"Skill at index {skillIndex} is null");
                    return false;
                }

                if (CurrentMana < skill.ManaCost)
                {
                    Log.Warning($"Not enough mana to use {skill.SkillName}. Need: {skill.ManaCost}, Have: {CurrentMana}");
                    return false;
                }

                CurrentMana -= skill.ManaCost;

                List<int> damageResults = [];
                List<string> effectDescriptions = [];

                if (skill.SkillDefs == null || skill.SkillDefs.Count == 0)
                {
                    Log.Warning($"Skill {skill.SkillName} has no skill definitions");
                    return false;
                }

                foreach (var skillDefent in skill.SkillDefs)
                {
                    if (skillDefent == null)
                    {
                        Log.Warning("Null skill definition found in skill");
                        continue;
                    }

                    switch (skillDefent.Type)
                    {
                        case Skill.SkillType.Attack:
                            var attackDamage = target.TakeDamage(this, skill);
                            damageResults.AddRange(attackDamage);
                            effectDescriptions.Add(string.Format(TranslationServer.Translate("skill_effect_damage"), string.Join(", ", attackDamage)));
                            break;

                        case Skill.SkillType.Healing:
                            var healAmount = Heal(this, skill);
                            damageResults.AddRange(healAmount);
                            effectDescriptions.Add(string.Format(TranslationServer.Translate("skill_effect_heal"), string.Join(", ", healAmount)));
                            break;

                        case Skill.SkillType.Defense:
                            if (BuffManagerInstance.Instance != null)
                            {
                                string defenseBuffId = $"defense_boost_{GetInstanceId()}";
                                var defenseBuff = PlayerSkillHelper.CreateDefenseBuff(defenseBuffId, skillDefent.DamageCoefficient, skillDefent.Duration);
                                BuffManagerInstance.Instance.RegisterBuffTemplate(defenseBuff);
                                BuffManagerInstance.Instance.ApplyBuff(defenseBuffId, this, this);
                                effectDescriptions.Add(string.Format(TranslationServer.Translate("skill_effect_defense_boost"), skillDefent.DamageCoefficient * 100));
                            }
                            else
                            {
                                Log.Warning("BuffManagerInstance is null, cannot apply defense buff");
                            }
                            break;

                        case Skill.SkillType.Support:
                            if (BuffManagerInstance.Instance != null)
                            {
                                string supportBuffId = $"attack_boost_{GetInstanceId()}";
                                var supportBuff = PlayerSkillHelper.CreateAttackBuff(supportBuffId, skillDefent.DamageCoefficient, skillDefent.Duration);
                                BuffManagerInstance.Instance.RegisterBuffTemplate(supportBuff);
                                BuffManagerInstance.Instance.ApplyBuff(supportBuffId, this, this);
                                effectDescriptions.Add(string.Format(TranslationServer.Translate("skill_effect_attack_boost"), skillDefent.DamageCoefficient * 100));
                            }
                            else
                            {
                                Log.Warning("BuffManagerInstance is null, cannot apply support buff");
                            }
                            break;

                        default:
                            Log.Warning($"Unknown skill type: {skillDefent.Type}");
                            break;
                    }
                }

                string effectsText = effectDescriptions.Count > 0 ? string.Join(", ", effectDescriptions) : TranslationServer.Translate("skill_effect_none");
                Log.Info(string.Format(TranslationServer.Translate("log_used_skill"), skill.SkillName, target.CreatureName, effectsText));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Error using skill: {ex.Message}");
                return false;
            }
        }

        public void CollectGold(int amount)
        {
            try
            {
                if (amount < 0)
                {
                    Log.Warning($"Attempted to collect negative gold: {amount}");
                    return;
                }

                Gold += amount;
                Log.Info(string.Format(TranslationServer.Translate("log_collected_gold"), amount, Gold));
            }
            catch (Exception ex)
            {
                Log.Error($"Error collecting gold: {ex.Message}");
            }
        }

        public void KillEnemy(Creature enemy)
        {
            try
            {
                if (enemy == null)
                {
                    Log.Error("Cannot kill null enemy");
                    return;
                }

                if (!enemy.IsAlive)
                {
                    Log.Warning("Enemy is already dead");
                    return;
                }

                KillCount++;
                Experience += enemy.Level * 10;

                CheckLevelUp();

                Log.Info(string.Format(TranslationServer.Translate("log_killed_enemy"), enemy.CreatureName, KillCount));
            }
            catch (Exception ex)
            {
                Log.Error($"Error killing enemy: {ex.Message}");
            }
        }

        private void CheckLevelUp()
        {
            int requiredExp = Level * 100;
            if (Experience >= requiredExp)
            {
                Level++;
                Experience -= requiredExp;

                CalculateStats();
                Health = MaxHealth;

                Log.Info(string.Format(TranslationServer.Translate("log_level_up"), Level));
            }
        }

        protected override void Die()
        {
            base.Die();
            DeathCount++;
            Log.Info(string.Format(TranslationServer.Translate("log_player_died"), DeathCount));
        }

        public override string GetCreatureInfo()
        {
            return base.GetCreatureInfo() + string.Format(TranslationServer.Translate("player_info_suffix"), Gold, KillCount, DeathCount);
        }
    }
}