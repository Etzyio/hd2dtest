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

        // Dependencies
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

            // Initialize Combatants for Battle (e.g. reset temp stats)
            foreach (var c in _allCombatants)
            {
                // c.OnBattleStart(); // If we add this method
            }

            Log.Info("Battle Started!");
            CurrentState = BattleState.TurnCalculation;
            CallDeferred(nameof(ProcessTurnQueue));
        }

        private void ProcessTurnQueue()
        {
            if (CheckBattleEnd()) return;

            // Simple CTB or Round-based sort by Speed
            // For Persona-style, usually 1 More adds an extra turn immediately, 
            // but basic flow is Speed based.
            
            // Filter dead
            _turnQueue = [.. _allCombatants.Where(c => c.IsAlive).OrderByDescending(c => c.Speed)];

            if (_turnQueue.Count > 0)
            {
                StartTurn(_turnQueue[0]);
            }
            else
            {
                // Should not happen if CheckBattleEnd is correct, but just in case
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
                // UI should listen to TurnStarted and show menu
            }
            else
            {
                CurrentState = BattleState.EnemyTurn;
                // Execute AI
                ExecuteEnemyTurn();
            }
        }

        private void ExecuteEnemyTurn()
        {
            // Simple AI delay
            GetTree().CreateTimer(0.5f).Timeout += () =>
            {
                if (_activeCreature is Monster monster)
                {
                    // Find target (random player)
                    var players = _allCombatants.Where(c => c is Player && c.IsAlive).ToList();
                    if (players.Count > 0)
                    {
                        var target = players[DamageCalculator.Randi(players.Count)];
                        // Monster AI logic here or call monster.ExecuteAI(target)
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
            
            // Perform attack
            // In a real scenario, we'd play animation, wait for signal, then deal damage
            // Here we just calculate immediately
            
            // Basic attack skill
            var basicAttack = new Skill { SkillName = "Attack", SkillDefs = [new()] };
            target.TakeDamage(_activeCreature, basicAttack);

            Log.Info($"{_activeCreature.CreatureName} attacks {target.CreatureName}");

            CheckBattleEnd(); // Check if target died
            EndTurn();
        }

        public void PlayerAction_Skill(Skill skill, Creature target)
        {
            if (CurrentState != BattleState.PlayerTurn) return;

            CurrentState = BattleState.ActionExecution;

            if (_activeCreature is Player player)
            {
                 // Mana check
                if (player.Mana < skill.ManaCost)
                {
                    Log.Info("Not enough Mana!");
                    CurrentState = BattleState.PlayerTurn; // Revert
                    return;
                }

                player.Mana -= skill.ManaCost;
                
                // Apply skill
                // Loop through defs
                foreach(var def in skill.SkillDefs)
                {
                     if (def.Type == Skill.SkillType.Attack)
                     {
                         target.TakeDamage(player, skill);
                     }
                     else if (def.Type == Skill.SkillType.Healing)
                     {
                         target.Heal(player, skill);
                     }
                     // Support/Defense...
                }
                
                Log.Info($"{player.CreatureName} uses {skill.SkillName} on {target.CreatureName}");
            }

            CheckBattleEnd();
            EndTurn();
        }
        
        // Switch Member Action
        public void PlayerAction_SwitchMember(Player incomingMember)
        {
             // Validate
             if (CurrentState != BattleState.PlayerTurn) return;
             
             // Swap logic in PartyManager (not implemented yet)
             // PartyManager.Instance.SwapMember(_activeCreature as Player, incomingMember);
             
             Log.Info($"{_activeCreature.CreatureName} switched with {incomingMember.CreatureName}");
             EndTurn();
        }

        public void EndTurn()
        {
            if (CurrentState == BattleState.Victory || CurrentState == BattleState.Defeat) return;

            // Process End of Turn effects (DoT, Buff expiry)
            
            // Move to next in queue? Or re-calculate?
            // For simple Round Robin, we could just index++, but usually we Recalculate or pop.
            // Let's assume we remove active from queue and pick next.
            
            _turnQueue.Remove(_activeCreature);
            
            if (_turnQueue.Count > 0)
            {
                StartTurn(_turnQueue[0]);
            }
            else
            {
                // Round Over, Recalculate
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
