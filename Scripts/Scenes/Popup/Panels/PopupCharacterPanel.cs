/*
 * File: PopupCharacterPanel.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 人物面板，显示玩家角色和队友的详细状态信息。
 * 包含属性、装备和队友三个标签页。
 *
 * Key Features:
 * - 属性页：HP/MP/EXP进度条、等级、职业、攻防速、金币、击杀数、装备加成
 * - 装备页：武器 + 6个防具槽位 + 被动技能，支持装备/卸下交互
 * - 队友页：队友卡片（名称、等级、HP/MP条）
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scenes.Popup
{
    public partial class PopupCharacterPanel : PopupPanelBase
    {
        private TabBar _tabBar;
        private Control _statsTab;
        private Control _equipTab;
        private Control _teammateTab;

        // Stats tab
        private Label _nameLabel;
        private Label _levelLabel;
        private Label _classLabel;
        private ProgressBar _hpBar;
        private ProgressBar _mpBar;
        private ProgressBar _expBar;
        private Label _hpLabel;
        private Label _mpLabel;
        private Label _expLabel;
        private Label _statsLabel;
        private Label _bonusLabel;
        private RichTextLabel _classInfoLabel;

        // Equipment tab
        private VBoxContainer _equipSlotsContainer;

        // Teammate tab
        private VBoxContainer _teammateListContainer;

        public Action OnBackPressed { get; set; }

        public override void _Ready()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Name = "CharacterPanel";
            Visible = false;
            LayoutMode = 1;
            AnchorLeft = 0.05f; AnchorTop = 0.05f;
            AnchorRight = 0.95f; AnchorBottom = 0.95f;
            MouseFilter = MouseFilterEnum.Stop;

            var bg = new ColorRect
            {
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1,
                Color = ColorBg
            };
            AddChild(bg);

            var vbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.02f, AnchorTop = 0.02f,
                AnchorRight = 0.98f, AnchorBottom = 0.98f
            };
            AddChild(vbox);

            vbox.AddChild(CreateTitleLabel("Characters"));
            vbox.AddChild(CreateSeparator());

            _tabBar = new TabBar();
            _tabBar.AddTab("Stats");
            _tabBar.AddTab("Equipment");
            _tabBar.AddTab("Teammates");
            _tabBar.TabChanged += OnTabChanged;
            vbox.AddChild(_tabBar);

            var tabContainer = new Control
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            vbox.AddChild(tabContainer);

            CreateStatsTab(tabContainer);
            CreateEquipTab(tabContainer);
            CreateTeammateTab(tabContainer);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

            var backBtn = CreateSecondaryButton("Back");
            backBtn.Pressed += () => OnBackPressed?.Invoke();
            vbox.AddChild(backBtn);

            _statsTab.Visible = true;
            _equipTab.Visible = false;
            _teammateTab.Visible = false;
        }

        #region Stats Tab

        private void CreateStatsTab(Control parent)
        {
            _statsTab = new Control
            {
                LayoutMode = 1, AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1
            };
            parent.AddChild(_statsTab);

            var scroll = new ScrollContainer
            {
                LayoutMode = 1, AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1
            };
            _statsTab.AddChild(scroll);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(vbox);

            // Name
            _nameLabel = CreateTitleLabel("");
            _nameLabel.AddThemeFontSizeOverride("font_size", 26);
            vbox.AddChild(_nameLabel);
            vbox.AddChild(CreateSeparator());

            // Level row
            var levelHbox = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            _levelLabel = CreateInfoLabel("");
            _levelLabel.AddThemeFontSizeOverride("font_size", 16);
            _levelLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
            levelHbox.AddChild(_levelLabel);

            _classLabel = CreateInfoLabel("");
            _classLabel.AddThemeFontSizeOverride("font_size", 14);
            _classLabel.AddThemeColorOverride("font_color", ColorPrimary);
            levelHbox.AddChild(_classLabel);
            vbox.AddChild(levelHbox);

            // Class info
            _classInfoLabel = new RichTextLabel
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                BbcodeEnabled = true,
                ScrollActive = false,
                CustomMinimumSize = new Vector2(0, 0)
            };
            _classInfoLabel.AddThemeFontOverride("normal_font", GetFont());
            _classInfoLabel.AddThemeFontSizeOverride("normal_font_size", 13);
            vbox.AddChild(_classInfoLabel);

            // HP
            var hpHeader = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            hpHeader.AddChild(new Label { Text = "HP", SizeFlagsHorizontal = SizeFlags.ExpandFill });
            _hpLabel = CreateInfoLabel("");
            hpHeader.AddChild(_hpLabel);
            vbox.AddChild(hpHeader);

            _hpBar = CreateProgressBar(ColorDanger);
            vbox.AddChild(_hpBar);

            // MP
            var mpHeader = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            mpHeader.AddChild(new Label { Text = "MP", SizeFlagsHorizontal = SizeFlags.ExpandFill });
            _mpLabel = CreateInfoLabel("");
            mpHeader.AddChild(_mpLabel);
            vbox.AddChild(mpHeader);

            _mpBar = CreateProgressBar(new Color(0.3f, 0.5f, 0.95f, 1f));
            vbox.AddChild(_mpBar);

            // EXP
            var expHeader = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            expHeader.AddChild(new Label { Text = "EXP", SizeFlagsHorizontal = SizeFlags.ExpandFill });
            _expLabel = CreateInfoLabel("");
            expHeader.AddChild(_expLabel);
            vbox.AddChild(expHeader);

            _expBar = CreateProgressBar(ColorSuccess);
            vbox.AddChild(_expBar);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });
            vbox.AddChild(CreateSeparator());

            // Combat stats
            var combatTitle = CreateSectionLabel("Combat Stats");
            vbox.AddChild(combatTitle);

            _statsLabel = CreateInfoLabel("");
            _statsLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
            _statsLabel.AddThemeFontSizeOverride("font_size", 15);
            vbox.AddChild(_statsLabel);

            vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });
            vbox.AddChild(CreateSeparator());

            // Equipment bonuses
            var bonusTitle = CreateSectionLabel("Equipment Bonuses");
            vbox.AddChild(bonusTitle);
            _bonusLabel = CreateInfoLabel("");
            _bonusLabel.AddThemeColorOverride("font_color", ColorSuccess);
            vbox.AddChild(_bonusLabel);
        }

        #endregion

        #region Equipment Tab

        private void CreateEquipTab(Control parent)
        {
            _equipTab = new Control
            {
                LayoutMode = 1, AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1
            };
            parent.AddChild(_equipTab);

            var scroll = new ScrollContainer
            {
                LayoutMode = 1, AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1
            };
            _equipTab.AddChild(scroll);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(vbox);

            vbox.AddChild(CreateTitleLabel("Equipment"));
            vbox.AddChild(CreateSeparator());

            _equipSlotsContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            vbox.AddChild(_equipSlotsContainer);
        }

        #endregion

        #region Teammate Tab

        private void CreateTeammateTab(Control parent)
        {
            _teammateTab = new Control
            {
                LayoutMode = 1, AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1
            };
            parent.AddChild(_teammateTab);

            var scroll = new ScrollContainer
            {
                LayoutMode = 1, AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1
            };
            _teammateTab.AddChild(scroll);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(vbox);

            vbox.AddChild(CreateTitleLabel("Teammates"));
            vbox.AddChild(CreateSeparator());

            _teammateListContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            vbox.AddChild(_teammateListContainer);
        }

        #endregion

        #region Tab Switching

        private void OnTabChanged(long tab)
        {
            _statsTab.Visible = tab == 0;
            _equipTab.Visible = tab == 1;
            _teammateTab.Visible = tab == 2;

            if (tab == 0) RefreshStats();
            else if (tab == 1) RefreshEquip();
            else if (tab == 2) RefreshTeammates();
        }

        #endregion

        #region Refresh

        public void Refresh()
        {
            _tabBar.CurrentTab = 0;
            RefreshStats();
            _statsTab.Visible = true;
            _equipTab.Visible = false;
            _teammateTab.Visible = false;
        }

        private void RefreshStats()
        {
            var player = GetPlayer();
            if (player == null)
            {
                _nameLabel.Text = "No player data";
                return;
            }

            CharacterStatsManager.Instance?.CalculateStats(player);

            _nameLabel.Text = player.CreatureName ?? "Player";
            _levelLabel.Text = $"Lv.{player.Level}";

            string mainClass = player.MainClass?.ClassName ?? "None";
            string subClass = player.SubClass?.ClassName ?? "None";
            _classLabel.Text = $"  |  {mainClass} / {subClass}";

            // Class info with BBC
            string mainClassColor = "#" + ColorTextSecondary.ToHtml();
            string growthColor = "#4D8BD9";
            string classInfo = $"[color={mainClassColor}]{mainClass}[/color]";
            if (player.MainClass != null)
            {
                classInfo += $"\n[color={growthColor}]HP:{player.MainClass.HealthGrowth:F1} ATK:{player.MainClass.AttackGrowth:F1} DEF:{player.MainClass.DefenseGrowth:F1} SPD:{player.MainClass.SpeedGrowth:F1}[/color]";
            }
            _classInfoLabel.Text = classInfo;

            // HP
            _hpBar.Value = player.Health;
            _hpBar.MaxValue = Math.Max(player.MaxHealth, 1);
            _hpLabel.Text = $"{player.Health:F0}/{player.MaxHealth:F0}";

            // MP
            _mpBar.Value = player.CurrentMana;
            _mpBar.MaxValue = Math.Max(player.MaxMana, 1);
            _mpLabel.Text = $"{player.CurrentMana:F0}/{player.MaxMana:F0}";

            // EXP
            _expBar.Value = player.Experience;
            int nextLevelExp = player.Level * 100;
            _expBar.MaxValue = Math.Max(nextLevelExp, 1);
            _expLabel.Text = $"{player.Experience}/{nextLevelExp}";

            // Stats
            _statsLabel.Text = $"ATK: {player.Attack:F0}   DEF: {player.Defense:F0}   SPD: {player.Speed:F0}\n"
                + $"Gold: {player.Gold}   Kills: {player.KillCount}   Deaths: {player.DeathCount}";

            // Equipment bonuses
            float bonusAtk = 0, bonusDef = 0, bonusHp = 0, bonusMp = 0, bonusSpd = 0;
            foreach (var eq in player.Equipments)
            {
                bonusAtk += eq.Attack;
                bonusDef += eq.Defense;
                bonusHp += eq.Health;
                bonusMp += eq.Mana;
                bonusSpd += eq.Speed;
            }
            if (player.CurrentWeapon != null)
            {
                bonusAtk += player.CurrentWeapon.AttackPower;
            }

            _bonusLabel.Text = $"ATK +{bonusAtk:F0}   DEF +{bonusDef:F0}   HP +{bonusHp:F0}   MP +{bonusMp:F0}   SPD +{bonusSpd:F0}";
        }

        #endregion

        #region Equipment Tab Refresh (with interaction)

        private void RefreshEquip()
        {
            ClearChildren(_equipSlotsContainer);
            var player = GetPlayer();
            if (player == null) return;

            // Weapon slot
            AddEquipSlotWithAction("Weapon",
                player.CurrentWeapon?.WeaponName,
                player.CurrentWeapon != null ? $"ATK: {player.CurrentWeapon.AttackPower:F0}  [{player.CurrentWeapon.GetTypeName()}]" : null,
                () =>
                {
                    if (player.CurrentWeapon == null) return;
                    player.UnequipWeapon();
                    RefreshEquip();
                },
                () => ShowEquipSelection(player, EquipType.Weapon));

            // Equipment slots
            var slotDefs = new (string Name, Equipment.EquipmentType Type)[]
            {
                ("Helmet", Equipment.EquipmentType.Helmet),
                ("Armor", Equipment.EquipmentType.Armor),
                ("Boots", Equipment.EquipmentType.Boots),
                ("Gloves", Equipment.EquipmentType.Gloves),
                ("Accessory", Equipment.EquipmentType.Accessory),
                ("Shield", Equipment.EquipmentType.Shield)
            };

            foreach (var (slotName, slotType) in slotDefs)
            {
                var eq = player.Equipments.FirstOrDefault(e => e.EquipmentTypeValue == slotType);

                string bonusStr = null;
                if (eq != null)
                {
                    var parts = new List<string>();
                    if (eq.Defense > 0) parts.Add($"DEF:{eq.Defense:F0}");
                    if (eq.Health > 0) parts.Add($"HP:{eq.Health:F0}");
                    if (eq.Mana > 0) parts.Add($"MP:{eq.Mana:F0}");
                    if (eq.Speed > 0) parts.Add($"SPD:{eq.Speed:F0}");
                    if (eq.Attack > 0) parts.Add($"ATK:{eq.Attack:F0}");
                    bonusStr = string.Join(" ", parts);
                }

                AddEquipSlotWithAction(slotName, eq?.EquipmentName, bonusStr,
                    () =>
                    {
                        if (eq == null) return;
                        player.UnequipEquipment(eq);
                        RefreshEquip();
                    },
                    () => ShowEquipSelection(player, EquipType.Equipment, slotType));
            }

            // Passive skills section
            var passiveSep = CreateSeparator();
            _equipSlotsContainer.AddChild(passiveSep);

            var passiveTitle = CreateSectionLabel("Passive Skills");
            _equipSlotsContainer.AddChild(passiveTitle);

            if (player.EquippedPassives.Count == 0)
            {
                _equipSlotsContainer.AddChild(CreateInfoLabel("(None equipped)"));
            }
            foreach (var passive in player.EquippedPassives)
            {
                _equipSlotsContainer.AddChild(CreateInfoLabel($"  {passive.PassiveName}"));
            }
        }

        private enum EquipType { Weapon, Equipment }

        private void AddEquipSlotWithAction(string slotName, string itemName, string bonusInfo,
            Action onUnequip, Action onEquip)
        {
            var card = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var cardStyle = new StyleBoxFlat
            {
                BgColor = ColorSurface,
                BorderWidthLeft = 1, BorderWidthTop = 1,
                BorderWidthRight = 1, BorderWidthBottom = 1,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.3f),
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ContentMarginLeft = 8, ContentMarginTop = 4,
                ContentMarginRight = 8, ContentMarginBottom = 4
            };
            card.AddThemeStyleboxOverride("panel", cardStyle);
            _equipSlotsContainer.AddChild(card);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            card.AddChild(vbox);

            var topRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            vbox.AddChild(topRow);

            // Slot name
            var slotLabel = new Label { Text = $"[{slotName}]", CustomMinimumSize = new Vector2(90, 0) };
            slotLabel.AddThemeFontOverride("font", GetFont());
            slotLabel.AddThemeFontSizeOverride("font_size", 14);
            slotLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.6f, 0.9f, 1f));
            topRow.AddChild(slotLabel);

            // Item name
            var itemLabel = new Label
            {
                Text = itemName ?? "(Empty)",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            itemLabel.AddThemeFontOverride("font", GetFont());
            itemLabel.AddThemeFontSizeOverride("font_size", 14);
            itemLabel.AddThemeColorOverride("font_color",
                itemName != null ? ColorTextPrimary : ColorTextSecondary);
            topRow.AddChild(itemLabel);

            // Action buttons
            var btnContainer = new HBoxContainer();
            topRow.AddChild(btnContainer);

            if (itemName != null)
            {
                var unequipBtn = CreateSmallButton("X");
                unequipBtn.CustomMinimumSize = new Vector2(30, 28);
                unequipBtn.AddThemeColorOverride("font_color", ColorDanger);
                unequipBtn.Pressed += onUnequip;
                btnContainer.AddChild(unequipBtn);
            }

            var equipBtn = CreateSmallButton("+");
            equipBtn.CustomMinimumSize = new Vector2(30, 28);
            equipBtn.AddThemeColorOverride("font_color", ColorSuccess);
            equipBtn.Pressed += onEquip;
            btnContainer.AddChild(equipBtn);

            // Bonus info
            if (bonusInfo != null)
            {
                var bonusLabel = new Label { Text = $"  {bonusInfo}" };
                bonusLabel.AddThemeFontSizeOverride("font_size", 11);
                bonusLabel.AddThemeColorOverride("font_color", ColorSuccess);
                vbox.AddChild(bonusLabel);
            }
        }

        private void ShowEquipSelection(Player player, EquipType equipType, Equipment.EquipmentType? slotType = null)
        {
            var popup = new AcceptDialog
            {
                Title = equipType == EquipType.Weapon ? "Select Weapon" : $"Select {slotType}",
                MinSize = new Vector2I(450, 350)
            };
            AddChild(popup);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            popup.AddChild(vbox);

            var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
            vbox.AddChild(scroll);

            var list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(list);

            if (equipType == EquipType.Weapon)
            {
                // Show available weapons from player's Weapons list
                foreach (var wpn in player.Weapons)
                {
                    if (wpn == player.CurrentWeapon) continue;
                    var btn = CreateSecondaryButton($"{wpn.WeaponName}  (ATK:{wpn.AttackPower:F0})");
                    btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                    var captured = wpn;
                    btn.Pressed += () =>
                    {
                        player.EquipWeapon(captured);
                        popup.QueueFree();
                        RefreshEquip();
                    };
                    list.AddChild(btn);
                }
            }
            else if (slotType.HasValue)
            {
                // Show available equipment from GameDataManager (using dummy items for now)
                var equipList = GameDataManager.Instance?.EquipmentList ?? new Dictionary<string, int>();
                bool found = false;
                foreach (var kv in equipList)
                {
                    if (kv.Value <= 0) continue;
                    // Look up equipment data
                    var eqData = ResourcesManager.GetEquipment(kv.Key);
                    if (eqData == null) continue;
                    if (eqData.EquipmentTypeValue != slotType.Value) continue;

                    var btn = CreateSecondaryButton($"{eqData.EquipmentName}  (DEF:{eqData.Defense:F0})");
                    btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                    string capturedId = kv.Key;
                    btn.Pressed += () =>
                    {
                        var newEq = ResourcesManager.GetEquipment(capturedId);
                        if (newEq != null)
                        {
                            // Remove old equipment of same type
                            var old = player.Equipments.FirstOrDefault(e => e.EquipmentTypeValue == slotType.Value);
                            if (old != null) player.UnequipEquipment(old);
                            player.EquipEquipment(newEq);
                            // Remove from GameDataManager
                            GameDataManager.Instance?.RemoveEquipment(newEq);
                        }
                        popup.QueueFree();
                        RefreshEquip();
                    };
                    list.AddChild(btn);
                    found = true;
                }
                if (!found)
                {
                    list.AddChild(CreateInfoLabel("(No equipment available)"));
                }
            }

            popup.Popup();
        }

        #endregion

        #region Teammate Tab

        private void RefreshTeammates()
        {
            ClearChildren(_teammateListContainer);
            var teammates = GameDataManager.Instance?.Teammates?.Get() ?? new List<Player>();

            if (teammates.Count == 0)
            {
                _teammateListContainer.AddChild(CreateInfoLabel("No teammates."));
                return;
            }

            foreach (var tm in teammates)
                _teammateListContainer.AddChild(CreateTeammateCard(tm));
        }

        private Control CreateTeammateCard(Player teammate)
        {
            var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddThemeStyleboxOverride("panel", CreateSlotStyle(ColorSurface));

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddChild(vbox);

            // Name + level + class
            var header = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var nameLabel = new Label { Text = $"{teammate.CreatureName}  Lv.{teammate.Level}",
                SizeFlagsHorizontal = SizeFlags.ExpandFill };
            nameLabel.AddThemeFontOverride("font", GetFont());
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            nameLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
            header.AddChild(nameLabel);

            string cls = teammate.MainClass?.ClassName ?? "";
            if (!string.IsNullOrEmpty(cls))
            {
                var clsLabel = new Label { Text = $"[{cls}]" };
                clsLabel.AddThemeFontOverride("font", GetFont());
                clsLabel.AddThemeFontSizeOverride("font_size", 12);
                clsLabel.AddThemeColorOverride("font_color", ColorPrimary);
                header.AddChild(clsLabel);
            }
            vbox.AddChild(header);

            // HP bar
            var hpBar = CreateSmallBar(ColorDanger);
            hpBar.Value = teammate.Health;
            hpBar.MaxValue = Math.Max(teammate.MaxHealth, 1);
            hpBar.CustomMinimumSize = new Vector2(200, 12);
            vbox.AddChild(hpBar);

            // MP bar
            var mpBar = CreateSmallBar(new Color(0.3f, 0.5f, 0.95f, 1f));
            mpBar.Value = teammate.CurrentMana;
            mpBar.MaxValue = Math.Max(teammate.MaxMana, 1);
            mpBar.CustomMinimumSize = new Vector2(200, 12);
            vbox.AddChild(mpBar);

            return panel;
        }

        #endregion
    }
}
