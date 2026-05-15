using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;
using System.Linq;
using Godot;
using hd2dtest.Scripts.Modules.SkillSystem;

namespace hd2dtest.Scripts.Modules
{
    public partial class Player : Creature
    {
        // 移动相关属性
        [Export] public float MoveSpeed = 5.0f;
        [Export] public float SprintMultiplier = 1.5f;
        public bool IsSprinting { get; private set; } = false;

        // 玩家装备
        public List<Weapon> Weapons { get; } = [];
        public List<Equipment> Equipments { get; } = [];
        public Weapon CurrentWeapon { get; private set; } = null;

        // 玩家技能
        public List<Skill> Skills { get; } = [];

        // 状态属性
        public bool IsMoving { get; set; } = false;
        public bool IsAttacking { get; set; } = false;
        public bool IsDefending { get; set; } = false;
        public Vector2 Direction { get; set; } = Vector2.Zero;

        // 玩家统计数据
        public int Gold { get; set; } = 0;
        public int KillCount { get; set; } = 0;
        public int DeathCount { get; set; } = 0;
        public Dictionary<string, int> Inventory { get; set; } = [];

        // 魔法值
        public float Mana { get; set; } = 50f;
        public float MaxMana { get; set; } = 50f;

        // 职业系统
        public Battle.BattleClass MainClass { get; private set; }
        public Battle.BattleClass SubClass { get; private set; }
        public List<Battle.PassiveSkill> EquippedPassives { get; } = [];
        public const int MaxPassiveSlots = 4;

        public float CurrentMana
        {
            get => Mana;
            set => Mana = Mathf.Clamp(value, 0, MaxMana);
        }

        public void SetMainClass(Battle.BattleClass mainClass)
        {
            MainClass = mainClass;
            CalculateStats();
        }

        public void SetSubClass(Battle.BattleClass subClass)
        {
            SubClass = subClass;
            CalculateStats();
        }

        public bool EquipPassive(Battle.PassiveSkill passive)
        {
            if (EquippedPassives.Count >= MaxPassiveSlots) return false;
            if (EquippedPassives.Contains(passive)) return false;

            EquippedPassives.Add(passive);
            CalculateStats();
            return true;
        }

        public void UnequipPassive(Battle.PassiveSkill passive)
        {
            if (EquippedPassives.Remove(passive))
            {
                CalculateStats();
            }
        }

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
            if (weapon == null) return;

            if (!Weapons.Contains(weapon))
            {
                Weapons.Add(weapon);
            }

            CurrentWeapon = weapon;
            CalculateStats();
            Log.Info(string.Format(TranslationServer.Translate("log_equipped_weapon"), weapon.WeaponName));
        }

        public void EquipEquipment(Equipment equipment)
        {
            if (equipment == null) return;

            if (!Equipments.Contains(equipment))
            {
                Equipments.Add(equipment);
            }

            CalculateStats();
            Log.Info(string.Format(TranslationServer.Translate("log_equipped_equipment"), equipment.EquipmentName));
        }

        public bool UseSkill(int skillIndex, Creature target)
        {
            if (skillIndex < 0 || skillIndex >= Skills.Count || !IsAlive || target == null || !target.IsAlive)
            {
                return false;
            }

            Skill skill = Skills[skillIndex];

            if (CurrentMana < skill.ManaCost)
            {
                Log.Warning($"Not enough mana to use {skill.SkillName}. Need: {skill.ManaCost}, Have: {CurrentMana}");
                return false;
            }

            CurrentMana -= skill.ManaCost;

            List<int> damageResults = [];
            List<string> effectDescriptions = [];

            foreach (var skillDefent in skill.SkillDefs)
            {
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

        public void CollectGold(int amount)
        {
            Gold += amount;
            Log.Info(string.Format(TranslationServer.Translate("log_collected_gold"), amount, Gold));
        }

        public void KillEnemy(Creature enemy)
        {
            KillCount++;
            Experience += enemy.Level * 10;

            CheckLevelUp();

            Log.Info(string.Format(TranslationServer.Translate("log_killed_enemy"), enemy.CreatureName, KillCount));
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