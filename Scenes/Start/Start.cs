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
				_centerContainer2 = GetNode<CenterContainer>("CanvasLayer/CenterContainer2");
				_settingMenu = GetNode<VBoxContainer>("CanvasLayer/CenterContainer2/SettingMenu");
				_settingBackButton = GetNode<Button>("CanvasLayer/CenterContainer2/SettingMenu/BackButton");
				
				// 音量设置节点
				_masterVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/MasterVolumeHBox/MasterVolumeSlider");
				_musicVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/MusicVolumeHBox/MusicVolumeSlider");
				_sfxVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/SFXVolumeHBox/SFXVolumeSlider");
				_voiceVolumeSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/AudioSection/VoiceVolumeHBox/VoiceVolumeSlider");
				
				// 图形设置节点
				_brightnessSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/BrightnessHBox/BrightnessSlider");
				_contrastSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/ContrastHBox/ContrastSlider");
				_saturationSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/SaturationHBox/SaturationSlider");
				_fullscreenToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/FullscreenHBox/FullscreenToggle");
				_vsyncToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GraphicsSection/VSyncHBox/VSyncToggle");
				
				// 游戏设置节点
				_autoSaveToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GameSection/AutoSaveHBox/AutoSaveToggle");
				_textSpeedSlider = GetNode<HSlider>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GameSection/TextSpeedHBox/TextSpeedSlider");
				_showFPSToggle = GetNode<CheckButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/GameSection/ShowFPSHBox/ShowFPSToggle");
				
				// 语言设置节点
				_languageSelector = GetNode<OptionButton>("CanvasLayer/CenterContainer2/SettingMenu/ScrollContainer/SettingsContainer/LanguageSection/LanguageSelectorHBox/LanguageSelector");

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
				
				// 语言设置信号
				_languageSelector.ItemSelected += OnLanguageSelected;

				Log.Info("All signals connected successfully");

				// 确保ConfigManager初始化
				ConfigManager.GetInstance();
				
				// 从配置加载设置
				LoadSettings();

				// 初始化存档列表UI
				InitializeSaveListUI();
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
				
				// 初始化语言选择器
				if (_languageSelector != null)
				{
					// 清除现有选项
					_languageSelector.Clear();
					// 添加语言选项
					_languageSelector.AddItem("简体中文");
					_languageSelector.AddItem("English");
					// _languageSelector.AddItem("日本語");
					// 从配置加载语言设置
					string savedLanguage = ConfigManager.Instance.CurrentConfig.Language;
					int selectedIndex = savedLanguage == "en_US" ? 1 : 0;
					_languageSelector.Selected = selectedIndex;
					// 应用语言设置
					ConfigManager.Instance.SetLanguage(savedLanguage);
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
			ShowSaveList();

		}

		// 显示存档列表
		private void ShowSaveList()
		{
			// 清空存档列表
			ClearSaveList();
			
			// 获取所有存档信息
			List<SaveInfo> saveInfos = SaveManager.Instance.GetAllSaveInfos();
			Log.Info($"Found {saveInfos.Count} saves");
			if (saveInfos.Count == 0)
			{
				// 如果没有存档，显示提示
				Label noSaveLabel = new()
				{
					Text = TranslationServer.Translate("no_saves"),
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

		// 初始化存档列表UI
		private void InitializeSaveListUI()
		{
			// 从场景中获取存档列表UI元素
			_saveListPanel = GetNode<ColorRect>("CanvasLayer/SaveListPanel");
			_saveListContainer = GetNode<VBoxContainer>("CanvasLayer/SaveListPanel/CenterContainer/Panel/VBoxContainer/ScrollContainer/SaveListContainer");
			_backButton = GetNode<Button>("CanvasLayer/SaveListPanel/CenterContainer/Panel/VBoxContainer/BackButton");

			// 连接返回按钮信号
			_backButton.Pressed += OnBackButtonPressed;
		}

		// 设置按钮点击事件
		private void OnSettingsButtonPressed()
		{
			Log.Info("Settings button pressed");
			// 隐藏主菜单，显示设置页面
			_mainButton.Visible = false;
			_centerContainer2.Visible = true;
			_settingMenu.Visible = true;
		}

		// 设置页面返回按钮点击事件
		private void OnSettingBackButtonPressed()
		{
			Log.Info("Setting back button pressed");
			// 显示主菜单，隐藏设置页面
			_settingMenu.Visible = false;
			_centerContainer2.Visible = false;
			_mainButton.Visible = true;
			ConfigManager.Instance.SaveConfig();
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

		// 语言选择事件
		private void OnLanguageSelected(long index)
		{
			string language = index == 0 ? "zh_CN" : "en_US";
			Log.Info($"Language selected: {language}");
			ConfigManager.Instance.SetLanguage(language);
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

			// 创建CenterContainer用于居中显示存档信息
			CenterContainer centerContainer = new();
			centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			saveItemPanel.AddChild(centerContainer);

			// 创建垂直容器
			VBoxContainer saveItemVBox = new()
			{
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
			};
			saveItemVBox.AddThemeConstantOverride("margin_left", 10);
			saveItemVBox.AddThemeConstantOverride("margin_top", 10);
			saveItemVBox.AddThemeConstantOverride("margin_right", 10);
			saveItemVBox.AddThemeConstantOverride("margin_bottom", 10);
			centerContainer.AddChild(saveItemVBox);

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
			string levelText = TranslationServer.Translate("level");
			string playTimeText = TranslationServer.Translate("play_time");
			string saveTimeText = TranslationServer.Translate("save_time");
			infoLabel.Text = $"{levelText}: {saveInfo.AveragePlayerLevel} | {playTimeText}: {playTime} | {saveTimeText}: {saveInfo.SaveTime:yyyy-MM-dd HH:mm}";
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
				Text = TranslationServer.Translate("load"),
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
				Text = TranslationServer.Translate("delete"),
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
		private void OnLoadSavePressed(string saveId)
		{
			Log.Info($"Load save pressed for slot {saveId}");
			// 加载存档
			SaveData saveData = SaveManager.Instance.LoadGame(saveId);
			if (saveData != null)
			{
				// 隐藏存档列表
				_saveListPanel.Visible = false;
				// 切换到存档中的场景
				string targetScene = saveData.CurrentScene;
				Log.Info($"Loading scene: {targetScene}");
				_ = GameViewManager.SwitchScene(targetScene);
			}
			else
			{
				string failedText = TranslationServer.Translate("load_save_failed");
				ShowToast($"{failedText} {saveId}");
			}
		}

		// 删除存档按钮点击事件
		private void OnDeleteSavePressed(string saveId)
		{
			Log.Info($"Delete save pressed for slot {saveId}");
			// 删除存档
			bool success = SaveManager.Instance.DeleteSave(saveId);
			if (success)
			{
				string successText = TranslationServer.Translate("delete_save_success");
				ShowToast($"{successText} {saveId}");
				// 重新显示存档列表
				ShowSaveList();
			}
			else
			{
				string failedText = TranslationServer.Translate("delete_save_failed");
				ShowToast($"{failedText} {saveId}");
			}
		}

		// 显示提示信息
		private void ShowToast(string message)
		{
			// 这里可以实现一个简单的提示信息显示
			Log.Info($"Toast: {message}");
		}
	}
}
