using Godot;
using System;
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
		/// 物品列表
		/// </summary>
		private ItemList _itemList;
		
		/// <summary>
		/// 物品描述
		/// </summary>
		private Label _itemDescription;
		
		/// <summary>
		/// 玩家实例
		/// </summary>
		private Player _player;

		/// <summary>
		/// 节点就绪时的回调
		/// 初始化面板、UI元素、按钮连接和设置
		/// </summary>
		public override void _Ready()
		{
			// 初始化面板引用
			InitializePanels();

			// 初始化UI元素引用
			InitializeUIReferences();

			// 尝试获取玩家实例
			FindPlayer();

			// 连接按钮信号
			ConnectButtons();

			// 初始化设置面板
			InitializeSettingsPanel();

			// 显示主菜单面板
			ShowMainMenu();
		}

		/// <summary>
		/// 初始化面板引用
		/// 获取所有面板的引用并设置默认可见性
		/// </summary>
		private void InitializePanels()
		{
			_mainMenuPanel = GetNode<Control>("VBoxContainer");
			_inventoryPanel = GetNode<Control>("InventoryPanel");
			_mapPanel = GetNode<Control>("MapPanel");
			_statusPanel = GetNode<Control>("StatusPanel");
			_equipmentPanel = GetNode<Control>("EquipmentPanel");
			_settingsPanel = GetNode<Control>("SettingsPanel");

			// 默认隐藏所有面板，除了主菜单
			_inventoryPanel.Visible = false;
			_mapPanel.Visible = false;
			_statusPanel.Visible = false;
			_equipmentPanel.Visible = false;
			_settingsPanel.Visible = false;
		}

		/// <summary>
		/// 初始化UI元素引用
		/// 获取物品列表和描述的引用并连接信号
		/// </summary>
		private void InitializeUIReferences()
		{
			// 获取物品列表和描述
			_itemList = _inventoryPanel.GetNode<ItemList>("VBoxContainer/ItemList");
			_itemDescription = _inventoryPanel.GetNode<Label>("VBoxContainer/ItemDescription");

			// 连接物品列表信号
			if (_itemList != null)
			{
				_itemList.ItemSelected += OnItemSelected;
			}
		}

		/// <summary>
		/// 查找玩家实例
		/// 尝试从场景树或SaveManager获取玩家实例
		/// </summary>
		private void FindPlayer()
		{
			// 尝试从场景树中获取玩家实例
			var root = GetTree().Root;
			_player = null;

			// 简化查找过程，直接使用SaveManager中的玩家实例
			if (SaveManager.Instance != null)
			{
				// 这里可以从SaveManager获取玩家实例
				// 或者使用其他方式获取玩家
				Log.Info("Using SaveManager to find player in PopupMenu");
			}

			if (_player != null)
			{
				Log.Info("Player found in PopupMenu");
				UpdateInventoryList();
				UpdateStatusPanel();
			}
			else
			{
				Log.Info("Player not found in PopupMenu");
			}
		}

		/// <summary>
		/// 连接按钮信号
		/// 连接所有面板的按钮信号
		/// </summary>
		private void ConnectButtons()
		{
			// 主菜单按钮
			var inventoryButton = GetNode<Button>("VBoxContainer/GridContainer/InventoryButton");
			var mapButton = GetNode<Button>("VBoxContainer/GridContainer/MapButton");
			var statusButton = GetNode<Button>("VBoxContainer/GridContainer/StatusButton");
			var equipmentButton = GetNode<Button>("VBoxContainer/GridContainer/EquipmentButton");
			var settingsButton = GetNode<Button>("VBoxContainer/GridContainer/SettingsButton");
			var saveButton = GetNode<Button>("VBoxContainer/GridContainer/SaveButton");
			var backButton = GetNode<Button>("VBoxContainer/GridContainer/BackButton");
			var exitButton = GetNode<Button>("VBoxContainer/GridContainer/ExitButton");

			// 连接主菜单按钮信号
			inventoryButton.Pressed += OnInventoryButtonPressed;
			mapButton.Pressed += OnMapButtonPressed;
			statusButton.Pressed += OnStatusButtonPressed;
			equipmentButton.Pressed += OnEquipmentButtonPressed;
			settingsButton.Pressed += OnSettingsButtonPressed;
			saveButton.Pressed += OnSaveButtonPressed;
			backButton.Pressed += OnBackButtonPressed;
			exitButton.Pressed += OnExitButtonPressed;

			// 连接各个面板的返回按钮
			var inventoryBackButton = _inventoryPanel.GetNode<Button>("VBoxContainer/BackButton");
			var mapBackButton = _mapPanel.GetNode<Button>("VBoxContainer/BackButton");
			var statusBackButton = _statusPanel.GetNode<Button>("VBoxContainer/BackButton");
			var equipmentBackButton = _equipmentPanel.GetNode<Button>("VBoxContainer/BackButton");
			var settingsBackButton = _settingsPanel.GetNode<Button>("VBoxContainer/BackButton");

			inventoryBackButton.Pressed += OnBackButtonPressed;
			mapBackButton.Pressed += OnBackButtonPressed;
			statusBackButton.Pressed += OnBackButtonPressed;
			equipmentBackButton.Pressed += OnBackButtonPressed;
			settingsBackButton.Pressed += OnBackButtonPressed;

			// 连接设置面板按钮
			var saveSettingsButton = _settingsPanel.GetNode<Button>("VBoxContainer/HBoxContainer4/SaveButton");
			var resetSettingsButton = _settingsPanel.GetNode<Button>("VBoxContainer/HBoxContainer4/ResetButton");

			saveSettingsButton.Pressed += OnSaveSettingsButtonPressed;
			resetSettingsButton.Pressed += OnResetSettingsButtonPressed;
		}

		/// <summary>
		/// 初始化设置面板
		/// 设置图形质量选项
		/// </summary>
		private void InitializeSettingsPanel()
		{
			// 初始化设置面板的选项
			var optionButton = _settingsPanel.GetNode<OptionButton>("VBoxContainer/VBoxContainer/OptionButton");
			optionButton.Clear();
			optionButton.AddItem(TranslationServer.Translate("low"));
			optionButton.AddItem(TranslationServer.Translate("medium"));
			optionButton.AddItem(TranslationServer.Translate("high"));
			optionButton.AddItem(TranslationServer.Translate("ultra"));
			optionButton.Selected = 2; // 默认选择 High
		}

		/// <summary>
		/// 显示主菜单
		/// </summary>
		private void ShowMainMenu()
		{
			// 隐藏所有面板
			_inventoryPanel.Visible = false;
			_mapPanel.Visible = false;
			_statusPanel.Visible = false;
			_equipmentPanel.Visible = false;
			_settingsPanel.Visible = false;

			// 显示主菜单
			_mainMenuPanel.Visible = true;
		}

		/// <summary>
		/// 显示指定面板
		/// </summary>
		/// <param name="panel">要显示的面板</param>
		private void ShowPanel(Control panel)
		{
			// 隐藏所有面板
			_mainMenuPanel.Visible = false;
			_inventoryPanel.Visible = false;
			_mapPanel.Visible = false;
			_statusPanel.Visible = false;
			_equipmentPanel.Visible = false;
			_settingsPanel.Visible = false;

			// 显示指定面板
			panel.Visible = true;
		}

		/// <summary>
		/// 更新物品列表
		/// </summary>
		private void UpdateInventoryList()
		{
			if (_player != null && _player.Inventory != null)
			{
				_itemList.Clear();
				foreach (var item in _player.Inventory)
				{
					_itemList.AddItem($"{item.Key} x{item.Value}");
				}
			}
		}

		/// <summary>
		/// 更新状态面板
		/// </summary>
		private void UpdateStatusPanel()
		{
			if (_player != null)
			{
				var nameLabel = _statusPanel.GetNode<Label>("VBoxContainer/PlayerStatus/NameLabel");
				var healthLabel = _statusPanel.GetNode<Label>("VBoxContainer/PlayerStatus/HealthLabel");
				var manaLabel = _statusPanel.GetNode<Label>("VBoxContainer/PlayerStatus/ManaLabel");
				var levelLabel = _statusPanel.GetNode<Label>("VBoxContainer/PlayerStatus/LevelLabel");

				nameLabel.Text = _player.CreatureName;
				healthLabel.Text = string.Format(TranslationServer.Translate("health_label"), _player.Health.ToString("F0"), _player.MaxHealth.ToString("F0"));
				manaLabel.Text = string.Format(TranslationServer.Translate("mana_label"), _player.Mana.ToString("F0"), _player.MaxMana.ToString("F0"));
				levelLabel.Text = string.Format(TranslationServer.Translate("level_label"), _player.Level);
			}
		}

		/// <summary>
		/// 物品选择事件
		/// </summary>
		/// <param name="index">物品索引</param>
		private void OnItemSelected(long index)
		{
			if (_player != null && _player.Inventory != null && index < _player.Inventory.Count)
			{
				var item = _player.Inventory.ElementAt((int)index);
				_itemDescription.Text = string.Format(TranslationServer.Translate("item_description"), item.Key, item.Value);
			}
		}

		/// <summary>
		/// 背包按钮点击事件
		/// </summary>
		private void OnInventoryButtonPressed()
		{
			ShowPanel(_inventoryPanel);
		}

		/// <summary>
		/// 地图按钮点击事件
		/// </summary>
		private void OnMapButtonPressed()
		{
			ShowPanel(_mapPanel);
		}

		/// <summary>
		/// 状态按钮点击事件
		/// </summary>
		private void OnStatusButtonPressed()
		{
			UpdateStatusPanel();
			ShowPanel(_statusPanel);
		}

		/// <summary>
		/// 装备按钮点击事件
		/// </summary>
		private void OnEquipmentButtonPressed()
		{
			ShowPanel(_equipmentPanel);
		}

		/// <summary>
		/// 设置按钮点击事件
		/// </summary>
		private void OnSettingsButtonPressed()
		{
			ShowPanel(_settingsPanel);
		}

		/// <summary>
		/// 保存按钮点击事件
		/// </summary>
		private void OnSaveButtonPressed()
		{
			//TODO: 在保存游戏时可以任意选择存档。
			// 保存游戏
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
							playerData.Position = new System.Numerics.Vector2(_player.Position.X, _player.Position.Y);
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
			// 检查当前是否显示的是主菜单
			if (_mainMenuPanel.Visible)
			{
				// 如果是主菜单，则关闭整个弹窗
				GameViewManager.ClosePopup();
			}
			else
			{
				// 如果是子面板，则返回到主菜单
				ShowMainMenu();
			}
		}

		/// <summary>
		/// 处理未处理的输入事件
		/// </summary>
		/// <param name="@event">输入事件</param>
		public override void _UnhandledInput(InputEvent @event)
		{
			// 处理Esc键输入
			if (@event.IsActionPressed("ui_cancel"))
			{
				OnBackButtonPressed();
			}
		}

		/// <summary>
		/// 退出按钮点击事件
		/// </summary>
		private void OnExitButtonPressed()
		{
			// 退出到标题界面
			GameViewManager.SwitchScene("start");
		}

		/// <summary>
		/// 保存设置按钮点击事件
		/// </summary>
		private void OnSaveSettingsButtonPressed()
		{
			// 保存设置
			ShowToast(TranslationServer.Translate("settings_saved"));
		}

		/// <summary>
		/// 重置设置按钮点击事件
		/// </summary>
		private void OnResetSettingsButtonPressed()
		{
			// 重置设置到默认值
			ShowToast(TranslationServer.Translate("settings_reset"));
		}

		/// <summary>
		/// 显示提示信息
		/// </summary>
		/// <param name="message">提示信息内容</param>
		private void ShowToast(string message)
		{
			// 创建一个简单的提示信息
			var toast = new Label
			{
				Text = message,
				Modulate = new Color(1, 1, 1, 0),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			AddChild(toast);
			toast.AnchorBottom = 0.1f;
			toast.AnchorLeft = 0.3f;
			toast.AnchorRight = 0.7f;
			toast.AnchorTop = 0.05f;

			// 添加淡入淡出动画
			var tween = CreateTween();
			tween.TweenProperty(toast, "modulate:a", 1, 0.5f);
			tween.TweenInterval(1.0f);
			tween.TweenProperty(toast, "modulate:a", 0, 0.5f);
			tween.Finished += () => toast.QueueFree();
		}
	}
}
