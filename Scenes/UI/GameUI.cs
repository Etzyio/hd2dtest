using Godot;
using System;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace hd2dtest.Scenes.UI
{
    /// <summary>
    /// 游戏UI管理类，负责显示玩家和队友的状态面板
    /// </summary>
    public partial class GameUI : CanvasLayer
    {
        private class TeammateUI
        {
            public Label NameLabel;
            public Label HealthValue;
            public ProgressBar HealthBar;
            public Label ManaValue;
            public ProgressBar ManaBar;
            public Control Root;
        }

        private List<TeammateUI> _teammateUIs = new();

        /// <summary>
        /// 玩家生命值显示标签
        /// </summary>
        private Label HealthValue;

        /// <summary>
        /// 玩家生命值进度条
        /// </summary>
        private ProgressBar HealthBar;

        /// <summary>
        /// 玩家魔法值显示标签
        /// </summary>
        private Label ManaValue;

        /// <summary>
        /// 玩家魔法值进度条
        /// </summary>
        private ProgressBar ManaBar;

        /// <summary>
        /// 玩家实例引用
        /// </summary>
        private Player _player;
        private Label _crossPassivesLabel;
        
        /// <summary>
        /// 队友实例数组，最多支持4个队友
        /// </summary>
        private Player[] _teammates = new Player[4];

        public override void _Ready()
        {
            InitializeUIReferences();
            FindPlayer();
            InitializeTeammatePanels();
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            UpdatePlayerStatus();
            UpdateTeammateStatus();
        }

        private void InitializeUIReferences()
        {
            // 获取玩家状态面板元素
            var playerPanel = GetNode<ColorRect>("PlayerStatusPanel");
            var playerVBox = playerPanel.GetNode<VBoxContainer>("VBoxContainer");
            var healthHBox = playerVBox.GetNode<HBoxContainer>("HBoxContainer");
            HealthValue = healthHBox.GetNode<Label>("HealthValue");
            HealthBar = playerVBox.GetNode<ProgressBar>("HealthBar");
            var manaHBox = playerVBox.GetNode<HBoxContainer>("HBoxContainer2");
            ManaValue = manaHBox.GetNode<Label>("ManaValue");
            ManaBar = playerVBox.GetNode<ProgressBar>("ManaBar");
            _crossPassivesLabel = new Label();
            _crossPassivesLabel.Text = ResourcesManager.GetLocalizedString("cross_passives_empty");
            playerVBox.AddChild(_crossPassivesLabel);

            // 获取队友状态面板元素
            var teammatesPanel = GetNode<ColorRect>("TeammatesStatusPanel");
            var teammatesVBox = teammatesPanel.GetNode<VBoxContainer>("VBoxContainer");
            
            _teammateUIs.Clear();
            for (int i = 1; i <= 4; i++)
            {
                var ui = new TeammateUI();
                // Using generic GetNode with string formatting to avoid hardcoded duplication
                // Assuming the node structure matches "Teammate{i}"
                var vbox = teammatesVBox.GetNode<VBoxContainer>($"Teammate{i}");
                ui.Root = vbox;
                var header = vbox.GetNode<HBoxContainer>($"Teammate{i}Header");
                ui.NameLabel = header.GetNode<Label>($"Teammate{i}Label");
                var stats = vbox.GetNode<HBoxContainer>($"Teammate{i}Stats");
                ui.HealthValue = stats.GetNode<Label>($"Teammate{i}HealthValue");
                ui.HealthBar = stats.GetNode<ProgressBar>($"Teammate{i}HealthBar");
                ui.ManaValue = stats.GetNode<Label>($"Teammate{i}ManaValue");
                ui.ManaBar = stats.GetNode<ProgressBar>($"Teammate{i}ManaBar");
                
                SetHealthBarColor(ui.HealthBar, 1.0f);
                SetManaBarColor(ui.ManaBar, 1.0f);
                _teammateUIs.Add(ui);
            }
        }

        private void InitializeTeammatePanels()
        {
            for (int i = 0; i < _teammateUIs.Count; i++)
            {
                var ui = _teammateUIs[i];
                // Use localized string for default name
                ui.NameLabel.Text = string.Format(ResourcesManager.GetLocalizedString($"teammate_name_{i+1}"), i + 1);
                ui.HealthValue.Text = "100/100";
                ui.HealthBar.Value = 100;
                ui.ManaValue.Text = "100/100";
                ui.ManaBar.Value = 100;
                SetTeammateVisible(i, false);
            }
        }

        private void SetTeammateVisible(int index, bool visible)
        {
            if (index >= 0 && index < _teammateUIs.Count)
            {
                var ui = _teammateUIs[index];
                ui.NameLabel.Visible = visible;
                ui.HealthValue.Visible = visible;
                ui.HealthBar.Visible = visible;
                ui.ManaValue.Visible = visible;
                ui.ManaBar.Visible = visible;
            }
        }

        private void FindPlayer()
        {
            var root = GetTree().Root;
            _player = null;

            if (SaveManager.Instance != null)
            {
                Log.Info("Using SaveManager to find player");
            }

            if (_player == null)
            {
                Log.Info("Player not found in scene tree");
            }
        }

        private void UpdatePlayerStatus()
        {
            if (_player != null)
            {
                float healthPercentage = _player.Health / _player.MaxHealth;
                HealthValue.Text = $"{_player.Health:F0}/{_player.MaxHealth:F0}";
                AnimateProgressBar(HealthBar, healthPercentage * 100);
                SetHealthBarColor(HealthBar, healthPercentage);

                float manaPercentage = _player.Mana / _player.MaxMana;
                ManaValue.Text = $"{_player.Mana:F0}/{_player.MaxMana:F0}";
                AnimateProgressBar(ManaBar, manaPercentage * 100);
                SetManaBarColor(ManaBar, manaPercentage);
                if (_crossPassivesLabel != null)
                {
                    var passives = _player.EquippedPassives;
                    var passiveNames = passives.Select(p => p.PassiveName);
                    var passiveText = string.Join(", ", passiveNames);
                    
                    if (string.IsNullOrEmpty(passiveText))
                    {
                         _crossPassivesLabel.Text = ResourcesManager.GetLocalizedString("cross_passives_empty");
                    }
                    else
                    {
                        // Use string.Format with localized format string
                        _crossPassivesLabel.Text = string.Format(ResourcesManager.GetLocalizedString("cross_passives_label"), passiveText);
                    }
                }
            }
        }

        private void UpdateTeammateStatus()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_teammates[i] != null)
                {
                    float healthPercentage = _teammates[i].Health / _teammates[i].MaxHealth;
                    float manaPercentage = _teammates[i].Mana / _teammates[i].MaxMana;
                    UpdateTeammatePanel(i, _teammates[i].CreatureName, healthPercentage, _teammates[i].Health, _teammates[i].MaxHealth, manaPercentage, _teammates[i].Mana, _teammates[i].MaxMana);
                }
            }
        }

        private void UpdateTeammatePanel(int index, string name, float healthPercentage, float health, float maxHealth, float manaPercentage, float mana, float maxMana)
        {
            if (index >= 0 && index < _teammateUIs.Count)
            {
                var ui = _teammateUIs[index];
                ui.NameLabel.Text = name;
                // Localized format for health label if needed, currently hardcoded format in original
                ui.HealthValue.Text = $"{health:F0}/{maxHealth:F0}";
                AnimateProgressBar(ui.HealthBar, healthPercentage * 100);
                SetHealthBarColor(ui.HealthBar, healthPercentage);
                ui.ManaValue.Text = $"{mana:F0}/{maxMana:F0}";
                AnimateProgressBar(ui.ManaBar, manaPercentage * 100);
                SetManaBarColor(ui.ManaBar, manaPercentage);
                SetTeammateVisible(index, true);
            }
        }

        private void HideTeammatePanel(int index)
        {
            SetTeammateVisible(index, false);
        }

        public void AddTeammate(Player teammate)
        {
            for (int i = 0; i < _teammates.Length; i++)
            {
                if (_teammates[i] == null)
                {
                    _teammates[i] = teammate;
                    float healthPercentage = teammate.Health / teammate.MaxHealth;
                    float manaPercentage = teammate.Mana / teammate.MaxMana;
                    UpdateTeammatePanel(i, teammate.CreatureName, healthPercentage, teammate.Health, teammate.MaxHealth, manaPercentage, teammate.Mana, teammate.MaxMana);
                    break;
                }
            }
        }

        public void RemoveTeammate(Player teammate)
        {
            for (int i = 0; i < _teammates.Length; i++)
            {
                if (_teammates[i] == teammate)
                {
                    _teammates[i] = null;
                    HideTeammatePanel(i);
                    break;
                }
            }
        }

        public void SetPlayer(Player player)
        {
            _player = player;
        }

        private void AnimateProgressBar(ProgressBar progressBar, float targetValue)
        {
            float currentValue = (float)progressBar.Value;
            float difference = targetValue - currentValue;
            if (Math.Abs(difference) > 0.1f)
            {
                progressBar.Value = (float)Mathf.Lerp(currentValue, targetValue, 0.1f);
            }
            else
            {
                progressBar.Value = targetValue;
            }
        }

        private void SetHealthBarColor(ProgressBar progressBar, float healthPercentage)
        {
            Color color;
            if (healthPercentage > 0.7f)
            {
                color = Colors.Green;
            }
            else if (healthPercentage > 0.3f)
            {
                color = Colors.Yellow;
            }
            else
            {
                color = Colors.Red;
            }

            progressBar.Modulate = color;
        }

        private void SetManaBarColor(ProgressBar progressBar, float manaPercentage)
        {
            Color color = new Color(0.2f, 0.4f, 1.0f, 1.0f);
            progressBar.Modulate = color;
        }
    }
}
