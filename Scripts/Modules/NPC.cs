using System;
using System.Collections.Generic;
using System.Numerics;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Quest;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;
using System.Linq;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// NPC类，继承自Creature类，定义游戏中非玩家角色的属性和行为
    /// </summary>
    /// <remarks>
    /// NPC类包含NPC的基本属性、AI状态、交互类型和对话系统
    /// 支持多种NPC类型（村民、商人、任务给予者、守卫、治疗师）和交互方式（对话、交易、任务、治疗、跟随）
    /// </remarks>
    public partial class NPC : Creature
    {
        /// <summary>
        /// NPC类型枚举
        /// </summary>
        public enum NPCType
        {
            /// <summary>村民</summary>
            Villager,
            /// <summary>商人</summary>
            Merchant,
            /// <summary>任务给予者</summary>
            QuestGiver,
            /// <summary>守卫</summary>
            Guard,
            /// <summary>治疗师</summary>
            Healer
        }

        /// <summary>
        /// NPC状态枚举
        /// </summary>
        public enum NPCState
        {
            /// <summary>空闲状态</summary>
            Idle,
            /// <summary>巡逻状态</summary>
            Patrolling,
            /// <summary>对话状态</summary>
            Talking,
            /// <summary>跟随状态</summary>
            Following,
            /// <summary>警戒状态</summary>
            Alert
        }

        /// <summary>
        /// 交互类型枚举
        /// </summary>
        public enum InteractionType
        {
            /// <summary>对话</summary>
            Talk,
            /// <summary>交易</summary>
            Trade,
            /// <summary>任务</summary>
            Quest,
            /// <summary>治疗</summary>
            Heal,
            /// <summary>跟随</summary>
            Follow
        }

        // NPC属性
        /// <summary>
        /// NPC类型
        /// </summary>
        /// <value>NPC的类型枚举值，默认为Villager</value>
        public NPCType Type { get; set; } = NPCType.Villager;

        /// <summary>
        /// NPC ID
        /// </summary>
        /// <value>NPC的唯一标识符</value>
        public string NPCId { get; set; } = "";

        /// <summary>
        /// NPC对话内容
        /// </summary>
        /// <value>NPC的默认对话文本，默认为"Hello!"</value>
        public string Dialogue { get; set; } = "Hello!";

        /// <summary>
        /// 是否可以交互
        /// </summary>
        /// <value>true 表示NPC可以交互，false 表示NPC不能交互</value>
        public bool IsInteractive { get; set; } = true;

        /// <summary>
        /// 可用的交互类型列表
        /// </summary>
        /// <value>NPC支持的交互类型集合</value>
        public List<InteractionType> AvailableInteractions { get; set; } = [];

        /// <summary>
        /// 是否可以对话
        /// </summary>
        /// <value>true 表示NPC可以对话，false 表示NPC不能对话</value>
        public bool CanTalk => AvailableInteractions.Contains(InteractionType.Talk);

        /// <summary>
        /// 是否可以交易
        /// </summary>
        /// <value>true 表示NPC可以交易，false 表示NPC不能交易</value>
        public bool CanTrade => AvailableInteractions.Contains(InteractionType.Trade);

        /// <summary>
        /// 是否可以接受任务
        /// </summary>
        /// <value>true 表示NPC可以给予任务，false 表示NPC不能给予任务</value>
        public bool CanQuest => AvailableInteractions.Contains(InteractionType.Quest);

        /// <summary>
        /// 是否可以治疗
        /// </summary>
        /// <value>true 表示NPC可以治疗，false 表示NPC不能治疗</value>
        public bool CanHeal => AvailableInteractions.Contains(InteractionType.Heal);

        /// <summary>
        /// 是否可以跟随
        /// </summary>
        /// <value>true 表示NPC可以跟随，false 表示NPC不能跟随</value>
        public bool CanFollow => AvailableInteractions.Contains(InteractionType.Follow);

        // AI属性
        /// <summary>
        /// 当前AI状态
        /// </summary>
        /// <value>NPC当前的AI状态枚举值，默认为Idle</value>
        public NPCState CurrentState { get; set; } = NPCState.Idle;

        /// <summary>
        /// 检测半径
        /// </summary>
        /// <value>NPC检测玩家的范围，默认为50f</value>
        public float DetectionRadius { get; set; } = 50f;

        /// <summary>
        /// 巡逻速度
        /// </summary>
        /// <value>NPC巡逻时的移动速度，默认为25f</value>
        public float PatrolSpeed { get; set; } = 25f;

        /// <summary>
        /// 是否正在移动
        /// </summary>
        /// <value>true 表示NPC正在移动，false 表示NPC静止</value>
        public bool IsMoving { get; set; } = false;

        /// <summary>
        /// 移动方向
        /// </summary>
        /// <value>NPC的当前移动方向向量</value>
        public Vector2 Direction { get; set; } = Vector2.Zero;

        // 巡逻点
        /// <summary>
        /// 巡逻点列表
        /// </summary>
        /// <value>NPC的巡逻点坐标集合</value>
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
        private readonly float _maxIdleTime = 2f;

        /// <summary>
        /// 目标角色
        /// </summary>
        private Creature _target = null;

        /// <summary>
        /// 初始化NPC
        /// </summary>
        /// <remarks>
        /// 重置NPC状态，并根据NPC类型设置默认属性和可用交互类型
        /// 不同类型的NPC具有不同的默认属性和交互能力
        /// </remarks>
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
        /// <param name="delta">时间增量（秒）</param>
        /// <remarks>
        /// 更新NPC的AI状态，处理状态转换和行为逻辑
        /// 包括玩家检测和状态处理
        /// </remarks>
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
        /// <returns>检测到的玩家对象</returns>
        /// <remarks>
        /// 简化处理，实际应该通过场景树查找玩家
        /// </remarks>
        private static Creature DetectPlayer()
        {
            // 这里简化处理，实际应该通过场景树查找玩家
            return null;
        }

        /// <summary>
        /// 处理空闲状态
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        /// <remarks>
        /// 空闲一段时间后开始巡逻
        /// </remarks>
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
        /// <param name="delta">时间增量（秒）</param>
        /// <remarks>
        /// 移动到巡逻点，到达后切换到空闲状态
        /// </remarks>
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
        /// <param name="delta">时间增量（秒）</param>
        /// <param name="target">跟随目标</param>
        /// <remarks>
        /// 保持一定距离跟随目标，距离太远时移动靠近
        /// </remarks>
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
        /// <remarks>
        /// 检查目标是否在检测范围内，朝向目标
        /// </remarks>
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
        /// <remarks>
        /// 切换到对话状态，设置目标为玩家，朝向玩家，显示对话内容
        /// 对话条件：NPC可对话、NPC存活、玩家不为空
        /// </remarks>
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
        /// <remarks>
        /// 切换回空闲状态，清除目标
        /// </remarks>
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
        /// <remarks>
        /// 根据交互类型执行相应的交互逻辑
        /// 交互条件：NPC可交互、NPC存活、玩家存活且不为空、交互类型可用
        /// </remarks>
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
        /// <remarks>
        /// 开始与玩家的交易，实际应该打开交易界面
        /// </remarks>
        private void StartTrade(Creature player)
        {
            Log.Info($"{CreatureName}: Let's trade!");
            // 这里简化处理，实际应该打开交易界面
        }

        /// <summary>
        /// 提供任务
        /// </summary>
        /// <param name="player">接收任务的玩家</param>
        /// <remarks>
        /// 向玩家提供任务，实际应该打开任务界面
        /// </remarks>
        private void OfferQuest(Creature player)
        {
            Log.Info($"{CreatureName}: I have a quest for you!");
            // 这里简化处理，实际应该打开任务界面
        }

        /// <summary>
        /// 治疗玩家
        /// </summary>
        /// <param name="player">需要治疗的玩家</param>
        /// <remarks>
        /// 使用治疗技能恢复玩家的生命值
        /// </remarks>
        private void HealPlayer(Creature player)
        {
            // 治疗玩家
            // float healAmount = player.MaxHealth * 0.5f; // 恢复50%生命值
            List<int> healAmount = player.Heal(this, ResourcesManager.SkillsCache["Heal"]); // 使用NPC的第一个技能进行治疗

            Log.Info($"{CreatureName}: Feel better now?");
            Log.Info($"{player.CreatureName} recovered {healAmount.Sum():F0} HP!");
        }

        /// <summary>
        /// 切换跟随状态
        /// </summary>
        /// <param name="player">跟随的玩家</param>
        /// <remarks>
        /// 切换NPC的跟随状态，如果当前在跟随则停止跟随，否则开始跟随
        /// </remarks>
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
        /// <returns>NPC信息字符串</returns>
        /// <remarks>
        /// 格式化输出NPC的基本属性、类型和当前状态
        /// </remarks>
        public override string GetCreatureInfo()
        {
            return $"{base.GetCreatureInfo()} - Type: {Type} - State: {CurrentState}";
        }

        /// <summary>
        /// 获取NPC对于特定任务的对话内容
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <param name="talkType">对话类型</param>
        /// <returns>对话内容</returns>
        /// <remarks>
        /// 根据任务ID和对话类型返回相应的对话内容
        /// 检查NPC是否可以交互和对话，任务是否与该NPC关联
        /// </remarks>
        public string GetNPCTalkForQuest(string questId, NPCTalkType talkType)
        {
            // 检查NPC是否可以交互和对话
            if (!IsInteractive || !CanTalk || !IsAlive)
            {
                return "你好！";
            }

            // 获取任务状态
            var questStatus = hd2dtest.Scripts.Quest.QuestManager.Instance.GetQuestStatus(questId);
            var quest = hd2dtest.Scripts.Quest.QuestManager.Instance.GetQuest(questId);

            if (quest != null)
            {
                // 检查任务是否与该NPC关联
                if (quest.NPCAssociations != null)
                {
                    var npcAssociation = quest.NPCAssociations.Find(assoc => assoc.NPCId == NPCId);
                    if (npcAssociation != null && npcAssociation.TalkTypes.Contains(talkType))
                    {
                        // 根据任务状态和对话类型返回不同的对话内容
                        return talkType switch
                        {
                            NPCTalkType.Accept => $"你好，我需要你帮我完成一个任务：{quest.Description}",
                            NPCTalkType.HandIn => "太好了！你完成了任务，这是你的奖励。",
                            NPCTalkType.Progress => "任务进展如何了？",
                            NPCTalkType.Complete => "任务已经完成了，谢谢你的帮助！",
                            _ => "你好！",
                        };
                    }
                }
            }

            return "你好！";
        }
    }
}