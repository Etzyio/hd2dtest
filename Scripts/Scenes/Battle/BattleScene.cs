/*
 * File: BattleScene.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 战斗场景控制器，负责初始化战斗环境、创建测试战斗单位、触发战斗并管理战斗结束流程。
 *
 * Key Features:
 * - 验证BattleManager单例存在
 * - 从GameDataManager获取玩家或创建测试玩家
 * - 创建测试怪物（Slime, Goblin）
 * - 调用BattleManager.StartBattle启动战斗
 * - 监听BattleEnded信号显示结算画面
 * - 监听TurnStarted信号通知BattleUI更新
 */

using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Modules.Battle;
using hd2dtest.Scripts.Modules.SkillSystem;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scenes.UI;

namespace hd2dtest.Scenes
{
    /// <summary>
    /// 战斗场景控制器
    /// </summary>
    public partial class BattleScene : Node
    {
        private BattleUI _battleUI;
        private Control _resultOverlay;
        private Label _resultLabel;
        private bool _battleEnded;

        public override void _Ready()
        {
            _battleUI = GetNodeOrNull<BattleUI>("BattleUI");
            if (_battleUI == null)
            {
                Log.Error("BattleUI not found in scene tree");
                return;
            }

            if (BattleManager.Instance == null)
            {
                var bm = new BattleManager { Name = "BattleManager" };
                AddChild(bm);
                Log.Info("BattleManager created and added to scene");
            }

            // Connect signals
            BattleManager.Instance.TurnStarted += OnTurnStarted;
            BattleManager.Instance.BattleEnded += OnBattleEnded;
            _battleUI.OnContinuePressed += OnSettlementContinue;

            // Create combatants and start battle
            var players = CreatePlayers();
            var enemies = CreateEnemies();

            _battleUI.SetupBattle(players, enemies);
            BattleManager.Instance.StartBattle(players, enemies);
            _battleUI.ShowMessage("Battle Start!");

            Log.Info("BattleScene initialized and battle started");
        }

        public override void _Process(double delta)
        {
            if (_battleEnded) return;

            if (BattleManager.Instance != null)
            {
                bool isPlayerTurn = BattleManager.Instance.CurrentState == BattleManager.BattleState.PlayerTurn;
                _battleUI.SetActionsEnabled(isPlayerTurn);
            }
        }

        private void OnTurnStarted(Creature activeCreature)
        {
            if (_battleEnded) return;

            _battleUI.RefreshHUD();
            _battleUI.HighlightActiveCreature(activeCreature);
            _battleUI.ShowMessage($"{activeCreature.CreatureName}'s turn!");
        }

        private void OnBattleEnded(bool victory)
        {
            _battleEnded = true;
            _battleUI.SetActionsEnabled(false);

            var rewards = BattleManager.Instance?.LastBattleRewards;
            _battleUI.ShowResult(rewards);

            Log.Info(victory ? "Battle won!" : "Battle lost...");
        }

        private void OnSettlementContinue()
        {
            if (Main.Instance != null)
                Main.Instance.ClosePopup();
            else
                Log.Warning("Main.Instance is null, cannot switch scene");
        }

        #region Test Combatant Creation

        private static List<Creature> CreatePlayers()
        {
            var players = new List<Creature>();

            var playerData = GameDataManager.Instance?.Teammates?.Player;
            if (playerData != null)
            {
                playerData.Health = playerData.MaxHealth;
                playerData.CurrentMana = playerData.MaxMana;
                players.Add(playerData);
                Log.Info($"Using existing player: {playerData.CreatureName} Lv.{playerData.Level}");
                return players;
            }

            // Create test player with full class system initialization
            var player = new Player();
            player.Initialize();

            // Override stats for testing
            player.CreatureName = "Hero";
            player.MaxHealth = 200;
            player.Health = 200;
            player.MaxMana = 100;
            player.CurrentMana = 100;
            player.Attack = 30;
            player.Defense = 15;
            player.Speed = 20;
            player.Level = 5;

            // Equip all class skills for testing (not just first 4)
            // By default, swordsman gets first 4 auto-equipped via InitializeClassSystem
            Log.Info($"Created test player: Hero Lv.5 with {player.GetEquippedActiveSkills().Count} equipped skills");

            players.Add(player);
            return players;
        }

        private static List<Creature> CreateEnemies()
        {
            var enemies = new List<Creature>();

            var slime = new Monster
            {
                CreatureName = "Slime",
                MaxHealth = 80,
                Health = 80,
                Attack = 15,
                Defense = 5,
                Speed = 10,
                Level = 3,
                Type = Monster.MonsterType.Normal
            };
            enemies.Add(slime);

            var goblin = new Monster
            {
                CreatureName = "Goblin",
                MaxHealth = 120,
                Health = 120,
                Attack = 25,
                Defense = 10,
                Speed = 15,
                Level = 4,
                Type = Monster.MonsterType.Elite
            };
            enemies.Add(goblin);

            Log.Info("Created test enemies: Slime Lv.3, Goblin Lv.4");
            return enemies;
        }

        #endregion
    }
}
