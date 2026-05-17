/*
 * File: PopupSettingsPanel.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 设置面板，允许玩家调整游戏的声音、画面、游戏和控制设置。
 * 所有更改可保存或恢复默认值。
 *
 * Key Features:
 * - 音频设置：主音量/音乐/音效/语音 滑块
 * - 画面设置：亮度/对比度/饱和度 滑块、分辨率 下拉框、全屏/垂直同步 复选框
 * - 游戏设置：语言 下拉框、自动保存 复选框、文本速度 滑块、显示FPS 复选框
 * - 控制设置：按键绑定显示、重置按键绑定
 * - 保存设置和恢复默认值按钮
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
    /// <summary>
    /// 设置面板
    /// </summary>
    public partial class PopupSettingsPanel : PopupPanelBase
    {
        // Audio
        private HSlider _masterSlider;
        private HSlider _musicSlider;
        private HSlider _sfxSlider;
        private HSlider _voiceSlider;

        // Graphics
        private HSlider _brightnessSlider;
        private HSlider _contrastSlider;
        private HSlider _saturationSlider;
        private OptionButton _resolutionOption;
        private CheckButton _fullscreenCheck;
        private CheckButton _vsyncCheck;

        // Game
        private OptionButton _langOption;
        private CheckButton _autoSaveCheck;
        private HSlider _textSpeedSlider;
        private CheckButton _showFpsCheck;

        // Controls
        private VBoxContainer _keybindingsList;
        private Button _resetKeysBtn;
        private HSlider _sensitivitySlider;
        private CheckButton _invertYCheck;

        // Notifications
        private CheckButton _gameNotifyCheck;
        private CheckButton _achievementNotifyCheck;

        // Keybinding rebind state
        private string _listeningForAction = null;
        private Button _listeningButton = null;

        /// <summary>
        /// 返回主菜单的回调
        /// </summary>
        public Action OnBackPressed { get; set; }

        public override void _Ready()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            SetupPanel("SettingsPanel");

            var scroll = new ScrollContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.02f, AnchorTop = 0.02f,
                AnchorRight = 0.98f, AnchorBottom = 0.98f
            };
            AddChild(scroll);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(vbox);

            vbox.AddChild(CreateTitleLabel(TranslationServer.Translate("settings_title")));
            vbox.AddChild(CreateSeparator());

            // === Audio ===
            vbox.AddChild(CreateSectionLabel(TranslationServer.Translate("settings_audio")));
            _masterSlider = CreateSettingSlider(TranslationServer.Translate("settings_master_volume"), 100, vbox);
            _musicSlider = CreateSettingSlider(TranslationServer.Translate("settings_music_volume"), 100, vbox);
            _sfxSlider = CreateSettingSlider(TranslationServer.Translate("settings_sfx_volume"), 100, vbox);
            _voiceSlider = CreateSettingSlider(TranslationServer.Translate("settings_voice_volume"), 100, vbox);

            vbox.AddChild(CreateSeparator());

            // === Graphics ===
            vbox.AddChild(CreateSectionLabel(TranslationServer.Translate("settings_graphics")));
            _brightnessSlider = CreateSettingSliderFloat(TranslationServer.Translate("settings_brightness"), 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _contrastSlider = CreateSettingSliderFloat(TranslationServer.Translate("settings_contrast"), 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _saturationSlider = CreateSettingSliderFloat(TranslationServer.Translate("settings_saturation"), 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _resolutionOption = CreateSettingOption(TranslationServer.Translate("settings_resolution"),
                new[] { "720×640", "1280×720", "1920×1080" }, vbox);
            _fullscreenCheck = CreateSettingCheck(TranslationServer.Translate("settings_fullscreen"), vbox);
            _vsyncCheck = CreateSettingCheck(TranslationServer.Translate("settings_vsync"), vbox);

            vbox.AddChild(CreateSeparator());

            // === Game ===
            vbox.AddChild(CreateSectionLabel(TranslationServer.Translate("settings_game")));
            _langOption = CreateSettingOption(TranslationServer.Translate("settings_language"), new[] { "zh_CN", "en_US", "ja_JP" }, vbox);
            _autoSaveCheck = CreateSettingCheck(TranslationServer.Translate("settings_auto_save"), vbox);
            _textSpeedSlider = CreateSettingSliderFloat(TranslationServer.Translate("settings_text_speed"), 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _showFpsCheck = CreateSettingCheck(TranslationServer.Translate("settings_show_fps"), vbox);

            vbox.AddChild(CreateSeparator());

            // === Controls ===
            vbox.AddChild(CreateSectionLabel(TranslationServer.Translate("settings_controls")));
            _sensitivitySlider = CreateSettingSliderFloat(TranslationServer.Translate("settings_sensitivity"), 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _invertYCheck = CreateSettingCheck(TranslationServer.Translate("settings_invert_y"), vbox);

            // Keybindings
            var keyLabel = new Label
            {
                Text = TranslationServer.Translate("settings_keybindings"),
                CustomMinimumSize = new Vector2(0, 24)
            };
            keyLabel.AddThemeFontOverride("font", GetFont());
            keyLabel.AddThemeFontSizeOverride("font_size", 14);
            keyLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
            vbox.AddChild(keyLabel);

            _keybindingsList = new VBoxContainer();
            vbox.AddChild(_keybindingsList);

            _resetKeysBtn = CreateSecondaryButton(TranslationServer.Translate("settings_reset_keys"));
            _resetKeysBtn.Pressed += OnResetKeys;
            var keysHbox = new HBoxContainer();
            keysHbox.AddChild(_resetKeysBtn);
            vbox.AddChild(keysHbox);

            vbox.AddChild(CreateSeparator());

            // === Notifications ===
            vbox.AddChild(CreateSectionLabel(TranslationServer.Translate("settings_notifications")));
            _gameNotifyCheck = CreateSettingCheck(TranslationServer.Translate("settings_game_notifications"), vbox);
            _achievementNotifyCheck = CreateSettingCheck(TranslationServer.Translate("settings_achievement_notifications"), vbox);

            vbox.AddChild(CreateSeparator());

            // === Action Buttons ===
            var btnHbox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            vbox.AddChild(btnHbox);

            var saveBtn = CreatePrimaryButton(TranslationServer.Translate("settings_save"));
            saveBtn.Pressed += OnSave;
            btnHbox.AddChild(saveBtn);

            btnHbox.AddChild(new Control { CustomMinimumSize = new Vector2(16, 0) });

            var resetBtn = CreateSecondaryButton(TranslationServer.Translate("settings_reset"));
            resetBtn.Pressed += OnReset;
            btnHbox.AddChild(resetBtn);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

            var backBtn = CreateSecondaryButton(TranslationServer.Translate("settings_back"));
            backBtn.Pressed += () => OnBackPressed?.Invoke();
            vbox.AddChild(backBtn);
        }

        public void Refresh()
        {
            var cfg = ConfigManager.Instance?.CurrentConfig;
            if (cfg == null) return;

            // Audio
            _masterSlider.Value = cfg.MasterVolume * 100;
            _musicSlider.Value = cfg.MusicVolume * 100;
            _sfxSlider.Value = cfg.SoundEffectVolume * 100;
            _voiceSlider.Value = cfg.VoiceVolume * 100;

            // Graphics
            _brightnessSlider.Value = cfg.Brightness * 100;
            _contrastSlider.Value = cfg.Contrast * 100;
            _saturationSlider.Value = cfg.Saturation * 100;
            SelectResolutionOption(cfg.ResolutionWidth, cfg.ResolutionHeight);
            _fullscreenCheck.ButtonPressed = cfg.Fullscreen;
            _vsyncCheck.ButtonPressed = cfg.VSync;

            // Game
            for (int i = 0; i < _langOption.ItemCount; i++)
            {
                if (_langOption.GetItemText(i) == cfg.Language)
                {
                    _langOption.Selected = i;
                    break;
                }
            }
            _autoSaveCheck.ButtonPressed = cfg.AutoSave;
            _textSpeedSlider.Value = cfg.TextSpeed * 100;
            _showFpsCheck.ButtonPressed = cfg.ShowFPS;

            // Controls
            _sensitivitySlider.Value = cfg.MouseSensitivity * 100;
            _invertYCheck.ButtonPressed = cfg.InvertY;
            RefreshKeybindings(cfg.KeyBindings);

            // Notifications
            _gameNotifyCheck.ButtonPressed = cfg.ShowGameNotifications;
            _achievementNotifyCheck.ButtonPressed = cfg.ShowAchievementNotifications;

            CancelListening();
        }

        private void SelectResolutionOption(int width, int height)
        {
            var key = $"{width}×{height}";
            for (int i = 0; i < _resolutionOption.ItemCount; i++)
            {
                if (_resolutionOption.GetItemText(i) == key)
                {
                    _resolutionOption.Selected = i;
                    return;
                }
            }
        }

        private void RefreshKeybindings(Dictionary<string, string> bindings)
        {
            ClearChildren(_keybindingsList);

            var rebindableActions = InputManager.GetRebindableActions();
            foreach (var action in rebindableActions)
            {
                var row = new HBoxContainer();
                var actionLabel = new Label
                {
                    Text = TranslationServer.Translate($"keybind_{action}"),
                    CustomMinimumSize = new Vector2(140, 0)
                };
                actionLabel.AddThemeFontOverride("font", GetFont());
                actionLabel.AddThemeFontSizeOverride("font_size", 13);
                actionLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
                row.AddChild(actionLabel);

                string currentLabel = InputManager.Instance?.GetActionLabel(action) ?? "---";
                var capturedAction = action;
                var keyBtn = new Button
                {
                    Text = currentLabel,
                    CustomMinimumSize = new Vector2(120, 28),
                    TooltipText = TranslationServer.Translate("settings_click_to_rebind")
                };
                keyBtn.AddThemeFontOverride("font", GetFont());
                keyBtn.AddThemeFontSizeOverride("font_size", 13);
                keyBtn.AddThemeColorOverride("font_color", ColorTextPrimary);
                ApplyButtonStyle(keyBtn);

                keyBtn.Pressed += () => StartListening(capturedAction, keyBtn);
                row.AddChild(keyBtn);

                _keybindingsList.AddChild(row);
            }
        }

        private void StartListening(string action, Button btn)
        {
            _listeningForAction = action;
            _listeningButton = btn;
            btn.Text = TranslationServer.Translate("settings_press_key");
            btn.AddThemeColorOverride("font_color", ColorWarning);
        }

        private void CancelListening()
        {
            _listeningForAction = null;
            _listeningButton = null;
        }

        public override void _Input(InputEvent @event)
        {
            if (!IsVisibleInTree() || _listeningForAction == null) return;

            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                // Delegate to InputManager for actual InputMap modification + persistence
                InputManager.Instance?.RebindAction(_listeningForAction, keyEvent.Keycode);

                if (_listeningButton != null)
                {
                    _listeningButton.Text = OS.GetKeycodeString(keyEvent.Keycode);
                    _listeningButton.AddThemeColorOverride("font_color", ColorTextPrimary);
                }
                CancelListening();
                GetViewport().SetInputAsHandled();
            }
        }

        private void OnSave()
        {
            var cm = ConfigManager.Instance;
            if (cm == null) return;

            // Audio
            cm.SetMasterVolume((float)(_masterSlider.Value / 100.0));
            cm.SetMusicVolume((float)(_musicSlider.Value / 100.0));
            cm.SetSoundEffectVolume((float)(_sfxSlider.Value / 100.0));
            cm.SetVoiceVolume((float)(_voiceSlider.Value / 100.0));

            // Graphics
            cm.SetBrightness((float)(_brightnessSlider.Value / 100.0));
            cm.SetContrast((float)(_contrastSlider.Value / 100.0));
            cm.SetSaturation((float)(_saturationSlider.Value / 100.0));
            ApplyResolutionSelection();
            cm.SetFullscreen(_fullscreenCheck.ButtonPressed);
            cm.SetVSync(_vsyncCheck.ButtonPressed);

            // Game
            if (_langOption.Selected >= 0)
                cm.SetLanguage(_langOption.GetItemText(_langOption.Selected));
            cm.SetAutoSave(_autoSaveCheck.ButtonPressed);
            cm.SetTextSpeed((float)(_textSpeedSlider.Value / 100.0));
            cm.SetShowFPS(_showFpsCheck.ButtonPressed);

            // Controls
            cm.SetMouseSensitivity((float)(_sensitivitySlider.Value / 100.0));
            cm.SetInvertY(_invertYCheck.ButtonPressed);

            // Notifications
            cm.SetShowGameNotifications(_gameNotifyCheck.ButtonPressed);
            cm.SetShowAchievementNotifications(_achievementNotifyCheck.ButtonPressed);

            cm.SaveConfig();
            cm.ApplyConfig();
            Log.Info("Settings saved and applied");
        }

        private void ApplyResolutionSelection()
        {
            if (_resolutionOption.Selected < 0) return;
            var text = _resolutionOption.GetItemText(_resolutionOption.Selected);
            var parts = text.Split('×');
            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                ConfigManager.Instance?.SetResolution(w, h);
        }

        private void OnReset()
        {
            ConfigManager.Instance?.ResetToDefaults();
            Refresh();
            Log.Info("Settings reset to defaults");
        }

        private void OnResetKeys()
        {
            InputManager.Instance?.ResetAllBindings();
            RefreshKeybindings(ConfigManager.Instance?.CurrentConfig?.KeyBindings);
            Log.Info("Keybindings reset to defaults");
        }
    }
}
