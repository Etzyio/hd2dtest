using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
	/// <summary>
	/// 弹出菜单类
	/// 处理游戏内的菜单功能，包括背包、地图、状态、装备和设置
	/// </summary>
	public partial class PopupMenu : Control
	{
		/// <summary>
		/// 背包面板
		/// </summary>
		private Control _inventoryPanel;
		
		/// <summary>
		/// 地图面板
		/// </summary>
		private Control _mapPanel;
		
		/// <summary>
		/// 状态面板
		/// </summary>
		private Control _statusPanel;
		
		/// <summary>
		/// 装备面板
		/// </summary>
		private Control _equipmentPanel;
		
		/// <summary>
		/// 设置面板
		/// </summary>
		private Control _settingsPanel;
		
		/// <summary>
		/// 主菜单面板
		/// </summary>
		private Control _mainMenuPanel;
		
		/// <summary>
		/// 主菜单按钮
		/// </summary>
		private Button _inventoryButton;
		
		/// <summary>
		/// 地图按钮
		/// </summary>
		private Button _mapButton;
		
		/// <summary>
		/// 状态按钮
		/// </summary>
		private Button _statusButton;
		
		/// <summary>
		/// 装备按钮
		/// </summary>
		private Button _equipmentButton;
		
		/// <summary>
		/// 设置按钮
		/// </summary>
		private Button _settingsButton;
		
		/// <summary>
		/// 保存按钮
		/// </summary>
		private Button _saveButton;
		
		/// <summary>
		/// 返回按钮
		/// </summary>
		private Button _backButton;
		
		/// <summary>
		/// 玩家实例
		/// </summary>
		private Player _player;
		
		/// <summary>
		/// 存档选择器
		/// </summary>
		private SaveSlotSelector _saveSlotSelector;

		/// <summary>
		/// 初始化方法，在节点进入场景树时调用
		/// </summary>
		public override void _Ready()
		{
			// 初始化面板
			InitializePanels();
			
			// 初始化UI元素引用
			InitializeUIReferences();

			// 尝试获取玩家实例
			FindPlayer();

			// 初始化存档选择器
			InitializeSaveSlotSelector();

			// 连接按钮信号
			ConnectButtons();

			// 初始化设置面板
			InitializeSettingsPanel();

			// 显示主菜单面板
			ShowMainMenu();
		}

		/// <summary>
		/// 初始化存档选择器
		/// </summary>
		private void InitializeSaveSlotSelector()
		{
			_saveSlotSelector = new SaveSlotSelector
			{
				Name = "SaveSlotSelector",
				Visible = false,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			
			AddChild(_saveSlotSelector);
			
			// 连接存档选择器的信号
			_saveSlotSelector.SaveSlotSelected += OnSaveSlotSelected;
			_saveSlotSelector.SaveSlotDeleted += OnSaveSlotDeleted;
		}

		/// <summary>
		/// 存档槽位选择事件
		/// </summary>
		private void OnSaveSlotSelected(string slotId)
		{
			// 使用Godot的日志系统
			GD.Print($"Selected save slot: {slotId}");
			
			// 执行保存操作
			PerformSaveGame(slotId);
			
			// 隐藏存档选择器
			_saveSlotSelector.HideSelector();
		}

		/// <summary>
		/// 存档删除事件
		/// </summary>
		private void OnSaveSlotDeleted(string slotId)
		{
			// 使用Godot的日志系统
			GD.Print($"Deleted save slot: {slotId}");
			// 可以在这里添加删除成功的提示
			ShowToast(string.Format(TranslationServer.Translate("save_slot_deleted_toast"), slotId));
		}

		/// <summary>
		/// 执行游戏保存
		/// </summary>
		private void PerformSaveGame(string slotId)
		{
			if (SaveManager.Instance != null)
			{
				var saveData = SaveManager.CreateDefaultSaveData(slotId);
				if (saveData != null)
				{
					// 更新玩家数据
					if (_player != null)
					{
						if (saveData.Players.Count > 0)
						{
							var playerData = saveData.Players[0];
							playerData.Health = _player.Health;
							playerData.Mana = _player.Mana;
							playerData.Inventory = _player.Inventory;
							playerData.Position = new Vector2(_player.Position.X, _player.Position.Y);
							
							// 更新扩展属性
							playerData.Level = _player.Level;
							playerData.Experience = _player.Experience;
							playerData.Gold = _player.Gold;
							playerData.KillCount = _player.KillCount;
							playerData.DeathCount = _player.DeathCount;
							playerData.MainClassName = _player.MainClass?.ClassName;
							playerData.SubClassName = _player.SubClass?.ClassName;
							playerData.EquippedPassiveNames = _player.EquippedPassives.Select(p => p.PassiveName).ToList();
							playerData.Attack = (int)_player.Attack;
							playerData.Defense = (int)_player.Defense;
							playerData.Speed = _player.Speed;
							playerData.EquippedWeapon = _player.CurrentWeapon?.WeaponName;
							playerData.EquippedEquipment = _player.Equipments.ToDictionary(e => e.EquipmentTypeValue.ToString(), e => e.EquipmentName);
							playerData.LearnedSkills = _player.Skills.Select(s => s.Id).ToList();
						}
					}

					bool success = SaveManager.Instance.SaveGame(saveData, slotId);
					if (success)
					{
						// 显示保存成功提示
						ShowToast(string.Format(TranslationServer.Translate("save_success_toast"), slotId));
						// 使用Godot的日志系统
						GD.Print($"Game saved successfully to slot: {slotId}");
					}
					else
					{
						// 显示保存失败提示
						ShowToast(TranslationServer.Translate("save_fail_toast"));
						// 使用Godot的日志系统
						GD.PrintErr($"Failed to save game to slot: {slotId}");
					}
				}
			}
		}

		/// <summary>
		/// 初始化面板引用
		/// </summary>
		private void InitializePanels()
		{
			_mainMenuPanel = GetNode<Control>("VBoxContainer");
			_inventoryPanel = GetNode<Control>("InventoryPanel");
			_mapPanel = GetNode<Control>("MapPanel");
			_statusPanel = GetNode<Control>("StatusPanel");
			_equipmentPanel = GetNode<Control>("EquipmentPanel");
			_settingsPanel = GetNode<Control>("SettingsPanel");
			
			// 初始时隐藏所有面板
			_inventoryPanel.Hide();
			_mapPanel.Hide();
			_statusPanel.Hide();
			_equipmentPanel.Hide();
			_settingsPanel.Hide();
		}

		/// <summary>
		/// 初始化UI元素引用
		/// </summary>
		private void InitializeUIReferences()
		{
			_inventoryButton = GetNode<Button>("VBoxContainer/GridContainer/InventoryButton");
			_inventoryButton.Text = TranslationServer.Translate("ui_inventory");
			
			_mapButton = GetNode<Button>("VBoxContainer/GridContainer/MapButton");
			_mapButton.Text = TranslationServer.Translate("ui_map");
			
			_statusButton = GetNode<Button>("VBoxContainer/GridContainer/StatusButton");
			_statusButton.Text = TranslationServer.Translate("ui_status");
			
			_equipmentButton = GetNode<Button>("VBoxContainer/GridContainer/EquipmentButton");
			_equipmentButton.Text = TranslationServer.Translate("ui_equipment");
			
			_settingsButton = GetNode<Button>("VBoxContainer/GridContainer/SettingsButton");
			_settingsButton.Text = TranslationServer.Translate("ui_settings");
			
			_saveButton = GetNode<Button>("VBoxContainer/GridContainer/SaveButton");
			_saveButton.Text = TranslationServer.Translate("ui_save");
			
			_backButton = GetNode<Button>("VBoxContainer/GridContainer/BackButton");
			_backButton.Text = TranslationServer.Translate("ui_back");
		}

		/// <summary>
		/// 查找玩家实例
		/// </summary>
		private void FindPlayer()
		{
			// 尝试从场景中查找玩家节点
			_player = GetTree().Root.GetNodeOrNull<Player>("Main/Player");
			if (_player == null)
			{
				// 使用Godot的日志系统
				GD.PrintErr("Player not found in scene. Some functionality may be limited.");
			}
		}

		/// <summary>
		/// 连接按钮信号
		/// </summary>
		private void ConnectButtons()
		{
			_inventoryButton.Pressed += OnInventoryButtonPressed;
			_mapButton.Pressed += OnMapButtonPressed;
			_statusButton.Pressed += OnStatusButtonPressed;
			_equipmentButton.Pressed += OnEquipmentButtonPressed;
			_settingsButton.Pressed += OnSettingsButtonPressed;
			_saveButton.Pressed += OnSaveButtonPressed;
			_backButton.Pressed += OnBackButtonPressed;
		}

		/// <summary>
		/// 显示主菜单面板
		/// </summary>
		private void ShowMainMenu()
		{
			// 隐藏所有面板
			HideAllPanels();
			
			// 显示主菜单
			_mainMenuPanel.Show();
		}

		/// <summary>
		/// 隐藏所有面板
		/// </summary>
		private void HideAllPanels()
		{
			_mainMenuPanel.Hide();
			_inventoryPanel.Hide();
			_mapPanel.Hide();
			_statusPanel.Hide();
			_equipmentPanel.Hide();
			_settingsPanel.Hide();
			
			// 隐藏存档选择器
			if (_saveSlotSelector != null)
			{
				_saveSlotSelector.HideSelector();
			}
		}

		/// <summary>
		/// 显示指定面板
		/// </summary>
		private void ShowPanel(Control panel)
		{
			HideAllPanels();
			panel.Show();
		}

		/// <summary>
		/// 背包按钮点击事件
		/// </summary>
		private void OnInventoryButtonPressed()
		{
			ShowPanel(_inventoryPanel);
			// 使用Godot的日志系统
			GD.Print("Inventory panel opened");
		}

		/// <summary>
		/// 地图按钮点击事件
		/// </summary>
		private void OnMapButtonPressed()
		{
			ShowPanel(_mapPanel);
			// 使用Godot的日志系统
			GD.Print("Map panel opened");
		}

		/// <summary>
		/// 状态按钮点击事件
		/// </summary>
		private void OnStatusButtonPressed()
		{
			ShowPanel(_statusPanel);
			// 使用Godot的日志系统
			GD.Print("Status panel opened");
		}

		/// <summary>
		/// 装备按钮点击事件
		/// </summary>
		private void OnEquipmentButtonPressed()
		{
			ShowPanel(_equipmentPanel);
			// 使用Godot的日志系统
			GD.Print("Equipment panel opened");
		}

		/// <summary>
		/// 设置按钮点击事件
		/// </summary>
		private void OnSettingsButtonPressed()
		{
			ShowPanel(_settingsPanel);
			// 使用Godot的日志系统
			GD.Print("Settings panel opened");
		}

		/// <summary>
		/// 保存按钮点击事件 - 现在显示存档选择器
		/// </summary>
		private void OnSaveButtonPressed()
		{
			// 显示存档选择器而不是直接保存
			if (_saveSlotSelector != null)
			{
				_saveSlotSelector.ShowSelector();
				// 使用Godot的日志系统
				GD.Print("Save slot selector opened");
			}
			else
			{
				// 如果存档选择器未初始化，回退到原来的保存逻辑
				// 使用Godot的日志系统
				GD.PrintErr("Save slot selector not available, falling back to quick save");
				PerformQuickSave();
			}
		}

		/// <summary>
		/// 快速保存（回退方案）
		/// </summary>
		private void PerformQuickSave()
		{
			if (SaveManager.Instance != null)
			{
				var saveData = SaveManager.CreateDefaultSaveData("quick_save");
				if (saveData != null)
				{
					// 更新玩家数据
					if (_player != null)
					{
						if (saveData.Players.Count > 0)
						{
							var playerData = saveData.Players[0];
							playerData.Health = _player.Health;
							playerData.Mana = _player.Mana;
							playerData.Inventory = _player.Inventory;
							playerData.Position = new Vector2(_player.Position.X, _player.Position.Y);
							
							// 更新扩展属性
							playerData.Level = _player.Level;
							playerData.Experience = _player.Experience;
							playerData.Gold = _player.Gold;
							playerData.KillCount = _player.KillCount;
							playerData.DeathCount = _player.DeathCount;
							playerData.MainClassName = _player.MainClass?.ClassName;
							playerData.SubClassName = _player.SubClass?.ClassName;
							playerData.EquippedPassiveNames = _player.EquippedPassives.Select(p => p.PassiveName).ToList();
							playerData.Attack = (int)_player.Attack;
							playerData.Defense = (int)_player.Defense;
							playerData.Speed = _player.Speed;
							playerData.EquippedWeapon = _player.CurrentWeapon?.WeaponName;
							playerData.EquippedEquipment = _player.Equipments.ToDictionary(e => e.EquipmentTypeValue.ToString(), e => e.EquipmentName);
							playerData.LearnedSkills = _player.Skills.Select(s => s.Id).ToList();
						}
					}

					bool success = SaveManager.Instance.SaveGame(saveData, "quick_save");
					if (success)
					{
						// 显示保存成功提示
						ShowToast(TranslationServer.Translate("game_saved_successfully"));
					}
					else
					{
						// 显示保存失败提示
						ShowToast(TranslationServer.Translate("game_save_failed"));
					}
				}
			}
		}

		/// <summary>
		/// 返回按钮点击事件
		/// </summary>
		private void OnBackButtonPressed()
		{
			// 如果当前显示的是子面板，返回主菜单
			if (_inventoryPanel.Visible || _mapPanel.Visible || _statusPanel.Visible || 
			    _equipmentPanel.Visible || _settingsPanel.Visible || 
			    (_saveSlotSelector != null && _saveSlotSelector.Visible))
			{
				ShowMainMenu();
				// 使用Godot的日志系统
				Log.Info("Returned to main menu");
			}
			else
			{
				// 如果已经在主菜单，关闭整个菜单
				Hide();
				HideAllPanels();
				// 使用Godot的日志系统
				Log.Info("Popup menu closed");
			}
		}

		/// <summary>
		/// 初始化设置面板
		/// </summary>
		private void InitializeSettingsPanel()
		{
			// 这里可以添加设置面板的初始化逻辑
			// 例如：音量控制、画质设置等
		}

		/// <summary>
		/// 显示提示信息
		/// </summary>
		private void ShowToast(string message)
		{
			// 这里可以实现一个Toast提示功能
			// 暂时使用日志记录
			// 使用Godot的日志系统
			GD.Print($"Toast: {message}");
		}

		/// <summary>
		/// 输入事件处理
		/// </summary>
		public override void _Input(InputEvent @event)
		{
			if (!IsVisibleInTree()) return;
			
			if (@event.IsActionPressed("ui_cancel"))
			{
				OnBackButtonPressed();
				GetViewport().SetInputAsHandled();
			}
		}
	}
}