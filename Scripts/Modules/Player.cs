using System;
using System.Collections.Generic;
using System.Numerics;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;
using System.Linq;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 玩家类，继承自Creature类，定义游戏中玩家角色的属性和行为
    /// </summary>
    /// <remarks>
    /// 玩家类包含玩家的装备、技能、状态属性和统计数据
    /// 支持武器装备、装备穿戴、技能使用、金币收集、敌人击杀等功能
    /// </remarks>
    public partial class Player : Creature
    {
        // 玩家装备
        /// <summary>
        /// 武器列表
        /// </summary>
        /// <value>玩家拥有的武器集合</value>
        public List<Weapon> Weapons { get; } = [];

        /// <summary>
        /// 装备列表
        /// </summary>
        /// <value>玩家拥有的装备集合</value>
        public List<Equipment> Equipments { get; } = [];

        /// <summary>
        /// 当前装备的武器
        /// </summary>
        /// <value>玩家当前使用的武器</value>
        public Weapon CurrentWeapon { get; private set; } = null;

        // 玩家技能
        /// <summary>
        /// 技能列表
        /// </summary>
        /// <value>玩家拥有的技能集合</value>
        public List<Skill> Skills { get; } = [];

        // 状态属性
        /// <summary>
        /// 是否正在移动
        /// </summary>
        /// <value>true 表示玩家正在移动，false 表示玩家静止</value>
        public bool IsMoving { get; set; } = false;
        
        /// <summary>
        /// 是否正在攻击
        /// </summary>
        /// <value>true 表示玩家正在攻击，false 表示玩家未在攻击</value>
        public bool IsAttacking { get; set; } = false;
        
        /// <summary>
        /// 是否正在防御
        /// </summary>
        /// <value>true 表示玩家正在防御，false 表示玩家未在防御</value>
        public bool IsDefending { get; set; } = false;
        
        /// <summary>
        /// 移动方向
        /// </summary>
        /// <value>玩家的当前移动方向向量</value>
        public Vector2 Direction { get; set; } = Vector2.Zero;
       
        // 玩家统计数据
        /// <summary>
        /// 金币数量
        /// </summary>
        /// <value>玩家拥有的金币数量</value>
        public int Gold { get; set; } = 0;

        /// <summary>
        /// 击杀敌人数量
        /// </summary>
        /// <value>玩家击杀的敌人总数</value>
        public int KillCount { get; set; } = 0;

        /// <summary>
        /// 死亡次数
        /// </summary>
        /// <value>玩家的死亡次数</value>
        public int DeathCount { get; set; } = 0;

        /// <summary>
        /// 道具库存
        /// </summary>
        /// <value>玩家的道具库存，键为道具ID，值为数量</value>
        public Dictionary<string, int> Inventory { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// 魔法值
        /// </summary>
        /// <value>玩家的当前魔法值</value>
        public float Mana { get; set; } = 50f;

        /// <summary>
        /// 最大魔法值
        /// </summary>
        /// <value>玩家的最大魔法值</value>
        public float MaxMana { get; set; } = 50f;

        /// <summary>
        /// 初始化玩家
        /// </summary>
        /// <remarks>
        /// 重置玩家状态，设置默认属性，初始化武器、装备和技能
        /// </remarks>
        public override void Initialize()
        {
            base.Initialize();

            // 设置默认属性
            CreatureName = "Player";
            Health = 100f;
            MaxHealth = 100f;
            Mana = 50f;
            MaxMana = 50f;
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
        /// <remarks>
        /// 创建初始武器并添加到武器列表，设置为当前武器
        /// </remarks>
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
        /// <remarks>
        /// 创建初始装备并添加到装备列表
        /// </remarks>
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
        /// <remarks>
        /// 创建初始技能并添加到技能列表
        /// </remarks>
        private void InitializeSkills()
        {

            //TODO：增加skill
            // Skills.Add(defaultSkill);
        }

        /// <summary>
        /// 装备武器
        /// </summary>
        /// <param name="weapon">要装备的武器</param>
        /// <remarks>
        /// 将武器添加到武器列表（如果不存在），设置为当前武器，更新攻击属性
        /// </remarks>
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
        /// <remarks>
        /// 将装备添加到装备列表（如果不存在），更新玩家属性（防御、生命值）
        /// </remarks>
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
        /// <remarks>
        /// 使用指定索引的技能攻击或治疗目标
        /// 使用条件：技能索引有效、玩家存活、目标存活且不为空
        /// </remarks>
        public bool UseSkill(int skillIndex, Creature target)
        {
            if (skillIndex < 0 || skillIndex >= Skills.Count || !IsAlive || target == null || !target.IsAlive)
            {
                return false;
            }

            Skill skill = Skills[skillIndex];

            // TODO：区分攻击技能
            // 使用技能 - 直接调用目标的TakeDamage或Heal方法
            List<int> damage = [];
            foreach(var skillDefent in skill.SkillDefs){
                if (skillDefent.Type == Skill.SkillType.Attack)
                {
                    damage = target.TakeDamage(this, skill);
                }
                else if (skillDefent.Type == Skill.SkillType.Healing)
                {
                    damage = this.Heal(this, skill);
                }
            }
            

            Log.Info($"Used skill: {skill.SkillName} on {target.CreatureName}, dealing {damage:F1} damage.");

            return true;
        }

        /// <summary>
        /// 收集金币
        /// </summary>
        /// <param name="amount">金币数量</param>
        /// <remarks>
        /// 增加玩家的金币数量并记录日志
        /// </remarks>
        public void CollectGold(int amount)
        {
            Gold += amount;
            Log.Info($"Collected {amount} gold. Total: {Gold}");
        }

        /// <summary>
        /// 杀死敌人
        /// </summary>
        /// <param name="enemy">被杀死的敌人</param>
        /// <remarks>
        /// 增加击杀计数和经验值，检查是否升级
        /// </remarks>
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
        /// <remarks>
        /// 简单的升级逻辑，当经验值达到要求时升级，提升属性
        /// </remarks>
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
        /// <remarks>
        /// 调用父类Die方法，增加死亡计数并记录日志
        /// </remarks>
        protected override void Die()
        {
            base.Die();

            DeathCount++;

            Log.Info($"Player died. Death count: {DeathCount}");
        }

        /// <summary>
        /// 获取玩家信息（重写父类方法）
        /// </summary>
        /// <returns>玩家信息字符串</returns>
        /// <remarks>
        /// 格式化输出玩家的基本属性、金币、击杀数和死亡数
        /// </remarks>
        public override string GetCreatureInfo()
        {
            return $"{base.GetCreatureInfo()} - Gold: {Gold} - Kills: {KillCount} - Deaths: {DeathCount}";
        }
    }
}