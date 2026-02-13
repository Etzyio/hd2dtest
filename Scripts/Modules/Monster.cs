using System;
using System.Collections.Generic;
using System.Numerics;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 怪物类，继承自Creature类，定义游戏中怪物的属性和行为
    /// </summary>
    /// <remarks>
    /// 怪物类包含怪物的基本属性、AI状态、巡逻行为、攻击逻辑和掉落系统
    /// 支持多种怪物类型（普通、精英、BOSS）和AI状态（空闲、巡逻、追逐、攻击、逃跑）
    /// </remarks>
    public partial class Monster : Creature
    {
        /// <summary>
        /// 怪物类型枚举
        /// </summary>
        public enum MonsterType
        {
            /// <summary>普通怪物</summary>
            Normal,
            /// <summary>精英怪物</summary>
            Elite,
            /// <summary>BOSS怪物</summary>
            Boss
        }

        /// <summary>
        /// 怪物AI状态枚举
        /// </summary>
        public enum MonsterState
        {
            /// <summary>空闲状态</summary>
            Idle,
            /// <summary>巡逻状态</summary>
            Patrol,
            /// <summary>追逐状态</summary>
            Chase,
            /// <summary>攻击状态</summary>
            Attack,
            /// <summary>逃跑状态</summary>
            Flee
        }

        // 怪物属性
        /// <summary>
        /// 怪物类型
        /// </summary>
        /// <value>怪物的类型枚举值，默认为Normal</value>
        public MonsterType Type { get; set; } = MonsterType.Normal;

        /// <summary>
        /// 仇恨范围
        /// </summary>
        /// <value>怪物发现玩家的范围，默认为100f</value>
        public float AggroRange { get; set; } = 100f;

        /// <summary>
        /// 攻击范围
        /// </summary>
        /// <value>怪物可以攻击玩家的范围，默认为40f</value>
        public float AttackRange { get; set; } = 40f;

        /// <summary>
        /// 追逐速度
        /// </summary>
        /// <value>怪物追逐玩家时的移动速度，默认为60f</value>
        public float ChaseSpeed { get; set; } = 60f;

        /// <summary>
        /// 巡逻速度
        /// </summary>
        /// <value>怪物巡逻时的移动速度，默认为30f</value>
        public float PatrolSpeed { get; set; } = 30f;

        // AI属性
        /// <summary>
        /// 当前AI状态
        /// </summary>
        /// <value>怪物当前的AI状态枚举值，默认为Idle</value>
        public MonsterState CurrentState { get; set; } = MonsterState.Idle;

        /// <summary>
        /// 是否可以追逐
        /// </summary>
        /// <value>true 表示怪物可以追逐玩家，false 表示怪物不能追逐玩家</value>
        public bool CanChase { get; set; } = true;

        /// <summary>
        /// 是否可以逃跑
        /// </summary>
        /// <value>true 表示怪物可以逃跑，false 表示怪物不能逃跑</value>
        public bool CanFlee { get; set; } = false;

        // 巡逻点
        /// <summary>
        /// 巡逻点列表
        /// </summary>
        /// <value>怪物的巡逻点坐标集合</value>
        public List<Vector2> PatrolPoints { get; set; } = [];

        // 掉落物品
        /// <summary>
        /// 掉落物品列表
        /// </summary>
        /// <value>怪物死亡时可能掉落的物品ID集合</value>
        public List<string> DropItems { get; set; } = [];

        /// <summary>
        /// 当前巡逻点索引
        /// </summary>
        private int _currentPatrolIndex = 0;

        /// <summary>
        /// 空闲计时器
        /// </summary>
        private float _idleTimer = 0f;

        /// <summary>
        /// 最大空闲时间
        /// </summary>
        private float _maxIdleTime = 2f;

        /// <summary>
        /// 攻击冷却计时器
        /// </summary>
        private float _attackCooldown = 0f;

        /// <summary>
        /// 最大攻击冷却时间
        /// </summary>
        private float _maxAttackCooldown = 1f;

        /// <summary>
        /// 目标角色
        /// </summary>
        private Creature _target = null;

        /// <summary>
        /// 初始化怪物
        /// </summary>
        /// <remarks>
        /// 重置怪物状态，并根据怪物类型设置默认属性
        /// 精英和BOSS怪物具有更高的属性值
        /// </remarks>
        public override void Initialize()
        {
            base.Initialize();

            // 根据怪物类型设置默认属性
            switch (Type)
            {
                case MonsterType.Elite:
                    CreatureName = "Elite Monster";
                    Health = 150f;
                    MaxHealth = 150f;
                    Attack = 20f;
                    Defense = 10f;
                    AggroRange = 120f;
                    AttackRange = 50f;
                    break;
                case MonsterType.Boss:
                    CreatureName = "Boss Monster";
                    Health = 500f;
                    MaxHealth = 500f;
                    Attack = 35f;
                    Defense = 20f;
                    AggroRange = 150f;
                    AttackRange = 60f;
                    _maxAttackCooldown = 0.8f;
                    break;
                default:
                    CreatureName = "Monster";
                    Health = 80f;
                    MaxHealth = 80f;
                    Attack = 12f;
                    Defense = 5f;
                    break;
            }
        }

        /// <summary>
        /// AI更新逻辑
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        /// <remarks>
        /// 更新怪物的AI状态，处理状态转换和行为逻辑
        /// 包括攻击冷却更新、玩家检测和状态处理
        /// </remarks>
        public virtual void UpdateAI(float delta)
        {
            if (!IsAlive)
            {
                return;
            }

            // 更新攻击冷却
            if (_attackCooldown > 0)
            {
                _attackCooldown -= delta;
            }

            // 检测玩家
            Creature player = DetectPlayer();

            switch (CurrentState)
            {
                case MonsterState.Idle:
                    HandleIdleState(delta, player);
                    break;
                case MonsterState.Patrol:
                    HandlePatrolState(delta, player);
                    break;
                case MonsterState.Chase:
                    HandleChaseState(delta, player);
                    break;
                case MonsterState.Attack:
                    HandleAttackState(delta, player);
                    break;
                case MonsterState.Flee:
                    HandleFleeState(delta, player);
                    break;
            }
        }

        /// <summary>
        /// 检测玩家
        /// </summary>
        /// <returns>检测到的玩家对象</returns>
        /// <remarks>
        /// 简化处理，实际应该通过场景树查找玩家
        /// </remarks>
        private Creature DetectPlayer()
        {
            // 这里简化处理，实际应该通过场景树查找玩家
            return null;
        }

        /// <summary>
        /// 处理空闲状态
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        /// <param name="player">检测到的玩家</param>
        /// <remarks>
        /// 检查玩家是否进入仇恨范围，空闲一段时间后开始巡逻
        /// </remarks>
        private void HandleIdleState(float delta, Creature player)
        {
            // 检查玩家是否在攻击范围内
            if (player != null && Position.DistanceTo(player.Position) <= AggroRange)
            {
                CurrentState = MonsterState.Chase;
                _target = player;
                return;
            }

            _idleTimer += delta;

            // 空闲一段时间后开始巡逻
            if (_idleTimer >= _maxIdleTime && PatrolPoints.Count > 0)
            {
                CurrentState = MonsterState.Patrol;
                _idleTimer = 0f;
            }
        }

        /// <summary>
        /// 处理巡逻状态
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        /// <param name="player">检测到的玩家</param>
        /// <remarks>
        /// 检查玩家是否进入仇恨范围，移动到巡逻点，到达后切换到空闲状态
        /// </remarks>
        private void HandlePatrolState(float delta, Creature player)
        {
            // 检查玩家是否在攻击范围内
            if (player != null && Position.DistanceTo(player.Position) <= AggroRange)
            {
                CurrentState = MonsterState.Chase;
                _target = player;
                return;
            }

            if (PatrolPoints.Count == 0)
            {
                CurrentState = MonsterState.Idle;
                return;
            }

            // 移动到当前巡逻点
            Vector2 targetPos = PatrolPoints[_currentPatrolIndex];
            Vector2 direction = (targetPos - Position).Normalized();

            // 检查是否到达巡逻点
            if (Position.DistanceTo(targetPos) < 5f)
            {
                // 移动到下一个巡逻点
                _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Count;
                CurrentState = MonsterState.Idle;
            }
            else
            {
                // 直接移动，不使用Move方法
                Speed = PatrolSpeed;
                Position += direction * Speed * delta;
            }
        }

        /// <summary>
        /// 处理追逐状态
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        /// <param name="player">检测到的玩家</param>
        /// <remarks>
        /// 检查玩家是否超出追逐范围，是否进入攻击范围，否则追逐玩家
        /// </remarks>
        private void HandleChaseState(float delta, Creature player)
        {
            if (player == null)
            {
                CurrentState = MonsterState.Idle;
                _target = null;
                return;
            }

            // 检查玩家是否超出追逐范围
            if (Position.DistanceTo(player.Position) > AggroRange * 1.5f)
            {
                CurrentState = MonsterState.Idle;
                _target = null;
                return;
            }

            // 检查玩家是否在攻击范围内
            if (Position.DistanceTo(player.Position) <= AttackRange)
            {
                CurrentState = MonsterState.Attack;
                return;
            }

            // 追逐玩家，直接移动
            Speed = ChaseSpeed;
            Vector2 direction = (player.Position - Position).Normalized();
            Position += direction * Speed * delta;
        }

        /// <summary>
        /// 处理攻击状态
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        /// <param name="player">检测到的玩家</param>
        /// <remarks>
        /// 检查玩家是否超出攻击范围，否则在攻击冷却结束后攻击玩家
        /// </remarks>
        private void HandleAttackState(float delta, Creature player)
        {
            if (player == null)
            {
                CurrentState = MonsterState.Idle;
                _target = null;
                return;
            }

            // 检查玩家是否超出攻击范围
            if (Position.DistanceTo(player.Position) > AttackRange * 1.2f)
            {
                CurrentState = MonsterState.Chase;
                return;
            }

            // 检查是否可以攻击
            if (_attackCooldown <= 0f)
            {
                // 攻击玩家
                AttackTarget(player);
                _attackCooldown = _maxAttackCooldown;
            }
        }

        /// <summary>
        /// 处理逃跑状态
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        /// <param name="player">检测到的玩家</param>
        /// <remarks>
        /// 检查玩家是否超出范围，否则远离玩家
        /// </remarks>
        private void HandleFleeState(float delta, Creature player)
        {
            if (player == null)
            {
                CurrentState = MonsterState.Idle;
                _target = null;
                return;
            }

            // 检查玩家是否超出范围
            if (Position.DistanceTo(player.Position) > AggroRange * 2f)
            {
                CurrentState = MonsterState.Idle;
                _target = null;
                return;
            }

            // 远离玩家
            Speed = ChaseSpeed;
            Vector2 direction = (Position - player.Position).Normalized();
            Position += direction * Speed * delta;
        }

        /// <summary>
        /// 攻击目标
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否攻击成功</returns>
        /// <remarks>
        /// 使用默认技能攻击目标，设置攻击冷却
        /// 攻击条件：攻击者和目标都存活
        /// </remarks>
        public bool AttackTarget(Creature target)
        {
            if (!IsAlive || target == null || !target.IsAlive)
            {
                return false;
            }

            // 使用默认技能攻击目标
            // 这里简化处理，实际应该使用怪物的技能
            Skill defaultSkill = new Skill
            {
                SkillName = "Monster Attack",
                SkillTypeValue = Skill.SkillType.Attack,
                Damage = Attack,
                Cooldown = 1f
            };

            target.TakeDamage(this, defaultSkill);
            _attackCooldown = _maxAttackCooldown;
            return true;
        }

        /// <summary>
        /// 怪物死亡
        /// </summary>
        /// <remarks>
        /// 调用父类Die方法，掉落物品，通知玩家
        /// </remarks>
        protected new void Die()
        {
            base.Die();

            // 掉落物品
            DropLoot();

            // 通知玩家（如果玩家存在）
            if (_target != null && _target is Player player)
            {
                player.KillEnemy(this);
            }
        }

        /// <summary>
        /// 掉落物品
        /// </summary>
        /// <remarks>
        /// 随机选择一个物品掉落，掉落金币
        /// </remarks>
        private void DropLoot()
        {
            // 简单的掉落逻辑
            if (DropItems.Count > 0)
            {
                // 随机选择一个物品掉落
                int randomIndex = DamageCalculator.Randi(DropItems.Count);
                string droppedItem = DropItems[randomIndex];

                Log.Info($"{CreatureName} dropped {droppedItem}");
            }

            // 掉落金币
            int gold = DamageCalculator.Randi(Level * 20) + 5;
            if (gold > 0 && _target != null && _target is Player player)
            {
                player.CollectGold(gold);
            }
        }

        /// <summary>
        /// 获取怪物信息
        /// </summary>
        /// <returns>怪物信息字符串</returns>
        /// <remarks>
        /// 格式化输出怪物的基本属性、类型和当前状态
        /// </remarks>
        public override string GetCreatureInfo()
        {
            return $"{base.GetCreatureInfo()} - Type: {Type} - State: {CurrentState}";
        }
    }
}
