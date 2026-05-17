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
			SetupInputHandling();
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

			var playerNode = GetTree()?.CurrentScene?.GetNode<Node2D>("Player");
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
			_inventoryUI = new InventoryUI { Name = "InventoryUI" };

			var playerData = GetPlayerData();
			if (playerData != null)
			{
				_inventoryUI.PlayerData = playerData;
			}

			AddChild(_inventoryUI);
		}

		private void CreateCharacterStatusUI()
		{
			_characterStatus = new CharacterStatus { Name = "CharacterStatus" };

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
			_quickAccessBar = new Panel
			{
				Name = "QuickAccessBar",
				AnchorLeft = 0.5f,
				AnchorRight = 0.5f,
				AnchorTop = 1.0f,
				AnchorBottom = 1.0f,
				OffsetLeft = -300,
				OffsetRight = 300,
				OffsetTop = -70,
				OffsetBottom = -10
			};

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

			var mainHBox = new HBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};

			CreateStatusBarSection(mainHBox);
			CreateInfoSection(mainHBox);
			CreateQuickButtons(mainHBox);

			_quickAccessBar.AddChild(mainHBox);
			AddChild(_quickAccessBar);
		}

		private void CreateStatusBarSection(HBoxContainer parent)
		{
			var statusVBox = new VBoxContainer { SizeFlagsVertical = Control.SizeFlags.ShrinkCenter };

			_playerNameLabel = new Label { Text = "Player" };
			_playerNameLabel.AddThemeFontSizeOverride("font_size", 12);
			_playerNameLabel.AddThemeColorOverride("font_color", new Color(0.93f, 0.94f, 0.95f, 1f));

			_healthBar = CreateSmallProgressBar(new Color(0.9f, 0.25f, 0.25f, 1f));
			_manaBar = CreateSmallProgressBar(new Color(0.3f, 0.5f, 0.95f, 1f));

			statusVBox.AddChild(_playerNameLabel);
			statusVBox.AddChild(_healthBar);
			statusVBox.AddChild(_manaBar);

			parent.AddChild(statusVBox);
		}

		private ProgressBar CreateSmallProgressBar(Color color)
		{
			var progressBar = new ProgressBar
			{
				Value = 100,
				MaxValue = 100,
				CustomMinimumSize = new Vector2(180, 16),
				ShowPercentage = false
			};

			var bgStyle = new StyleBoxFlat
			{
				BgColor = color * new Color(0.25f, 0.25f, 0.25f, 1f),
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
			};
			progressBar.AddThemeStyleboxOverride("background", bgStyle);

			var fillStyle = new StyleBoxFlat
			{
				BgColor = color,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
			};
			progressBar.AddThemeStyleboxOverride("fill", fillStyle);

			return progressBar;
		}

		private void CreateInfoSection(HBoxContainer parent)
		{
			var infoVBox = new VBoxContainer
			{
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};

			_goldLabel = new Label
			{
				Text = "💰 0",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			_goldLabel.AddThemeFontSizeOverride("font_size", 14);
			_goldLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			_goldLabel.AddThemeColorOverride("font_color", new Color(1f, 0.75f, 0.2f, 1f));

			infoVBox.AddChild(_goldLabel);
			parent.AddChild(infoVBox);
		}

		private void CreateQuickButtons(HBoxContainer parent)
		{
			_quickButtonsContainer = new HBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center
			};
			_quickButtonsContainer.AddThemeConstantOverride("separation", 8);

			string[] buttonLabels = { "📦", "⚔️", "📊" };
			string[] tooltips = { "Inventory (I)", "Character (C)", "Map (M)" };

			for (int i = 0; i < buttonLabels.Length; i++)
			{
				var button = new Button
				{
					Text = buttonLabels[i],
					TooltipText = tooltips[i],
					CustomMinimumSize = new Vector2(44, 44)
				};
				int index = i;
				button.Pressed += () => OnQuickButtonPressed(index);
				ApplyQuickButtonStyle(button);

				_quickButtonsContainer.AddChild(button);
			}

			parent.AddChild(_quickButtonsContainer);
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

		private void SetupInputHandling()
		{
			Connect("input", Callable.From<InputEvent>(OnInputEvent));
		}

		private void OnInputEvent(InputEvent @event)
		{
			if (@event is InputEventKey key && key.Pressed && !key.Echo)
			{
				switch (key.Keycode)
				{
					case Key.I:
						ToggleInventory();
						break;
					case Key.C:
						ToggleCharacterStatus();
						break;
					case Key.M:
						ToggleMinimap();
						break;
					case Key.Escape:
						CloseAllPanels();
						break;
				}
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
