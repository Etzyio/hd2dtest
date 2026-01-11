using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 玩家类（单例模式）
    /// </summary>
    public partial class Player : Character
    {
        // 单例实例
        /// <summary>
        /// 单例实例
        /// </summary>
        private static Player _instance;

        /// <summary>
        /// 获取玩家实例
        /// </summary>
        public static Player Instance => _instance;

        /// <summary>
        /// 私有构造函数，防止外部实例化
        /// </summary>
        private Player() { }

        // 玩家特定属性
        /// <summary>
        /// 跳跃力
        /// </summary>
        [Export]
        public float JumpForce { get; set; } = 300f;

        /// <summary>
        /// 最大跳跃高度
        /// </summary>
        [Export]
        public float MaxJumpHeight { get; set; } = 100f;

        /// <summary>
        /// 跳跃到最高点的时间
        /// </summary>
        [Export]
        public float JumpTimeToPeak { get; set; } = 0.4f;

        /// <summary>
        /// 从最高点下落的时间
        /// </summary>
        [Export]
        public float JumpTimeToFall { get; set; } = 0.3f;

        // 玩家状态
        /// <summary>
        /// 是否正在跳跃
        /// </summary>
        public bool IsJumping { get; private set; } = false;

        /// <summary>
        /// 是否在地面上
        /// </summary>
        public bool IsOnGround { get; set; } = false;

        /// <summary>
        /// 跳跃速度
        /// </summary>
        public float JumpVelocity { get; private set; } = 0f;

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
        /// 获取或创建玩家实例（单例模式）
        /// </summary>
        /// <returns>玩家实例</returns>
        public static Player GetInstance()
        {
            _instance ??= new Player();
            return _instance;
        }

        /// <summary>
        /// 初始化玩家
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // 设置单例实例
            _instance = this;

            // 设置默认属性
            CharacterName = "Player";
            Health = 100f;
            MaxHealth = 100f;
            Attack = 15f;
            Defense = 8f;
            Speed = 75f;
            Level = 1;

            // 初始化跳跃参数
            JumpVelocity = -2f * MaxJumpHeight / JumpTimeToPeak;

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
                AttackPower = 5f,
                AttackSpeed = 1f,
                Range = 30f
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
        /// 玩家跳跃
        /// </summary>
        public void Jump()
        {
            if (!IsAlive || !IsOnGround || IsJumping)
            {
                return;
            }

            IsJumping = true;
            IsOnGround = false;

            // 应用跳跃力
            JumpVelocity = JumpForce;

            // 播放跳跃动画
            AnimatedSprite?.Play("jump");
        }

        /// <summary>
        /// 更新跳跃逻辑
        /// </summary>
        /// <param name="delta">时间增量</param>
        public void UpdateJump(float delta)
        {
            if (IsJumping)
            {
                // 应用重力
                JumpVelocity += 9.8f * 100f * delta;

                // 更新位置
                Position += new Vector2(0, JumpVelocity * delta);

                // 检查是否到达地面
                if (IsOnGround)
                {
                    IsJumping = false;
                    JumpVelocity = 0f;

                    // 播放着陆动画
                    AnimatedSprite?.Play("idle");
                }
            }
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
        public bool UseSkill(int skillIndex, Character target)
        {
            if (skillIndex < 0 || skillIndex >= Skills.Count || !IsAlive || target == null || !target.IsAlive)
            {
                return false;
            }

            Skill skill = Skills[skillIndex];

            // 检查技能是否可用
            if (!skill.IsAvailable)
            {
                return false;
            }

            // 使用技能
            skill.Use(this, target);

            Log.Info($"Used skill: {skill.SkillName} on {target.CharacterName}");

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
        public void KillEnemy(Character enemy)
        {
            KillCount++;
            Experience += enemy.Level * 10;

            // 检查是否升级
            CheckLevelUp();

            Log.Info($"Killed {enemy.CharacterName}. Kill count: {KillCount}");
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
            IsJumping = false;
            JumpVelocity = 0f;

            Log.Info($"Player died. Death count: {DeathCount}");
        }

        /// <summary>
        /// 更新动画状态（重写父类方法）
        /// </summary>
        protected override void UpdateAnimation()
        {
            if (AnimatedSprite == null)
            {
                return;
            }

            if (IsJumping)
            {
                AnimatedSprite.Play("jump");
            }
            else if (IsMoving)
            {
                AnimatedSprite.Play("move");

                // 设置动画方向
                if (Mathf.Abs(Direction.X) > Mathf.Abs(Direction.Y))
                {
                    AnimatedSprite.FlipH = Direction.X < 0;
                }
            }
            else
            {
                AnimatedSprite.Play("idle");
            }
        }

        /// <summary>
        /// 获取玩家信息（重写父类方法）
        /// </summary>
        /// <returns>玩家信息</returns>
        public override string GetCharacterInfo()
        {
            return $"{base.GetCharacterInfo()} - Gold: {Gold} - Kills: {KillCount} - Deaths: {DeathCount}";
        }
    }
}