/*
 * File: PopupMenu.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 游戏内弹出菜单控制器，管理六大功能面板的导航和显示。
 * 每个面板均为独立的组件类，PopupMenu仅负责面板的创建、切换和输入处理。
 *
 * Key Features:
 * - 主菜单导航（6个功能按钮 + 关闭按钮）
 * - 面板生命周期管理（创建、显示、隐藏）
 * - Escape键输入处理（逐级返回）
 * - SaveSlotSelector集成
 */

using Godot;
using System;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
    /// <summary>
    /// 弹出菜单控制器，管理所有功能面板的导航
    /// </summary>
    public partial class PopupMenu : Control
    {
        private Control _mainMenuPanel;
        private PopupInventoryPanel _inventoryPanel;
        private PopupMapPanel _mapPanel;
        private PopupCharacterPanel _characterPanel;
        private PopupSkillsPanel _skillsPanel;
        private PopupSettingsPanel _settingsPanel;
        private PopupSavePanel _savePanel;
        private SaveSlotSelector _saveSlotSelector;

        #region Colors (for main menu only)

        private static readonly Color ColorBg = new(0, 0, 0, 0.9f);
        private static readonly Color ColorSurface = new(0.13f, 0.19f, 0.25f, 0.95f);
        private static readonly Color ColorPrimary = new(0.2f, 0.6f, 0.86f, 1f);
        private static readonly Color ColorTextPrimary = new(0.93f, 0.94f, 0.95f, 1f);

        #endregion

        public override void _Ready()
        {
            CreateMainMenu();
            CreatePanels();
            InitializeSaveSlotSelector();
            ShowMainMenu();
            Log.Info("PopupMenu initialized with 6 panels");
        }

        #region Main Menu

        private void CreateMainMenu()
        {
            _mainMenuPanel = new Control
            {
                Name = "MainMenuPanel",
                LayoutMode = 1,
                AnchorLeft = 0.25f, AnchorTop = 0.15f,
                AnchorRight = 0.75f, AnchorBottom = 0.85f
            };
            AddChild(_mainMenuPanel);

            var bg = new ColorRect
            {
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1,
                Color = ColorBg
            };
            _mainMenuPanel.AddChild(bg);

            var vbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.05f, AnchorTop = 0.05f,
                AnchorRight = 0.95f, AnchorBottom = 0.95f,
                Alignment = BoxContainer.AlignmentMode.Center
            };
            _mainMenuPanel.AddChild(vbox);

            var title = MakeTitle("Menu");
            vbox.AddChild(title);
            vbox.AddChild(new HSeparator());

            var grid = new GridContainer { Columns = 2 };
            grid.AddThemeConstantOverride("h_separation", 16);
            grid.AddThemeConstantOverride("v_separation", 12);

            var buttons = new (string, string, Action)[]
            {
                ("Items", "📦", () => ShowPanel(_inventoryPanel)),
                ("Map", "🗺️", () => ShowPanel(_mapPanel)),
                ("Characters", "👤", () => ShowPanel(_characterPanel)),
                ("Skills", "⚔️", () => ShowPanel(_skillsPanel)),
                ("Settings", "⚙️", () => ShowPanel(_settingsPanel)),
                ("Save/Load", "💾", () => ShowPanel(_savePanel)),
            };

            foreach (var (text, icon, action) in buttons)
            {
                var btn = MakeMenuButton($"{icon}  {text}");
                btn.Pressed += () => action();
                grid.AddChild(btn);
            }

            vbox.AddChild(grid);

            var spacer = new Control { CustomMinimumSize = new Vector2(0, 20) };
            vbox.AddChild(spacer);

            var closeBtn = MakeMenuButton("Close Menu");
            closeBtn.Pressed += ClosePopup;
            vbox.AddChild(closeBtn);
        }

        #endregion

        #region Panel Creation

        private void CreatePanels()
        {
            _inventoryPanel = new PopupInventoryPanel { OnBackPressed = ShowMainMenu };
            AddChild(_inventoryPanel);

            _mapPanel = new PopupMapPanel { OnBackPressed = ShowMainMenu };
            AddChild(_mapPanel);

            _characterPanel = new PopupCharacterPanel { OnBackPressed = ShowMainMenu };
            AddChild(_characterPanel);

            _skillsPanel = new PopupSkillsPanel { OnBackPressed = ShowMainMenu };
            AddChild(_skillsPanel);

            _settingsPanel = new PopupSettingsPanel { OnBackPressed = ShowMainMenu };
            AddChild(_settingsPanel);

            _savePanel = new PopupSavePanel
            {
                OnBackPressed = ShowMainMenu,
                OnShowSlotSelector = () => _saveSlotSelector?.ShowSelector(),
                OnHideSlotSelector = () => _saveSlotSelector?.HideSelector()
            };
            AddChild(_savePanel);
        }

        #endregion

        #region Save Slot Selector

        private void InitializeSaveSlotSelector()
        {
            _saveSlotSelector = new SaveSlotSelector
            {
                Name = "SaveSlotSelector",
                Visible = false,
                LayoutMode = 1,
                AnchorLeft = 0f, AnchorTop = 0f,
                AnchorRight = 1f, AnchorBottom = 1f
            };
            AddChild(_saveSlotSelector);

            _savePanel.SetSlotSelector(_saveSlotSelector);
        }

        #endregion

        #region Panel Navigation

        public void ShowMainMenu()
        {
            HideAllPanels();
            _mainMenuPanel.Show();
            _saveSlotSelector?.HideSelector();
        }

        private void HideAllPanels()
        {
            _mainMenuPanel?.Hide();
            _inventoryPanel?.Hide();
            _mapPanel?.Hide();
            _characterPanel?.Hide();
            _skillsPanel?.Hide();
            _settingsPanel?.Hide();
            _savePanel?.Hide();
        }

        private void ShowPanel(Control panel)
        {
            HideAllPanels();
            panel.Show();

            if (panel == _inventoryPanel) _inventoryPanel.Refresh();
            else if (panel == _mapPanel) _mapPanel.Refresh();
            else if (panel == _characterPanel) _characterPanel.Refresh();
            else if (panel == _skillsPanel) _skillsPanel.Refresh();
            else if (panel == _settingsPanel) _settingsPanel.Refresh();
            else if (panel == _savePanel) _savePanel.Refresh();
        }

        private void ClosePopup()
        {
            Hide();
            HideAllPanels();
            _saveSlotSelector?.HideSelector();
            Log.Info("Popup menu closed");
            Main.Instance.ClosePopup();
        }

        #endregion

        #region Input Handling

        public override void _Input(InputEvent @event)
        {
            if (!IsVisibleInTree()) return;

            if (@event.IsActionPressed("ui_cancel"))
            {
                if (_saveSlotSelector?.Visible == true)
                {
                    _saveSlotSelector.HideSelector();
                }
                else if (_mainMenuPanel.Visible)
                {
                    ClosePopup();
                }
                else
                {
                    ShowMainMenu();
                }
                GetViewport().SetInputAsHandled();
            }
        }

        #endregion

        #region Main Menu Styling Helpers

        private static Font _font;
        private static Font GetFont() => _font ??= GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf");

        private static Label MakeTitle(string text)
        {
            var label = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center };
            label.AddThemeFontOverride("font", GetFont());
            label.AddThemeFontSizeOverride("font_size", 22);
            label.AddThemeColorOverride("font_color", ColorTextPrimary);
            return label;
        }

        private static Button MakeMenuButton(string text)
        {
            var btn = new Button { Text = text, CustomMinimumSize = new Vector2(220, 56) };
            var normal = new StyleBoxFlat
            {
                BgColor = ColorSurface,
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
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 18);
            return btn;
        }

        #endregion
    }
}
