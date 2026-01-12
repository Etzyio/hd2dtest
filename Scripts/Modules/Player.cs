using System;
using System.Collections.Generic;
using System.Numerics;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 玩家类（单例模式）
    /// </summary>
    public partial class Player : Creature
    {
        // 单例实例
        private static Player _instance;
        public static Player Instance => _instance;

        /// <summary>
        /// 私有构造函数，防止外部实例化
        /// </summary>
        private Player() { }
        
        // 初始化单例
        static Player()
        {
            _instance = new Player();
        }
        
        // 玩家装备
        /// <summary>
        /// 武器列表
        /// </summary>
        public List<Weapon> Weapons { get; } = [];

        /// <summary>
        /// 装备列表
        /// </summary>
        public List<Equipment> Equipments { get; } = [];

        /// <summary>
        /// 当前装备的武器
        /// </summary>
        public Weapon CurrentWeapon { get; private set; } = null;

        // 玩家技能
        /// <summary>
        /// 技能列表
        /// </summary>
        public List<Skill> Skills { get; } = [];

        // 状态属性
        /// <summary>
        /// 是否正在移动
        /// </summary>
        public bool IsMoving { get; set; } = false;
        
        /// <summary>
        /// 是否正在攻击
        /// </summary>
        public bool IsAttacking { get; set; } = false;
        
        /// <summary>
        /// 是否正在防御
        /// </summary>
        public bool IsDefending { get; set; } = false;
        
        /// <summary>
        /// 移动方向
        /// </summary>
        public Vector2 Direction { get; set; } = Vector2.Zero;
       
        
        // 玩家统计数据
        /// <summary>
        /// 金币数量
        /// </summary>
        public int Gold { get; set; } = 0;

        /// <summary>
        /// 击杀敌人数量
        /// </summary>
        public int KillCount { get; set; } = 0;

        /// <summary>
        /// 死亡次数
        /// </summary>
        public int DeathCount { get; set; } = 0;

        /// <summary>
        /// 初始化玩家
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // 设置默认属性
            CreatureName = "Player";
            Health = 100f;
            MaxHealth = 100f;
            Attack = 15f;
            Defense = 8f;
            Speed = 75f;
            Level = 1;

            // 初始化装备和技能
            InitializeWeapons();
            InitializeEquipments();
            InitializeSkills();
        }

        /// <summary>
        /// 初始化武器
        /// </summary>
        private void InitializeWeapons()
        {
            // 创建初始武器
            Weapon defaultWeapon = new()
            {
                WeaponName = "Default Sword",
                WeaponTypeValue = Weapon.WeaponType.Sword,
                AttackPower = 5f
            };

            Weapons.Add(defaultWeapon);
            CurrentWeapon = defaultWeapon;
        }

        /// <summary>
        /// 初始化装备
        /// </summary>
        private void InitializeEquipments()
        {
            // 创建初始装备
            Equipment defaultArmor = new()
            {
                EquipmentName = "Default Armor",
                EquipmentTypeValue = Equipment.EquipmentType.Armor,
                Defense = 3f,
                Health = 20f
            };

            Equipments.Add(defaultArmor);
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        private void InitializeSkills()
        {
            // 创建初始技能
            Skill defaultSkill = new()
            {
                SkillName = "Basic Attack",
                SkillTypeValue = Skill.SkillType.Attack,
                Damage = 10f,
                Cooldown = 0f,
                ManaCost = 0
            };

            Skills.Add(defaultSkill);
        }

        /// <summary>
        /// 装备武器
        /// </summary>
        /// <param name="weapon">要装备的武器</param>
        public void EquipWeapon(Weapon weapon)
        {
            if (weapon == null)
            {
                return;
            }

            // 如果武器不在武器列表中，添加到列表
            if (!Weapons.Contains(weapon))
            {
                Weapons.Add(weapon);
            }

            // 装备武器
            CurrentWeapon = weapon;

            // 更新攻击属性
            Attack = weapon.AttackPower;

            Log.Info($"Equipped weapon: {weapon.WeaponName}");
        }

        /// <summary>
        /// 装备装备
        /// </summary>
        /// <param name="equipment">要装备的装备</param>
        public void EquipEquipment(Equipment equipment)
        {
            if (equipment == null)
            {
                return;
            }

            // 如果装备不在装备列表中，添加到列表
            if (!Equipments.Contains(equipment))
            {
                Equipments.Add(equipment);
            }

            // 更新属性
            Defense += equipment.Defense;
            MaxHealth += equipment.Health;
            Health += equipment.Health;

            Log.Info($"Equipped equipment: {equipment.EquipmentName}");
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="skillIndex">技能索引</param>
        /// <param name="target">技能目标</param>
        /// <returns>是否成功使用技能</returns>
        public bool UseSkill(int skillIndex, Creature target)
        {
            if (skillIndex < 0 || skillIndex >= Skills.Count || !IsAlive || target == null || !target.IsAlive)
            {
                return false;
            }

            Skill skill = Skills[skillIndex];

            // 使用技能 - 直接调用目标的TakeDamage或Heal方法
            float damage = 0f;
            if (skill.SkillTypeValue == Skill.SkillType.Attack)
            {
                damage = target.TakeDamage(this, skill);
            }
            else if (skill.SkillTypeValue == Skill.SkillType.Healing)
            {
                damage = this.Heal(this, skill);
            }

            Log.Info($"Used skill: {skill.SkillName} on {target.CreatureName}, dealing {damage:F1} damage.");

            return true;
        }

        /// <summary>
        /// 收集金币
        /// </summary>
        /// <param name="amount">金币数量</param>
        public void CollectGold(int amount)
        {
            Gold += amount;
            Log.Info($"Collected {amount} gold. Total: {Gold}");
        }

        /// <summary>
        /// 杀死敌人
        /// </summary>
        /// <param name="enemy">被杀死的敌人</param>
        public void KillEnemy(Creature enemy)
        {
            KillCount++;
            Experience += enemy.Level * 10;

            // 检查是否升级
            CheckLevelUp();

            Log.Info($"Killed {enemy.CreatureName}. Kill count: {KillCount}");
        }

        /// <summary>
        /// 检查是否升级
        /// </summary>
        private void CheckLevelUp()
        {
            // 简单的升级逻辑
            int requiredExp = Level * 100;
            if (Experience >= requiredExp)
            {
                Level++;
                Experience -= requiredExp;

                // 升级属性
                MaxHealth += 10f;
                Health = MaxHealth;
                Attack += 2f;
                Defense += 1f;

                Log.Info($"Level up! Now level {Level}");
            }
        }

        /// <summary>
        /// 玩家死亡（重写父类方法）
        /// </summary>
        protected override void Die()
        {
            base.Die();

            DeathCount++;

            Log.Info($"Player died. Death count: {DeathCount}");
        }



        /// <summary>
        /// 获取玩家信息（重写父类方法）
        /// </summary>
        /// <returns>玩家信息</returns>
        public override string GetCreatureInfo()
        {
            return $"{base.GetCreatureInfo()} - Gold: {Gold} - Kills: {KillCount} - Deaths: {DeathCount}";
        }
    }
}