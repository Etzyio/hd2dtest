using Godot;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;
using System;

namespace hd2dtest.Scripts.Modules.Battle
{
    /// <summary>
    /// 战斗管理器，负责管理回合制战斗系统
    /// </summary>
    /// <remarks>
    /// 该类继承自 Godot.Node，作为单例使用，负责管理战斗的流程和状态。
    /// 支持回合制战斗，包括玩家回合、敌人回合、技能使用、攻击和成员切换等功能。
    /// 战斗状态基于速度排序，支持女神异闻录风格的回合制系统。
    /// </remarks>
    public partial class BattleManager : Node
    {
        /// <summary>
        /// 战斗管理器的单例实例
        /// </summary>
        public static BattleManager Instance { get; private set; }

        /// <summary>
        /// 战斗状态枚举
        /// </summary>
        public enum BattleState
        {
            /// <summary>战斗准备阶段</summary>
            Setup,
            /// <summary>回合计算阶段</summary>
            TurnCalculation,
            /// <summary>玩家回合</summary>
            PlayerTurn,
            /// <summary>敌人回合</summary>
            EnemyTurn,
            /// <summary>动作执行阶段</summary>
            ActionExecution,
            /// <summary>胜利状态</summary>
            Victory,
            /// <summary>失败状态</summary>
            Defeat
        }

        /// <summary>
        /// 回合开始信号
        /// </summary>
        /// <param name="activeCreature">当前行动的战斗单位</param>
        [Signal] public delegate void TurnStartedEventHandler(Creature activeCreature);

        /// <summary>
        /// 战斗结束信号
        /// </summary>
        /// <param name="victory">是否胜利</param>
        [Signal] public delegate void BattleEndedEventHandler(bool victory);

        /// <summary>
        /// 当前战斗状态
        /// </summary>
        /// <value>战斗状态枚举值，默认为Setup</value>
        public BattleState CurrentState { get; private set; } = BattleState.Setup;

        /// <summary>
        /// 最近一次战斗的结算奖励（胜利时填充，失败时为null）
        /// </summary>
        public BattleRewards LastBattleRewards { get; private set; }

        /// <summary>
        /// 所有战斗单位列表
        /// </summary>
        private List<Creature> _allCombatants = [];

        /// <summary>
        /// 回合队列
        /// </summary>
        private List<Creature> _turnQueue = [];

        /// <summary>
        /// 当前行动的战斗单位
        /// </summary>
        private Creature _activeCreature;

        /// <summary>
        /// 战斗场景根节点
        /// </summary>
        private Node _battleSceneRoot;

        /// <summary>
        /// 节点准备就绪时调用
        /// </summary>
        /// <remarks>
        /// 设置单例实例
        /// </remarks>
        public override void _Ready()
        {
            Instance = this;
        }

        public void StartBattle(List<Creature> players, List<Creature> enemies)
        {
            try
            {
                if (CurrentState != BattleState.Setup && CurrentState != BattleState.Victory && CurrentState != BattleState.Defeat)
                {
                    Log.Error($"Cannot start battle: already in battle state {CurrentState}");
                    return;
                }

                if (players == null || players.Count == 0)
                {
                    Log.Error("Cannot start battle: no players provided");
                    return;
                }

                if (enemies == null || enemies.Count == 0)
                {
                    Log.Error("Cannot start battle: no enemies provided");
                    return;
                }

                _allCombatants.Clear();
                
                foreach (var player in players)
                {
                    if (player == null)
                    {
                        Log.Warning("Null player in players list, skipping");
                        continue;
                    }
                    if (!player.IsAlive)
                    {
                        Log.Warning($"Player {player.CreatureName} is not alive, skipping");
                        continue;
                    }
                    _allCombatants.Add(player);
                }

                foreach (var enemy in enemies)
                {
                    if (enemy == null)
                    {
                        Log.Warning("Null enemy in enemies list, skipping");
                        continue;
                    }
                    if (!enemy.IsAlive)
                    {
                        Log.Warning($"Enemy {enemy.CreatureName} is not alive, skipping");
                        continue;
                    }
                    _allCombatants.Add(enemy);
                }

                if (_allCombatants.Count == 0)
                {
                    Log.Error("Cannot start battle: no valid combatants");
                    return;
                }

                Log.Info("Battle Started!");
                CurrentState = BattleState.TurnCalculation;
                CallDeferred(nameof(ProcessTurnQueue));
            }
            catch (Exception ex)
            {
                Log.Error($"Error starting battle: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理回合队列
        /// </summary>
        /// <remarks>
        /// 根据战斗单位的速度排序，确定回合顺序
        /// 支持女神异闻录风格的回合制系统
        /// </remarks>
        private void ProcessTurnQueue()
        {
            if (CheckBattleEnd()) return;

            _turnQueue = [.. _allCombatants.Where(c => c.IsAlive).OrderByDescending(c => c.Speed)];

            if (_turnQueue.Count > 0)
            {
                StartTurn(_turnQueue[0]);
            }
            else
            {
                Log.Warning("No valid combatants left?");
                CheckBattleEnd();
            }
        }

        /// <summary>
        /// 开始回合
        /// </summary>
        /// <param name="creature">当前行动的战斗单位</param>
        /// <remarks>
        /// 设置当前行动单位，发送回合开始信号，根据单位类型设置战斗状态
        /// </remarks>
        private void StartTurn(Creature creature)
        {
            _activeCreature = creature;
            Log.Info($"Turn Start: {_activeCreature.CreatureName}");
            EmitSignal(SignalName.TurnStarted, _activeCreature);

            if (_activeCreature is Player)
            {
                CurrentState = BattleState.PlayerTurn;
            }
            else
            {
                CurrentState = BattleState.EnemyTurn;
                ExecuteEnemyTurn();
            }
        }

        /// <summary>
        /// 执行敌人回合
        /// </summary>
        /// <remarks>
        /// 延迟0.5秒后执行敌人AI，随机选择一个玩家作为目标进行攻击
        /// </remarks>
        private void ExecuteEnemyTurn()
        {
            GetTree().CreateTimer(0.5f).Timeout += () =>
            {
                if (_activeCreature is Monster monster)
                {
                    var players = _allCombatants.Where(c => c is Player && c.IsAlive).ToList();
                    if (players.Count > 0)
                    {
                        var target = players[DamageCalculator.Randi(players.Count)];
                        monster.AttackTarget(target);
                        EndTurn();
                    }
                    else
                    {
                        EndTurn();
                    }
                }
            };
        }

        public void PlayerAction_Attack(Creature target)
        {
            try
            {
                if (CurrentState != BattleState.PlayerTurn)
                {
                    Log.Warning($"Cannot attack: not player turn. Current state: {CurrentState}");
                    return;
                }

                if (_activeCreature == null)
                {
                    Log.Error("Cannot attack: no active creature");
                    return;
                }

                if (!_activeCreature.IsAlive)
                {
                    Log.Error($"Cannot attack: active creature {_activeCreature.CreatureName} is dead");
                    return;
                }

                if (target == null)
                {
                    Log.Error("Cannot attack null target");
                    return;
                }

                if (!target.IsAlive)
                {
                    Log.Warning($"Cannot attack dead target: {target.CreatureName}");
                    return;
                }

                CurrentState = BattleState.ActionExecution;

                // 基础攻击技能
                var basicAttack = new Skill { SkillName = "Attack", SkillDefs = [new()] };
                target.TakeDamage(_activeCreature, basicAttack);

                Log.Info($"{_activeCreature.CreatureName} attacks {target.CreatureName}");

                CheckBattleEnd();
                EndTurn();
            }
            catch (Exception ex)
            {
                Log.Error($"Error in PlayerAction_Attack: {ex.Message}");
                CurrentState = BattleState.PlayerTurn;
            }
        }

        public void PlayerAction_Skill(Skill skill, Creature target)
        {
            try
            {
                if (CurrentState != BattleState.PlayerTurn)
                {
                    Log.Warning($"Cannot use skill: not player turn. Current state: {CurrentState}");
                    return;
                }

                if (_activeCreature == null)
                {
                    Log.Error("Cannot use skill: no active creature");
                    return;
                }

                if (!_activeCreature.IsAlive)
                {
                    Log.Error($"Cannot use skill: active creature {_activeCreature.CreatureName} is dead");
                    return;
                }

                if (skill == null)
                {
                    Log.Error("Cannot use null skill");
                    return;
                }

                if (target == null)
                {
                    Log.Error("Cannot use skill on null target");
                    return;
                }

                CurrentState = BattleState.ActionExecution;

                if (_activeCreature is Player player)
                {
                    if (player.Mana < skill.ManaCost)
                    {
                        Log.Warning($"Not enough Mana to use {skill.SkillName}! Need: {skill.ManaCost}, Have: {player.Mana}");
                        CurrentState = BattleState.PlayerTurn;
                        return;
                    }

                    player.Mana -= skill.ManaCost;

                    if (skill.SkillDefs == null || skill.SkillDefs.Count == 0)
                    {
                        Log.Warning($"Skill {skill.SkillName} has no definitions");
                        CurrentState = BattleState.PlayerTurn;
                        return;
                    }

                    foreach (var def in skill.SkillDefs)
                    {
                        if (def == null)
                        {
                            Log.Warning("Null skill definition in skill");
                            continue;
                        }

                        if (def.Type == Skill.SkillType.Attack)
                        {
                            target.TakeDamage(player, skill);
                        }
                        else if (def.Type == Skill.SkillType.Healing)
                        {
                            target.Heal(player, skill);
                        }
                    }

                    Log.Info($"{player.CreatureName} uses {skill.SkillName} on {target.CreatureName}");
                }

                CheckBattleEnd();
                EndTurn();
            }
            catch (Exception ex)
            {
                Log.Error($"Error in PlayerAction_Skill: {ex.Message}");
                CurrentState = BattleState.PlayerTurn;
            }
        }

        // 切换成员动作
        public void PlayerAction_SwitchMember(Player incomingMember)
        {
            try
            {
                if (CurrentState != BattleState.PlayerTurn)
                {
                    Log.Warning($"Cannot switch member: not player turn. Current state: {CurrentState}");
                    return;
                }

                if (_activeCreature == null)
                {
                    Log.Error("Cannot switch member: no active creature");
                    return;
                }

                if (incomingMember == null)
                {
                    Log.Error("Cannot switch to null member");
                    return;
                }

                if (!incomingMember.IsAlive)
                {
                    Log.Warning($"Cannot switch to dead member: {incomingMember.CreatureName}");
                    return;
                }

                if (_activeCreature is Player currentPlayer && currentPlayer == incomingMember)
                {
                    Log.Warning($"Cannot switch member with themselves: {currentPlayer.CreatureName}");
                    return;
                }

                Log.Info($"{_activeCreature.CreatureName} switched with {incomingMember.CreatureName}");
                EndTurn();
            }
            catch (Exception ex)
            {
                Log.Error($"Error in PlayerAction_SwitchMember: {ex.Message}");
            }
        }

        public void EndTurn()
        {
            try
            {
                if (CurrentState == BattleState.Victory || CurrentState == BattleState.Defeat)
                {
                    Log.Warning("Cannot end turn: battle already ended");
                    return;
                }

                _turnQueue.Remove(_activeCreature);

                if (_turnQueue.Count > 0)
                {
                    StartTurn(_turnQueue[0]);
                }
                else
                {
                    ProcessTurnQueue();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in EndTurn: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        /// <returns>战斗结束返回true，否则返回false</returns>
        /// <remarks>
        /// 检查所有玩家和敌人是否存活，判断战斗胜负
        /// 如果所有玩家死亡，则战斗失败；如果所有敌人死亡，则战斗胜利
        /// </remarks>
        private bool CheckBattleEnd()
        {
            bool anyPlayerAlive = _allCombatants.Any(c => c is Player && c.IsAlive);
            bool anyEnemyAlive = _allCombatants.Any(c => c is Monster && c.IsAlive);

            if (!anyPlayerAlive)
            {
                CurrentState = BattleState.Defeat;
                LastBattleRewards = new BattleRewards { Victory = false };
                Log.Info("DEFEAT");
                EmitSignal(SignalName.BattleEnded, false);
                return true;
            }

            if (!anyEnemyAlive)
            {
                CurrentState = BattleState.Victory;
                LastBattleRewards = CalculateRewards();
                ApplyRewards(LastBattleRewards);
                Log.Info("VICTORY");
                EmitSignal(SignalName.BattleEnded, true);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 计算战斗奖励（金币、经验、掉落物品、升级）
        /// </summary>
        private BattleRewards CalculateRewards()
        {
            var rewards = new BattleRewards { Victory = true };
            var rng = new Random();

            foreach (var c in _allCombatants)
            {
                if (c is Monster monster && !monster.IsAlive)
                {
                    // Gold: same formula as Monster.DropLoot
                    int gold = rng.Next(5, monster.Level * 20 + 6);
                    rewards.TotalGold += gold;

                    // Experience: same as Player.KillEnemy
                    int exp = monster.Level * 10;
                    rewards.TotalExperience += exp;

                    // JP: based on monster level and type
                    int jp = monster.Level * 2;
                    if (monster.Type == Monster.MonsterType.Elite) jp *= 2;
                    if (monster.Type == Monster.MonsterType.Boss) jp *= 4;
                    rewards.TotalJP += jp;

                    // Items: 40% chance to drop one item from DropItems
                    if (monster.DropItems.Count > 0 && rng.NextDouble() < 0.4)
                    {
                        string itemId = monster.DropItems[rng.Next(monster.DropItems.Count)];
                        rewards.DroppedItems.Add(itemId);
                    }
                }
            }

            return rewards;
        }

        /// <summary>
        /// 将奖励应用到玩家
        /// </summary>
        private void ApplyRewards(BattleRewards rewards)
        {
            foreach (var c in _allCombatants)
            {
                if (c is Player player)
                {
                    int prevLevel = player.Level;

                    player.Gold += rewards.TotalGold;
                    player.JP += rewards.TotalJP;
                    player.Experience += rewards.TotalExperience;

                    // Check for level up
                    int requiredExp = player.Level * 100;
                    while (player.Experience >= requiredExp)
                    {
                        player.Level++;
                        player.Experience -= requiredExp;
                        requiredExp = player.Level * 100;
                    }

                    if (player.Level > prevLevel)
                        rewards.LevelUps.Add(player.CreatureName);

                    // Add dropped items to inventory
                    foreach (var itemId in rewards.DroppedItems)
                    {
                        if (player.Inventory.TryGetValue(itemId, out int qty))
                            player.Inventory[itemId] = qty + 1;
                        else
                            player.Inventory[itemId] = 1;
                    }
                }
            }
        }
    }
}
