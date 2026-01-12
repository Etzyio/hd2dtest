using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// NPC类
    /// </summary>
    public partial class NPC : Creature
    {
        // NPC类型枚举
        public enum NPCType
        {
            Villager,
            Merchant,
            QuestGiver,
            Guard,
            Healer
        }

        // NPC状态枚举
        public enum NPCState
        {
            Idle,
            Patrolling,
            Talking,
            Following,
            Alert
        }

        // 交互类型枚举
        public enum InteractionType
        {
            Talk,
            Trade,
            Quest,
            Heal,
            Follow
        }

        // NPC属性
        /// <summary>
        /// NPC类型
        /// </summary>
        [Export]
        public NPCType Type { get; set; } = NPCType.Villager;

        /// <summary>
        /// NPC对话内容
        /// </summary>
        [Export]
        public string Dialogue { get; set; } = "Hello!";

        /// <summary>
        /// 是否可以交互
        /// </summary>
        [Export]
        public bool IsInteractive { get; set; } = true;

        /// <summary>
        /// 可用的交互类型列表
        /// </summary>
        public List<InteractionType> AvailableInteractions { get; set; } = [];

        /// <summary>
        /// 是否可以对话
        /// </summary>
        public bool CanTalk => AvailableInteractions.Contains(InteractionType.Talk);

        /// <summary>
        /// 是否可以交易
        /// </summary>
        public bool CanTrade => AvailableInteractions.Contains(InteractionType.Trade);

        /// <summary>
        /// 是否可以接受任务
        /// </summary>
        public bool CanQuest => AvailableInteractions.Contains(InteractionType.Quest);

        /// <summary>
        /// 是否可以治疗
        /// </summary>
        public bool CanHeal => AvailableInteractions.Contains(InteractionType.Heal);

        /// <summary>
        /// 是否可以跟随
        /// </summary>
        public bool CanFollow => AvailableInteractions.Contains(InteractionType.Follow);

        // AI属性
        /// <summary>
        /// 当前AI状态
        /// </summary>
        [Export]
        public NPCState CurrentState { get; set; } = NPCState.Idle;

        /// <summary>
        /// 检测半径
        /// </summary>
        [Export]
        public float DetectionRadius { get; set; } = 50f;

        /// <summary>
        /// 巡逻速度
        /// </summary>
        [Export]
        public float PatrolSpeed { get; set; } = 25f;

        /// <summary>
        /// 是否正在移动
        /// </summary>
        public bool IsMoving { get; set; } = false;

        /// <summary>
        /// 移动方向
        /// </summary>
        public Vector2 Direction { get; set; } = Vector2.Zero;

        // 巡逻点
        /// <summary>
        /// 巡逻点列表
        /// </summary>
        public List<Vector2> PatrolPoints { get; set; } = [];

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
        /// 目标角色
        /// </summary>
        private Creature _target = null;

        /// <summary>
        /// 初始化NPC
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // 根据NPC类型设置默认属性和交互类型
            switch (Type)
            {
                case NPCType.Merchant:
                    CreatureName = "Merchant";
                    AvailableInteractions.AddRange([InteractionType.Talk, InteractionType.Trade]);
                    break;
                case NPCType.QuestGiver:
                    CreatureName = "Quest Giver";
                    AvailableInteractions.AddRange([InteractionType.Talk, InteractionType.Quest]);
                    break;
                case NPCType.Guard:
                    CreatureName = "Guard";
                    Attack = 15f;
                    Defense = 10f;
                    DetectionRadius = 80f;
                    AvailableInteractions.Add(InteractionType.Talk);
                    break;
                case NPCType.Healer:
                    CreatureName = "Healer";
                    AvailableInteractions.AddRange([InteractionType.Talk, InteractionType.Heal]);
                    break;
                default:
                    CreatureName = "Villager";
                    AvailableInteractions.Add(InteractionType.Talk);
                    break;
            }
        }

        /// <summary>
        /// AI更新逻辑
        /// </summary>
        /// <param name="delta">时间增量</param>
        public virtual void UpdateAI(float delta)
        {
            if (!IsAlive || CurrentState == NPCState.Talking)
            {
                return;
            }

            // 检测玩家
            Creature player = DetectPlayer();

            switch (CurrentState)
            {
                case NPCState.Idle:
                    HandleIdleState(delta);
                    break;
                case NPCState.Patrolling:
                    HandlePatrollingState(delta);
                    break;
                case NPCState.Following:
                    HandleFollowingState(delta, player);
                    break;
                case NPCState.Alert:
                    HandleAlertState(player);
                    break;
            }
        }

        /// <summary>
        /// 检测玩家
        /// </summary>
        /// <returns>检测到的玩家</returns>
        private static Creature DetectPlayer()
        {
            // 这里简化处理，实际应该通过场景树查找玩家
            return null;
        }

        /// <summary>
        /// 处理空闲状态
        /// </summary>
        /// <param name="delta">时间增量</param>
        private void HandleIdleState(float delta)
        {
            _idleTimer += delta;

            // 空闲一段时间后开始巡逻
            if (_idleTimer >= _maxIdleTime && PatrolPoints.Count > 0)
            {
                CurrentState = NPCState.Patrolling;
                _idleTimer = 0f;
            }
        }

        /// <summary>
        /// 处理巡逻状态
        /// </summary>
        /// <param name="delta">时间增量</param>
        private void HandlePatrollingState(float delta)
        {
            if (PatrolPoints.Count == 0)
            {
                CurrentState = NPCState.Idle;
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
                CurrentState = NPCState.Idle;
                IsMoving = false;
            }
            else
            {
                // 移动
                Speed = PatrolSpeed;
                IsMoving = true;
                Direction = direction;
                Position += direction * Speed * delta;
            }
        }

        /// <summary>
        /// 处理跟随状态
        /// </summary>
        /// <param name="delta">时间增量</param>
        /// <param name="target">跟随目标</param>
        private void HandleFollowingState(float delta, Creature target)
        {
            if (target == null)
            {
                CurrentState = NPCState.Idle;
                IsMoving = false;
                return;
            }

            // 保持一定距离跟随
            float followDistance = 30f;
            Vector2 direction = (target.Position - Position).Normalized();

            if (Position.DistanceTo(target.Position) > followDistance)
            {
                Speed = PatrolSpeed;
                IsMoving = true;
                Direction = direction;
                Position += direction * Speed * delta;
            }
            else
            {
                IsMoving = false;
            }
        }

        /// <summary>
        /// 处理警戒状态
        /// </summary>
        /// <param name="target">警戒目标</param>
        private void HandleAlertState(Creature target)
        {
            if (target == null)
            {
                CurrentState = NPCState.Idle;
                return;
            }

            // 检查目标是否在检测范围内
            if (Position.DistanceTo(target.Position) > DetectionRadius)
            {
                CurrentState = NPCState.Idle;
                return;
            }

            // 朝向目标
            Direction = (target.Position - Position).Normalized();
        }

        /// <summary>
        /// 开始对话
        /// </summary>
        /// <param name="player">对话对象</param>
        public virtual void StartDialogue(Creature player)
        {
            if (!CanTalk || !IsAlive || player == null)
            {
                return;
            }

            CurrentState = NPCState.Talking;
            _target = player;
            IsMoving = false;

            // 朝向玩家
            Direction = (player.Position - Position).Normalized();

            Log.Info($"{CreatureName}: {Dialogue}");
        }

        /// <summary>
        /// 结束对话
        /// </summary>
        public virtual void EndDialogue()
        {
            CurrentState = NPCState.Idle;
            _target = null;
        }

        /// <summary>
        /// 与NPC进行交互
        /// </summary>
        /// <param name="player">交互的玩家</param>
        /// <param name="interactionType">交互类型</param>
        public void Interact(Creature player, InteractionType interactionType)
        {
            if (!IsInteractive || !IsAlive || player == null || !player.IsAlive)
            {
                return;
            }

            // 检查交互类型是否可用
            if (!AvailableInteractions.Contains(interactionType))
            {
                Log.Info($"{CreatureName}: I can't do that.");
                return;
            }

            // 根据交互类型执行不同的逻辑
            switch (interactionType)
            {
                case InteractionType.Talk:
                    StartDialogue(player);
                    break;
                case InteractionType.Trade:
                    StartTrade(player);
                    break;
                case InteractionType.Quest:
                    OfferQuest(player);
                    break;
                case InteractionType.Heal:
                    HealPlayer(player);
                    break;
                case InteractionType.Follow:
                    ToggleFollow(player);
                    break;
            }
        }

        /// <summary>
        /// 开始交易
        /// </summary>
        /// <param name="player">交易的玩家</param>
        private void StartTrade(Creature player)
        {
            Log.Info($"{CreatureName}: Let's trade!");
            // 这里简化处理，实际应该打开交易界面
        }

        /// <summary>
        /// 提供任务
        /// </summary>
        /// <param name="player">接收任务的玩家</param>
        private void OfferQuest(Creature player)
        {
            Log.Info($"{CreatureName}: I have a quest for you!");
            // 这里简化处理，实际应该打开任务界面
        }

        /// <summary>
        /// 治疗玩家
        /// </summary>
        /// <param name="player">需要治疗的玩家</param>
        private void HealPlayer(Creature player)
        {
            // 治疗玩家
            // float healAmount = player.MaxHealth * 0.5f; // 恢复50%生命值
            float healAmount = player.Heal(this, Skills[0]); // 使用NPC的第一个技能进行治疗

            Log.Info($"{CreatureName}: Feel better now?");
            Log.Info($"{player.CreatureName} recovered {healAmount:F0} HP!");
        }

        /// <summary>
        /// 切换跟随状态
        /// </summary>
        /// <param name="player">跟随的玩家</param>
        private void ToggleFollow(Creature player)
        {
            if (CurrentState == NPCState.Following)
            {
                Log.Info($"{CreatureName}: I'll stop following you now.");
                CurrentState = NPCState.Idle;
                _target = null;
            }
            else
            {
                Log.Info($"{CreatureName}: I'll follow you!");
                CurrentState = NPCState.Following;
                _target = player;
            }
        }

        /// <summary>
        /// 获取NPC信息
        /// </summary>
        /// <returns>NPC信息</returns>
        public override string GetCreatureInfo()
        {
            return $"{base.GetCreatureInfo()} - Type: {Type} - State: {CurrentState}";
        }
    }
}