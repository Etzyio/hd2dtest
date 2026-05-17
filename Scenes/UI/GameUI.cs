﻿using Godot;
using hd2dtest.Scripts.Managers;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scenes.UI
{
	public partial class GameUI : CanvasLayer
	{
		private VBoxContainer _teammatesContainer;
		private PackedScene _teammateScene;
		private List<Teammate> _teammateUIs = [];

		private Minimap _minimap;
		private InventoryUI _inventoryUI;
		private CharacterStatus _characterStatus;

		private Panel _quickAccessBar;
		private HBoxContainer _quickButtonsContainer;
		private Label _goldLabel;
		private ProgressBar _healthBar;
		private ProgressBar _manaBar;
		private Label _playerNameLabel;

		private bool _uiInitialized = false;

		public override void _Ready()
		{
			InitializeUIReferences();
			CreateCoreUIComponents();
			SetProcess(true);
			_uiInitialized = true;
		}

		public override void _Process(double delta)
		{
			if (!_uiInitialized) return;

			UpdatePlayerStatus();
			UpdateTeammateStatus();
			HandleQuickAccessUpdates();
		}

		private void InitializeUIReferences()
		{
			_teammatesContainer = GetNode<VBoxContainer>("VBoxContainer");
			_teammateScene = GD.Load<PackedScene>("res://Scenes/UI/Teammate.tscn");

			_quickAccessBar = GetNode<Panel>("QuickAccessBar");
			_playerNameLabel = GetNode<Label>("QuickAccessBar/MainHBox/StatusVBox/PlayerNameLabel");
			_healthBar = GetNode<ProgressBar>("QuickAccessBar/MainHBox/StatusVBox/HealthBar");
			_manaBar = GetNode<ProgressBar>("QuickAccessBar/MainHBox/StatusVBox/ManaBar");
			_goldLabel = GetNode<Label>("QuickAccessBar/MainHBox/InfoVBox/GoldLabel");
			_quickButtonsContainer = GetNode<HBoxContainer>("QuickAccessBar/MainHBox/QuickButtonsContainer");
		}

		private void CreateCoreUIComponents()
		{
			CreateMinimap();
			CreateInventoryUI();
			CreateCharacterStatusUI();
			CreateQuickAccessBar();
		}

		private void CreateMinimap()
		{
			_minimap = new Minimap { Name = "Minimap" };

			var playerNode = GetTree()?.CurrentScene?.GetNodeOrNull<Node2D>("Player");
			if (playerNode != null)
			{
				_minimap.Player = playerNode;
			}

			AddChild(_minimap);
			PopulateMinimapMarkers();
		}

		private void PopulateMinimapMarkers()
		{
			if (_minimap == null) return;

			_minimap.AddMarker("quest_start", new Vector2(150, 200), Minimap.MarkerType.Quest, "Main Quest Start");
			_minimap.AddMarker("npc_merchant", new Vector2(-100, 50), Minimap.MarkerType.NPC, "Merchant");
			_minimap.AddMarker("treasure_chest", new Vector2(300, -150), Minimap.MarkerType.Treasure, "Hidden Treasure");
			_minimap.AddMarker("save_shrine", new Vector2(-200, -100), Minimap.MarkerType.SavePoint, "Save Shrine");
			_minimap.AddMarker("boss_arena", new Vector2(400, 300), Minimap.MarkerType.Enemy, "Boss Arena", false);
		}

		private void CreateInventoryUI()
		{
			var inventoryScene = GD.Load<PackedScene>("res://Scenes/UI/InventoryUI.tscn");
			_inventoryUI = inventoryScene.Instantiate<InventoryUI>();

			var playerData = GetPlayerData();
			if (playerData != null)
			{
				_inventoryUI.PlayerData = playerData;
			}

			AddChild(_inventoryUI);
		}

		private void CreateCharacterStatusUI()
		{
			var charScene = GD.Load<PackedScene>("res://Scenes/UI/CharacterStatus.tscn");
			_characterStatus = charScene.Instantiate<CharacterStatus>();

			var playerData = GetPlayerData();
			if (playerData != null)
			{
				_characterStatus.PlayerData = playerData;
			}

			AddChild(_characterStatus);
		}

		private static Player GetPlayerData()
		{
			return GameDataManager.Instance?.Teammates?.Player;
		}

		private void CreateQuickAccessBar()
		{
			var barStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.13f, 0.19f, 0.25f, 0.95f),
				BorderWidthLeft = 2, BorderWidthRight = 2,
				BorderWidthTop = 2, BorderWidthBottom = 2,
				BorderColor = new Color(0.3f, 0.45f, 0.6f, 0.8f),
				CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10,
				CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
				ShadowSize = 6,
				ShadowColor = new Color(0, 0, 0, 0.4f)
			};
			_quickAccessBar.AddThemeStyleboxOverride("panel", barStyle);

			ApplyStatusBarStyles();
			ApplyInfoSectionStyles();
			ApplyQuickButtonStyles();
		}

		private void ApplyStatusBarStyles()
		{
			_playerNameLabel.AddThemeFontSizeOverride("font_size", 12);
			_playerNameLabel.AddThemeColorOverride("font_color", new Color(0.93f, 0.94f, 0.95f, 1f));

			ApplyProgressBarStyle(_healthBar, new Color(0.9f, 0.25f, 0.25f, 1f));
			ApplyProgressBarStyle(_manaBar, new Color(0.3f, 0.5f, 0.95f, 1f));
		}

		private static void ApplyProgressBarStyle(ProgressBar bar, Color color)
		{
			var bgStyle = new StyleBoxFlat
			{
				BgColor = color * new Color(0.25f, 0.25f, 0.25f, 1f),
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
			};
			bar.AddThemeStyleboxOverride("background", bgStyle);

			var fillStyle = new StyleBoxFlat
			{
				BgColor = color,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
			};
			bar.AddThemeStyleboxOverride("fill", fillStyle);
		}

		private void ApplyInfoSectionStyles()
		{
			_goldLabel.AddThemeFontSizeOverride("font_size", 14);
			_goldLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			_goldLabel.AddThemeColorOverride("font_color", new Color(1f, 0.75f, 0.2f, 1f));
		}

		private void ApplyQuickButtonStyles()
		{
			var buttons = new[]
			{
				GetNode<Button>("QuickAccessBar/MainHBox/QuickButtonsContainer/InvButton"),
				GetNode<Button>("QuickAccessBar/MainHBox/QuickButtonsContainer/CharButton"),
				GetNode<Button>("QuickAccessBar/MainHBox/QuickButtonsContainer/MapButton")
			};

			for (int i = 0; i < buttons.Length; i++)
			{
				int index = i;
				buttons[i].Pressed += () => OnQuickButtonPressed(index);
				ApplyQuickButtonStyle(buttons[i]);
			}
		}

		private static void ApplyQuickButtonStyle(Button button)
		{
			Color primaryColor = new(0.2f, 0.6f, 0.86f, 1f);

			var normalStyle = new StyleBoxFlat
			{
				BgColor = primaryColor * new Color(0.7f, 0.7f, 0.7f, 1f),
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
			};
			button.AddThemeStyleboxOverride("normal", normalStyle);

			var hoverStyle = new StyleBoxFlat
			{
				BgColor = primaryColor * new Color(1f, 1f, 1f, 1f),
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
			};
			button.AddThemeStyleboxOverride("hover", hoverStyle);

			var pressedStyle = new StyleBoxFlat
			{
				BgColor = primaryColor * new Color(0.5f, 0.5f, 0.5f, 1f),
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
			};
			button.AddThemeStyleboxOverride("pressed", pressedStyle);

			button.AddThemeFontSizeOverride("font_size", 18);
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event.IsActionPressed("quick_action_1"))
			{
				ToggleInventory();
			}
			else if (@event.IsActionPressed("quick_action_2"))
			{
				ToggleCharacterStatus();
			}
			else if (@event.IsActionPressed("map"))
			{
				ToggleMinimap();
			}
			else if (@event.IsActionPressed("cancel"))
			{
				CloseAllPanels();
			}
		}

		private void OnQuickButtonPressed(int index)
		{
			switch (index)
			{
				case 0: ToggleInventory(); break;
				case 1: ToggleCharacterStatus(); break;
				case 2: ToggleMinimap(); break;
			}
		}

		public void ToggleInventory()
		{
			if (_inventoryUI != null)
			{
				_inventoryUI.ToggleVisibility();

				var playerData = GetPlayerData();
				if (_inventoryUI.Visible && playerData != null)
				{
					_inventoryUI.LoadInventory(playerData);
				}
			}
		}

		public void ToggleCharacterStatus()
		{
			if (_characterStatus != null)
			{
				_characterStatus.ToggleVisibility();

				var playerData = GetPlayerData();
				if (_characterStatus.Visible && playerData != null)
				{
					_characterStatus.LoadCharacterData(playerData);
				}
			}
		}

		public void ToggleMinimap()
		{
			if (_minimap != null)
			{
				_minimap.Visible = !_minimap.Visible;
			}
		}

		public void CloseAllPanels()
		{
			if (_inventoryUI != null && _inventoryUI.Visible)
			{
				_inventoryUI.ToggleVisibility();
			}

			if (_characterStatus != null && _characterStatus.Visible)
			{
				_characterStatus.ToggleVisibility();
			}
		}

		private void UpdatePlayerStatus()
		{
			var playerData = GetPlayerData();
			if (playerData == null) return;

			if (_healthBar != null)
			{
				_healthBar.Value = playerData.Health;
				_healthBar.MaxValue = playerData.MaxHealth;
			}

			if (_manaBar != null)
			{
				_manaBar.Value = playerData.Mana;
				_manaBar.MaxValue = playerData.MaxMana;
			}

			if (_playerNameLabel != null)
			{
				_playerNameLabel.Text = $"{playerData.CreatureName} (Lv.{playerData.Level})";
			}

			if (_goldLabel != null)
			{
				_goldLabel.Text = $"💰 {playerData.Gold}";
			}
		}

		private void UpdateTeammateStatus()
		{
			var teammates = GameDataManager.Instance.Teammates.Get();

			if (teammates.Count != _teammateUIs.Count)
			{
				RefreshTeammateUI();
				return;
			}

			for (int i = 0; i < teammates.Count; i++)
			{
				if (i < _teammateUIs.Count && teammates[i] != null)
				{
					var teammate = teammates[i];
					_teammateUIs[i].UpdateStatus(
						teammate.CreatureName,
						teammate.Health,
						teammate.MaxHealth,
						teammate.Mana,
						teammate.MaxMana
					);
				}
			}
		}

		private void HandleQuickAccessUpdates() { }

		public void RefreshTeammateUI()
		{
			foreach (var ui in _teammateUIs)
			{
				ui.QueueFree();
			}
			_teammateUIs.Clear();

			var teammates = GameDataManager.Instance.Teammates.Get();
			foreach (var teammate in teammates)
			{
				var teammateInstance = _teammateScene.Instantiate<Teammate>();
				_teammatesContainer.AddChild(teammateInstance);
				_teammateUIs.Add(teammateInstance);

				teammateInstance.UpdateStatus(
					teammate.CreatureName,
					teammate.Health,
					teammate.MaxHealth,
					teammate.Mana,
					teammate.MaxMana
				);
			}
		}

		public void SetBattleMode(bool inBattle)
		{
			if (_minimap != null)
			{
				_minimap.SetBattleMode(inBattle);
			}

			if (inBattle)
			{
				CloseAllPanels();
			}
		}

		public void AddMapMarker(string id, Vector2 position, Minimap.MarkerType type, string label = "", bool isVisible = true)
		{
			_minimap?.AddMarker(id, position, type, label, isVisible);
		}

		public void RemoveMapMarker(string id)
		{
			_minimap?.RemoveMarker(id);
		}

		public InventoryUI GetInventoryUI() => _inventoryUI;
		public CharacterStatus GetCharacterStatusUI() => _characterStatus;
		public Minimap GetMinimap() => _minimap;
	}
}
