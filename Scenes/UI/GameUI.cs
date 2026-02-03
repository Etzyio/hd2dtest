using Godot;
using System;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.UI
{
    /// <summary>
    /// 游戏UI管理类，负责显示玩家和队友的状态面板
    /// </summary>
    /// <remarks>
    /// 该类继承自CanvasLayer，用于在游戏中显示玩家的生命值、魔法值
    /// 以及队友的状态信息，包括生命值和名称
    /// </remarks>
    public partial class GameUI : CanvasLayer
    {
        /// <summary>
        /// 玩家生命值显示标签
        /// </summary>
        [Export]
        private Label HealthValue;

        /// <summary>
        /// 玩家生命值进度条
        /// </summary>
        [Export]
        private ProgressBar HealthBar;

        /// <summary>
        /// 玩家魔法值显示标签
        /// </summary>
        [Export]
        private Label ManaValue;

        /// <summary>
        /// 玩家魔法值进度条
        /// </summary>
        [Export]
        private ProgressBar ManaBar;

        /// <summary>
        /// 队友1名称标签
        /// </summary>
        [Export]
        private Label Teammate1Label;

        /// <summary>
        /// 队友1生命值值显示标签
        /// </summary>
        [Export]
        private Label Teammate1HealthValue;

        /// <summary>
        /// 队友1生命值进度条
        /// </summary>
        [Export]
        private ProgressBar Teammate1HealthBar;

        /// <summary>
        /// 队友1魔法值值显示标签
        /// </summary>
        [Export]
        private Label Teammate1ManaValue;

        /// <summary>
        /// 队友1魔法值进度条
        /// </summary>
        [Export]
        private ProgressBar Teammate1ManaBar;

        /// <summary>
        /// 队友2名称标签
        /// </summary>
        [Export]
        private Label Teammate2Label;

        /// <summary>
        /// 队友2生命值值显示标签
        /// </summary>
        [Export]
        private Label Teammate2HealthValue;

        /// <summary>
        /// 队友2生命值进度条
        /// </summary>
        [Export]
        private ProgressBar Teammate2HealthBar;

        /// <summary>
        /// 队友2魔法值值显示标签
        /// </summary>
        [Export]
        private Label Teammate2ManaValue;

        /// <summary>
        /// 队友2魔法值进度条
        /// </summary>
        [Export]
        private ProgressBar Teammate2ManaBar;

        /// <summary>
        /// 队友3名称标签
        /// </summary>
        [Export]
        private Label Teammate3Label;

        /// <summary>
        /// 队友3生命值值显示标签
        /// </summary>
        [Export]
        private Label Teammate3HealthValue;

        /// <summary>
        /// 队友3生命值进度条
        /// </summary>
        [Export]
        private ProgressBar Teammate3HealthBar;

        /// <summary>
        /// 队友3魔法值值显示标签
        /// </summary>
        [Export]
        private Label Teammate3ManaValue;

        /// <summary>
        /// 队友3魔法值进度条
        /// </summary>
        [Export]
        private ProgressBar Teammate3ManaBar;

        /// <summary>
        /// 队友4名称标签
        /// </summary>
        [Export]
        private Label Teammate4Label;

        /// <summary>
        /// 队友4生命值值显示标签
        /// </summary>
        [Export]
        private Label Teammate4HealthValue;

        /// <summary>
        /// 队友4生命值进度条
        /// </summary>
        [Export]
        private ProgressBar Teammate4HealthBar;

        /// <summary>
        /// 队友4魔法值值显示标签
        /// </summary>
        [Export]
        private Label Teammate4ManaValue;

        /// <summary>
        /// 队友4魔法值进度条
        /// </summary>
        [Export]
        private ProgressBar Teammate4ManaBar;

        /// <summary>
        /// 玩家实例引用
        /// </summary>
        private Player _player;
        
        /// <summary>
        /// 队友实例数组，最多支持4个队友
        /// </summary>
        private Player[] _teammates = new Player[4];

        /// <summary>
        /// 节点就绪时调用的方法
        /// </summary>
        /// <remarks>
        /// 初始化UI元素引用，尝试获取玩家实例，初始化队友状态面板
        /// 并开始更新UI
        /// </remarks>
        public override void _Ready()
        {
            // 初始化UI元素引用
            InitializeUIReferences();

            // 尝试获取玩家实例
            FindPlayer();

            // 初始化队友状态面板
            InitializeTeammatePanels();

            // 开始更新UI
            SetProcess(true);
        }

        /// <summary>
        /// 每帧更新时调用的方法
        /// </summary>
        /// <param name="delta">帧间隔时间</param>
        /// <remarks>
        /// 更新玩家状态和队友状态，确保UI显示最新的游戏数据
        /// </remarks>
        public override void _Process(double delta)
        {
            // 更新玩家状态
            UpdatePlayerStatus();

            // 更新队友状态
            UpdateTeammateStatus();
        }

        /// <summary>
        /// 初始化UI元素引用
        /// </summary>
        /// <remarks>
        /// 从场景树中获取所有需要的UI元素引用，包括玩家状态面板
        /// 和队友状态面板的各个组件
        /// </remarks>
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

            // 获取队友状态面板元素
            var teammatesPanel = GetNode<ColorRect>("TeammatesStatusPanel");
            var teammatesVBox = teammatesPanel.GetNode<VBoxContainer>("VBoxContainer");
            
            // 获取队友1元素
            var teammate1VBox = teammatesVBox.GetNode<VBoxContainer>("Teammate1");
            var teammate1Header = teammate1VBox.GetNode<HBoxContainer>("Teammate1Header");
            Teammate1Label = teammate1Header.GetNode<Label>("Teammate1Label");
            var teammate1Stats = teammate1VBox.GetNode<HBoxContainer>("Teammate1Stats");
            Teammate1HealthValue = teammate1Stats.GetNode<Label>("Teammate1HealthValue");
            Teammate1HealthBar = teammate1Stats.GetNode<ProgressBar>("Teammate1HealthBar");
            Teammate1ManaValue = teammate1Stats.GetNode<Label>("Teammate1ManaValue");
            Teammate1ManaBar = teammate1Stats.GetNode<ProgressBar>("Teammate1ManaBar");
            
            // 获取队友2元素
            var teammate2VBox = teammatesVBox.GetNode<VBoxContainer>("Teammate2");
            var teammate2Header = teammate2VBox.GetNode<HBoxContainer>("Teammate2Header");
            Teammate2Label = teammate2Header.GetNode<Label>("Teammate2Label");
            var teammate2Stats = teammate2VBox.GetNode<HBoxContainer>("Teammate2Stats");
            Teammate2HealthValue = teammate2Stats.GetNode<Label>("Teammate2HealthValue");
            Teammate2HealthBar = teammate2Stats.GetNode<ProgressBar>("Teammate2HealthBar");
            Teammate2ManaValue = teammate2Stats.GetNode<Label>("Teammate2ManaValue");
            Teammate2ManaBar = teammate2Stats.GetNode<ProgressBar>("Teammate2ManaBar");
            
            // 获取队友3元素
            var teammate3VBox = teammatesVBox.GetNode<VBoxContainer>("Teammate3");
            var teammate3Header = teammate3VBox.GetNode<HBoxContainer>("Teammate3Header");
            Teammate3Label = teammate3Header.GetNode<Label>("Teammate3Label");
            var teammate3Stats = teammate3VBox.GetNode<HBoxContainer>("Teammate3Stats");
            Teammate3HealthValue = teammate3Stats.GetNode<Label>("Teammate3HealthValue");
            Teammate3HealthBar = teammate3Stats.GetNode<ProgressBar>("Teammate3HealthBar");
            Teammate3ManaValue = teammate3Stats.GetNode<Label>("Teammate3ManaValue");
            Teammate3ManaBar = teammate3Stats.GetNode<ProgressBar>("Teammate3ManaBar");
            
            // 获取队友4元素
            var teammate4VBox = teammatesVBox.GetNode<VBoxContainer>("Teammate4");
            var teammate4Header = teammate4VBox.GetNode<HBoxContainer>("Teammate4Header");
            Teammate4Label = teammate4Header.GetNode<Label>("Teammate4Label");
            var teammate4Stats = teammate4VBox.GetNode<HBoxContainer>("Teammate4Stats");
            Teammate4HealthValue = teammate4Stats.GetNode<Label>("Teammate4HealthValue");
            Teammate4HealthBar = teammate4Stats.GetNode<ProgressBar>("Teammate4HealthBar");
            Teammate4ManaValue = teammate4Stats.GetNode<Label>("Teammate4ManaValue");
            Teammate4ManaBar = teammate4Stats.GetNode<ProgressBar>("Teammate4ManaBar");

            // 设置UI元素的初始状态
            SetHealthBarColor(HealthBar, 1.0f);
            SetManaBarColor(ManaBar, 1.0f);
            SetHealthBarColor(Teammate1HealthBar, 1.0f);
            SetManaBarColor(Teammate1ManaBar, 1.0f);
            SetHealthBarColor(Teammate2HealthBar, 1.0f);
            SetManaBarColor(Teammate2ManaBar, 1.0f);
            SetHealthBarColor(Teammate3HealthBar, 1.0f);
            SetManaBarColor(Teammate3ManaBar, 1.0f);
            SetHealthBarColor(Teammate4HealthBar, 1.0f);
            SetManaBarColor(Teammate4ManaBar, 1.0f);
        }

        /// <summary>
        /// 尝试从场景树中获取玩家实例
        /// </summary>
        /// <remarks>
        /// 简化查找过程，直接使用SaveManager中的玩家实例
        /// 如果玩家实例为null，会记录日志信息
        /// </remarks>
        private void FindPlayer()
        {
            // 尝试从场景树中获取玩家实例
            var root = GetTree().Root;
            _player = null;

            // 简化查找过程，直接使用SaveManager中的玩家实例
            if (SaveManager.Instance != null)
            {
                // 这里可以从SaveManager获取玩家实例
                // 或者使用其他方式获取玩家
                Log.Info("Using SaveManager to find player");
            }

            if (_player == null)
            {
                Log.Info("Player not found in scene tree");
            }
        }

        /// <summary>
        /// 初始化队友面板的默认状态
        /// </summary>
        /// <remarks>
        /// 设置队友面板的默认文本和生命值、魔法值，并默认隐藏所有队友面板
        /// </remarks>
        private void InitializeTeammatePanels()
        {
            // 初始化队友面板的默认状态
            Teammate1Label.Text = "Teammate 1";
            Teammate1HealthValue.Text = "100/100";
            Teammate1HealthBar.Value = 100;
            Teammate1ManaValue.Text = "100/100";
            Teammate1ManaBar.Value = 100;
            
            Teammate2Label.Text = "Teammate 2";
            Teammate2HealthValue.Text = "100/100";
            Teammate2HealthBar.Value = 100;
            Teammate2ManaValue.Text = "100/100";
            Teammate2ManaBar.Value = 100;
            
            Teammate3Label.Text = "Teammate 3";
            Teammate3HealthValue.Text = "100/100";
            Teammate3HealthBar.Value = 100;
            Teammate3ManaValue.Text = "100/100";
            Teammate3ManaBar.Value = 100;
            
            Teammate4Label.Text = "Teammate 4";
            Teammate4HealthValue.Text = "100/100";
            Teammate4HealthBar.Value = 100;
            Teammate4ManaValue.Text = "100/100";
            Teammate4ManaBar.Value = 100;

            // 默认隐藏所有队友面板
            Teammate1Label.Visible = false;
            Teammate1HealthValue.Visible = false;
            Teammate1HealthBar.Visible = false;
            Teammate1ManaValue.Visible = false;
            Teammate1ManaBar.Visible = false;
            
            Teammate2Label.Visible = false;
            Teammate2HealthValue.Visible = false;
            Teammate2HealthBar.Visible = false;
            Teammate2ManaValue.Visible = false;
            Teammate2ManaBar.Visible = false;
            
            Teammate3Label.Visible = false;
            Teammate3HealthValue.Visible = false;
            Teammate3HealthBar.Visible = false;
            Teammate3ManaValue.Visible = false;
            Teammate3ManaBar.Visible = false;
            
            Teammate4Label.Visible = false;
            Teammate4HealthValue.Visible = false;
            Teammate4HealthBar.Visible = false;
            Teammate4ManaValue.Visible = false;
            Teammate4ManaBar.Visible = false;
        }

        /// <summary>
        /// 更新玩家状态面板
        /// </summary>
        /// <remarks>
        /// 如果玩家实例不为null，更新玩家的生命值和魔法值显示
        /// 包括文本显示、进度条动画和颜色变化
        /// </remarks>
        private void UpdatePlayerStatus()
        {
            if (_player != null)
            {
                // 更新生命值
                float healthPercentage = _player.Health / _player.MaxHealth;
                HealthValue.Text = $"{_player.Health:F0}/{_player.MaxHealth:F0}";
                AnimateProgressBar(HealthBar, healthPercentage * 100);
                SetHealthBarColor(HealthBar, healthPercentage);

                // 更新魔法值
                float manaPercentage = _player.Mana / _player.MaxMana;
                ManaValue.Text = $"{_player.Mana:F0}/{_player.MaxMana:F0}";
                AnimateProgressBar(ManaBar, manaPercentage * 100);
                SetManaBarColor(ManaBar, manaPercentage);
            }
        }

        /// <summary>
        /// 更新队友状态面板
        /// </summary>
        /// <remarks>
        /// 根据实际的队友数据更新队友状态面板，包括生命值和魔法值
        /// 目前使用模拟数据，后续可以替换为实际的队友数据
        /// </remarks>
        private void UpdateTeammateStatus()
        {
            // 这里可以根据实际的队友数据更新队友状态面板
            // 目前使用模拟数据
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

        /// <summary>
        /// 更新指定索引的队友面板
        /// </summary>
        /// <param name="index">队友索引（0-3）</param>
        /// <param name="name">队友名称</param>
        /// <param name="healthPercentage">队友生命值百分比（0-1）</param>
        /// <param name="health">队友当前生命值</param>
        /// <param name="maxHealth">队友最大生命值</param>
        /// <param name="manaPercentage">队友魔法值百分比（0-1）</param>
        /// <param name="mana">队友当前魔法值</param>
        /// <param name="maxMana">队友最大魔法值</param>
        /// <remarks>
        /// 根据队友索引更新对应的队友面板，包括名称、生命值值、生命值进度条、
        /// 魔法值值、魔法值进度条的动画和颜色变化，并显示队友面板
        /// </remarks>
        private void UpdateTeammatePanel(int index, string name, float healthPercentage, float health, float maxHealth, float manaPercentage, float mana, float maxMana)
        {
            switch (index)
            {
                case 0:
                    Teammate1Label.Text = name;
                    Teammate1HealthValue.Text = $"{health:F0}/{maxHealth:F0}";
                    AnimateProgressBar(Teammate1HealthBar, healthPercentage * 100);
                    SetHealthBarColor(Teammate1HealthBar, healthPercentage);
                    Teammate1ManaValue.Text = $"{mana:F0}/{maxMana:F0}";
                    AnimateProgressBar(Teammate1ManaBar, manaPercentage * 100);
                    SetManaBarColor(Teammate1ManaBar, manaPercentage);
                    Teammate1Label.Visible = true;
                    Teammate1HealthValue.Visible = true;
                    Teammate1HealthBar.Visible = true;
                    Teammate1ManaValue.Visible = true;
                    Teammate1ManaBar.Visible = true;
                    break;
                case 1:
                    Teammate2Label.Text = name;
                    Teammate2HealthValue.Text = $"{health:F0}/{maxHealth:F0}";
                    AnimateProgressBar(Teammate2HealthBar, healthPercentage * 100);
                    SetHealthBarColor(Teammate2HealthBar, healthPercentage);
                    Teammate2ManaValue.Text = $"{mana:F0}/{maxMana:F0}";
                    AnimateProgressBar(Teammate2ManaBar, manaPercentage * 100);
                    SetManaBarColor(Teammate2ManaBar, manaPercentage);
                    Teammate2Label.Visible = true;
                    Teammate2HealthValue.Visible = true;
                    Teammate2HealthBar.Visible = true;
                    Teammate2ManaValue.Visible = true;
                    Teammate2ManaBar.Visible = true;
                    break;
                case 2:
                    Teammate3Label.Text = name;
                    Teammate3HealthValue.Text = $"{health:F0}/{maxHealth:F0}";
                    AnimateProgressBar(Teammate3HealthBar, healthPercentage * 100);
                    SetHealthBarColor(Teammate3HealthBar, healthPercentage);
                    Teammate3ManaValue.Text = $"{mana:F0}/{maxMana:F0}";
                    AnimateProgressBar(Teammate3ManaBar, manaPercentage * 100);
                    SetManaBarColor(Teammate3ManaBar, manaPercentage);
                    Teammate3Label.Visible = true;
                    Teammate3HealthValue.Visible = true;
                    Teammate3HealthBar.Visible = true;
                    Teammate3ManaValue.Visible = true;
                    Teammate3ManaBar.Visible = true;
                    break;
                case 3:
                    Teammate4Label.Text = name;
                    Teammate4HealthValue.Text = $"{health:F0}/{maxHealth:F0}";
                    AnimateProgressBar(Teammate4HealthBar, healthPercentage * 100);
                    SetHealthBarColor(Teammate4HealthBar, healthPercentage);
                    Teammate4ManaValue.Text = $"{mana:F0}/{maxMana:F0}";
                    AnimateProgressBar(Teammate4ManaBar, manaPercentage * 100);
                    SetManaBarColor(Teammate4ManaBar, manaPercentage);
                    Teammate4Label.Visible = true;
                    Teammate4HealthValue.Visible = true;
                    Teammate4HealthBar.Visible = true;
                    Teammate4ManaValue.Visible = true;
                    Teammate4ManaBar.Visible = true;
                    break;
            }
        }

        /// <summary>
        /// 为进度条添加动画效果
        /// </summary>
        /// <param name="progressBar">要动画的进度条</param>
        /// <param name="targetValue">目标值（0-100）</param>
        /// <remarks>
        /// 使用线性插值实现简单的进度条动画效果
        /// 当当前值与目标值的差异大于0.1时，使用插值平滑过渡
        /// 否则直接设置为目标值
        /// </remarks>
        private void AnimateProgressBar(ProgressBar progressBar, float targetValue)
        {
            // 简单的线性动画
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

        /// <summary>
        /// 根据生命值百分比设置进度条颜色
        /// </summary>
        /// <param name="progressBar">要设置颜色的进度条</param>
        /// <param name="healthPercentage">生命值百分比（0-1）</param>
        /// <remarks>
        /// 根据生命值百分比设置不同的颜色：
        /// - 生命值大于70%时显示绿色
        /// - 生命值大于30%时显示黄色
        /// - 生命值小于等于30%时显示红色
        /// </remarks>
        private void SetHealthBarColor(ProgressBar progressBar, float healthPercentage)
        {
            // 根据生命值百分比设置颜色
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

            // 设置进度条颜色
            progressBar.Modulate = color;
        }

        /// <summary>
        /// 根据魔法值百分比设置进度条颜色
        /// </summary>
        /// <param name="progressBar">要设置颜色的进度条</param>
        /// <param name="manaPercentage">魔法值百分比（0-1）</param>
        /// <remarks>
        /// 为魔法值进度条设置固定的蓝色
        /// </remarks>
        private void SetManaBarColor(ProgressBar progressBar, float manaPercentage)
        {
            // 根据魔法值百分比设置颜色
            Color color = new Color(0.2f, 0.4f, 1.0f, 1.0f);
            progressBar.Modulate = color;
        }

        /// <summary>
        /// 添加队友到UI
        /// </summary>
        /// <param name="teammate">队友实例</param>
        /// <remarks>
        /// 找到第一个空的队友位置，添加队友实例并更新队友面板，包括生命值和魔法值
        /// </remarks>
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

        /// <summary>
        /// 从UI中移除队友
        /// </summary>
        /// <param name="teammate">要移除的队友实例</param>
        /// <remarks>
        /// 找到对应的队友位置，设置为null并隐藏队友面板
        /// </remarks>
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

        /// <summary>
        /// 隐藏指定索引的队友面板
        /// </summary>
        /// <param name="index">队友索引（0-3）</param>
        /// <remarks>
        /// 根据队友索引隐藏对应的队友面板、标签、生命值值显示和魔法值值显示
        /// </remarks>
        private void HideTeammatePanel(int index)
        {
            switch (index)
            {
                case 0:
                    Teammate1Label.Visible = false;
                    Teammate1HealthValue.Visible = false;
                    Teammate1HealthBar.Visible = false;
                    Teammate1ManaValue.Visible = false;
                    Teammate1ManaBar.Visible = false;
                    break;
                case 1:
                    Teammate2Label.Visible = false;
                    Teammate2HealthValue.Visible = false;
                    Teammate2HealthBar.Visible = false;
                    Teammate2ManaValue.Visible = false;
                    Teammate2ManaBar.Visible = false;
                    break;
                case 2:
                    Teammate3Label.Visible = false;
                    Teammate3HealthValue.Visible = false;
                    Teammate3HealthBar.Visible = false;
                    Teammate3ManaValue.Visible = false;
                    Teammate3ManaBar.Visible = false;
                    break;
                case 3:
                    Teammate4Label.Visible = false;
                    Teammate4HealthValue.Visible = false;
                    Teammate4HealthBar.Visible = false;
                    Teammate4ManaValue.Visible = false;
                    Teammate4ManaBar.Visible = false;
                    break;
            }
        }

        /// <summary>
        /// 设置玩家引用
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <remarks>
        /// 用于外部设置玩家实例引用，确保UI能够正确显示玩家状态
        /// </remarks>
        public void SetPlayer(Player player)
        {
            _player = player;
        }
    }
}
