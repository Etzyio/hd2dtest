using Godot;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.Battle
{
    public partial class BattleManager : Node
    {
        public static BattleManager Instance { get; private set; }

        public enum BattleState
        {
            Setup,
            TurnCalculation,
            PlayerTurn,
            EnemyTurn,
            ActionExecution,
            Victory,
            Defeat
        }

        [Signal] public delegate void TurnStartedEventHandler(Creature activeCreature);
        [Signal] public delegate void BattleEndedEventHandler(bool victory);

        public BattleState CurrentState { get; private set; } = BattleState.Setup;

        private List<Creature> _allCombatants = [];
        private List<Creature> _turnQueue = [];
        private Creature _activeCreature;

        // 依赖项
        private Node _battleSceneRoot;

        public override void _Ready()
        {
            Instance = this;
        }

        public void StartBattle(List<Creature> players, List<Creature> enemies)
        {
            _allCombatants.Clear();
            _allCombatants.AddRange(players);
            _allCombatants.AddRange(enemies);

            // 初始化战斗人员（例如重置临时属性）
            foreach (var c in _allCombatants)
            {
                // c.OnBattleStart(); // 如果我们添加这个方法
            }

            Log.Info("Battle Started!");
            CurrentState = BattleState.TurnCalculation;
            CallDeferred(nameof(ProcessTurnQueue));
        }

        private void ProcessTurnQueue()
        {
            if (CheckBattleEnd()) return;

            // 简单的 CTB 或回合制按速度排序
            // 对于女神异闻录风格，1 More 会立即增加一个额外回合，
            // 但基本流程是基于速度的

            // 过滤死亡单位
            _turnQueue = [.. _allCombatants.Where(c => c.IsAlive).OrderByDescending(c => c.Speed)];

            if (_turnQueue.Count > 0)
            {
                StartTurn(_turnQueue[0]);
            }
            else
            {
                // 不应该发生，如果 CheckBattleEnd 正确的话，但以防万一
                Log.Warning("No valid combatants left?");
                CheckBattleEnd();
            }
        }

        private void StartTurn(Creature creature)
        {
            _activeCreature = creature;
            Log.Info($"Turn Start: {_activeCreature.CreatureName}");
            EmitSignal(SignalName.TurnStarted, _activeCreature);

            if (_activeCreature is Player)
            {
                CurrentState = BattleState.PlayerTurn;
                // UI 应该监听 TurnStarted 并显示菜单
            }
            else
            {
                CurrentState = BattleState.EnemyTurn;
                // 执行 AI
                ExecuteEnemyTurn();
            }
        }

        private void ExecuteEnemyTurn()
        {
            // 简单的 AI 延迟
            GetTree().CreateTimer(0.5f).Timeout += () =>
            {
                if (_activeCreature is Monster monster)
                {
                    // 查找目标（随机玩家）
                    var players = _allCombatants.Where(c => c is Player && c.IsAlive).ToList();
                    if (players.Count > 0)
                    {
                        var target = players[DamageCalculator.Randi(players.Count)];
                        // 怪物 AI 逻辑在这里或调用 monster.ExecuteAI(target)
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
            if (CurrentState != BattleState.PlayerTurn) return;

            CurrentState = BattleState.ActionExecution;

            // 执行攻击
            // 在真实场景中，我们会播放动画，等待信号，然后造成伤害
            // 这里我们立即计算

            // 基础攻击技能
            var basicAttack = new Skill { SkillName = "Attack", SkillDefs = [new()] };
            target.TakeDamage(_activeCreature, basicAttack);

            Log.Info($"{_activeCreature.CreatureName} attacks {target.CreatureName}");

            CheckBattleEnd(); // 检查目标是否死亡
            EndTurn();
        }

        public void PlayerAction_Skill(Skill skill, Creature target)
        {
            if (CurrentState != BattleState.PlayerTurn) return;

            CurrentState = BattleState.ActionExecution;

            if (_activeCreature is Player player)
            {
                // 法力检查
                if (player.Mana < skill.ManaCost)
                {
                    Log.Info("Not enough Mana!");
                    CurrentState = BattleState.PlayerTurn; // 恢复
                    return;
                }

                player.Mana -= skill.ManaCost;

                // 应用技能
                // 遍历定义
                foreach (var def in skill.SkillDefs)
                {
                    if (def.Type == Skill.SkillType.Attack)
                    {
                        target.TakeDamage(player, skill);
                    }
                    else if (def.Type == Skill.SkillType.Healing)
                    {
                        target.Heal(player, skill);
                    }
                    // 支持/防御技能...
                }

                Log.Info($"{player.CreatureName} uses {skill.SkillName} on {target.CreatureName}");
            }

            CheckBattleEnd();
            EndTurn();
        }

        // 切换成员动作
        public void PlayerAction_SwitchMember(Player incomingMember)
        {
            // 验证
            if (CurrentState != BattleState.PlayerTurn) return;

            // 交换逻辑在 PartyManager 中（尚未实现）
            // PartyManager.Instance.SwapMember(_activeCreature as Player, incomingMember);

            Log.Info($"{_activeCreature.CreatureName} switched with {incomingMember.CreatureName}");
            EndTurn();
        }

        public void EndTurn()
        {
            if (CurrentState == BattleState.Victory || CurrentState == BattleState.Defeat) return;

            // 处理回合结束效果（持续伤害、Buff 到期）

            // 移动到队列中的下一个？还是重新计算？
            // 对于简单的轮转制，我们可以直接索引++，但通常会重新计算或弹出。
            // 让我们假设我们移除活跃单位并选择下一个。

            _turnQueue.Remove(_activeCreature);

            if (_turnQueue.Count > 0)
            {
                StartTurn(_turnQueue[0]);
            }
            else
            {
                // 回合结束，重新计算
                ProcessTurnQueue();
            }
        }

        private bool CheckBattleEnd()
        {
            bool anyPlayerAlive = _allCombatants.Any(c => c is Player && c.IsAlive);
            bool anyEnemyAlive = _allCombatants.Any(c => c is Monster && c.IsAlive);

            if (!anyPlayerAlive)
            {
                CurrentState = BattleState.Defeat;
                Log.Info("DEFEAT");
                EmitSignal(SignalName.BattleEnded, false);
                return true;
            }

            if (!anyEnemyAlive)
            {
                CurrentState = BattleState.Victory;
                Log.Info("VICTORY");
                EmitSignal(SignalName.BattleEnded, true);
                return true;
            }

            return false;
        }
    }
}
