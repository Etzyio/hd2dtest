using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scenes.UI
{
	[GlobalClass]
	public partial class CharacterStatus : Control
	{
		[Export] public Player PlayerData { get; set; }

		private Panel _mainPanel;
		private TabBar _tabBar;

		private Control _statsTab;
		private Control _equipmentTab;
		private Control _skillsTab;

		private Label _characterNameLabel;
		private Label _classLabel;
		private Label _levelLabel;
		private TextureRect _portraitRect;
		private Label _healthLabel;
		private ProgressBar _healthBar;
		private Label _manaLabel;
		private ProgressBar _manaBar;
		private ProgressBar _expBar;
		private Label _expLabel;

		private GridContainer _statsGrid;
		private Label _attackValueLabel;
		private Label _defenseValueLabel;
		private Label _speedValueLabel;
		private Label _critValueLabel;
		private Label _goldLabel;
		private Label _killCountLabel;

		private GridContainer _equipmentSlots;
		private Dictionary<string, EquipmentSlot> _equipmentSlotNodes = [];
		private RichTextLabel _equipmentBonusLabel;

		private ScrollContainer _skillsScrollContainer;
		private VBoxContainer _skillsContainer;
		private Tree _skillTree;

		private Button _closeButton;
		private bool _isVisible = false;

		private readonly Color _colorBackground = new(0.13f, 0.19f, 0.25f, 0.98f);
		private readonly Color _colorSurface = new(0.18f, 0.26f, 0.34f, 1f);
		private readonly Color _colorPrimary = new(0.2f, 0.6f, 0.86f, 1f);
		private readonly Color _colorSecondary = new(0.18f, 0.8f, 0.44f, 1f);
		private readonly Color _colorDanger = new(0.9f, 0.3f, 0.23f, 1f);
		private readonly Color _colorWarning = new(0.94f, 0.77f, 0.06f, 1f);
		private readonly Color _colorTextPrimary = new(0.93f, 0.94f, 0.95f, 1f);
		private readonly Color _colorTextSecondary = new(0.74f, 0.76f, 0.78f, 1f);
		private readonly Color _colorHealth = new(0.9f, 0.25f, 0.25f, 1f);
		private readonly Color _colorMana = new(0.3f, 0.5f, 0.95f, 1f);
		private readonly Color _colorExp = new(1f, 0.75f, 0.2f, 1f);

		public override void _Ready()
		{
			InitializeUI();
			SetupSignals();
			Visible = false;
		}

		private void InitializeUI()
		{
			AnchorLeft = 0.5f;
			AnchorRight = 0.5f;
			AnchorTop = 0.5f;
			AnchorBottom = 0.5f;
			OffsetLeft = -450;
			OffsetRight = 450;
			OffsetTop = -350;
			OffsetBottom = 350;

			CreateMainPanel();
			CreateHeader();
			CreateTabContainer();
			CreateStatsTabContent();
			CreateEquipmentTabContent();
			CreateSkillsTabContent();
		}

		private void CreateMainPanel()
		{
			_mainPanel = new Panel
			{
				Name = "MainPanel",
				AnchorLeft = 0,
				AnchorRight = 1,
				AnchorTop = 0,
				AnchorBottom = 1
			};

			var styleBox = new StyleBoxFlat
			{
				BgColor = _colorBackground,
				BorderWidthLeft = 3, BorderWidthRight = 3,
				BorderWidthTop = 3, BorderWidthBottom = 3,
				BorderColor = _colorPrimary,
				CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
				ShadowSize = 10,
				ShadowColor = new Color(0, 0, 0, 0.4f)
			};
			_mainPanel.AddThemeStyleboxOverride("panel", styleBox);
			AddChild(_mainPanel);
		}

		private void CreateHeader()
		{
			var headerPanel = new PanelContainer { Name = "HeaderPanel" };
			var headerStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				BorderWidthBottom = 2,
				BorderColor = new Color(_colorPrimary.R, _colorPrimary.G, _colorPrimary.B, 0.3f),
				CornerRadiusTopLeft = 9, CornerRadiusTopRight = 9,
				ContentMarginLeft = 20, ContentMarginRight = 20,
				ContentMarginTop = 14, ContentMarginBottom = 14
			};
			headerPanel.AddThemeStyleboxOverride("panel", headerStyle);

			var headerHBox = new HBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};

			var titleLabel = new Label
			{
				Text = "⚔️ Character Status",
				HorizontalAlignment = HorizontalAlignment.Left
			};
			titleLabel.AddThemeFontSizeOverride("font_size", 20);
			titleLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			titleLabel.AddThemeColorOverride("font_color", _colorTextPrimary);

			_closeButton = new Button { Text = "✕", TooltipText = "Close character status" };
			_closeButton.Pressed += ToggleVisibility;
			ApplyButtonStyle(_closeButton, true, _colorDanger);

			headerHBox.AddChild(titleLabel);
			headerHBox.AddChild(_closeButton);
			headerPanel.AddChild(headerHBox);

			headerPanel.AnchorLeft = 0; headerPanel.AnchorRight = 1;
			headerPanel.AnchorTop = 0; headerPanel.AnchorBottom = 0;
			headerPanel.OffsetBottom = 55;
			_mainPanel.AddChild(headerPanel);
		}

		private void CreateTabContainer()
		{
			_tabBar = new TabBar { Name = "MainTabs", TabAlignment = TabBar.AlignmentMode.Center };
			string[] tabs = { "📊 Stats", "🛡️ Equipment", "✨ Skills" };
			foreach (var tab in tabs) _tabBar.AddTab(tab);
			_tabBar.TabChanged += OnTabChanged;

			var tabContainer = new MarginContainer
			{
				AnchorLeft = 0, AnchorRight = 1,
				AnchorTop = 0, AnchorBottom = 0,
				OffsetTop = 60, OffsetBottom = 95
			};
			tabContainer.AddChild(_tabBar);
			_mainPanel.AddChild(tabContainer);
		}

		private void CreateStatsTabContent()
		{
			_statsTab = new Control
			{
				Name = "StatsTab",
				AnchorLeft = 0, AnchorRight = 1,
				AnchorTop = 0, AnchorBottom = 1,
				OffsetTop = 100, OffsetLeft = 15,
				OffsetRight = -15, OffsetBottom = -15
			};

			var mainHBox = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
			CreateCharacterInfoSection(mainHBox);
			CreateStatsSection(mainHBox);
			_statsTab.AddChild(mainHBox);
			_mainPanel.AddChild(_statsTab);
		}

		private void CreateCharacterInfoSection(HBoxContainer parent)
		{
			var infoVBox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			var infoPanel = new PanelContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
			var panelStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};
			infoPanel.AddThemeStyleboxOverride("panel", panelStyle);

			var innerVBox = new VBoxContainer();

			_characterNameLabel = new Label { Text = "Character Name", HorizontalAlignment = HorizontalAlignment.Center };
			_characterNameLabel.AddThemeFontSizeOverride("font_size", 22);
			_characterNameLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			_characterNameLabel.AddThemeColorOverride("font_color", _colorPrimary);

			_classLabel = new Label { Text = "Class: Warrior", HorizontalAlignment = HorizontalAlignment.Center };
			_classLabel.AddThemeFontSizeOverride("font_size", 14);
			_classLabel.AddThemeColorOverride("font_color", _colorTextSecondary);

			_levelLabel = new Label { Text = "Level: 1", HorizontalAlignment = HorizontalAlignment.Center };
			_levelLabel.AddThemeFontSizeOverride("font_size", 16);
			_levelLabel.AddThemeColorOverride("font_color", _colorExp);

			_portraitRect = new TextureRect
			{
				CustomMinimumSize = new Vector2(120, 120),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered
			};
			CreatePortraitTexture();

			innerVBox.AddChild(_characterNameLabel);
			innerVBox.AddChild(_classLabel);
			innerVBox.AddChild(_levelLabel);
			innerVBox.AddChild(_portraitRect);
			innerVBox.AddChild(new HSeparator());

			_healthLabel = new Label { Text = "❤️ HP" };
			_healthLabel.AddThemeFontSizeOverride("font_size", 12);
			_healthLabel.AddThemeColorOverride("font_color", _colorTextSecondary);

			_healthBar = CreateProgressBar(_colorHealth, new Vector2(200, 24));

			_manaLabel = new Label { Text = "💧 MP" };
			_manaLabel.AddThemeFontSizeOverride("font_size", 12);
			_manaLabel.AddThemeColorOverride("font_color", _colorTextSecondary);

			_manaBar = CreateProgressBar(_colorMana, new Vector2(200, 24));

			_expBar = CreateProgressBar(_colorExp, new Vector2(200, 20));

			_expLabel = new Label { Text = "EXP: 0/100", HorizontalAlignment = HorizontalAlignment.Center };
			_expLabel.AddThemeFontSizeOverride("font_size", 11);
			_expLabel.AddThemeColorOverride("font_color", _colorExp);

			innerVBox.AddChild(_healthLabel);
			innerVBox.AddChild(_healthBar);
			innerVBox.AddChild(_manaLabel);
			innerVBox.AddChild(_manaBar);
			innerVBox.AddChild(_expBar);
			innerVBox.AddChild(_expLabel);

			infoPanel.AddChild(innerVBox);
			infoVBox.AddChild(infoPanel);
			parent.AddChild(infoVBox);
		}

		private ProgressBar CreateProgressBar(Color color, Vector2 minSize)
		{
			var bar = new ProgressBar
			{
				Value = 100,
				MaxValue = 100,
				CustomMinimumSize = minSize,
				ShowPercentage = false
			};

			var bgStyle = new StyleBoxFlat
			{
				BgColor = color * new Color(0.3f, 0.3f, 0.3f, 1f),
				CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6
			};
			bar.AddThemeStyleboxOverride("background", bgStyle);

			var fillStyle = new StyleBoxFlat
			{
				BgColor = color,
				CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6
			};
			bar.AddThemeStyleboxOverride("fill", fillStyle);

			return bar;
		}

		private void CreatePortraitTexture()
		{
			var img = Image.CreateEmpty(120, 120, false, Image.Format.Rgba8);
			img.Fill(new Color(0.25f, 0.32f, 0.4f, 1f));

			for (int x = 30; x < 90; x++)
				for (int y = 25; y < 75; y++)
					img.SetPixel(x, y, new Color(0.85f, 0.72f, 0.58f, 1f));

			for (int x = 40; x < 80; x++)
				for (int y = 15; y < 35; y++)
					img.SetPixel(x, y, new Color(0.45f, 0.35f, 0.28f, 1f));

			for (int x = 50; x < 70; x++)
				img.SetPixel(x, 35, new Color(0.3f, 0.25f, 0.2f, 1f));

			for (int x = 35; x < 55; x++)
				for (int y = 75; y < 105; y++)
					img.SetPixel(x, y, new Color(0.25f, 0.32f, 0.5f, 1f));

			for (int x = 65; x < 85; x++)
				for (int y = 75; y < 105; y++)
					img.SetPixel(x, y, new Color(0.25f, 0.32f, 0.5f, 1f));

			_portraitRect.Texture = ImageTexture.CreateFromImage(img);
		}

		private void CreateStatsSection(HBoxContainer parent)
		{
			var statsVBox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			var statsPanel = new PanelContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
			var panelStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};
			statsPanel.AddThemeStyleboxOverride("panel", panelStyle);

			var titleLabel = new Label { Text = "📋 Attributes" };
			titleLabel.AddThemeFontSizeOverride("font_size", 16);
			titleLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			titleLabel.AddThemeColorOverride("font_color", _colorPrimary);

			_statsGrid = new GridContainer { Columns = 2, SizeFlagsVertical = Control.SizeFlags.ExpandFill };

			AddStatRow(_statsGrid, "⚔️ Attack", out _attackValueLabel, "15");
			AddStatRow(_statsGrid, "🛡️ Defense", out _defenseValueLabel, "8");
			AddStatRow(_statsGrid, "💨 Speed", out _speedValueLabel, "75");
			AddStatRow(_statsGrid, "💥 Crit Rate", out _critValueLabel, "5%");
			AddStatRow(_statsGrid, "💰 Gold", out _goldLabel, "0");
			AddStatRow(_statsGrid, "💀 Kills", out _killCountLabel, "0");

			var innerVBox = new VBoxContainer();
			innerVBox.AddChild(titleLabel);
			innerVBox.AddChild(new HSeparator());
			innerVBox.AddChild(_statsGrid);

			statsPanel.AddChild(innerVBox);
			statsVBox.AddChild(statsPanel);
			parent.AddChild(statsVBox);
		}

		private static void AddStatRow(GridContainer grid, string label, out Label valueLabel, string defaultValue)
		{
			var labelNode = new Label { Text = label, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			labelNode.AddThemeFontSizeOverride("font_size", 14);
			labelNode.AddThemeColorOverride("font_color", new Color(0.74f, 0.76f, 0.78f, 1f));

			valueLabel = new Label
			{
				Text = defaultValue,
				HorizontalAlignment = HorizontalAlignment.Right,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			valueLabel.AddThemeFontSizeOverride("font_size", 14);
			valueLabel.AddThemeColorOverride("font_color", new Color(0.93f, 0.94f, 0.95f, 1f));

			grid.AddChild(labelNode);
			grid.AddChild(valueLabel);
		}

		private void CreateEquipmentTabContent()
		{
			_equipmentTab = new Control
			{
				Name = "EquipmentTab",
				AnchorLeft = 0, AnchorRight = 1,
				AnchorTop = 0, AnchorBottom = 1,
				OffsetTop = 100, OffsetLeft = 15,
				OffsetRight = -15, OffsetBottom = -15
			};

			var mainHBox = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
			CreateEquipmentSlotsSection(mainHBox);
			CreateEquipmentBonusSection(mainHBox);
			_equipmentTab.AddChild(mainHBox);
			_mainPanel.AddChild(_equipmentTab);
		}

		private void CreateEquipmentSlotsSection(HBoxContainer parent)
		{
			var slotsVBox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			var slotsPanel = new PanelContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
			var panelStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};
			slotsPanel.AddThemeStyleboxOverride("panel", panelStyle);

			var titleLabel = new Label { Text = "🎽 Equipment Slots" };
			titleLabel.AddThemeFontSizeOverride("font_size", 16);
			titleLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			titleLabel.AddThemeColorOverride("font_color", _colorPrimary);

			_equipmentSlots = new GridContainer { Columns = 3, SizeFlagsVertical = Control.SizeFlags.ExpandFill };

			CreateEquipmentSlot("Weapon", "⚔️");
			CreateEquipmentSlot("Helmet", "🪖");
			CreateEquipmentSlot("Armor", "👕");
			CreateEquipmentSlot("Gloves", "🧤");
			CreateEquipmentSlot("Boots", "👢");
			CreateEquipmentSlot("Accessory", "💍");

			var innerVBox = new VBoxContainer();
			innerVBox.AddChild(titleLabel);
			innerVBox.AddChild(new HSeparator());
			innerVBox.AddChild(_equipmentSlots);
			slotsPanel.AddChild(innerVBox);
			slotsVBox.AddChild(slotsPanel);
			parent.AddChild(slotsVBox);
		}

		private void CreateEquipmentSlot(string slotType, string icon)
		{
			var slot = new EquipmentSlot(slotType, icon, _colorSurface, _colorPrimary, _colorTextSecondary);
			_equipmentSlots.AddChild(slot.Container);
			_equipmentSlotNodes[slotType] = slot;
		}

		private void CreateEquipmentBonusSection(HBoxContainer parent)
		{
			var bonusVBox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			var bonusPanel = new PanelContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
			var panelStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};
			bonusPanel.AddThemeStyleboxOverride("panel", panelStyle);

			var titleLabel = new Label { Text = "✨ Total Bonuses" };
			titleLabel.AddThemeFontSizeOverride("font_size", 16);
			titleLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			titleLabel.AddThemeColorOverride("font_color", _colorPrimary);

			_equipmentBonusLabel = new RichTextLabel
			{
				FitContent = true,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			_equipmentBonusLabel.AddThemeConstantOverride("lines_skipped", 0);
			_equipmentBonusLabel.AddThemeColorOverride("default_color", _colorTextSecondary);

			UpdateEquipmentBonusDisplay();

			var innerVBox = new VBoxContainer();
			innerVBox.AddChild(titleLabel);
			innerVBox.AddChild(new HSeparator());
			innerVBox.AddChild(_equipmentBonusLabel);
			bonusPanel.AddChild(innerVBox);
			bonusVBox.AddChild(bonusPanel);
			parent.AddChild(bonusVBox);
		}

		private void UpdateEquipmentBonusDisplay()
		{
			if (PlayerData == null || _equipmentBonusLabel == null) return;

			float totalDefense = 0, totalHealth = 0, totalMana = 0, totalSpeed = 0, totalAttack = 0;

			foreach (var eq in PlayerData.Equipments)
			{
				totalDefense += eq.Defense;
				totalHealth += eq.Health;
				totalMana += eq.Mana;
				totalSpeed += eq.Speed;
				totalAttack += eq.Attack;
			}

			if (PlayerData.CurrentWeapon != null)
			{
				totalAttack += PlayerData.CurrentWeapon.AttackPower;
			}

			string text = $"[color=#{_colorTextSecondary.ToHtml()}][b]Equipment Statistics[/b][/color]\n\n";
			if (totalAttack > 0) text += $"[color=#{_colorDanger.ToHtml()}]⚔️ Attack: +{totalAttack:F0}[/color]\n";
			if (totalDefense > 0) text += $"[color=#{_colorPrimary.ToHtml()}]🛡️ Defense: +{totalDefense:F0}[/color]\n";
			if (totalHealth > 0) text += $"[color=#{_colorHealth.ToHtml()}]❤️ Health: +{totalHealth:F0}[/color]\n";
			if (totalMana > 0) text += $"[color=#{_colorMana.ToHtml()}]💧 Mana: +{totalMana:F0}[/color]\n";
			if (totalSpeed > 0) text += $"[color=#{_colorSecondary.ToHtml()}]💨 Speed: +{totalSpeed:F0}[/color]\n";

			if (totalAttack + totalDefense + totalHealth + totalMana + totalSpeed == 0)
			{
				text += "\n[i]No equipment bonuses yet.[/i]";
			}

			_equipmentBonusLabel.Text = text;
		}

		private void CreateSkillsTabContent()
		{
			_skillsTab = new Control
			{
				Name = "SkillsTab",
				AnchorLeft = 0, AnchorRight = 1,
				AnchorTop = 0, AnchorBottom = 1,
				OffsetTop = 100, OffsetLeft = 15,
				OffsetRight = -15, OffsetBottom = -15
			};

			_skillsScrollContainer = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
			_skillsContainer = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};

			CreateSkillTreeSection();
			CreatePassiveSkillsSection();

			_skillsScrollContainer.AddChild(_skillsContainer);
			_skillsTab.AddChild(_skillsScrollContainer);
			_mainPanel.AddChild(_skillsTab);
		}

		private void CreateSkillTreeSection()
		{
			var treePanel = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			var panelStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};
			treePanel.AddThemeStyleboxOverride("panel", panelStyle);

			var headerHBox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

			var titleLabel = new Label { Text = "🌳 Skill Tree" };
			titleLabel.AddThemeFontSizeOverride("font_size", 16);
			titleLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			titleLabel.AddThemeColorOverride("font_color", _colorPrimary);

			var pointsLabel = new Label { Text = "Skill Points: 0" };
			pointsLabel.AddThemeFontSizeOverride("font_size", 13);
			pointsLabel.AddThemeColorOverride("font_color", _colorExp);

			headerHBox.AddChild(titleLabel);
			headerHBox.AddChild(pointsLabel);

			_skillTree = new Tree
			{
				SizeFlagsVertical = Control.SizeFlags.ExpandFill,
				CustomMinimumSize = new Vector2(0, 250),
				HideRoot = true
			};
			_skillTree.SetColumnTitle(0, "Skill");
			_skillTree.SetColumnTitle(1, "Info");
			_skillTree.ColumnTitlesVisible = true;

			var innerVBox = new VBoxContainer();
			innerVBox.AddChild(headerHBox);
			innerVBox.AddChild(new HSeparator());
			innerVBox.AddChild(_skillTree);
			treePanel.AddChild(innerVBox);
			_skillsContainer.AddChild(treePanel);
		}

		private void PopulateSkillTree()
		{
			if (PlayerData?.Skills == null || _skillTree == null) return;

			_skillTree.Clear();
			TreeItem root = _skillTree.CreateItem();
			root.SetText(0, "Active Skills");

			foreach (var skill in PlayerData.Skills)
			{
				TreeItem skillItem = _skillTree.CreateItem(root);
				skillItem.SetText(0, skill.SkillName);

				string infoText = skill.IsUnlocked ? "Unlocked" : "Locked";
				if (skill.SkillDefs != null && skill.SkillDefs.Count > 0)
				{
					infoText = skill.SkillDefs[0].Type.ToString();
				}
				skillItem.SetText(1, infoText);
				skillItem.SetTooltipText(0, skill.Description ?? "A powerful skill");

				Color skillColor = new(0.74f, 0.76f, 0.78f, 1f);
				if (skill.SkillDefs != null && skill.SkillDefs.Count > 0)
				{
					skillColor = skill.SkillDefs[0].Type switch
					{
						Skill.SkillType.Attack => _colorDanger,
						Skill.SkillType.Healing => _colorSecondary,
						Skill.SkillType.Defense => _colorPrimary,
						Skill.SkillType.Support => _colorWarning,
						_ => _colorTextSecondary
					};
				}
				skillItem.SetCustomColor(0, skillColor);
			}
		}

		private void CreatePassiveSkillsSection()
		{
			var passivePanel = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			var panelStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};
			passivePanel.AddThemeStyleboxOverride("panel", panelStyle);

			var titleLabel = new Label { Text = "🔮 Passive Skills" };
			titleLabel.AddThemeFontSizeOverride("font_size", 16);
			titleLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			titleLabel.AddThemeColorOverride("font_color", _colorPrimary);

			var passiveGrid = new GridContainer
			{
				Columns = Player.MaxPassiveSlots > 0 ? Player.MaxPassiveSlots : 4
			};

			if (PlayerData != null)
			{
				for (int i = 0; i < Player.MaxPassiveSlots; i++)
				{
					if (i < PlayerData.EquippedPassives.Count)
					{
						CreatePassiveSlot(passiveGrid, PlayerData.EquippedPassives[i].PassiveName, true);
					}
					else
					{
						CreatePassiveSlot(passiveGrid, $"Empty Slot {i + 1}", false);
					}
				}
			}

			var innerVBox = new VBoxContainer();
			innerVBox.AddChild(titleLabel);
			innerVBox.AddChild(new HSeparator());
			innerVBox.AddChild(passiveGrid);
			passivePanel.AddChild(innerVBox);
			_skillsContainer.AddChild(passivePanel);
		}

		private void CreatePassiveSlot(GridContainer grid, string name, bool filled)
		{
			var slot = new PanelContainer { CustomMinimumSize = new Vector2(90, 80) };
			var slotStyle = new StyleBoxFlat
			{
				BgColor = filled ? _colorPrimary * new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(_colorSurface.R, _colorSurface.G, _colorSurface.B, 0.5f),
				CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
				BorderWidthLeft = 2, BorderWidthRight = 2,
				BorderWidthTop = 2, BorderWidthBottom = 2,
				BorderColor = filled ? _colorPrimary : new Color(1, 1, 1, 0.1f),
				ContentMarginLeft = 8, ContentMarginRight = 8,
				ContentMarginTop = 8, ContentMarginBottom = 8
			};
			slot.AddThemeStyleboxOverride("panel", slotStyle);

			var vbox = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };

			var icon = new Label { Text = filled ? "◆" : "◇", HorizontalAlignment = HorizontalAlignment.Center };
			icon.AddThemeFontSizeOverride("font_size", 20);
			icon.AddThemeColorOverride("font_color", filled ? _colorPrimary : _colorTextSecondary);

			var nameLabel = new Label
			{
				Text = name,
				HorizontalAlignment = HorizontalAlignment.Center,
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			nameLabel.AddThemeFontSizeOverride("font_size", 10);
			nameLabel.AddThemeColorOverride("font_color", _colorTextSecondary);

			vbox.AddChild(icon);
			vbox.AddChild(nameLabel);
			slot.AddChild(vbox);
			grid.AddChild(slot);
		}

		private void ApplyButtonStyle(Button button, bool isSmall = false, Color? customColor = null)
		{
			Color color = customColor ?? _colorPrimary;
			int ml = isSmall ? 8 : 12, mr = isSmall ? 8 : 12, mt = isSmall ? 4 : 8, mb = isSmall ? 4 : 8;
			int cr = isSmall ? 4 : 6;

			var normalStyle = new StyleBoxFlat
			{
				BgColor = color * new Color(0.8f, 0.8f, 0.8f, 1f),
				CornerRadiusTopLeft = cr, CornerRadiusTopRight = cr,
				CornerRadiusBottomLeft = cr, CornerRadiusBottomRight = cr,
				ContentMarginLeft = ml, ContentMarginRight = mr,
				ContentMarginTop = mt, ContentMarginBottom = mb
			};
			button.AddThemeStyleboxOverride("normal", normalStyle);

			var hoverStyle = new StyleBoxFlat
			{
				BgColor = color * new Color(1f, 1f, 1f, 1f),
				CornerRadiusTopLeft = cr, CornerRadiusTopRight = cr,
				CornerRadiusBottomLeft = cr, CornerRadiusBottomRight = cr,
				ContentMarginLeft = ml, ContentMarginRight = mr,
				ContentMarginTop = mt, ContentMarginBottom = mb
			};
			button.AddThemeStyleboxOverride("hover", hoverStyle);

			var pressedStyle = new StyleBoxFlat
			{
				BgColor = color * new Color(0.7f, 0.7f, 0.7f, 1f),
				CornerRadiusTopLeft = cr, CornerRadiusTopRight = cr,
				CornerRadiusBottomLeft = cr, CornerRadiusBottomRight = cr,
				ContentMarginLeft = ml, ContentMarginRight = mr,
				ContentMarginTop = mt, ContentMarginBottom = mb
			};
			button.AddThemeStyleboxOverride("pressed", pressedStyle);

			button.AddThemeColorOverride("font_color", _colorTextPrimary);
			button.AddThemeFontSizeOverride("font_size", isSmall ? 12 : 14);
		}

		private void SetupSignals()
		{
			GuiInput += OnGuiInput;
		}

		public override void _Process(double delta)
		{
			if (_isVisible && PlayerData != null)
			{
				UpdateCharacterInfo();
			}
		}

		private void UpdateCharacterInfo()
		{
			if (PlayerData == null) return;

			_healthBar.Value = PlayerData.Health;
			_healthBar.MaxValue = PlayerData.MaxHealth;
			_healthLabel.Text = $"❤️ HP: {PlayerData.Health:F0}/{PlayerData.MaxHealth:F0}";

			_manaBar.Value = PlayerData.Mana;
			_manaBar.MaxValue = PlayerData.MaxMana;
			_manaLabel.Text = $"💧 MP: {PlayerData.Mana:F0}/{PlayerData.MaxMana:F0}";

			int requiredExp = PlayerData.Level * 100;
			_expBar.Value = PlayerData.Experience;
			_expBar.MaxValue = requiredExp;
			_expLabel.Text = $"EXP: {PlayerData.Experience}/{requiredExp}";

			_attackValueLabel.Text = $"{PlayerData.Attack:F0}";
			_defenseValueLabel.Text = $"{PlayerData.Defense:F0}";
			_speedValueLabel.Text = $"{PlayerData.Speed:F0}";
			_goldLabel.Text = $"{PlayerData.Gold}";
			_killCountLabel.Text = $"{PlayerData.KillCount}";
		}

		public void LoadCharacterData(Player player)
		{
			PlayerData = player;
			if (player == null) return;

			_characterNameLabel.Text = player.CreatureName;
			_levelLabel.Text = $"Level: {player.Level}";

			string mainClass = player.MainClass?.ClassName ?? "None";
			string subClass = player.SubClass != null ? $"/ {player.SubClass.ClassName}" : "";
			_classLabel.Text = $"Class: {mainClass} {subClass}";

			UpdateCharacterInfo();
			UpdateEquipmentSlots();
			UpdateEquipmentBonusDisplay();
			PopulateSkillTree();
		}

		private void UpdateEquipmentSlots()
		{
			if (PlayerData == null) return;

			if (_equipmentSlotNodes.TryGetValue("Weapon", out var weaponSlot))
			{
				if (PlayerData.CurrentWeapon != null)
				{
					weaponSlot.SetEquipped(PlayerData.CurrentWeapon.WeaponName, PlayerData.CurrentWeapon.AttackPower.ToString());
				}
				else
				{
					weaponSlot.Clear();
				}
			}

			foreach (var eq in PlayerData.Equipments)
			{
				string slotKey = eq.EquipmentTypeValue.ToString();
				if (_equipmentSlotNodes.TryGetValue(slotKey, out var slot))
				{
					slot.SetEquipped(eq.EquipmentName, eq.GetBonusDescription());
				}
			}
		}

		private void OnTabChanged(long tab) { }

		private void OnGuiInput(InputEvent @event)
		{
			if (@event is InputEventKey key && key.Pressed)
			{
				if (key.Keycode == Key.Escape && _isVisible)
				{
					ToggleVisibility();
				}
			}
		}

		public void ToggleVisibility()
		{
			_isVisible = !_isVisible;

			if (_isVisible)
			{
				Visible = true;
				if (PlayerData != null) LoadCharacterData(PlayerData);

				var tween = CreateTween();
				tween.TweenProperty(this, "modulate:a", 1.0f, 0.2).From(0.0f);
				tween.TweenProperty(this, "scale", Vector2.One, 0.2).From(new Vector2(0.95f, 0.95f));
			}
			else
			{
				var tween = CreateTween();
				tween.TweenProperty(this, "modulate:a", 0.0f, 0.15);
				tween.TweenCallback(Callable.From(() => Visible = false));
			}
		}

		private class EquipmentSlot
		{
			public PanelContainer Container { get; }
			private readonly Label _nameLabel;
			private readonly Label _bonusLabel;
			private readonly Color _surfaceColor;
			private readonly Color _primaryColor;

			public EquipmentSlot(string slotType, string icon, Color surface, Color primary, Color textColor)
			{
				_surfaceColor = surface;
				_primaryColor = primary;

				Container = new PanelContainer { CustomMinimumSize = new Vector2(80, 90) };

				var style = new StyleBoxFlat
				{
					BgColor = _surfaceColor,
					CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
					CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
					BorderWidthLeft = 2, BorderWidthRight = 2,
					BorderWidthTop = 2, BorderWidthBottom = 2,
					BorderColor = new Color(primary.R, primary.G, primary.B, 0.2f),
					ContentMarginLeft = 8, ContentMarginRight = 8,
					ContentMarginTop = 8, ContentMarginBottom = 8
				};
				Container.AddThemeStyleboxOverride("panel", style);
				Container.TooltipText = $"{slotType} Slot - Empty";

				var vbox = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };

				var iconLabel = new Label { Text = icon, HorizontalAlignment = HorizontalAlignment.Center };
				iconLabel.AddThemeFontSizeOverride("font_size", 22);
				iconLabel.AddThemeColorOverride("font_color", textColor);

				_nameLabel = new Label { Text = slotType, HorizontalAlignment = HorizontalAlignment.Center };
				_nameLabel.AddThemeFontSizeOverride("font_size", 11);
				_nameLabel.AddThemeColorOverride("font_color", textColor);

				_bonusLabel = new Label { Text = "", HorizontalAlignment = HorizontalAlignment.Center };
				_bonusLabel.AddThemeFontSizeOverride("font_size", 9);
				_bonusLabel.AddThemeColorOverride("font_color", _primaryColor);

				vbox.AddChild(iconLabel);
				vbox.AddChild(_nameLabel);
				vbox.AddChild(_bonusLabel);
				Container.AddChild(vbox);
			}

			public void SetEquipped(string name, string bonus)
			{
				_nameLabel.Text = StringExtensions.Truncate(name, 12);
				_bonusLabel.Text = bonus;
				Container.TooltipText = $"{name}\n{bonus}";

				var style = (StyleBoxFlat)Container.GetThemeStylebox("panel");
				style.BorderColor = _primaryColor;
				style.BgColor = _primaryColor * new Color(0.15f, 0.15f, 0.15f, 1f);
			}

			public void Clear()
			{
				_nameLabel.Text = "Empty";
				_bonusLabel.Text = "";

				var style = (StyleBoxFlat)Container.GetThemeStylebox("panel");
				style.BorderColor = new Color(_primaryColor.R, _primaryColor.G, _primaryColor.B, 0.2f);
				style.BgColor = _surfaceColor;
			}
		}
	}

	internal static class StringExtensions
	{
		public static string Truncate(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
		}
	}
}
