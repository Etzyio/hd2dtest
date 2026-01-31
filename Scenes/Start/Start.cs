using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts
{
    public partial class Start : Node2D
    {
        private CanvasLayer _canvasLayer;
        private Button _startButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _exitButton;
        private VBoxContainer _mainButton;

        // 存档列表相关节点
        private ColorRect _saveListPanel;
        private Button _backButton;
        private VBoxContainer _saveListContainer;

        // 设置页面相关节点
        private VBoxContainer _settingMenu;
        private Button _settingBackButton;
        
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

        public override void _Ready()
        {
            try
            {
                _canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
                // 获取按钮节点
                _mainButton = GetNode<VBoxContainer>("CanvasLayer/CenterContainer/Panel/MainButton");
                _startButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/MainButton/StartButton");
                _continueButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/MainButton/ContinueButton");
                _settingsButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/MainButton/SettingsButton");
                _exitButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/MainButton/ExitButton");

                // 获取设置页面节点
                _settingMenu = GetNode<VBoxContainer>("CanvasLayer/CenterContainer/Panel/SettingMenu");
                _settingBackButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/SettingMenu/BackButton");
                
                // 音量设置节点
                _masterVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/MasterVolumeHBox/MasterVolumeSlider");
                _musicVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/MusicVolumeHBox/MusicVolumeSlider");
                _sfxVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/SFXVolumeHBox/SFXVolumeSlider");
                _voiceVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/VoiceVolumeHBox/VoiceVolumeSlider");
                
                // 图形设置节点
                _brightnessSlider = GetNode<HSlider>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/BrightnessHBox/BrightnessSlider");
                _contrastSlider = GetNode<HSlider>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/ContrastHBox/ContrastSlider");
                _saturationSlider = GetNode<HSlider>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/SaturationHBox/SaturationSlider");
                _fullscreenToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/FullscreenHBox/FullscreenToggle");
                _vsyncToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/VSyncHBox/VSyncToggle");
                
                // 游戏设置节点
                _autoSaveToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/GameSection/AutoSaveHBox/AutoSaveToggle");
                _textSpeedSlider = GetNode<HSlider>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/GameSection/TextSpeedHBox/TextSpeedSlider");
                _showFPSToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer/Panel/SettingMenu/ScrollContainer/SettingsContainer/GameSection/ShowFPSHBox/ShowFPSToggle");

                Log.Info("All nodes retrieved successfully");

                // 连接信号
                _startButton.Pressed += OnStartButtonPressed;
                _continueButton.Pressed += OnContinueButtonPressed;
                _settingsButton.Pressed += OnSettingsButtonPressed;
                _exitButton.Pressed += OnExitButtonPressed;
                _settingBackButton.Pressed += OnSettingBackButtonPressed;
                
                // 音量设置信号
                _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
                _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
                _sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;
                _voiceVolumeSlider.ValueChanged += OnVoiceVolumeChanged;
                
                // 图形设置信号
                _brightnessSlider.ValueChanged += OnBrightnessChanged;
                _contrastSlider.ValueChanged += OnContrastChanged;
                _saturationSlider.ValueChanged += OnSaturationChanged;
                _fullscreenToggle.Toggled += OnFullscreenToggled;
                _vsyncToggle.Toggled += OnVSyncToggled;
                
                // 游戏设置信号
                _autoSaveToggle.Toggled += OnAutoSaveToggled;
                _textSpeedSlider.ValueChanged += OnTextSpeedChanged;
                _showFPSToggle.Toggled += OnShowFPSToggled;

                Log.Info("All signals connected successfully");

                // 确保ConfigManager初始化
                ConfigManager.GetInstance();
                
                // 从配置加载设置
                LoadSettings();

                // 初始化存档列表UI
                // CreateSaveListUI();
                GameViewManager.TriggerSceneReady();
                
                Log.Info("Start scene ready");
            }
            catch (Exception e)
            {
                Log.Error($"Error in _Ready: {e.Message}");
                Log.Error($"Stack trace: {e.StackTrace}");
            }
        }

        // 加载设置
        private void LoadSettings()
        {
            try
            {
                // 确保ConfigManager实例存在
                if (ConfigManager.Instance == null)
                {
                    Log.Error("ConfigManager instance is null");
                    return;
                }
                
                // 从ConfigManager加载音量设置
                if (_masterVolumeSlider != null)
                    _masterVolumeSlider.Value = ConfigManager.Instance.CurrentConfig.MasterVolume;
                if (_musicVolumeSlider != null)
                    _musicVolumeSlider.Value = ConfigManager.Instance.CurrentConfig.MusicVolume;
                if (_sfxVolumeSlider != null)
                    _sfxVolumeSlider.Value = ConfigManager.Instance.CurrentConfig.SoundEffectVolume;
                if (_voiceVolumeSlider != null)
                    _voiceVolumeSlider.Value = ConfigManager.Instance.CurrentConfig.VoiceVolume;
                
                // 从ConfigManager加载图形设置
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
                
                // 从ConfigManager加载游戏设置
                if (_autoSaveToggle != null)
                    _autoSaveToggle.ButtonPressed = ConfigManager.Instance.CurrentConfig.AutoSave;
                if (_textSpeedSlider != null)
                    _textSpeedSlider.Value = ConfigManager.Instance.CurrentConfig.TextSpeed;
                if (_showFPSToggle != null)
                    _showFPSToggle.ButtonPressed = ConfigManager.Instance.CurrentConfig.ShowFPS;
                
                // 应用音量设置到AudioStreamPlayer
                AudioStreamPlayer audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
                if (audioPlayer != null)
                {
                    audioPlayer.VolumeDb = Mathf.LinearToDb(ConfigManager.Instance.CurrentConfig.MasterVolume);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error loading settings: {e.Message}");
            }
        }

        public override void _Process(double delta)
        {
            if (!Visible)
            {
                _canvasLayer.Visible = false;
            }
        }

        // 开始游戏按钮点击事件
        private void OnStartButtonPressed()
        {
            Log.Info("Start button pressed");
            // 切换到First场景
            _ = GameViewManager.SwitchScene("first");
        }

        // 继续游戏按钮点击事件
        private void OnContinueButtonPressed()
        {
            Log.Info("Continue button pressed");
            // 显示存档列表
            // ShowSaveList();

        }

        // 显示存档列表
        private void ShowSaveList()
        {
            // 获取所有存档信息
            List<SaveInfo> saveInfos = SaveManager.Instance.GetAllSaveInfos();
            Log.Info($"Found {saveInfos.Count} saves");
            if (saveInfos.Count == 0)
            {
                // 如果没有存档，显示提示
                Label noSaveLabel = new()
                {
                    Text = "没有找到存档",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                noSaveLabel.AddThemeFontSizeOverride("font_size", 20);
                noSaveLabel.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                noSaveLabel.AddThemeConstantOverride("margin_top", 20);
                noSaveLabel.AddThemeConstantOverride("margin_bottom", 20);
                _saveListContainer.AddChild(noSaveLabel);
            }
            else
            {
                // 动态创建存档项
                foreach (SaveInfo saveInfo in saveInfos)
                {
                    CreateSaveItem(saveInfo);
                }
            }

            // 显示存档列表面板
            _saveListPanel.Visible = true;
        }

        // 创建存档列表UI
        private void CreateSaveListUI()
        {
            // 获取CanvasLayer节点
            CanvasLayer canvasLayer = GetNode<CanvasLayer>("CanvasLayer");

            // 创建存档列表面板
            _saveListPanel = new ColorRect
            {
                Name = "SaveListPanel",
                Visible = false
            };
            _saveListPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _saveListPanel.SizeFlagsHorizontal = Control.SizeFlags.Fill;
            _saveListPanel.SizeFlagsVertical = Control.SizeFlags.Fill;
            _saveListPanel.Modulate = new Color(0, 0, 0, 0.8f);
            canvasLayer.AddChild(_saveListPanel);

            // 创建居中容器
            CenterContainer centerContainer = new()
            {
                Name = "CenterContainer"
            };
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            centerContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill;
            centerContainer.SizeFlagsVertical = Control.SizeFlags.Fill;
            _saveListPanel.AddChild(centerContainer);

            // 创建面板
            Panel panel = new()
            {
                Name = "Panel",
                CustomMinimumSize = new Vector2(600, 400)
            };
            centerContainer.AddChild(panel);

            // 创建垂直容器
            VBoxContainer vBoxContainer = new()
            {
                Name = "VBoxContainer"
            };
            vBoxContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vBoxContainer.AddThemeConstantOverride("margin_left", 10);
            vBoxContainer.AddThemeConstantOverride("margin_top", 10);
            vBoxContainer.AddThemeConstantOverride("margin_right", 10);
            vBoxContainer.AddThemeConstantOverride("margin_bottom", 10);
            vBoxContainer.Alignment = BoxContainer.AlignmentMode.Center;
            panel.AddChild(vBoxContainer);

            // 创建标题标签
            Label titleLabel = new()
            {
                Name = "TitleLabel",
                Text = "选择存档",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 32);
            vBoxContainer.AddChild(titleLabel);

            // 创建间隔
            MarginContainer marginContainer = new()
            {
                Name = "MarginContainer",
                CustomMinimumSize = new Vector2(0, 20)
            };
            vBoxContainer.AddChild(marginContainer);

            // 创建滚动容器
            ScrollContainer scrollContainer = new()
            {
                Name = "ScrollContainer",
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                SizeFlagsVertical = Control.SizeFlags.Fill
            };
            vBoxContainer.AddChild(scrollContainer);

            // 创建存档列表容器
            _saveListContainer = new VBoxContainer
            {
                Name = "SaveListContainer",
                SizeFlagsHorizontal = Control.SizeFlags.Fill
            };
            scrollContainer.AddChild(_saveListContainer);

            // 创建返回按钮
            _backButton = new Button
            {
                Name = "BackButton",
                Text = "返回",
                CustomMinimumSize = new Vector2(0, 50)
            };
            _backButton.AddThemeFontSizeOverride("font_size", 20);
            _backButton.Pressed += OnBackButtonPressed;
            vBoxContainer.AddChild(_backButton);
        }

        // 设置按钮点击事件
        private void OnSettingsButtonPressed()
        {
            Log.Info("Settings button pressed");
            // 隐藏主菜单，显示设置页面
            _mainButton.Visible = false;
            _settingMenu.Visible = true;
        }

        // 设置页面返回按钮点击事件
        private void OnSettingBackButtonPressed()
        {
            Log.Info("Setting back button pressed");
            // 显示主菜单，隐藏设置页面
            _settingMenu.Visible = false;
            _mainButton.Visible = true;
        }

        // 主音量变更事件
        private void OnMasterVolumeChanged(double value)
        {
            Log.Info($"Master volume changed to: {value}");
            ConfigManager.Instance.SetMasterVolume((float)value);
            
            // 应用音量设置到AudioStreamPlayer
            AudioStreamPlayer audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
            if (audioPlayer != null)
            {
                audioPlayer.VolumeDb = Mathf.LinearToDb((float)value);
            }
        }

        // 音乐音量变更事件
        private void OnMusicVolumeChanged(double value)
        {
            Log.Info($"Music volume changed to: {value}");
            ConfigManager.Instance.SetMusicVolume((float)value);
        }

        // 音效音量变更事件
        private void OnSFXVolumeChanged(double value)
        {
            Log.Info($"SFX volume changed to: {value}");
            ConfigManager.Instance.SetSoundEffectVolume((float)value);
        }

        // 语音音量变更事件
        private void OnVoiceVolumeChanged(double value)
        {
            Log.Info($"Voice volume changed to: {value}");
            ConfigManager.Instance.SetVoiceVolume((float)value);
        }

        // 亮度变更事件
        private void OnBrightnessChanged(double value)
        {
            Log.Info($"Brightness changed to: {value}");
            ConfigManager.Instance.SetBrightness((float)value);
        }

        // 对比度变更事件
        private void OnContrastChanged(double value)
        {
            Log.Info($"Contrast changed to: {value}");
            ConfigManager.Instance.SetContrast((float)value);
        }

        // 饱和度变更事件
        private void OnSaturationChanged(double value)
        {
            Log.Info($"Saturation changed to: {value}");
            ConfigManager.Instance.SetSaturation((float)value);
        }

        // 全屏切换事件
        private void OnFullscreenToggled(bool toggled)
        {
            Log.Info($"Fullscreen toggled to: {toggled}");
            ConfigManager.Instance.SetFullscreen(toggled);
        }

        // 垂直同步切换事件
        private void OnVSyncToggled(bool toggled)
        {
            Log.Info($"VSync toggled to: {toggled}");
            ConfigManager.Instance.SetVSync(toggled);
        }

        // 自动保存切换事件
        private void OnAutoSaveToggled(bool toggled)
        {
            Log.Info($"Auto save toggled to: {toggled}");
            ConfigManager.Instance.SetAutoSave(toggled);
        }

        // 文本速度变更事件
        private void OnTextSpeedChanged(double value)
        {
            Log.Info($"Text speed changed to: {value}");
            ConfigManager.Instance.SetTextSpeed((float)value);
        }

        // 显示FPS切换事件
        private void OnShowFPSToggled(bool toggled)
        {
            Log.Info($"Show FPS toggled to: {toggled}");
            ConfigManager.Instance.SetShowFPS(toggled);
        }

        // 退出按钮点击事件
        private void OnExitButtonPressed()
        {
            Log.Info("Exit button pressed");
            // 退出游戏
            GetTree().Quit();
        }

        // 返回按钮点击事件
        private void OnBackButtonPressed()
        {
            // 隐藏存档列表
            _saveListPanel.Visible = false;
        }

        // 清空存档列表
        private void ClearSaveList()
        {
            foreach (Node child in _saveListContainer.GetChildren())
            {
                child.QueueFree();
            }
        }

        // 创建存档项
        private void CreateSaveItem(SaveInfo saveInfo)
        {
            // 创建存档项容器
            Panel saveItemPanel = new()
            {
                CustomMinimumSize = new Vector2(0, 120)
            };
            saveItemPanel.AddThemeConstantOverride("margin_bottom", 10);
            _saveListContainer.AddChild(saveItemPanel);

            // 创建垂直容器
            VBoxContainer saveItemVBox = new();
            saveItemVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            saveItemVBox.AddThemeConstantOverride("margin_left", 10);
            saveItemVBox.AddThemeConstantOverride("margin_top", 10);
            saveItemVBox.AddThemeConstantOverride("margin_right", 10);
            saveItemVBox.AddThemeConstantOverride("margin_bottom", 10);
            saveItemPanel.AddChild(saveItemVBox);

            // 存档名称
            Label nameLabel = new()
            {
                Text = saveInfo.SaveName
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 24);
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            saveItemVBox.AddChild(nameLabel);

            // 存档信息
            Label infoLabel = new();
            string playTime = FormatPlayTime(saveInfo.PlayTime);
            infoLabel.Text = $"等级: {saveInfo.AveragePlayerLevel} | 游戏时间: {playTime} | 保存时间: {saveInfo.SaveTime:yyyy-MM-dd HH:mm}";
            infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            infoLabel.AddThemeFontSizeOverride("font_size", 16);
            saveItemVBox.AddChild(infoLabel);

            // 操作按钮容器
            HBoxContainer buttonContainer = new()
            {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            buttonContainer.AddThemeConstantOverride("margin_top", 10);
            saveItemVBox.AddChild(buttonContainer);

            // 加载按钮
            Button loadButton = new()
            {
                Text = "加载",
                CustomMinimumSize = new Vector2(100, 30)
            };
            loadButton.AddThemeFontSizeOverride("font_size", 16);
            loadButton.Pressed += () => OnLoadSavePressed(saveInfo.SaveId);
            buttonContainer.AddChild(loadButton);

            // 间隔
            Control spacer = new()
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            buttonContainer.AddChild(spacer);

            // 删除按钮
            Button deleteButton = new()
            {
                Text = "删除",
                CustomMinimumSize = new Vector2(100, 30)
            };
            deleteButton.AddThemeFontSizeOverride("font_size", 16);
            deleteButton.Modulate = Colors.Red;
            deleteButton.Pressed += () => OnDeleteSavePressed(saveInfo.SaveId);
            buttonContainer.AddChild(deleteButton);

            // 添加点击事件
            saveItemPanel.MouseEntered += () =>
            {
                saveItemPanel.Modulate = new Color(1f, 1f, 1f, 0.8f);
            };
            saveItemPanel.MouseExited += () =>
            {
                saveItemPanel.Modulate = new Color(1f, 1f, 1f, 1f);
            };
        }

        // 格式化游戏时间
        private static string FormatPlayTime(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = seconds % 3600 / 60;
            int secs = seconds % 60;

            return hours > 0 ? $"{hours}h {minutes}m" : minutes > 0 ? $"{minutes}m {secs}s" : $"{secs}s";
        }

        // 加载存档按钮点击事件
        private static void OnLoadSavePressed(string saveId)
        {
            Log.Info($"Load save pressed for slot {saveId}");
            // TODO: 实现加载存档功能
            ShowToast($"加载存档 {saveId} 功能尚未实现");
        }

        // 删除存档按钮点击事件
        private static void OnDeleteSavePressed(string saveId)
        {
            Log.Info($"Delete save pressed for slot {saveId}");
            // TODO: 实现删除存档功能
            ShowToast($"删除存档 {saveId} 功能尚未实现");
        }

        // 显示提示信息
        private static void ShowToast(string message)
        {
            // 这里可以实现一个简单的提示信息显示
            Log.Info($"Toast: {message}");
        }
    }
}
