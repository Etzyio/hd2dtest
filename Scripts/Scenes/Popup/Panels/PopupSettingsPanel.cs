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
            Name = "SettingsPanel";
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

            var scroll = new ScrollContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.02f, AnchorTop = 0.02f,
                AnchorRight = 0.98f, AnchorBottom = 0.98f
            };
            AddChild(scroll);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(vbox);

            vbox.AddChild(CreateTitleLabel("Settings"));
            vbox.AddChild(CreateSeparator());

            // === Audio ===
            vbox.AddChild(CreateSectionLabel("Audio"));
            _masterSlider = CreateSettingSlider("Master Volume", 100, vbox);
            _musicSlider = CreateSettingSlider("Music Volume", 100, vbox);
            _sfxSlider = CreateSettingSlider("SFX Volume", 100, vbox);
            _voiceSlider = CreateSettingSlider("Voice Volume", 100, vbox);

            vbox.AddChild(CreateSeparator());

            // === Graphics ===
            vbox.AddChild(CreateSectionLabel("Graphics"));
            _brightnessSlider = CreateSettingSliderFloat("Brightness", 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _contrastSlider = CreateSettingSliderFloat("Contrast", 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _saturationSlider = CreateSettingSliderFloat("Saturation", 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _resolutionOption = CreateSettingOption("Resolution",
                new[] { "720×640", "1280×720", "1920×1080" }, vbox);
            _fullscreenCheck = CreateSettingCheck("Fullscreen", vbox);
            _vsyncCheck = CreateSettingCheck("VSync", vbox);

            vbox.AddChild(CreateSeparator());

            // === Game ===
            vbox.AddChild(CreateSectionLabel("Game"));
            _langOption = CreateSettingOption("Language", new[] { "zh_CN", "en_US", "ja_JP" }, vbox);
            _autoSaveCheck = CreateSettingCheck("Auto Save", vbox);
            _textSpeedSlider = CreateSettingSliderFloat("Text Speed", 200, 1,
                v => (v / 100.0).ToString("F2"), vbox);
            _showFpsCheck = CreateSettingCheck("Show FPS", vbox);

            vbox.AddChild(CreateSeparator());

            // === Controls ===
            vbox.AddChild(CreateSectionLabel("Controls"));
            _keybindingsList = new VBoxContainer();
            vbox.AddChild(_keybindingsList);

            _resetKeysBtn = CreateSecondaryButton("Reset Keybindings");
            _resetKeysBtn.Pressed += OnResetKeys;
            var keysHbox = new HBoxContainer();
            keysHbox.AddChild(_resetKeysBtn);
            vbox.AddChild(keysHbox);

            vbox.AddChild(CreateSeparator());

            // === Action Buttons ===
            var btnHbox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            vbox.AddChild(btnHbox);

            var saveBtn = CreatePrimaryButton("Save Settings");
            saveBtn.Pressed += OnSave;
            btnHbox.AddChild(saveBtn);

            btnHbox.AddChild(new Control { CustomMinimumSize = new Vector2(16, 0) });

            var resetBtn = CreateSecondaryButton("Reset Defaults");
            resetBtn.Pressed += OnReset;
            btnHbox.AddChild(resetBtn);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

            var backBtn = CreateSecondaryButton("Back");
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
            RefreshKeybindings(cfg.KeyBindings);
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

            foreach (var kv in bindings)
            {
                var row = new HBoxContainer();
                var actionLabel = new Label
                {
                    Text = kv.Key,
                    CustomMinimumSize = new Vector2(140, 0)
                };
                actionLabel.AddThemeFontOverride("font", GetFont());
                actionLabel.AddThemeFontSizeOverride("font_size", 13);
                actionLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
                row.AddChild(actionLabel);

                var keyLabel = new Label { Text = kv.Value };
                keyLabel.AddThemeFontOverride("font", GetFont());
                keyLabel.AddThemeFontSizeOverride("font_size", 13);
                keyLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
                row.AddChild(keyLabel);

                _keybindingsList.AddChild(row);
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
            ConfigManager.Instance?.ResetKeyBindings();
            var cfg = ConfigManager.Instance?.CurrentConfig;
            if (cfg != null) RefreshKeybindings(cfg.KeyBindings);
            Log.Info("Keybindings reset to defaults");
        }
    }
}
