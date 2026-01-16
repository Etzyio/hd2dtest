using Godot;
using System;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Core
{
	public partial class Main : Node
	{
		private VersionManager _versionManager;

		public override void _Ready()
		{
			// 初始化版本管理器
			InitializeVersionManager();

			Log.Info("HD2D Game Initialized - Loading Start Scene");

			// 初始化存档管理器（单例模式）
			InitializeSaveManager();

			// 初始化配置管理器（单例模式）
			InitializeConfigManager();

			// 输出当前存档信息
			PrintCurrentSaveInfo();

			// 注册输入映射 - 使用正确的方法调用
			InputMap.AddAction("open_first_scene");
			InputMap.ActionAddEvent("open_first_scene", new InputEventKey() { Keycode = Key.F1 });
			
			InputMap.AddAction("open_main_scene");
			InputMap.ActionAddEvent("open_main_scene", new InputEventKey() { Keycode = Key.F2 });

			// 加载Start场景到sceneLayer
			SwitchScene("res://Scenes/Start/Start.tscn");
		}

		public override void _Process(double delta)
		{
			// 处理场景切换快捷键
			HandleSceneSwitching();
		}

		private void HandleSceneSwitching()
		{
			if (Input.IsActionJustPressed("open_first_scene"))
			{
				// 直接切换到First场景，不显示弹窗
				SwitchScene("res://Scenes/First/First.tscn");
			}
			else if (Input.IsActionJustPressed("open_main_scene"))
			{
				// 直接切换到Start场景，不显示弹窗
				SwitchScene("res://Scenes/Start/Start.tscn");
			}
		}

		private void InitializeVersionManager()
		{
			// 创建并添加VersionManager节点
			_versionManager = new VersionManager();
			AddChild(_versionManager);

			// 显示版本信息
			Log.Info("=== Version Info ===");
			Log.Info(_versionManager.GetVersionString());
		}

		private void InitializeSaveManager()
		{
			// 使用单例模式获取SaveManager实例
			SaveManager saveManager = SaveManager.GetInstance();

			// 检查实例是否已添加到场景树
			if (saveManager.GetParent() == null)
			{
				AddChild(saveManager);
			}
		}

		private void InitializeConfigManager()
		{
			// 使用单例模式获取ConfigManager实例
			ConfigManager configManager = ConfigManager.GetInstance();

			// 检查实例是否已添加到场景树
			if (configManager.GetParent() == null)
			{
				AddChild(configManager);
			}

			Log.Info("ConfigManager initialized");
		}

		private static void PrintCurrentSaveInfo()
		{
			Log.Info("=== Current Save Info ===");

			// 使用单例模式获取SaveManager实例
			SaveManager saveManager = SaveManager.Instance;

			if (saveManager != null)
			{
				// 获取所有存档信息
				var saveInfos = saveManager.GetAllSaveInfos();

				if (saveInfos.Count == 0)
				{
					Log.Info("No saves found.");
				}
				else
				{
					Log.Info($"Found {saveInfos.Count} save(s):");
					foreach (var info in saveInfos)
					{
						Log.Info($"- Slot {info.SaveId}: {info.SaveName}");
						Log.Info($"  Version: {info.GameVersion}, Players: {info.PlayerCount}, Avg Level: {info.AveragePlayerLevel}, Game Score: {info.GameScore}");
						Log.Info($"  Play Time: {info.PlayTime} seconds, Scene: {info.CurrentScene}");
						Log.Info($"  Saved: {info.SaveTime}");
					}
				}
			}
			else
			{
				Log.Error("SaveManager instance not available.");
			}
		}

		// 场景切换方法 - 只切换sceneLayer中的场景内容
		private void SwitchScene(string scenePath)
		{
			Log.Info($"Switching scene content in sceneLayer to: {scenePath}");
			
			try
			{
				// 获取sceneLayer
				Node sceneLayer = GetNode<Node>("sceneLayer");
				
				// 清除sceneLayer中的所有子节点
				foreach (Node child in sceneLayer.GetChildren())
				{
					child.QueueFree();
				}
				
				// 加载新场景
				PackedScene scene = GD.Load<PackedScene>(scenePath);
				if (scene != null)
				{
					// 实例化场景并添加到sceneLayer
					Node sceneInstance = scene.Instantiate();
					sceneLayer.AddChild(sceneInstance);
					Log.Info($"Successfully loaded scene: {scenePath} into sceneLayer");
				}
				else
				{
					Log.Error($"Failed to load scene: {scenePath}");
				}
			}
			catch (Exception e)
			{
				Log.Error($"Error switching scene: {e.Message}");
			}
		}
	}
}
