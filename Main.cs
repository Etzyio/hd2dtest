using Godot;
using System;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;

namespace hd2dtest
{
	public partial class Main : Node
	{
		private VersionManager _versionManager;
		private Control _sceneLayer;
		private Control _popupLayer;

		public override void _Ready()
		{
			// 初始化版本管理器
			InitializeVersionManager();

			// 初始化存档管理器（单例模式）
			InitializeSaveManager();

			// 初始化配置管理器（单例模式）
			InitializeConfigManager();

			// 获取子节点
			_sceneLayer = GetNode<Control>("sceneLayer");
			_popupLayer = GetNode<Control>("popupLayer");
			// 初始化视图管理器
			GameViewManager.Init(_sceneLayer, _popupLayer);

			Log.Info("HD2D Game Initialized - Loading Start Scene");

			// 切换到开始界面
			GameViewManager.SwitchScene("start");
		}

		public override void _Process(double delta)
		{
		}

		private static void InitializeVersionManager()
		{
		}

		private void InitializeSaveManager()
		{
			// 使用单例模式获取SaveManager实例
			SaveManager saveManager = SaveManager.GetInstance();

			// 检查实例是否已经有父节点，如果没有则添加到场景树
			try
			{
				if (saveManager.GetParent() == null)
				{
					AddChild(saveManager);
				}
			}
			catch (Exception)
			{
				// 如果GetParent()失败，说明实例可能还没有被添加到场景树
				// 但为了安全起见，我们不直接添加，而是记录警告
				Log.Warning("SaveManager already has a parent, skipping AddChild");
			}

			Log.Info("SaveManager initialized successfully");
		}

		private void InitializeConfigManager()
		{
			// 使用单例模式获取ConfigManager实例
			ConfigManager configManager = ConfigManager.GetInstance();

			// 检查实例是否已经有父节点，如果没有则添加到场景树
			try
			{
				if (configManager.GetParent() == null)
				{
					AddChild(configManager);
				}
			}
			catch (Exception)
			{
				// 如果GetParent()失败，说明实例可能还没有被添加到场景树
				// 但为了安全起见，我们不直接添加，而是记录警告
				Log.Warning("ConfigManager already has a parent, skipping AddChild");
			}

			Log.Info("ConfigManager initialized");
		}


	}
}
