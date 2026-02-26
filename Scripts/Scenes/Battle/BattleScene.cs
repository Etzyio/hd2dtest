using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Modules.Battle;
using hd2dtest.Scripts.Utilities;
using PlayerData = hd2dtest.Scripts.Modules.Player;

namespace hd2dtest.Scripts.Scenes.Battle
{
    public partial class BattleScene : Control
    {
        private BattleManager _battleManager;
        private Label _statusLabel;
        private Button _attackButton;
        private Button _skillButton; // Placeholder
        
        // Test Data
        private PlayerData _testPlayer;
        private Monster _testMonster;

        public override void _Ready()
        {
            InitializeUI();
            InitializeBattle();
        }

        private void InitializeUI()
        {
            // Simple UI Setup
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(LayoutPreset.Center);
            AddChild(vbox);

            _statusLabel = new Label();
            _statusLabel.Text = "Initializing Battle...";
            vbox.AddChild(_statusLabel);

            _attackButton = new Button();
            _attackButton.Text = "Attack";
            _attackButton.Pressed += OnAttackPressed;
            _attackButton.Disabled = true;
            vbox.AddChild(_attackButton);

            _skillButton = new Button();
            _skillButton.Text = "Skill (Not Impl)";
            _skillButton.Disabled = true;
            vbox.AddChild(_skillButton);
        }

        private void InitializeBattle()
        {
            // 1. Create BattleManager
            _battleManager = new BattleManager();
            AddChild(_battleManager);

            // 2. Create Combatants
            _testPlayer = new PlayerData();
            _testPlayer.Initialize();
            _testPlayer.CreatureName = "Hero";
            // Set up some stats for testing
            _testPlayer.Speed = 50; 
            _testPlayer.Attack = 20;

            _testMonster = new Monster();
            _testMonster.Initialize();
            _testMonster.CreatureName = "Slime";
            _testMonster.Speed = 30;
            _testMonster.Health = 50;
            _testMonster.MaxHealth = 50;

            // 3. Connect Signals
            _battleManager.TurnStarted += OnTurnStarted;
            _battleManager.BattleEnded += OnBattleEnded;

            // 4. Start Battle
            _battleManager.StartBattle(new List<Creature> { _testPlayer }, new List<Creature> { _testMonster });
        }

        private void OnTurnStarted(Creature activeCreature)
        {
            _statusLabel.Text = $"Turn: {activeCreature.CreatureName}";
            
            if (activeCreature is PlayerData)
            {
                _attackButton.Disabled = false;
                _skillButton.Disabled = false; // Enable if implemented
                Log.Info("Player Input Enabled");
            }
            else
            {
                _attackButton.Disabled = true;
                _skillButton.Disabled = true;
                Log.Info("Player Input Disabled (Enemy Turn)");
            }
            
            UpdateStatusDisplay();
        }

        private void OnBattleEnded(bool victory)
        {
            _statusLabel.Text = victory ? "Victory!" : "Defeat...";
            _attackButton.Disabled = true;
            _skillButton.Disabled = true;
        }

        private void OnAttackPressed()
        {
            // Hardcoded target for prototype
            _battleManager.PlayerAction_Attack(_testMonster);
            UpdateStatusDisplay();
        }

        private void UpdateStatusDisplay()
        {
             // Update logic if needed, e.g. show HP
             // For now just logging
             Log.Info($"Player HP: {_testPlayer.Health}, Enemy HP: {_testMonster.Health}");
        }
    }
}
