/*
 * File: PopupSavePanel.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 存档面板，提供游戏的存档和读档功能。
 * 嵌入存档槽位列表，支持保存/读取/覆盖/删除，含确认对话框和状态反馈。
 *
 * Key Features:
 * - 存档/读档模式切换
 * - 当前游戏信息显示（玩家、等级、场景、时间）
 * - 内嵌10个存档槽位，显示存档详情
 * - 快速存档/读档
 * - 覆盖确认对话框
 * - 删除确认对话框
 * - 操作状态反馈消息
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
    /// <summary>
    /// 存档面板
    /// </summary>
    public partial class PopupSavePanel : PopupPanelBase
    {
        private bool _isLoadMode;
        private Label _infoLabel;
        private Button _saveModeBtn;
        private Button _loadModeBtn;
        private Button _actionBtn;
        private VBoxContainer _slotsContainer;
        private Label _statusLabel;
        private SaveSlotSelector _slotSelector;

        // Confirmation dialog
        private Control _confirmOverlay;
        private Label _confirmLabel;
        private Button _confirmYesBtn;
        private Button _confirmNoBtn;
        private Action _confirmAction;

        private const int MAX_SLOTS = 10;

        /// <summary>
        /// 返回主菜单的回调
        /// </summary>
        public Action OnBackPressed { get; set; }

        /// <summary>
        /// 请求显示存档槽位选择器
        /// </summary>
        public Action OnShowSlotSelector { get; set; }

        /// <summary>
        /// 请求隐藏存档槽位选择器
        /// </summary>
        public Action OnHideSlotSelector { get; set; }

        /// <summary>
        /// 设置存档槽位选择器引用
        /// </summary>
        public void SetSlotSelector(SaveSlotSelector selector)
        {
            _slotSelector = selector;
            if (_slotSelector != null)
            {
                _slotSelector.SaveSlotSelected += OnSlotSelected;
            }
        }

        public override void _Ready()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Name = "SavePanel";
            Visible = false;
            LayoutMode = 1;
            AnchorLeft = 0.05f; AnchorTop = 0.05f;
            AnchorRight = 0.95f; AnchorBottom = 0.95f;
            MouseFilter = MouseFilterEnum.Stop;

            var bg = new ColorRect
            {
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1,
                Color = ColorBg
            };
            AddChild(bg);

            var vbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.05f, AnchorTop = 0.05f,
                AnchorRight = 0.95f, AnchorBottom = 0.95f
            };
            AddChild(vbox);

            vbox.AddChild(CreateTitleLabel("Save / Load"));
            vbox.AddChild(CreateSeparator());

            // Mode toggle
            var modeHbox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            vbox.AddChild(modeHbox);

            _saveModeBtn = CreatePrimaryButton("Save Game");
            _saveModeBtn.Pressed += () => SetMode(false);
            modeHbox.AddChild(_saveModeBtn);

            modeHbox.AddChild(new Control { CustomMinimumSize = new Vector2(12, 0) });

            _loadModeBtn = CreateSecondaryButton("Load Game");
            _loadModeBtn.Pressed += () => SetMode(true);
            modeHbox.AddChild(_loadModeBtn);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

            // Game info
            vbox.AddChild(CreateSectionLabel("Current Game"));
            _infoLabel = CreateInfoLabel("");
            _infoLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            vbox.AddChild(_infoLabel);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });
            vbox.AddChild(CreateSeparator());

            // Slot list (scrollable)
            var slotsLabel = CreateSectionLabel("Save Slots");
            vbox.AddChild(slotsLabel);

            var scroll = new ScrollContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            vbox.AddChild(scroll);

            _slotsContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(_slotsContainer);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });

            // Status label
            _statusLabel = CreateInfoLabel("");
            _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(_statusLabel);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

            // Action buttons
            var btnHbox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            vbox.AddChild(btnHbox);

            _actionBtn = CreatePrimaryButton("Quick Save");
            _actionBtn.Pressed += OnActionPressed;
            btnHbox.AddChild(_actionBtn);

            btnHbox.AddChild(new Control { CustomMinimumSize = new Vector2(12, 0) });

            var backBtn = CreateSecondaryButton("Back");
            backBtn.Pressed += () =>
            {
                OnHideSlotSelector?.Invoke();
                OnBackPressed?.Invoke();
            };
            btnHbox.AddChild(backBtn);

            // Build confirmation overlay
            BuildConfirmOverlay();

            SetMode(false);
        }

        #region Confirmation Dialog

        private void BuildConfirmOverlay()
        {
            _confirmOverlay = new Control
            {
                Visible = false,
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1,
                MouseFilter = MouseFilterEnum.Stop
            };
            AddChild(_confirmOverlay);

            var dimBg = new ColorRect
            {
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1,
                Color = new Color(0, 0, 0, 0.6f)
            };
            _confirmOverlay.AddChild(dimBg);

            var panel = new PanelContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.3f, AnchorTop = 0.35f,
                AnchorRight = 0.7f, AnchorBottom = 0.65f
            };
            panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = ColorSurface,
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = ColorWarning,
                CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10,
                CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
                ContentMarginLeft = 20, ContentMarginTop = 16,
                ContentMarginRight = 20, ContentMarginBottom = 16
            });
            _confirmOverlay.AddChild(panel);

            var confirmVbox = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            panel.AddChild(confirmVbox);

            _confirmLabel = CreateInfoLabel("");
            _confirmLabel.HorizontalAlignment = HorizontalAlignment.Center;
            confirmVbox.AddChild(_confirmLabel);

            confirmVbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 16) });

            var btnRow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            confirmVbox.AddChild(btnRow);

            _confirmYesBtn = CreatePrimaryButton("Confirm");
            _confirmYesBtn.Pressed += () =>
            {
                _confirmOverlay.Visible = false;
                _confirmAction?.Invoke();
                _confirmAction = null;
            };
            btnRow.AddChild(_confirmYesBtn);

            btnRow.AddChild(new Control { CustomMinimumSize = new Vector2(20, 0) });

            _confirmNoBtn = CreateSecondaryButton("Cancel");
            _confirmNoBtn.Pressed += () =>
            {
                _confirmOverlay.Visible = false;
                _confirmAction = null;
            };
            btnRow.AddChild(_confirmNoBtn);
        }

        private void ShowConfirm(string message, Action onConfirm)
        {
            _confirmLabel.Text = message;
            _confirmAction = onConfirm;
            _confirmOverlay.Visible = true;
        }

        #endregion

        #region Mode & Navigation

        private void SetMode(bool isLoad)
        {
            _isLoadMode = isLoad;

            if (isLoad)
            {
                ApplyButtonStyle(_saveModeBtn);
                ApplyPrimaryButtonStyle(_loadModeBtn);
                _actionBtn.Text = "Quick Load";
            }
            else
            {
                ApplyPrimaryButtonStyle(_saveModeBtn);
                ApplyButtonStyle(_loadModeBtn);
                _actionBtn.Text = "Quick Save";
            }

            Refresh();
        }

        public void Refresh()
        {
            // Update game info from current state
            var player = GetPlayer();
            if (player != null)
            {
                _infoLabel.Text = $"Player: {player.CreatureName}  Lv.{player.Level}\n"
                    + $"Scene: {GetCurrentSceneName()}\n"
                    + $"HP: {player.Health:F0}/{player.MaxHealth:F0}  |  MP: {player.CurrentMana:F0}/{player.MaxMana:F0}\n"
                    + $"Gold: {player.Gold} G  |  Attack: {player.Attack:F0}  Defense: {player.Defense:F0}";
            }
            else
            {
                _infoLabel.Text = "No active player data.";
            }

            // Refresh slot list
            RefreshSlots();
        }

        private static string GetCurrentSceneName()
        {
            if (Main.Instance?.NowScene != null)
                return Main.Instance.NowScene.Name;
            return "Unknown";
        }

        #endregion

        #region Slot List

        private void RefreshSlots()
        {
            ClearChildren(_slotsContainer);

            // Get save infos from SaveManager
            var saveInfos = SaveManager.Instance?.GetAllSaveInfos() ?? new List<SaveInfo>();

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                var slotId = (i + 1).ToString();
                var info = saveInfos.FirstOrDefault(s => s.SaveId == slotId);
                CreateSlotRow(slotId, info);
            }
        }

        private void CreateSlotRow(string slotId, SaveInfo info)
        {
            var panel = new PanelContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 68)
            };
            panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = info != null ? new Color(0.12f, 0.16f, 0.22f, 0.95f) : new Color(0.08f, 0.1f, 0.14f, 0.8f),
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.4f),
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ContentMarginLeft = 12, ContentMarginTop = 6,
                ContentMarginRight = 12, ContentMarginBottom = 6
            });
            _slotsContainer.AddChild(panel);

            var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };

            // Left: slot info
            var infoVbox = new VBoxContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsStretchRatio = 3
            };

            var slotName = info != null
                ? $"Slot {slotId}  —  {info.SaveName ?? "Untitled"}"
                : $"Slot {slotId}  —  Empty";
            var nameLabel = new Label { Text = slotName };
            nameLabel.AddThemeFontOverride("font", GetFont());
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            nameLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
            infoVbox.AddChild(nameLabel);

            if (info != null)
            {
                var detail = $"Lv.{info.AveragePlayerLevel}  |  {info.CurrentScene ?? "Unknown"}  |  "
                    + $"{TimeSpan.FromSeconds(info.PlayTime).TotalHours:F1}h  |  {info.SaveTime:yyyy-MM-dd HH:mm}";
                var detailLabel = new Label { Text = detail };
                detailLabel.AddThemeFontOverride("font", GetFont());
                detailLabel.AddThemeFontSizeOverride("font_size", 11);
                detailLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
                infoVbox.AddChild(detailLabel);
            }

            row.AddChild(infoVbox);

            // Right: action buttons
            if (_isLoadMode)
            {
                if (info != null)
                {
                    var loadBtn = CreateSmallButton("Load");
                    loadBtn.Pressed += () => OnSlotSelected(slotId);
                    row.AddChild(loadBtn);
                }
                else
                {
                    var emptyLabel = new Label { Text = "No Data" };
                    emptyLabel.AddThemeFontOverride("font", GetFont());
                    emptyLabel.AddThemeFontSizeOverride("font_size", 12);
                    emptyLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
                    row.AddChild(emptyLabel);
                }
            }
            else
            {
                // Save mode
                if (info != null)
                {
                    var overwriteBtn = CreateSmallButton("Overwrite");
                    overwriteBtn.Pressed += () => ShowConfirm(
                        $"Overwrite save in Slot {slotId}?\n\"{info.SaveName}\" — {info.SaveTime:yyyy-MM-dd HH:mm}",
                        () => DoSave(slotId));
                    row.AddChild(overwriteBtn);

                    row.AddChild(new Control { CustomMinimumSize = new Vector2(8, 0) });

                    var deleteBtn = new Button { Text = "Delete", CustomMinimumSize = new Vector2(60, 30) };
                    ApplyButtonStyle(deleteBtn);
                    deleteBtn.AddThemeFontOverride("font", GetFont());
                    deleteBtn.AddThemeFontSizeOverride("font_size", 12);
                    deleteBtn.Pressed += () => ShowConfirm(
                        $"Delete save in Slot {slotId}?\n\"{info.SaveName}\" — This cannot be undone!",
                        () => DoDelete(slotId));
                    row.AddChild(deleteBtn);
                }
                else
                {
                    var saveBtn = CreateSmallButton("Save Here");
                    saveBtn.Pressed += () => OnSlotSelected(slotId);
                    row.AddChild(saveBtn);
                }
            }

            panel.AddChild(row);
        }

        #endregion

        #region Actions

        private void OnActionPressed()
        {
            if (_isLoadMode)
                PerformQuickLoad();
            else
                PerformQuickSave();
        }

        private void PerformQuickSave()
        {
            if (SaveManager.Instance == null) return;
            var currentPlayer = GetPlayer();
            var saveData = SaveManager.CreateDefaultSaveData("quick_save", null, currentPlayer);
            if (saveData != null)
            {
                bool success = SaveManager.Instance.SaveGame(saveData, "quick_save");
                ShowStatus(success ? "Quick save completed!" : "Quick save failed!", success);
                Log.Info(success ? "Quick save completed" : "Quick save failed");
            }
        }

        private void PerformQuickLoad()
        {
            if (SaveManager.Instance == null)
            {
                ShowStatus("SaveManager not available.", false);
                return;
            }
            var saveData = SaveManager.Instance.LoadGame("quick_save");
            if (saveData != null)
            {
                ShowStatus("Quick load completed! Switching scene...", true);
                Log.Info("Quick load completed");
                if (Main.Instance != null && !string.IsNullOrEmpty(saveData.CurrentScene))
                    Main.Instance.SwitchScene(saveData.CurrentScene);
            }
            else
            {
                ShowStatus("No quick save found to load.", false);
                Log.Warning("No quick save found to load");
            }
        }

        private void OnSlotSelected(string slotId)
        {
            if (_isLoadMode)
                DoLoad(slotId);
            else
                DoSave(slotId);
        }

        private void DoSave(string slotId)
        {
            if (SaveManager.Instance == null)
            {
                ShowStatus("SaveManager not available.", false);
                return;
            }
            var currentPlayer = GetPlayer();
            var saveData = SaveManager.CreateDefaultSaveData(slotId, null, currentPlayer);
            if (saveData != null)
            {
                bool success = SaveManager.Instance.SaveGame(saveData, slotId);
                ShowStatus(success ? $"Saved to Slot {slotId}!" : $"Failed to save to Slot {slotId}.", success);
                Log.Info(success ? $"Game saved to slot: {slotId}" : $"Failed to save to slot: {slotId}");
                if (success) RefreshSlots();
            }
        }

        private void DoLoad(string slotId)
        {
            if (SaveManager.Instance == null)
            {
                ShowStatus("SaveManager not available.", false);
                return;
            }
            var saveData = SaveManager.Instance.LoadGame(slotId);
            if (saveData != null)
            {
                ShowStatus($"Loaded from Slot {slotId}! Switching scene...", true);
                Log.Info($"Game loaded from slot: {slotId}");
                if (Main.Instance != null && !string.IsNullOrEmpty(saveData.CurrentScene))
                    Main.Instance.SwitchScene(saveData.CurrentScene);
            }
            else
            {
                ShowStatus($"Failed to load from Slot {slotId}.", false);
                Log.Error($"Failed to load game from slot: {slotId}");
            }
        }

        private void DoDelete(string slotId)
        {
            if (SaveManager.Instance == null) return;

            string filePath = ProjectSettings.GlobalizePath($"{SaveManager.Instance.SaveDirectory}/save_{slotId}.json");
            try
            {
                if (Godot.FileAccess.FileExists(filePath))
                {
                    using var dir = DirAccess.Open(ProjectSettings.GlobalizePath(SaveManager.Instance.SaveDirectory));
                    if (dir != null)
                    {
                        dir.Remove($"save_{slotId}.json");
                        ShowStatus($"Slot {slotId} deleted.", true);
                        Log.Info($"Deleted save slot: {slotId}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to delete Slot {slotId}.", false);
                Log.Error($"Failed to delete save slot {slotId}: {ex.Message}");
            }

            RefreshSlots();
        }

        #endregion

        #region Status Feedback

        private void ShowStatus(string message, bool success)
        {
            _statusLabel.Text = message;
            _statusLabel.AddThemeColorOverride("font_color", success ? ColorSuccess : ColorDanger);

            // Auto-clear after 3 seconds
            GetTree()?.CreateTimer(3.0f).Timeout += () =>
            {
                if (_statusLabel != null && _statusLabel.Text == message)
                    _statusLabel.Text = "";
            };
        }

        #endregion
    }
}
