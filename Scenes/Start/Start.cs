using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Start
{
    public partial class Start : Node2D
    {
        private CanvasLayer _canvasLayer;
        private Button _startButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _exitButton;
        private VBoxContainer _mainButton;
        private Label _versionLabel;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private ColorRect _decorativeLine;
        private ColorRect _backgroundTop;

        // 存档列表相关节点
        private ColorRect _saveListPanel;
        private Button _backButton;
        private VBoxContainer _saveListContainer;

        // 设置页面相关节点
        private VBoxContainer _settingMenu;
        private Button _settingBackButton;
        private CenterContainer _centerContainer2;

        // 音量设置节点
        private HSlider _masterVolumeSlider;
        private HSlider _musicVolumeSlider;
        private HSlider _sfxVolumeSlider;
        private HSlider _voiceVolumeSlider;

        // 图形设置节点
        private HSlider _brightnessSlider;
        private HSlider _contrastSlider;
        private HSlider _saturationSlider;
        private CheckButton _fullscreenToggle;
        private CheckButton _vsyncToggle;

        // 游戏设置节点
        private CheckButton _autoSaveToggle;
        private HSlider _textSpeedSlider;
        private CheckButton _showFPSToggle;

        // 语言设置节点
        private OptionButton _languageSelector;

        private float _elapsedTime = 0f;
        private bool _entranceDone = false;

        private static readonly Font GameFont = GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf");

        private static readonly Color ColorAccent = new(0.3f, 0.6f, 0.9f, 1f);
        private static readonly Color ColorMuted = new(0.6f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorSurface = new(0.1f, 0.14f, 0.2f, 0.95f);
        private static readonly Color ColorButtonHover = new(0.18f, 0.24f, 0.32f, 0.95f);

        public override void _Ready()
        {
            try
            {
                _canvasLayer = GetNode<CanvasLayer>("CanvasLayer");

                // Background references
                _backgroundTop = GetNode<ColorRect>("CanvasLayer/BackgroundTop");
                _decorativeLine = GetNode<ColorRect>("CanvasLayer/DecorativeLine");

                // Get button nodes
                _mainButton = GetNode<VBoxContainer>("CanvasLayer/CenterContainer/Panel/MainButton");
                _startButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/MainButton/StartButton");
                _continueButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/MainButton/ContinueButton");
                _settingsButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/MainButton/SettingsButton");
                _exitButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/MainButton/ExitButton");

                // Title and version
                _titleLabel = GetNode<Label>("CanvasLayer/CenterContainer/Panel/MainButton/TitleContainer/TitleLabel");
                _subtitleLabel = GetNode<Label>("CanvasLayer/CenterContainer/Panel/MainButton/TitleContainer/SubtitleLabel");
                _versionLabel = GetNode<Label>("CanvasLayer/VersionLabel");

                // Settings nodes
                _centerContainer2 = GetNode<CenterContainer>("CanvasLayer/CenterContainer2");
                _settingMenu = GetNode<VBoxContainer>("CanvasLayer/CenterContainer2/SettingMenu");
                _settingBackButton = GetNode<Button>("CanvasLayer/CenterContainer2/SettingMenu/BackButton");

                // Audio sliders
                _masterVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/MasterVolumeHBox/MasterVolumeSlider");
                _musicVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/MusicVolumeHBox/MusicVolumeSlider");
                _sfxVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/SFXVolumeHBox/SFXVolumeSlider");
                _voiceVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/VoiceVolumeHBox/VoiceVolumeSlider");

                // Graphics
                _brightnessSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/BrightnessHBox/BrightnessSlider");
                _contrastSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/ContrastHBox/ContrastSlider");
                _saturationSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/SaturationHBox/SaturationSlider");
                _fullscreenToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/FullscreenHBox/FullscreenToggle");
                _vsyncToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/VSyncHBox/VSyncToggle");

                // Game settings
                _autoSaveToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GameSection/AutoSaveHBox/AutoSaveToggle");
                _textSpeedSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GameSection/TextSpeedHBox/TextSpeedSlider");
                _showFPSToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GameSection/ShowFPSHBox/ShowFPSToggle");

                // Language
                _languageSelector = GetNode<OptionButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/LanguageSection/LanguageSelectorHBox/LanguageSelector");

                Log.Info("All Start nodes retrieved successfully");

                // Apply styling
                ApplyStartupStyling();

                // Connect signals
                _startButton.Pressed += OnStartButtonPressed;
                _continueButton.Pressed += OnContinueButtonPressed;
                _settingsButton.Pressed += OnSettingsButtonPressed;
                _exitButton.Pressed += OnExitButtonPressed;
                _settingBackButton.Pressed += OnSettingBackButtonPressed;

                // Audio signals
                _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
                _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
                _sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;
                _voiceVolumeSlider.ValueChanged += OnVoiceVolumeChanged;

                // Graphics signals
                _brightnessSlider.ValueChanged += OnBrightnessChanged;
                _contrastSlider.ValueChanged += OnContrastChanged;
                _saturationSlider.ValueChanged += OnSaturationChanged;
                _fullscreenToggle.Toggled += OnFullscreenToggled;
                _vsyncToggle.Toggled += OnVSyncToggled;

                // Game signals
                _autoSaveToggle.Toggled += OnAutoSaveToggled;
                _textSpeedSlider.ValueChanged += OnTextSpeedChanged;
                _showFPSToggle.Toggled += OnShowFPSToggled;

                // Language signal
                _languageSelector.ItemSelected += OnLanguageSelected;

                Log.Info("All Start signals connected successfully");

                ConfigManager.GetInstance();

                LoadSettings();
                InitializeSaveListUI();
                UpdateVersionDisplay();

                Main.Instance.TriggerSceneReady();

                _elapsedTime = 0f;
                _entranceDone = false;

                Log.Info("Start scene ready");
            }
            catch (Exception e)
            {
                Log.Error($"Error in Start._Ready: {e.Message}");
                Log.Error($"Stack trace: {e.StackTrace}");
            }
        }

        private void ApplyStartupStyling()
        {
            // Title styling
            _titleLabel.AddThemeFontOverride("font", GameFont);
            _titleLabel.AddThemeFontSizeOverride("font_size", 36);
            _titleLabel.AddThemeColorOverride("font_color", new Color(0.93f, 0.94f, 0.95f, 1f));

            // Subtitle styling
            if (_subtitleLabel != null)
            {
                _subtitleLabel.AddThemeFontOverride("font", GameFont);
                _subtitleLabel.AddThemeFontSizeOverride("font_size", 14);
                _subtitleLabel.AddThemeColorOverride("font_color", ColorMuted);
            }

            // Version label styling
            if (_versionLabel != null)
            {
                _versionLabel.AddThemeFontOverride("font", GameFont);
                _versionLabel.AddThemeFontSizeOverride("font_size", 11);
                _versionLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.45f, 0.5f, 0.7f));
            }

            // Decorative line
            if (_decorativeLine != null)
            {
                _decorativeLine.Modulate = new Color(0.3f, 0.6f, 0.9f, 0f);
            }

            // Style all main buttons
            StyleMainButton(_startButton);
            StyleMainButton(_continueButton);
            StyleMainButton(_settingsButton);
            StyleMainButton(_exitButton);

            // Initial transparency for entrance animation
            _mainButton.Modulate = new Color(1, 1, 1, 0);
            if (_backgroundTop != null) _backgroundTop.Modulate = new Color(1, 1, 1, 0);
        }

        private void StyleMainButton(Button btn)
        {
            btn.AddThemeFontOverride("font", GameFont);
            btn.AddThemeFontSizeOverride("font_size", 16);
            btn.AddThemeColorOverride("font_color", new Color(0.93f, 0.94f, 0.95f, 1f));

            var normal = new StyleBoxFlat
            {
                BgColor = ColorSurface,
                BorderWidthLeft = 1, BorderWidthTop = 1,
                BorderWidthRight = 1, BorderWidthBottom = 1,
                BorderColor = new Color(0.25f, 0.35f, 0.45f, 0.4f),
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ContentMarginLeft = 20, ContentMarginTop = 10,
                ContentMarginRight = 20, ContentMarginBottom = 10
            };
            btn.AddThemeStyleboxOverride("normal", normal);

            var hover = (StyleBoxFlat)normal.Duplicate();
            hover.BgColor = ColorButtonHover;
            hover.BorderColor = new Color(0.4f, 0.55f, 0.7f, 0.7f);
            btn.AddThemeStyleboxOverride("hover", hover);

            var pressed = (StyleBoxFlat)normal.Duplicate();
            pressed.BgColor = new Color(0.08f, 0.12f, 0.18f, 0.95f);
            btn.AddThemeStyleboxOverride("pressed", pressed);
        }

        private void UpdateVersionDisplay()
        {
            if (_versionLabel == null) return;
            try
            {
                string version = "";
                if (VersionManager.Instance != null)
                {
                    version = VersionManager.Instance.GameVersion;
                }
                _versionLabel.Text = $"v{version}";
            }
            catch
            {
                _versionLabel.Text = "";
            }
        }

        public override void _Process(double delta)
        {
            if (!Visible)
            {
                _canvasLayer.Visible = false;
                return;
            }

            // Entrance animation
            if (!_entranceDone)
            {
                _elapsedTime += (float)delta;

                // Fade in the top background
                float bgProgress = Mathf.Clamp(_elapsedTime / 1.0f, 0, 1);
                if (_backgroundTop != null)
                    _backgroundTop.Modulate = new Color(1, 1, 1, bgProgress * 0.85f);

                // Decorative line fades in after background
                float lineProgress = Mathf.Clamp((_elapsedTime - 0.5f) / 0.8f, 0, 1);
                if (_decorativeLine != null)
                    _decorativeLine.Modulate = new Color(0.3f, 0.6f, 0.9f, lineProgress * 0.4f);

                // Main buttons fade in last
                float btnProgress = Mathf.Clamp((_elapsedTime - 0.8f) / 0.6f, 0, 1);
                _mainButton.Modulate = new Color(1, 1, 1, btnProgress);

                if (_elapsedTime >= 2.0f)
                {
                    _entranceDone = true;
                    _mainButton.Modulate = new Color(1, 1, 1, 1);
                    if (_backgroundTop != null)
                        _backgroundTop.Modulate = new Color(1, 1, 1, 0.85f);
                    if (_decorativeLine != null)
                        _decorativeLine.Modulate = new Color(0.3f, 0.6f, 0.9f, 0.4f);
                }
            }
        }

        // Load settings from ConfigManager
        private void LoadSettings()
        {
            try
            {
                if (ConfigManager.Instance == null)
                {
                    Log.Error("ConfigManager instance is null");
                    return;
                }

                if (_masterVolumeSlider != null)
                    _masterVolumeSlider.Value = ConfigManager.Instance.CurrentConfig.MasterVolume;
                if (_musicVolumeSlider != null)
                    _musicVolumeSlider.Value = ConfigManager.Instance.CurrentConfig.MusicVolume;
                if (_sfxVolumeSlider != null)
                    _sfxVolumeSlider.Value = ConfigManager.Instance.CurrentConfig.SoundEffectVolume;
                if (_voiceVolumeSlider != null)
                    _voiceVolumeSlider.Value = ConfigManager.Instance.CurrentConfig.VoiceVolume;

                if (_brightnessSlider != null)
                    _brightnessSlider.Value = ConfigManager.Instance.CurrentConfig.Brightness;
                if (_contrastSlider != null)
                    _contrastSlider.Value = ConfigManager.Instance.CurrentConfig.Contrast;
                if (_saturationSlider != null)
                    _saturationSlider.Value = ConfigManager.Instance.CurrentConfig.Saturation;
                if (_fullscreenToggle != null)
                    _fullscreenToggle.ButtonPressed = ConfigManager.Instance.CurrentConfig.Fullscreen;
                if (_vsyncToggle != null)
                    _vsyncToggle.ButtonPressed = ConfigManager.Instance.CurrentConfig.VSync;

                if (_autoSaveToggle != null)
                    _autoSaveToggle.ButtonPressed = ConfigManager.Instance.CurrentConfig.AutoSave;
                if (_textSpeedSlider != null)
                    _textSpeedSlider.Value = ConfigManager.Instance.CurrentConfig.TextSpeed;
                if (_showFPSToggle != null)
                    _showFPSToggle.ButtonPressed = ConfigManager.Instance.CurrentConfig.ShowFPS;

                // Apply master volume to audio player
                AudioStreamPlayer audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
                if (audioPlayer != null)
                {
                    audioPlayer.VolumeDb = Mathf.LinearToDb(ConfigManager.Instance.CurrentConfig.MasterVolume);
                }

                // Language selector
                if (_languageSelector != null)
                {
                    _languageSelector.Clear();
                    _languageSelector.AddItem("language_zh");
                    _languageSelector.AddItem("language_en");
                    _languageSelector.AddItem("language_ja");
                    string savedLanguage = ConfigManager.Instance.CurrentConfig.Language;
                    int selectedIndex = savedLanguage switch
                    {
                        "en_US" => 1,
                        "ja_JP" => 2,
                        _ => 0
                    };
                    _languageSelector.Selected = selectedIndex;
                    TranslationServer.SetLocale(savedLanguage);
                }

                UpdateSettingLabelsWidth();
            }
            catch (Exception e)
            {
                Log.Error($"Error loading settings: {e.Message}");
            }
        }

        private void UpdateSettingLabelsWidth()
        {
            float minWidth = 250f;
            string basePath = "CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer";
            string[] labelPaths = new[]
            {
                $"{basePath}/LanguageSection/LanguageSelectorHBox/LanguageLabel",
                $"{basePath}/AudioSection/MasterVolumeHBox/MasterVolumeLabel",
                $"{basePath}/AudioSection/MusicVolumeHBox/MusicVolumeLabel",
                $"{basePath}/AudioSection/SFXVolumeHBox/SFXVolumeLabel",
                $"{basePath}/AudioSection/VoiceVolumeHBox/VoiceVolumeLabel",
                $"{basePath}/GraphicsSection/BrightnessHBox/BrightnessLabel",
                $"{basePath}/GraphicsSection/ContrastHBox/ContrastLabel",
                $"{basePath}/GraphicsSection/SaturationHBox/SaturationLabel",
                $"{basePath}/GraphicsSection/FullscreenHBox/FullscreenLabel",
                $"{basePath}/GraphicsSection/VSyncHBox/VSyncLabel",
                $"{basePath}/GameSection/AutoSaveHBox/AutoSaveLabel",
                $"{basePath}/GameSection/TextSpeedHBox/TextSpeedLabel",
                $"{basePath}/GameSection/ShowFPSHBox/ShowFPSLabel",
                $"{basePath}/ControlSection/SensitivityHBox/SensitivityLabel",
                $"{basePath}/ControlSection/InvertYHBox/InvertYLabel",
                $"{basePath}/NotificationSection/GameNotificationHBox/GameNotificationLabel",
                $"{basePath}/NotificationSection/AchievementNotificationHBox/AchievementNotificationLabel"
            };

            foreach (string path in labelPaths)
            {
                var label = GetNodeOrNull<Label>(path);
                if (label != null)
                {
                    label.CustomMinimumSize = new Vector2(minWidth, 0);
                }
            }
        }

        // --- Button Handlers ---

        private void OnStartButtonPressed()
        {
            Log.Info("Start button pressed");
            Main.Instance.SwitchScene("first");
        }

        private void OnContinueButtonPressed()
        {
            Log.Info("Continue button pressed");
            ShowSaveList();
        }

        private void ShowSaveList()
        {
            ClearSaveList();
            List<SaveInfo> saveInfos = SaveManager.Instance.GetAllSaveInfos();
            Log.Info($"Found {saveInfos.Count} saves");
            if (saveInfos.Count == 0)
            {
                Label noSaveLabel = new()
                {
                    Text = "no_saves",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                noSaveLabel.AddThemeFontOverride("font", GameFont);
                noSaveLabel.AddThemeFontSizeOverride("font_size", 20);
                noSaveLabel.AddThemeColorOverride("font_color", ColorMuted);
                noSaveLabel.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                _saveListContainer.AddChild(noSaveLabel);
            }
            else
            {
                foreach (SaveInfo saveInfo in saveInfos)
                {
                    CreateSaveItem(saveInfo);
                }
            }

            GetNode<CenterContainer>("CanvasLayer/CenterContainer").Visible = false;
            _saveListPanel.Visible = true;
        }

        private void InitializeSaveListUI()
        {
            _saveListPanel = GetNode<ColorRect>("CanvasLayer/SaveListPanel");
            _saveListContainer = GetNode<VBoxContainer>("CanvasLayer/SaveListPanel/CenterContainer/Panel/VBoxContainer/ScrollContainer/SaveListContainer");
            _backButton = GetNode<Button>("CanvasLayer/SaveListPanel/CenterContainer/Panel/VBoxContainer/BackButton");
            _backButton.Pressed += OnBackButtonPressed;
        }

        private void OnSettingsButtonPressed()
        {
            Log.Info("Settings button pressed");
            _mainButton.Visible = false;
            GetNode<CenterContainer>("CanvasLayer/CenterContainer").Visible = false;
            _centerContainer2.Visible = true;
            _settingMenu.Visible = true;
        }

        private void OnSettingBackButtonPressed()
        {
            Log.Info("Setting back button pressed");
            _settingMenu.Visible = false;
            _centerContainer2.Visible = false;
            _mainButton.Visible = true;
            GetNode<CenterContainer>("CanvasLayer/CenterContainer").Visible = true;
            ConfigManager.Instance.SaveConfig();
        }

        private void OnMasterVolumeChanged(double value)
        {
            ConfigManager.Instance.SetMasterVolume((float)value);
            AudioStreamPlayer audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
            if (audioPlayer != null)
            {
                audioPlayer.VolumeDb = Mathf.LinearToDb((float)value);
            }
        }

        private void OnMusicVolumeChanged(double value) => ConfigManager.Instance.SetMusicVolume((float)value);
        private void OnSFXVolumeChanged(double value) => ConfigManager.Instance.SetSoundEffectVolume((float)value);
        private void OnVoiceVolumeChanged(double value) => ConfigManager.Instance.SetVoiceVolume((float)value);
        private void OnBrightnessChanged(double value) => ConfigManager.Instance.SetBrightness((float)value);
        private void OnContrastChanged(double value) => ConfigManager.Instance.SetContrast((float)value);
        private void OnSaturationChanged(double value) => ConfigManager.Instance.SetSaturation((float)value);
        private void OnFullscreenToggled(bool toggled) => ConfigManager.Instance.SetFullscreen(toggled);
        private void OnVSyncToggled(bool toggled) => ConfigManager.Instance.SetVSync(toggled);
        private void OnAutoSaveToggled(bool toggled) => ConfigManager.Instance.SetAutoSave(toggled);
        private void OnTextSpeedChanged(double value) => ConfigManager.Instance.SetTextSpeed((float)value);
        private void OnShowFPSToggled(bool toggled) => ConfigManager.Instance.SetShowFPS(toggled);

        private void OnLanguageSelected(long index)
        {
            string language = index switch
            {
                0 => "zh_CN",
                1 => "en_US",
                2 => "ja_JP",
                _ => "zh_CN"
            };
            Log.Info($"Language selected: {language}");
            ConfigManager.Instance.SetLanguage(language);
            LoadSettings();
        }

        private void OnExitButtonPressed()
        {
            Log.Info("Exit button pressed");
            GetTree().Quit();
        }

        private void OnBackButtonPressed()
        {
            _saveListPanel.Visible = false;
            GetNode<CenterContainer>("CanvasLayer/CenterContainer").Visible = true;
        }

        private void ClearSaveList()
        {
            foreach (Node child in _saveListContainer.GetChildren())
                child.QueueFree();
        }

        private void CreateSaveItem(SaveInfo saveInfo)
        {
            Panel saveItemPanel = new()
            {
                CustomMinimumSize = new Vector2(0, 120)
            };
            _saveListContainer.AddChild(saveItemPanel);

            CenterContainer centerContainer = new();
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            saveItemPanel.AddChild(centerContainer);

            VBoxContainer saveItemVBox = new()
            {
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
                SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
            };
            centerContainer.AddChild(saveItemVBox);

            Label nameLabel = new() { Text = saveInfo.SaveName };
            nameLabel.AddThemeFontOverride("font", GameFont);
            nameLabel.AddThemeFontSizeOverride("font_size", 22);
            nameLabel.AddThemeColorOverride("font_color", new Color(0.93f, 0.94f, 0.95f, 1f));
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            saveItemVBox.AddChild(nameLabel);

            Label infoLabel = new();
            string playTime = FormatPlayTime(saveInfo.PlayTime);
            infoLabel.Text = $"Level: {saveInfo.AveragePlayerLevel} | Time: {playTime} | {saveInfo.SaveTime:yyyy-MM-dd HH:mm}";
            infoLabel.AddThemeFontOverride("font", GameFont);
            infoLabel.AddThemeFontSizeOverride("font_size", 14);
            infoLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.65f, 0.7f, 1f));
            infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            saveItemVBox.AddChild(infoLabel);

            HBoxContainer buttonContainer = new() { Alignment = BoxContainer.AlignmentMode.Center };
            saveItemVBox.AddChild(buttonContainer);

            Button loadButton = new() { Text = "load", CustomMinimumSize = new Vector2(120, 36) };
            loadButton.AddThemeFontOverride("font", GameFont);
            loadButton.AddThemeFontSizeOverride("font_size", 15);
            loadButton.AddThemeColorOverride("font_color", new Color(0.93f, 0.94f, 0.95f, 1f));
            StyleMainButton(loadButton);
            string capturedId = saveInfo.SaveId;
            loadButton.Pressed += () => OnLoadSavePressed(capturedId);
            buttonContainer.AddChild(loadButton);

            saveItemPanel.MouseEntered += () => saveItemPanel.Modulate = new Color(1f, 1f, 1f, 0.85f);
            saveItemPanel.MouseExited += () => saveItemPanel.Modulate = new Color(1f, 1f, 1f, 1f);
        }

        private static string FormatPlayTime(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = seconds % 3600 / 60;
            int secs = seconds % 60;
            return hours > 0 ? $"{hours}h {minutes}m" : minutes > 0 ? $"{minutes}m {secs}s" : $"{secs}s";
        }

        private void OnLoadSavePressed(string saveId)
        {
            Log.Info($"Load save pressed for slot {saveId}");
            SaveData saveData = SaveManager.Instance.LoadGame(saveId);
            if (saveData != null)
            {
                _saveListPanel.Visible = false;
                string targetScene = saveData.CurrentScene;
                Log.Info($"Loading scene: {targetScene}");
                Main.Instance.SwitchScene(targetScene);
            }
            else
            {
                ShowToast($"load_save_failed {saveId}");
            }
        }

        private void ShowToast(string message)
        {
            Log.Info($"Toast: {message}");
        }
    }
}
