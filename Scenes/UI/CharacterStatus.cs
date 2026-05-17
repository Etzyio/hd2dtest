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
			GetNodeReferences();
			ApplyStyles();
			SetupTabs();
			PopulateInitialEquipmentSlots();
			PopulatePassiveSlots();
			SetupSignals();
		}

		private void GetNodeReferences()
		{
			_mainPanel = GetNode<Panel>("MainPanel");
			_tabBar = GetNode<TabBar>("MainPanel/TabMarginContainer/MainTabs");
			_closeButton = GetNode<Button>("MainPanel/HeaderPanel/HeaderHBox/CloseButton");

			_statsTab = GetNode<Control>("MainPanel/StatsTab");
			_characterNameLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/CharacterNameLabel");
			_classLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/ClassLabel");
			_levelLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/LevelLabel");
			_portraitRect = GetNode<TextureRect>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/PortraitRect");
			_healthLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/HealthLabel");
			_healthBar = GetNode<ProgressBar>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/HealthBar");
			_manaLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/ManaLabel");
			_manaBar = GetNode<ProgressBar>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/ManaBar");
			_expBar = GetNode<ProgressBar>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/ExpBar");
			_expLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/InfoPanel/InfoVBox/ExpLabel");

			_statsGrid = GetNode<GridContainer>("MainPanel/StatsTab/StatsHBox/StatsPanel/StatsVBox/StatsGrid");
			_attackValueLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/StatsPanel/StatsVBox/StatsGrid/AttackValue");
			_defenseValueLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/StatsPanel/StatsVBox/StatsGrid/DefenseValue");
			_speedValueLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/StatsPanel/StatsVBox/StatsGrid/SpeedValue");
			_critValueLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/StatsPanel/StatsVBox/StatsGrid/CritValue");
			_goldLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/StatsPanel/StatsVBox/StatsGrid/GoldValue");
			_killCountLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/StatsPanel/StatsVBox/StatsGrid/KillsValue");

			_equipmentTab = GetNode<Control>("MainPanel/EquipmentTab");
			_equipmentSlots = GetNode<GridContainer>("MainPanel/EquipmentTab/EquipHBox/SlotsPanel/SlotsVBox/EquipmentSlots");
			_equipmentBonusLabel = GetNode<RichTextLabel>("MainPanel/EquipmentTab/EquipHBox/BonusPanel/BonusVBox/EquipmentBonusLabel");

			_skillsTab = GetNode<Control>("MainPanel/SkillsTab");
			_skillsScrollContainer = GetNode<ScrollContainer>("MainPanel/SkillsTab/SkillsScroll");
			_skillsContainer = GetNode<VBoxContainer>("MainPanel/SkillsTab/SkillsScroll/SkillsVBox");
			_skillTree = GetNode<Tree>("MainPanel/SkillsTab/SkillsScroll/SkillsVBox/SkillTreePanel/SkillTreeVBox/SkillTree");
		}

		private void ApplyStyles()
		{
			var font = GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf");

			var mainStyle = new StyleBoxFlat
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
			_mainPanel.AddThemeStyleboxOverride("panel", mainStyle);

			var headerPanel = GetNode<PanelContainer>("MainPanel/HeaderPanel");
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

			var titleLabel = GetNode<Label>("MainPanel/HeaderPanel/HeaderHBox/TitleLabel");
			titleLabel.AddThemeFontSizeOverride("font_size", 20);
			titleLabel.AddThemeFontOverride("font", font);
			titleLabel.AddThemeColorOverride("font_color", _colorTextPrimary);

			_closeButton.Pressed += ToggleVisibility;
			ApplyButtonStyle(_closeButton, true, _colorDanger);

			var statPanelStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
				ContentMarginLeft = 16, ContentMarginRight = 16,
				ContentMarginTop = 16, ContentMarginBottom = 16
			};

			var infoPanel = GetNode<PanelContainer>("MainPanel/StatsTab/StatsHBox/InfoPanel");
			infoPanel.AddThemeStyleboxOverride("panel", statPanelStyle);

			var statsPanel = GetNode<PanelContainer>("MainPanel/StatsTab/StatsHBox/StatsPanel");
			statsPanel.AddThemeStyleboxOverride("panel", statPanelStyle);

			_characterNameLabel.AddThemeFontSizeOverride("font_size", 22);
			_characterNameLabel.AddThemeFontOverride("font", font);
			_characterNameLabel.AddThemeColorOverride("font_color", _colorPrimary);

			_classLabel.AddThemeFontSizeOverride("font_size", 14);
			_classLabel.AddThemeColorOverride("font_color", _colorTextSecondary);

			_levelLabel.AddThemeFontSizeOverride("font_size", 16);
			_levelLabel.AddThemeColorOverride("font_color", _colorExp);

			_healthLabel.AddThemeFontSizeOverride("font_size", 12);
			_healthLabel.AddThemeColorOverride("font_color", _colorTextSecondary);

			ApplyProgressBarStyle(_healthBar, _colorHealth);
			_manaLabel.AddThemeFontSizeOverride("font_size", 12);
			_manaLabel.AddThemeColorOverride("font_color", _colorTextSecondary);

			ApplyProgressBarStyle(_manaBar, _colorMana);
			ApplyProgressBarStyle(_expBar, _colorExp);

			_expLabel.AddThemeFontSizeOverride("font_size", 11);
			_expLabel.AddThemeColorOverride("font_color", _colorExp);

			CreatePortraitTexture();

			// Stats grid labels
			Color textSecondary = new(0.74f, 0.76f, 0.78f, 1f);
			Color textPrimary = new(0.93f, 0.94f, 0.95f, 1f);
			foreach (var child in _statsGrid.GetChildren())
			{
				if (child is Label label)
				{
					label.AddThemeFontSizeOverride("font_size", 14);
					label.AddThemeColorOverride("font_color",
						label.HorizontalAlignment == HorizontalAlignment.Right ? textPrimary : textSecondary);
				}
			}

			var statsTitleLabel = GetNode<Label>("MainPanel/StatsTab/StatsHBox/StatsPanel/StatsVBox/StatsTitleLabel");
			statsTitleLabel.AddThemeFontSizeOverride("font_size", 16);
			statsTitleLabel.AddThemeFontOverride("font", font);
			statsTitleLabel.AddThemeColorOverride("font_color", _colorPrimary);

			// Equipment tab styles
			var slotsPanel = GetNode<PanelContainer>("MainPanel/EquipmentTab/EquipHBox/SlotsPanel");
			slotsPanel.AddThemeStyleboxOverride("panel", statPanelStyle);
			var slotsTitle = GetNode<Label>("MainPanel/EquipmentTab/EquipHBox/SlotsPanel/SlotsVBox/SlotsTitleLabel");
			slotsTitle.AddThemeFontSizeOverride("font_size", 16);
			slotsTitle.AddThemeFontOverride("font", font);
			slotsTitle.AddThemeColorOverride("font_color", _colorPrimary);

			var bonusPanel = GetNode<PanelContainer>("MainPanel/EquipmentTab/EquipHBox/BonusPanel");
			bonusPanel.AddThemeStyleboxOverride("panel", statPanelStyle);
			var bonusTitle = GetNode<Label>("MainPanel/EquipmentTab/EquipHBox/BonusPanel/BonusVBox/BonusTitleLabel");
			bonusTitle.AddThemeFontSizeOverride("font_size", 16);
			bonusTitle.AddThemeFontOverride("font", font);
			bonusTitle.AddThemeColorOverride("font_color", _colorPrimary);

			_equipmentBonusLabel.AddThemeConstantOverride("lines_skipped", 0);
			_equipmentBonusLabel.AddThemeColorOverride("default_color", _colorTextSecondary);

			// Skills tab styles
			var skillTreePanel = GetNode<PanelContainer>("MainPanel/SkillsTab/SkillsScroll/SkillsVBox/SkillTreePanel");
			skillTreePanel.AddThemeStyleboxOverride("panel", statPanelStyle);
			var skillTreeTitle = GetNode<Label>("MainPanel/SkillsTab/SkillsScroll/SkillsVBox/SkillTreePanel/SkillTreeVBox/SkillTreeHeader/SkillTreeTitle");
			skillTreeTitle.AddThemeFontSizeOverride("font_size", 16);
			skillTreeTitle.AddThemeFontOverride("font", font);
			skillTreeTitle.AddThemeColorOverride("font_color", _colorPrimary);

			var skillPointsLabel = GetNode<Label>("MainPanel/SkillsTab/SkillsScroll/SkillsVBox/SkillTreePanel/SkillTreeVBox/SkillTreeHeader/SkillPointsLabel");
			skillPointsLabel.AddThemeFontSizeOverride("font_size", 13);
			skillPointsLabel.AddThemeColorOverride("font_color", _colorExp);

			var passivePanel = GetNode<PanelContainer>("MainPanel/SkillsTab/SkillsScroll/SkillsVBox/PassivePanel");
			passivePanel.AddThemeStyleboxOverride("panel", statPanelStyle);
			var passiveTitle = GetNode<Label>("MainPanel/SkillsTab/SkillsScroll/SkillsVBox/PassivePanel/PassiveVBox/PassiveTitle");
			passiveTitle.AddThemeFontSizeOverride("font_size", 16);
			passiveTitle.AddThemeFontOverride("font", font);
			passiveTitle.AddThemeColorOverride("font_color", _colorPrimary);

			_skillTree.SetColumnTitle(0, "Skill");
			_skillTree.SetColumnTitle(1, "Info");
			_skillTree.ColumnTitlesVisible = true;

			UpdateEquipmentBonusDisplay();
		}

		private static void ApplyProgressBarStyle(ProgressBar bar, Color color)
		{
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
		}

		private void SetupTabs()
		{
			string[] tabs = { "📊 Stats", "🛡️ Equipment", "✨ Skills" };
			foreach (var tab in tabs) _tabBar.AddTab(tab);
			_tabBar.TabChanged += OnTabChanged;
		}

		private void PopulateInitialEquipmentSlots()
		{
			CreateEquipmentSlot("Weapon", "⚔️");
			CreateEquipmentSlot("Helmet", "🪖");
			CreateEquipmentSlot("Armor", "👕");
			CreateEquipmentSlot("Gloves", "🧤");
			CreateEquipmentSlot("Boots", "👢");
			CreateEquipmentSlot("Accessory", "💍");
		}

		private void PopulatePassiveSlots()
		{
			var passiveGrid = GetNode<GridContainer>("MainPanel/SkillsTab/SkillsScroll/SkillsVBox/PassivePanel/PassiveVBox/PassiveGrid");
			passiveGrid.Columns = Player.MaxPassiveSlots > 0 ? Player.MaxPassiveSlots : 4;

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

		private void CreateEquipmentSlot(string slotType, string icon)
		{
			var slot = new EquipmentSlot(slotType, icon, _colorSurface, _colorPrimary, _colorTextSecondary);
			_equipmentSlots.AddChild(slot.Container);
			_equipmentSlotNodes[slotType] = slot;
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
