/*
 * File: BattleUI.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 战斗界面HUD，显示敌我双方状态、回合顺序、动作按钮、技能/道具子菜单和战斗消息。
 * 所有UI均为程序化构建，通过BattleManager信号驱动刷新。
 *
 * Key Features:
 * - 敌我双方角色卡片（HP/MP进度条、名称、等级）
 * - 回合顺序指示器
 * - 动作面板：攻击/技能/道具/防御/跳过
 * - 技能和道具子菜单（动态填充）
 * - 目标选择模式（点击敌人卡片选择目标）
 * - 战斗消息日志
 * - 胜利/失败结算画面
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Modules.Battle;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.UI
{
    /// <summary>
    /// 战斗界面HUD
    /// </summary>
    public partial class BattleUI : CanvasLayer
    {
        #region Colors

        private static readonly Color ColorBg = new(0, 0, 0, 0.85f);
        private static readonly Color ColorEnemyCard = new(0.25f, 0.1f, 0.1f, 0.9f);
        private static readonly Color ColorPlayerCard = new(0.1f, 0.15f, 0.25f, 0.9f);
        private static readonly Color ColorHpBar = new(0.9f, 0.25f, 0.25f, 1f);
        private static readonly Color ColorMpBar = new(0.3f, 0.5f, 0.95f, 1f);
        private static readonly Color ColorActive = new(1f, 0.85f, 0.2f, 1f);
        private static readonly Color ColorTextPrimary = new(0.93f, 0.94f, 0.95f, 1f);
        private static readonly Color ColorTextSecondary = new(0.6f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorPrimary = new(0.2f, 0.6f, 0.86f, 1f);
        private static readonly Color ColorDanger = new(0.9f, 0.25f, 0.25f, 1f);

        #endregion

        #region Font

        private static Font _font;
        private static Font GetFont() => _font ??= GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf");

        #endregion

        #region State

        // Creature lists
        private List<Creature> _players = new();
        private List<Creature> _enemies = new();
        private Creature _activeCreature;

        // UI containers
        private Control _mainContainer;
        private HBoxContainer _enemyRow;
        private HBoxContainer _playerRow;
        private HBoxContainer _turnOrderBar;
        private Control _actionPanel;
        private Label _messageLabel;

        // Sub-menus
        private Control _skillSubMenu;
        private VBoxContainer _skillListContainer;
        private Control _itemSubMenu;
        private VBoxContainer _itemListContainer;

        // Result overlay
        private Control _resultOverlay;
        private Label _resultTitleLabel;
        private Label _resultGoldLabel;
        private Label _resultExpLabel;
        private Label _resultJpLabel;
        private VBoxContainer _resultItemsList;
        private Label _resultItemsHeader;
        private Label _resultLevelUpLabel;
        private Button _resultContinueBtn;

        /// <summary>
        /// 结算画面点击继续后的回调
        /// </summary>
        public Action OnContinuePressed { get; set; }

        // Action buttons
        private Button _attackBtn;
        private Button _skillBtn;
        private Button _itemsBtn;
        private Button _defendBtn;
        private Button _passBtn;

        // Target selection
        private bool _targetSelectMode;
        private Skill _pendingSkill;
        private List<PanelContainer> _enemyCardPanels = new();

        #endregion

        #region Font (re-declared as instance for helpers)

        Font Font => GetFont();

        #endregion

        public override void _Ready()
        {
            BuildMainHUD();
            BuildSkillSubMenu();
            BuildItemSubMenu();
            BuildResultOverlay();

            SetActionsEnabled(false);

            Log.Info("BattleUI initialized");
        }

        #region Build Main HUD

        private void BuildMainHUD()
        {
            // Full-screen background
            var bg = new ColorRect
            {
                Color = ColorBg,
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1
            };
            AddChild(bg);

            _mainContainer = new MarginContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1
            };
            _mainContainer.AddThemeConstantOverride("margin_left", 20);
            _mainContainer.AddThemeConstantOverride("margin_top", 20);
            _mainContainer.AddThemeConstantOverride("margin_right", 20);
            _mainContainer.AddThemeConstantOverride("margin_bottom", 20);
            AddChild(_mainContainer);

            var rootVbox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            _mainContainer.AddChild(rootVbox);

            // Enemy row
            _enemyRow = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rootVbox.AddChild(_enemyRow);

            rootVbox.AddChild(new Control { SizeFlagsVertical = Control.SizeFlags.ExpandFill });

            // Turn order bar
            var turnLabel = MakeLabel("Turn Order:", 14, ColorTextSecondary);
            turnLabel.HorizontalAlignment = HorizontalAlignment.Center;
            rootVbox.AddChild(turnLabel);

            _turnOrderBar = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rootVbox.AddChild(_turnOrderBar);

            rootVbox.AddChild(new Control { SizeFlagsVertical = Control.SizeFlags.ExpandFill });

            // Player row
            _playerRow = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rootVbox.AddChild(_playerRow);

            // Bottom bar: message + action panel
            var bottomHbox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            rootVbox.AddChild(bottomHbox);

            _messageLabel = MakeLabel("", 14, ColorTextPrimary);
            _messageLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _messageLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            bottomHbox.AddChild(_messageLabel);

            // Action panel (right side)
            _actionPanel = new VBoxContainer { CustomMinimumSize = new Vector2(160, 0) };
            bottomHbox.AddChild(_actionPanel);

            _attackBtn = MakeActionButton("Attack", () => EnterTargetSelectMode(null));
            _skillBtn = MakeActionButton("Skill", OpenSkillMenu);
            _itemsBtn = MakeActionButton("Items", OpenItemMenu);
            _defendBtn = MakeActionButton("Defend", OnDefend);
            _passBtn = MakeActionButton("Pass", OnPass);

            _actionPanel.AddChild(_attackBtn);
            _actionPanel.AddChild(_skillBtn);
            _actionPanel.AddChild(_itemsBtn);
            _actionPanel.AddChild(_defendBtn);
            _actionPanel.AddChild(_passBtn);
        }

        #endregion

        #region Build Sub-Menus

        private void BuildSkillSubMenu()
        {
            _skillSubMenu = new PanelContainer
            {
                Visible = false,
                LayoutMode = 1,
                AnchorLeft = 0.3f, AnchorTop = 0.15f,
                AnchorRight = 0.7f, AnchorBottom = 0.85f
            };
            _skillSubMenu.AddThemeStyleboxOverride("panel", MakePanelStyle(new Color(0.13f, 0.19f, 0.25f, 0.98f)));
            AddChild(_skillSubMenu);

            var vbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.05f, AnchorTop = 0.05f,
                AnchorRight = 0.95f, AnchorBottom = 0.95f
            };
            _skillSubMenu.AddChild(vbox);

            var title = MakeLabel("Select Skill", 20, ColorTextPrimary);
            title.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(title);
            vbox.AddChild(new HSeparator());

            var scroll = new ScrollContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            vbox.AddChild(scroll);

            _skillListContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            scroll.AddChild(_skillListContainer);

            var closeBtn = MakeSmallButton("Cancel");
            closeBtn.Pressed += CloseSkillMenu;
            vbox.AddChild(closeBtn);
        }

        private void BuildItemSubMenu()
        {
            _itemSubMenu = new PanelContainer
            {
                Visible = false,
                LayoutMode = 1,
                AnchorLeft = 0.3f, AnchorTop = 0.15f,
                AnchorRight = 0.7f, AnchorBottom = 0.85f
            };
            _itemSubMenu.AddThemeStyleboxOverride("panel", MakePanelStyle(new Color(0.13f, 0.19f, 0.25f, 0.98f)));
            AddChild(_itemSubMenu);

            var vbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.05f, AnchorTop = 0.05f,
                AnchorRight = 0.95f, AnchorBottom = 0.95f
            };
            _itemSubMenu.AddChild(vbox);

            var title = MakeLabel("Use Item", 20, ColorTextPrimary);
            title.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(title);
            vbox.AddChild(new HSeparator());

            var scroll = new ScrollContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            vbox.AddChild(scroll);

            _itemListContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            scroll.AddChild(_itemListContainer);

            var closeBtn = MakeSmallButton("Cancel");
            closeBtn.Pressed += CloseItemMenu;
            vbox.AddChild(closeBtn);
        }

        private void BuildResultOverlay()
        {
            _resultOverlay = new Control
            {
                Visible = false,
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            AddChild(_resultOverlay);

            var bg = new ColorRect
            {
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1,
                Color = new Color(0, 0, 0, 0.8f)
            };
            _resultOverlay.AddChild(bg);

            // Center panel
            var panel = new PanelContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.25f, AnchorTop = 0.18f,
                AnchorRight = 0.75f, AnchorBottom = 0.82f
            };
            panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.12f, 0.18f, 0.98f),
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.5f, 0.7f, 0.6f),
                CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12,
                CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
                ContentMarginLeft = 24, ContentMarginTop = 20,
                ContentMarginRight = 24, ContentMarginBottom = 20
            });
            _resultOverlay.AddChild(panel);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            panel.AddChild(vbox);

            // Title
            _resultTitleLabel = MakeLabel("", 34, ColorTextPrimary);
            _resultTitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(_resultTitleLabel);

            vbox.AddChild(new HSeparator());
            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 12) });

            // Rewards card
            var rewardsCard = new PanelContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rewardsCard.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(0.12f, 0.16f, 0.22f, 0.95f),
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 16, ContentMarginTop = 12,
                ContentMarginRight = 16, ContentMarginBottom = 12
            });
            vbox.AddChild(rewardsCard);

            var rewardsVbox = new VBoxContainer();
            rewardsCard.AddChild(rewardsVbox);

            _resultGoldLabel = MakeLabel("", 15, new Color(1f, 0.85f, 0.2f, 1f));
            rewardsVbox.AddChild(_resultGoldLabel);

            _resultExpLabel = MakeLabel("", 15, new Color(0.5f, 0.85f, 1f, 1f));
            rewardsVbox.AddChild(_resultExpLabel);

            _resultJpLabel = MakeLabel("", 15, new Color(0.6f, 0.4f, 1f, 1f));
            rewardsVbox.AddChild(_resultJpLabel);

            rewardsVbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });
            _resultItemsHeader = MakeLabel("Items Found:", 14, ColorTextSecondary);
            rewardsVbox.AddChild(_resultItemsHeader);

            _resultItemsList = new VBoxContainer();
            rewardsVbox.AddChild(_resultItemsList);

            rewardsVbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });
            _resultLevelUpLabel = MakeLabel("", 15, new Color(0.25f, 0.85f, 0.3f, 1f));
            rewardsVbox.AddChild(_resultLevelUpLabel);

            vbox.AddChild(new Control { SizeFlagsVertical = Control.SizeFlags.ExpandFill });

            // Continue button
            _resultContinueBtn = MakeActionButton("Continue", () =>
            {
                _resultOverlay.Visible = false;
                OnContinuePressed?.Invoke();
            });
            _resultContinueBtn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            vbox.AddChild(_resultContinueBtn);
        }

        #endregion

        #region Public API

        public void SetupBattle(List<Creature> players, List<Creature> enemies)
        {
            _players = players;
            _enemies = enemies;

            // Clear existing cards
            foreach (var child in _enemyRow.GetChildren()) child.QueueFree();
            foreach (var child in _playerRow.GetChildren()) child.QueueFree();
            _enemyCardPanels.Clear();

            // Build enemy cards
            foreach (var enemy in enemies)
            {
                var card = MakeCombatantCard(enemy, true);
                _enemyRow.AddChild(card);
                _enemyCardPanels.Add(card);
            }

            // Build player cards
            foreach (var player in players)
            {
                var card = MakeCombatantCard(player, false);
                _playerRow.AddChild(card);
            }

            // Build turn order
            RefreshTurnOrder();

            Log.Info($"BattleUI setup complete: {players.Count} players, {enemies.Count} enemies");
        }

        public void RefreshHUD()
        {
            // Refresh enemy cards
            int i = 0;
            foreach (var enemy in _enemies)
            {
                if (i < _enemyCardPanels.Count && enemy.IsAlive)
                    RefreshCombatantCard(_enemyCardPanels[i], enemy, true);
                i++;
            }

            // Refresh player cards
            i = 0;
            foreach (var player in _players)
            {
                if (i < _playerRow.GetChildCount())
                {
                    var card = _playerRow.GetChild(i) as PanelContainer;
                    if (card != null) RefreshCombatantCard(card, player, false);
                }
                i++;
            }

            RefreshTurnOrder();
        }

        public void HighlightActiveCreature(Creature creature)
        {
            _activeCreature = creature;

            // Highlight enemy cards
            for (int i = 0; i < _enemies.Count && i < _enemyCardPanels.Count; i++)
            {
                var borderColor = _enemies[i] == creature ? ColorActive : new Color(0.3f, 0.1f, 0.1f, 0.6f);
                if (_enemyCardPanels[i].GetThemeStylebox("panel") is StyleBoxFlat sb)
                    sb.BorderColor = borderColor;
            }

            // Highlight player cards
            for (int i = 0; i < _players.Count && i < _playerRow.GetChildCount(); i++)
            {
                if (_playerRow.GetChild(i) is PanelContainer card && card.GetThemeStylebox("panel") is StyleBoxFlat sb)
                    sb.BorderColor = _players[i] == creature ? ColorActive : new Color(0.15f, 0.2f, 0.35f, 0.6f);
            }

            // Enable target selection for enemies during player turn
            if (_activeCreature is Player && _targetSelectMode)
            {
                foreach (var card in _enemyCardPanels)
                {
                    if (card.GetThemeStylebox("panel") is StyleBoxFlat sb)
                        sb.BorderColor = ColorPrimary;
                }
            }
        }

        public void SetActionsEnabled(bool enabled)
        {
            _attackBtn.Disabled = !enabled;
            _skillBtn.Disabled = !enabled;
            _itemsBtn.Disabled = !enabled;
            _defendBtn.Disabled = !enabled;
            _passBtn.Disabled = !enabled;
        }

        public void ShowMessage(string text)
        {
            _messageLabel.Text = text;
        }

        public void ShowResult(BattleRewards rewards)
        {
            _resultOverlay.Visible = true;
            bool victory = rewards != null && rewards.Victory;

            _resultTitleLabel.Text = victory ? "VICTORY!" : "DEFEAT...";
            _resultTitleLabel.AddThemeColorOverride("font_color", victory ? new Color(1f, 0.85f, 0.2f, 1f) : ColorDanger);

            if (victory && rewards != null)
            {
                _resultGoldLabel.Text = $"Gold:       {rewards.TotalGold} G";
                _resultGoldLabel.Visible = true;

                _resultExpLabel.Text = $"Experience: {rewards.TotalExperience} XP";
                _resultExpLabel.Visible = true;

                _resultJpLabel.Text = $"JP:          {rewards.TotalJP} JP";
                _resultJpLabel.Visible = true;

                // Populate dropped items
                foreach (var child in _resultItemsList.GetChildren())
                    child.QueueFree();
                if (rewards.DroppedItems.Count > 0)
                {
                    foreach (var itemId in rewards.DroppedItems)
                    {
                        var itemLabel = MakeLabel($"  • {itemId}", 13, ColorTextPrimary);
                        _resultItemsList.AddChild(itemLabel);
                    }
                    _resultItemsList.Visible = true;
                    _resultItemsHeader.Visible = true;
                }
                else
                {
                    _resultItemsList.Visible = false;
                    _resultItemsHeader.Visible = false;
                }

                // Level up display
                if (rewards.LevelUps.Count > 0)
                {
                    _resultLevelUpLabel.Text = "Level Up: " + string.Join(", ", rewards.LevelUps) + "!";
                    _resultLevelUpLabel.Visible = true;
                }
                else
                {
                    _resultLevelUpLabel.Visible = false;
                }
            }
            else
            {
                _resultGoldLabel.Visible = false;
                _resultExpLabel.Visible = false;
                _resultJpLabel.Visible = false;
                _resultItemsList.Visible = false;
                _resultLevelUpLabel.Text = "All party members have fallen.";
                _resultLevelUpLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
                _resultLevelUpLabel.Visible = true;
            }
        }

        #endregion

        #region Card Building

        private PanelContainer MakeCombatantCard(Creature creature, bool isEnemy)
        {
            var panel = new PanelContainer
            {
                CustomMinimumSize = new Vector2(180, isEnemy ? 80 : 100),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
            };
            var cardColor = isEnemy ? ColorEnemyCard : ColorPlayerCard;
            panel.AddThemeStyleboxOverride("panel", MakePanelStyle(cardColor));

            var vbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.03f, AnchorTop = 0.03f,
                AnchorRight = 0.97f, AnchorBottom = 0.97f
            };
            panel.AddChild(vbox);

            // Name
            var nameLabel = MakeLabel(creature.CreatureName ?? "???", 14, ColorTextPrimary);
            vbox.AddChild(nameLabel);

            // HP bar
            var hpBar = MakeProgressBar(ColorHpBar);
            hpBar.Value = creature.Health;
            hpBar.MaxValue = Mathf.Max(creature.MaxHealth, 1);
            hpBar.CustomMinimumSize = new Vector2(0, 14);
            vbox.AddChild(hpBar);

            // MP bar (players only)
            if (!isEnemy && creature is Player player)
            {
                var mpBar = MakeProgressBar(ColorMpBar);
                mpBar.Value = player.CurrentMana;
                mpBar.MaxValue = Mathf.Max(player.MaxMana, 1);
                mpBar.CustomMinimumSize = new Vector2(0, 10);
                vbox.AddChild(mpBar);
            }

            // Enemy click handler for target selection
            if (isEnemy)
            {
                var btn = new Button { Text = "" };
                btn.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
                btn.Flat = true;
                btn.MouseFilter = Control.MouseFilterEnum.Stop;
                var captured = creature;
                btn.Pressed += () => OnEnemyClicked(captured);
                panel.AddChild(btn);
                btn.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            }

            return panel;
        }

        private void RefreshCombatantCard(PanelContainer card, Creature creature, bool isEnemy)
        {
            var vbox = card.GetChild(0) as VBoxContainer;
            if (vbox == null || vbox.GetChildCount() < 2) return;

            // Update name with HP info
            var nameLabel = vbox.GetChild(0) as Label;
            if (nameLabel != null)
                nameLabel.Text = $"{creature.CreatureName}  Lv.{creature.Level}";

            // Update HP bar
            var hpBar = vbox.GetChild(1) as ProgressBar;
            if (hpBar != null)
            {
                hpBar.Value = creature.Health;
                hpBar.MaxValue = Mathf.Max(creature.MaxHealth, 1);
            }

            // Update MP bar (players only)
            if (!isEnemy && creature is Player player && vbox.GetChildCount() >= 3)
            {
                var mpBar = vbox.GetChild(2) as ProgressBar;
                if (mpBar != null)
                {
                    mpBar.Value = player.CurrentMana;
                    mpBar.MaxValue = Mathf.Max(player.MaxMana, 1);
                }
            }

            // Gray out dead creatures
            card.Modulate = creature.IsAlive ? Colors.White : new Color(0.3f, 0.3f, 0.3f, 0.6f);
        }

        #endregion

        #region Turn Order

        private void RefreshTurnOrder()
        {
            foreach (var child in _turnOrderBar.GetChildren())
                child.QueueFree();

            var allAlive = _players.Concat<Creature>(_enemies)
                .Where(c => c.IsAlive)
                .OrderByDescending(c => c.Speed)
                .ToList();

            for (int i = 0; i < allAlive.Count; i++)
            {
                var c = allAlive[i];
                var label = MakeLabel(c.CreatureName ?? "?", 13,
                    c == _activeCreature ? ColorActive :
                    c is Player ? ColorMpBar : ColorHpBar);
                label.CustomMinimumSize = new Vector2(80, 0);
                _turnOrderBar.AddChild(label);

                if (i < allAlive.Count - 1)
                {
                    var arrow = MakeLabel("→", 13, ColorTextSecondary);
                    _turnOrderBar.AddChild(arrow);
                }
            }
        }

        #endregion

        #region Target Selection

        private void EnterTargetSelectMode(Skill skill)
        {
            _targetSelectMode = true;
            _pendingSkill = skill;
            ShowMessage(skill != null ? $"Select target for {skill.SkillName}" : "Select target to attack");

            foreach (var card in _enemyCardPanels)
            {
                if (card.GetThemeStylebox("panel") is StyleBoxFlat sb)
                    sb.BorderColor = ColorPrimary;
            }
        }

        private void ExitTargetSelectMode()
        {
            _targetSelectMode = false;
            _pendingSkill = null;

            if (_activeCreature != null)
                HighlightActiveCreature(_activeCreature);
        }

        private void OnEnemyClicked(Creature enemy)
        {
            if (!_targetSelectMode) return;
            if (BattleManager.Instance == null) return;
            if (BattleManager.Instance.CurrentState != BattleManager.BattleState.PlayerTurn) return;

            ExitTargetSelectMode();

            if (_pendingSkill != null)
            {
                BattleManager.Instance.PlayerAction_Skill(_pendingSkill, enemy);
                ShowMessage($"{_activeCreature?.CreatureName} uses {_pendingSkill.SkillName} on {enemy.CreatureName}!");
            }
            else
            {
                BattleManager.Instance.PlayerAction_Attack(enemy);
                ShowMessage($"{_activeCreature?.CreatureName} attacks {enemy.CreatureName}!");
            }

            RefreshHUD();
        }

        #endregion

        #region Action Handlers

        private void OnDefend()
        {
            if (BattleManager.Instance == null) return;
            if (BattleManager.Instance.CurrentState != BattleManager.BattleState.PlayerTurn) return;

            if (_activeCreature != null)
                ShowMessage($"{_activeCreature.CreatureName} defends!");

            BattleManager.Instance.EndTurn();
        }

        private void OnPass()
        {
            if (BattleManager.Instance == null) return;
            if (BattleManager.Instance.CurrentState != BattleManager.BattleState.PlayerTurn) return;

            if (_activeCreature != null)
                ShowMessage($"{_activeCreature.CreatureName} passes...");

            BattleManager.Instance.EndTurn();
        }

        #endregion

        #region Skill Sub-Menu

        private void OpenSkillMenu()
        {
            if (BattleManager.Instance?.CurrentState != BattleManager.BattleState.PlayerTurn) return;

            // Clear and populate skill list
            foreach (var child in _skillListContainer.GetChildren())
                child.QueueFree();

            if (_activeCreature is Player player)
            {
                // Show ultimate skill first (if available)
                if (player.UltimateSkill != null)
                {
                    var ult = player.UltimateSkill;
                    var ultBtn = MakeSubMenuButton($"★ {ult.SkillName}  ({ult.ManaCost} MP)");
                    ultBtn.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f, 1f));
                    ultBtn.Disabled = player.CurrentMana < ult.ManaCost || !ult.IsUnlocked;
                    var capturedUlt = ult;
                    ultBtn.Pressed += () =>
                    {
                        CloseSkillMenu();
                        EnterTargetSelectMode(capturedUlt);
                    };
                    _skillListContainer.AddChild(ultBtn);

                    // separator
                    var sep = new HSeparator();
                    _skillListContainer.AddChild(sep);
                }

                // Show equipped active skills (up to 4)
                var equippedSkills = player.GetEquippedActiveSkills();
                if (equippedSkills.Count > 0)
                {
                    foreach (var skill in equippedSkills)
                    {
                        var btn = MakeSubMenuButton($"{skill.SkillName}  ({skill.ManaCost} MP)");
                        btn.Disabled = player.CurrentMana < skill.ManaCost || !skill.IsUnlocked;
                        var captured = skill;
                        btn.Pressed += () =>
                        {
                            CloseSkillMenu();
                            EnterTargetSelectMode(captured);
                        };
                        _skillListContainer.AddChild(btn);
                    }
                }
                else
                {
                    _skillListContainer.AddChild(MakeLabel("No skills equipped.", 14, ColorTextSecondary));
                }
            }

            _skillSubMenu.Visible = true;
        }

        private void CloseSkillMenu()
        {
            _skillSubMenu.Visible = false;
        }

        #endregion

        #region Item Sub-Menu

        private void OpenItemMenu()
        {
            if (BattleManager.Instance?.CurrentState != BattleManager.BattleState.PlayerTurn) return;

            foreach (var child in _itemListContainer.GetChildren())
                child.QueueFree();

            var player = _activeCreature as Player;
            if (player == null) return;

            var allItems = InventoryManager.Instance?.GetAllItems() ?? new List<(string, Item, int)>();
            var items = allItems.Where(x => x.Item.IsUsable && x.Item.ItemTypeValue == Item.ItemType.Consumable).ToList();

            if (items.Count == 0)
            {
                _itemListContainer.AddChild(MakeLabel("No usable items.", 14, ColorTextSecondary));
            }

            foreach (var (itemId, item, qty) in items)
            {
                var btn = MakeSubMenuButton($"{item.ItemName}  x{qty}");
                var captured = item;
                var capturedId = itemId;
                btn.Pressed += () =>
                {
                    CloseItemMenu();
                    item.Use(player);
                    InventoryManager.Instance?.RemoveItem(capturedId, 1);
                    ShowMessage($"Used {item.ItemName}!");
                    RefreshHUD();
                };
                _itemListContainer.AddChild(btn);
            }

            _itemSubMenu.Visible = true;
        }

        private void CloseItemMenu()
        {
            _itemSubMenu.Visible = false;
        }

        #endregion

        #region Input Handling

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;

            if (@event.IsActionPressed("ui_cancel"))
            {
                if (_skillSubMenu.Visible)
                {
                    CloseSkillMenu();
                    GetViewport().SetInputAsHandled();
                }
                else if (_itemSubMenu.Visible)
                {
                    CloseItemMenu();
                    GetViewport().SetInputAsHandled();
                }
                else if (_targetSelectMode)
                {
                    ExitTargetSelectMode();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        #endregion

        #region UI Helpers

        private static Label MakeLabel(string text, int fontSize, Color color)
        {
            var label = new Label { Text = text };
            label.AddThemeFontOverride("font", GetFont());
            label.AddThemeFontSizeOverride("font_size", fontSize);
            label.AddThemeColorOverride("font_color", color);
            return label;
        }

        private Button MakeActionButton(string text, Action onPressed)
        {
            var btn = new Button { Text = text, CustomMinimumSize = new Vector2(160, 44) };
            ApplyButtonStyle(btn);
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 15);
            btn.Pressed += () => onPressed();
            return btn;
        }

        private Button MakeSubMenuButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                CustomMinimumSize = new Vector2(0, 40),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            ApplyButtonStyle(btn);
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 13);
            return btn;
        }

        private Button MakeSmallButton(string text)
        {
            var btn = new Button { Text = text, CustomMinimumSize = new Vector2(120, 36) };
            ApplyButtonStyle(btn);
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 13);
            return btn;
        }

        private static void ApplyButtonStyle(Button btn)
        {
            var normal = new StyleBoxFlat
            {
                BgColor = new Color(0.13f, 0.19f, 0.25f, 0.95f),
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.5f),
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 12, ContentMarginTop = 6,
                ContentMarginRight = 12, ContentMarginBottom = 6
            };
            btn.AddThemeStyleboxOverride("normal", normal);

            var hover = (StyleBoxFlat)normal.Duplicate();
            hover.BgColor = new Color(0.2f, 0.28f, 0.36f, 0.95f);
            hover.BorderColor = new Color(0.4f, 0.55f, 0.7f, 0.7f);
            btn.AddThemeStyleboxOverride("hover", hover);

            var pressed = (StyleBoxFlat)normal.Duplicate();
            pressed.BgColor = new Color(0.1f, 0.15f, 0.2f, 0.95f);
            btn.AddThemeStyleboxOverride("pressed", pressed);

            var disabled = (StyleBoxFlat)normal.Duplicate();
            disabled.BgColor = new Color(0.08f, 0.08f, 0.08f, 0.6f);
            btn.AddThemeStyleboxOverride("disabled", disabled);
        }

        private static StyleBoxFlat MakePanelStyle(Color bgColor)
        {
            return new StyleBoxFlat
            {
                BgColor = bgColor,
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.5f),
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 8, ContentMarginTop = 6,
                ContentMarginRight = 8, ContentMarginBottom = 6
            };
        }

        private static ProgressBar MakeProgressBar(Color fillColor)
        {
            var bar = new ProgressBar { Value = 100, MaxValue = 100, ShowPercentage = false };
            var bgStyle = new StyleBoxFlat
            {
                BgColor = fillColor * new Color(0.15f, 0.15f, 0.15f, 1f),
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4
            };
            bar.AddThemeStyleboxOverride("background", bgStyle);
            var fillStyle = new StyleBoxFlat
            {
                BgColor = fillColor,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4
            };
            bar.AddThemeStyleboxOverride("fill", fillStyle);
            return bar;
        }

        #endregion
    }
}
