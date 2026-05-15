/*
 * File: PopupSkillsPanel.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 技能面板，支持6角色职业切换、技能槽位管理、跨职业技能系统。
 * 每角色1职业8主动1大招4被动，主角额外专属技能。
 *
 * Key Features:
 * - 6角色切换（1主角+5可控角色）
 * - 职业切换与精通系统
 * - 主动技能槽位管理（最多4个）
 * - 被动技能槽位管理（最多4个）
 * - 大招展示
 * - 主角专属技能展示
 * - 跨职业技能装配（精通后可跨职业使用）
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Modules.Battle;
using hd2dtest.Scripts.Modules.SkillSystem;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
    public partial class PopupSkillsPanel : PopupPanelBase
    {
        // Character select
        private OptionButton _characterSelector;
        private Player _currentPlayer;

        // Class info
        private Label _classLabel;
        private Label _classDescLabel;
        private Label _proficiencyLabel;
        private Label _jpLabel;
        private Button _switchClassBtn;

        // Skill slots
        private VBoxContainer _activeSlotsContainer;
        private VBoxContainer _passiveSlotsContainer;
        private Control _ultimateSlotContainer;

        // Skill pool
        private VBoxContainer _learnedSkillsContainer;

        // Skill detail
        private Label _detailName;
        private Label _detailType;
        private Label _detailDesc;
        private Label _detailCost;
        private Label _detailCooldown;
        private VBoxContainer _effectsContainer;
        private Button _equipToSlotBtn;

        private Skill _selectedSkill;
        private int _pendingSlotIndex = -1;
        private bool _equippingPassive = false;

        private static readonly Color SlotActiveColor = new(0.2f, 0.5f, 0.9f, 0.3f);
        private static readonly Color SlotPassiveColor = new(0.5f, 0.2f, 0.8f, 0.3f);
        private static readonly Color SlotUltimateColor = new(0.9f, 0.7f, 0.2f, 0.3f);
        private static readonly Color SlotEmptyColor = new(0.15f, 0.15f, 0.2f, 0.5f);

        public Action OnBackPressed { get; set; }

        public override void _Ready()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Name = "SkillsPanel";
            Visible = false;
            LayoutMode = 1;
            AnchorLeft = 0.05f; AnchorTop = 0.03f;
            AnchorRight = 0.95f; AnchorBottom = 0.97f;
            MouseFilter = MouseFilterEnum.Stop;

            var bg = new ColorRect
            {
                LayoutMode = 1, AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1, Color = ColorBg
            };
            AddChild(bg);

            var mainHbox = new HBoxContainer
            {
                LayoutMode = 1, AnchorLeft = 0.01f, AnchorTop = 0.01f,
                AnchorRight = 0.99f, AnchorBottom = 0.99f
            };
            AddChild(mainHbox);

            // ===== Left Panel: Character + Slots =====
            var leftPanel = new VBoxContainer { CustomMinimumSize = new Vector2(350, 0) };
            mainHbox.AddChild(leftPanel);

            leftPanel.AddChild(CreateTitleLabel("Skills"));
            leftPanel.AddChild(CreateSeparator());

            // Character selector
            var charRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            _characterSelector = new OptionButton
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                Text = "Select Character"
            };
            _characterSelector.ItemSelected += OnCharacterSelected;
            charRow.AddChild(_characterSelector);

            var refreshBtn = CreateSmallButton("Refresh");
            refreshBtn.Pressed += RefreshCurrent;
            charRow.AddChild(refreshBtn);
            leftPanel.AddChild(charRow);

            // Class info
            var classPanel = CreateSurfacePanel();
            var classVbox = new VBoxContainer();
            classPanel.AddChild(classVbox);
            _classLabel = CreateInfoLabel("Class: --");
            _classLabel.AddThemeFontSizeOverride("font_size", 16);
            _classLabel.AddThemeColorOverride("font_color", ColorPrimary);
            classVbox.AddChild(_classLabel);

            _classDescLabel = CreateInfoLabel("");
            _classDescLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            classVbox.AddChild(_classDescLabel);

            _proficiencyLabel = CreateInfoLabel("");
            _proficiencyLabel.AddThemeColorOverride("font_color", ColorSuccess);
            classVbox.AddChild(_proficiencyLabel);

            _jpLabel = CreateInfoLabel("");
            _jpLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.4f, 1f, 1f));
            classVbox.AddChild(_jpLabel);

            _switchClassBtn = CreateSmallButton("Switch Class");
            _switchClassBtn.Pressed += OnSwitchClassPressed;
            classVbox.AddChild(_switchClassBtn);
            leftPanel.AddChild(classPanel);

            // Active skill slots
            var activeTitle = CreateSectionLabel("Active Skills (4)");
            leftPanel.AddChild(activeTitle);
            _activeSlotsContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            leftPanel.AddChild(_activeSlotsContainer);

            // Ultimate slot
            var ultimateTitle = CreateSectionLabel("Ultimate");
            leftPanel.AddChild(ultimateTitle);
            _ultimateSlotContainer = new Control { CustomMinimumSize = new Vector2(0, 40) };
            leftPanel.AddChild(_ultimateSlotContainer);

            // Passive skill slots
            var passiveTitle = CreateSectionLabel("Passive Skills (4)");
            leftPanel.AddChild(passiveTitle);
            _passiveSlotsContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            leftPanel.AddChild(_passiveSlotsContainer);

            leftPanel.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

            var backBtn = CreateSecondaryButton("Back");
            backBtn.Pressed += () => OnBackPressed?.Invoke();
            leftPanel.AddChild(backBtn);

            // ===== Right Panel: Skill Pool + Detail =====
            var rightPanel = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            mainHbox.AddChild(rightPanel);

            // Skill pool
            var poolTitle = CreateSectionLabel("Available Skills");
            rightPanel.AddChild(poolTitle);
            rightPanel.AddChild(CreateSeparator());

            var poolScroll = new ScrollContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 200)
            };
            rightPanel.AddChild(poolScroll);

            _learnedSkillsContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            poolScroll.AddChild(_learnedSkillsContainer);

            rightPanel.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

            // Skill detail
            var detailSurface = CreateSurfacePanel();
            rightPanel.AddChild(detailSurface);

            var detailVbox = new VBoxContainer();
            detailSurface.AddChild(detailVbox);

            _detailName = CreateTitleLabel("Select a skill");
            _detailName.AddThemeFontSizeOverride("font_size", 18);
            detailVbox.AddChild(_detailName);

            _detailType = CreateInfoLabel("");
            _detailType.AddThemeFontSizeOverride("font_size", 13);
            detailVbox.AddChild(_detailType);

            _detailDesc = CreateInfoLabel("");
            _detailDesc.AutowrapMode = TextServer.AutowrapMode.Word;
            detailVbox.AddChild(_detailDesc);

            var statsHbox = new HBoxContainer();
            _detailCost = CreateInfoLabel("");
            _detailCost.AddThemeFontSizeOverride("font_size", 13);
            _detailCost.AddThemeColorOverride("font_color", new Color(0.3f, 0.5f, 0.95f, 1f));
            statsHbox.AddChild(_detailCost);

            _detailCooldown = CreateInfoLabel("");
            _detailCooldown.AddThemeFontSizeOverride("font_size", 13);
            statsHbox.AddChild(_detailCooldown);
            detailVbox.AddChild(statsHbox);

            detailVbox.AddChild(CreateSeparator());
            var effectsTitle = CreateTitleLabel("Effects");
            effectsTitle.AddThemeFontSizeOverride("font_size", 14);
            detailVbox.AddChild(effectsTitle);

            _effectsContainer = new VBoxContainer();
            detailVbox.AddChild(_effectsContainer);

            _equipToSlotBtn = CreatePrimaryButton("Equip to Slot");
            _equipToSlotBtn.Pressed += OnEquipToSlot;
            _equipToSlotBtn.Disabled = true;
            detailVbox.AddChild(_equipToSlotBtn);
        }

        #region Refresh

        public void Refresh()
        {
            PopulateCharacterSelector();
            if (_characterSelector.ItemCount > 0)
            {
                _characterSelector.Selected = 0;
                OnCharacterSelected(0);
            }
        }

        private void RefreshCurrent()
        {
            if (_currentPlayer != null)
                RefreshForPlayer(_currentPlayer);
        }

        private void PopulateCharacterSelector()
        {
            _characterSelector.Clear();

            // Get main player
            var mainPlayer = GetPlayer();
            if (mainPlayer != null)
            {
                _characterSelector.AddItem($"★ {mainPlayer.CreatureName}");
            }

            // Get teammates
            var teammates = GameDataManager.Instance?.Teammates?.Get() ?? new List<Player>();
            foreach (var tm in teammates)
            {
                _characterSelector.AddItem(tm.CreatureName);
            }
        }

        private void OnCharacterSelected(long index)
        {
            int idx = (int)index;
            var mainPlayer = GetPlayer();
            if (idx == 0 && mainPlayer != null)
            {
                _currentPlayer = mainPlayer;
            }
            else
            {
                var teammates = GameDataManager.Instance?.Teammates?.Get() ?? new List<Player>();
                int teammateIdx = idx - 1;
                if (teammateIdx >= 0 && teammateIdx < teammates.Count)
                    _currentPlayer = teammates[teammateIdx];
                else
                    _currentPlayer = mainPlayer;
            }

            RefreshForPlayer(_currentPlayer);
        }

        #endregion

        #region Player Data Display

        private void RefreshForPlayer(Player player)
        {
            if (player == null) return;

            // Class info
            var bc = BattleClass.Get(player.CurrentProfessionId);
            string className = bc?.ClassName ?? player.MainClass?.ClassName ?? "None";
            _classLabel.Text = $"Class: {className}";
            _classDescLabel.Text = bc?.Description ?? "";

            // Proficiency
            string profText = "";
            if (!string.IsNullOrEmpty(player.CurrentProfessionId) &&
                player.ClassProficiencies.TryGetValue(player.CurrentProfessionId, out var prof))
            {
                profText = $"Proficiency: {prof.ProficiencyLevel}/100";
                if (player.IsClassProficient(player.CurrentProfessionId))
                    profText += " [MASTERED]";
            }
            _proficiencyLabel.Text = profText;

            // JP display
            _jpLabel.Text = $"JP: {player.JP}";

            // Slots
            RefreshActiveSlots(player);
            RefreshUltimateSlot(player);
            RefreshPassiveSlots(player);
            RefreshSkillPool(player);

            // Clear detail
            ClearDetail();
        }

        private void RefreshActiveSlots(Player player)
        {
            ClearChildren(_activeSlotsContainer);

            for (int i = 0; i < Player.MaxActiveSkillSlots; i++)
            {
                int slotIdx = i;
                string skillId = i < player.EquippedActiveSkillIds.Count ? player.EquippedActiveSkillIds[i] : "";
                var skill = !string.IsNullOrEmpty(skillId) ? player.Skills.FirstOrDefault(s => s.Id == skillId) : null;

                var slot = CreateSlotCard(
                    $"Slot {i + 1}",
                    skill?.SkillName ?? "(Empty)",
                    skill != null ? GetSkillTypeColor(skill) : SlotEmptyColor,
                    SlotActiveColor,
                    () => OnSlotClicked(slotIdx, false),
                    () =>
                    {
                        if (skill != null) OnUnequipSlot(slotIdx, false);
                    }
                );
                _activeSlotsContainer.AddChild(slot);
            }
        }

        private void RefreshUltimateSlot(Player player)
        {
            ClearChildren(_ultimateSlotContainer);

            var ult = player.UltimateSkill;
            var slot = CreateSlotCard(
                "Ultimate",
                ult?.SkillName ?? "(None)",
                ult != null ? GetSkillTypeColor(ult) : SlotEmptyColor,
                SlotUltimateColor,
                () =>
                {
                    if (ult != null) SelectSkill(ult);
                },
                null
            );
            _ultimateSlotContainer.AddChild(slot);
            _ultimateSlotContainer.CustomMinimumSize = new Vector2(0, 50);
        }

        private void RefreshPassiveSlots(Player player)
        {
            ClearChildren(_passiveSlotsContainer);

            var equippedPassiveIds = player.GetEquippedPassiveSkillIds();
            var allPassives = player.EquippedPassives;

            for (int i = 0; i < Player.MaxPassiveSkillSlots; i++)
            {
                int slotIdx = i;
                string passiveId = i < equippedPassiveIds.Count ? equippedPassiveIds[i] : "";
                var passive = !string.IsNullOrEmpty(passiveId)
                    ? allPassives.FirstOrDefault(p => p.PassiveName == passiveId || GetPassiveId(p) == passiveId)
                    : null;

                var slot = CreateSlotCard(
                    $"Passive {i + 1}",
                    passive?.PassiveName ?? "(Empty)",
                    passive != null ? ColorSuccess : SlotEmptyColor,
                    SlotPassiveColor,
                    () => OnSlotClicked(slotIdx, true),
                    () =>
                    {
                        if (passive != null) OnUnequipPassiveSlot(slotIdx, player);
                    }
                );
                _passiveSlotsContainer.AddChild(slot);
            }
        }

        private void RefreshSkillPool(Player player)
        {
            ClearChildren(_learnedSkillsContainer);

            var equippedIds = new HashSet<string>(player.EquippedActiveSkillIds.Where(id => !string.IsNullOrEmpty(id)));
            if (player.UltimateSkill != null)
                equippedIds.Add(player.UltimateSkill.Id);

            bool foundAny = false;

            // 1. Learned skills (not equipped)
            foreach (var skill in player.Skills)
            {
                if (equippedIds.Contains(skill.Id)) continue;
                foundAny = true;
                _learnedSkillsContainer.AddChild(CreateSkillPoolItem(skill, player));
            }

            // 2. Locked skills from current class (not yet learned)
            string cid = player.CurrentProfessionId;
            if (!string.IsNullOrEmpty(cid))
            {
                var (activeIds, ultimateId, _) = SkillManager.GetClassSkillIds(cid);

                // Locked active skills
                foreach (var sid in activeIds)
                {
                    if (player.Skills.Any(s => s.Id == sid)) continue; // already learned

                    foundAny = true;
                    var template = SkillManager.GetSkillTemplate(sid);
                    int jpCost = BattleClass.GetSkillJpCost(cid, sid);
                    var item = CreateLockedSkillItem(sid, template?.SkillName ?? sid, jpCost, player);
                    _learnedSkillsContainer.AddChild(item);
                }

                // Locked ultimate
                if (!string.IsNullOrEmpty(ultimateId) && player.UltimateSkill == null && !player.Skills.Any(s => s.Id == ultimateId))
                {
                    foundAny = true;
                    var ultTemplate = SkillManager.GetSkillTemplate(ultimateId);
                    int ultJpCost = BattleClass.GetUltimateJpCost(cid);
                    var item = CreateLockedSkillItem(ultimateId, ultTemplate?.SkillName ?? "Ultimate", ultJpCost, player);
                    // Mark as ultimate
                    var ultLabel = new Label { Text = " [ULTIMATE]" };
                    ultLabel.AddThemeFontSizeOverride("font_size", 10);
                    ultLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f, 1f));
                    item.AddChild(ultLabel);
                    _learnedSkillsContainer.AddChild(item);
                }
            }

            // 3. Cross-class skills (from proficient classes)
            foreach (var kv in player.ClassProficiencies)
            {
                if (kv.Key == player.CurrentProfessionId) continue;
                if (!player.IsClassProficient(kv.Key)) continue;

                foreach (var skillId in kv.Value.LearnedSkillIds)
                {
                    if (equippedIds.Contains(skillId)) continue;
                    if (player.Skills.Any(s => s.Id == skillId)) continue;

                    var skill = SkillManager.GetSkillTemplate(skillId);
                    if (skill != null)
                    {
                        foundAny = true;
                        var item = CreateSkillPoolItem(skill, player);
                        var crossLabel = new Label { Text = " [Cross-class]" };
                        crossLabel.AddThemeFontSizeOverride("font_size", 10);
                        crossLabel.AddThemeColorOverride("font_color", ColorWarning);
                        item.AddChild(crossLabel);
                        _learnedSkillsContainer.AddChild(item);
                    }
                }
            }

            if (!foundAny)
            {
                _learnedSkillsContainer.AddChild(CreateInfoLabel("No additional skills available."));
            }
        }

        /// <summary>
        /// 创建锁定技能的显示项（带JP消耗和学习按钮）
        /// </summary>
        private Control CreateLockedSkillItem(string skillId, string skillName, int jpCost, Player player)
        {
            var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.15f, 0.12f, 0.12f, 0.8f),
                BorderWidthLeft = 1, BorderWidthTop = 1,
                BorderWidthRight = 1, BorderWidthBottom = 1,
                BorderColor = new Color(0.4f, 0.3f, 0.3f, 0.3f),
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 6, ContentMarginTop = 3,
                ContentMarginRight = 6, ContentMarginBottom = 3
            };
            panel.AddThemeStyleboxOverride("panel", style);

            var hbox = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddChild(hbox);

            // Lock indicator
            var lockIcon = new Label { Text = "[LOCKED]", CustomMinimumSize = new Vector2(56, 32) };
            lockIcon.AddThemeFontSizeOverride("font_size", 11);
            lockIcon.AddThemeColorOverride("font_color", new Color(0.6f, 0.3f, 0.3f, 1f));
            hbox.AddChild(lockIcon);

            // Skill name
            var nameLabel = new Label
            {
                Text = skillName,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            nameLabel.AddThemeFontOverride("font", GetFont());
            nameLabel.AddThemeFontSizeOverride("font_size", 13);
            nameLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
            hbox.AddChild(nameLabel);

            // JP cost
            var costLabel = new Label { Text = $"{jpCost} JP" };
            costLabel.AddThemeFontSizeOverride("font_size", 12);
            bool canAfford = player.JP >= jpCost;
            costLabel.AddThemeColorOverride("font_color",
                canAfford ? new Color(0.6f, 0.4f, 1f, 1f) : new Color(0.8f, 0.3f, 0.3f, 0.6f));
            hbox.AddChild(costLabel);

            // Learn button
            var learnBtn = CreateSmallButton("Learn");
            learnBtn.Disabled = !canAfford;
            var capturedId = skillId;
            var capturedPlayer = player;
            learnBtn.Pressed += () =>
            {
                bool success = capturedPlayer.LearnSkill(capturedId);
                if (success)
                {
                    RefreshForPlayer(capturedPlayer);
                    ShowToast($"Learned {skillName}!");
                }
                else
                {
                    ShowToast("Failed to learn skill.");
                }
            };
            hbox.AddChild(learnBtn);

            return panel;
        }

        #endregion

        #region Slot Interaction

        private void OnSlotClicked(int slotIndex, bool isPassive)
        {
            _pendingSlotIndex = slotIndex;
            _equippingPassive = isPassive;

            if (_selectedSkill != null)
            {
                // Try to equip the currently selected skill
                TryEquipSkill(_selectedSkill.Id, slotIndex, isPassive);
            }
            else
            {
                // Prompt user to select a skill first
                _equipToSlotBtn.Text = isPassive ? "Equip Passive to Slot" : "Equip to Slot";
                _equipToSlotBtn.Disabled = true;
                ShowToast("Select a skill from the right panel, then click a slot.");
            }
        }

        private void OnUnequipSlot(int slotIndex, bool isPassive)
        {
            if (_currentPlayer == null) return;

            if (isPassive)
            {
                _currentPlayer.UnequipPassiveSkillSlot(slotIndex);
            }
            else
            {
                _currentPlayer.UnequipActiveSkillSlot(slotIndex);
            }
            RefreshCurrent();
        }

        private void OnUnequipPassiveSlot(int slotIndex, Player player)
        {
            player.UnequipPassiveSkillSlot(slotIndex);
            RefreshCurrent();
        }

        private void TryEquipSkill(string skillId, int slotIndex, bool isPassive)
        {
            if (_currentPlayer == null) return;

            bool success;
            if (isPassive)
            {
                success = _currentPlayer.EquipPassiveSkillSlot(skillId, slotIndex);
            }
            else
            {
                success = _currentPlayer.EquipActiveSkillSlot(skillId, slotIndex);
            }

            if (success)
            {
                _pendingSlotIndex = -1;
                _equippingPassive = false;
                RefreshCurrent();
                ShowToast($"Skill equipped to slot {slotIndex + 1}");
            }
            else
            {
                ShowToast("Failed to equip skill.");
            }
        }

        #endregion

        #region Skill Pool Items

        private Control CreateSkillPoolItem(Skill skill, Player player)
        {
            var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var style = new StyleBoxFlat
            {
                BgColor = _selectedSkill == skill ? ColorPrimary * new Color(0.5f, 0.5f, 0.5f, 1f) : ColorSurface,
                BorderWidthLeft = 1, BorderWidthTop = 1,
                BorderWidthRight = 1, BorderWidthBottom = 1,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.3f),
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 6, ContentMarginTop = 3,
                ContentMarginRight = 6, ContentMarginBottom = 3
            };
            panel.AddThemeStyleboxOverride("panel", style);

            var hbox = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddChild(hbox);

            // Type indicator
            var typeColor = GetSkillTypeColor(skill);
            var indicator = new ColorRect
            {
                CustomMinimumSize = new Vector2(4, 32), Color = typeColor
            };
            hbox.AddChild(indicator);

            // Name + info
            var infoVbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var nameLabel = new Label { Text = skill.SkillName ?? skill.Id };
            nameLabel.AddThemeFontOverride("font", GetFont());
            nameLabel.AddThemeFontSizeOverride("font_size", 13);
            nameLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
            infoVbox.AddChild(nameLabel);

            var subLabel = new Label
            {
                Text = IsSkillEquipped(skill, player) ? "[Equipped]" : $"MP:{skill.ManaCost} CD:{skill.Cooldown}s"
            };
            subLabel.AddThemeFontSizeOverride("font_size", 10);
            subLabel.AddThemeColorOverride("font_color",
                IsSkillEquipped(skill, player) ? ColorSuccess : ColorTextSecondary);
            infoVbox.AddChild(subLabel);
            hbox.AddChild(infoVbox);

            // Click to select
            var clickBtn = new Button { Text = "", Flat = true };
            clickBtn.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
            clickBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            clickBtn.MouseFilter = MouseFilterEnum.Stop;
            var captured = skill;
            clickBtn.Pressed += () => SelectSkill(captured);
            panel.AddChild(clickBtn);
            clickBtn.SetAnchorsPreset(LayoutPreset.FullRect);

            return panel;
        }

        private bool IsSkillEquipped(Skill skill, Player player)
        {
            return player.EquippedActiveSkillIds.Contains(skill.Id)
                || (player.UltimateSkill?.Id == skill.Id);
        }

        #endregion

        #region Slot Cards

        private Control CreateSlotCard(string label, string content, Color accentColor, Color bgColor,
            Action onClick, Action onUnequip)
        {
            var panel = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 36),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };

            var style = new StyleBoxFlat
            {
                BgColor = bgColor,
                BorderWidthLeft = 1, BorderWidthTop = 1,
                BorderWidthRight = 1, BorderWidthBottom = 1,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.3f),
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 8, ContentMarginTop = 4,
                ContentMarginRight = 8, ContentMarginBottom = 4
            };
            panel.AddThemeStyleboxOverride("panel", style);

            var hbox = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddChild(hbox);

            // Color accent
            var indicator = new ColorRect
            {
                CustomMinimumSize = new Vector2(4, 24), Color = accentColor
            };
            hbox.AddChild(indicator);

            // Label
            var slotLabel = new Label { Text = $"{label}:", CustomMinimumSize = new Vector2(80, 0) };
            slotLabel.AddThemeFontOverride("font", GetFont());
            slotLabel.AddThemeFontSizeOverride("font_size", 12);
            slotLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.5f, 0.6f, 1f));
            hbox.AddChild(slotLabel);

            // Content
            var contentLabel = new Label
            {
                Text = content,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            contentLabel.AddThemeFontOverride("font", GetFont());
            contentLabel.AddThemeFontSizeOverride("font_size", 12);
            contentLabel.AddThemeColorOverride("font_color",
                content != "(Empty)" && content != "(None)" ? ColorTextPrimary : ColorTextSecondary);
            hbox.AddChild(contentLabel);

            // Buttons
            var btnBox = new HBoxContainer();

            if (content != "(Empty)" && content != "(None)" && onUnequip != null)
            {
                var xBtn = CreateSmallButton("X");
                xBtn.CustomMinimumSize = new Vector2(24, 22);
                xBtn.AddThemeColorOverride("font_color", ColorDanger);
                xBtn.Pressed += onUnequip;
                btnBox.AddChild(xBtn);
            }

            // Clickable area
            var selectBtn = new Button { Text = "", Flat = true };
            selectBtn.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
            selectBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            selectBtn.MouseFilter = MouseFilterEnum.Stop;
            selectBtn.Pressed += () => onClick?.Invoke();
            panel.AddChild(selectBtn);
            selectBtn.SetAnchorsPreset(LayoutPreset.FullRect);

            return panel;
        }

        #endregion

        #region Skill Detail

        private void SelectSkill(Skill skill)
        {
            _selectedSkill = skill;
            _detailName.Text = skill.SkillName ?? skill.Id;

            if (skill.SkillDefs?.Count > 0)
            {
                var st = skill.SkillDefs[0].Type;
                _detailType.Text = $"Type: {st}";
                _detailType.AddThemeColorOverride("font_color", GetSkillTypeColor(skill));
            }

            _detailDesc.Text = skill.Description ?? "";
            _detailCost.Text = $"MP: {skill.ManaCost}  ";
            _detailCooldown.Text = $"CD: {skill.Cooldown}s";

            ClearChildren(_effectsContainer);
            if (skill.SkillDefs?.Count > 0)
            {
                foreach (var def in skill.SkillDefs)
                {
                    var effectLabel = new Label
                    {
                        Text = $"  {def.Type}: x{def.DamageCoefficient:F1} | {def.DamageTypeString} | {def.Duration}t"
                    };
                    effectLabel.AddThemeFontSizeOverride("font_size", 12);
                    effectLabel.AddThemeColorOverride("font_color", GetSkillTypeDefColor(def.Type));
                    _effectsContainer.AddChild(effectLabel);
                }
            }

            // Enable equip button if a slot was selected
            _equipToSlotBtn.Disabled = false;
            if (_pendingSlotIndex >= 0)
            {
                _equipToSlotBtn.Text = _equippingPassive
                    ? $"Equip to Passive Slot {_pendingSlotIndex + 1}"
                    : $"Equip to Slot {_pendingSlotIndex + 1}";
            }
            else
            {
                _equipToSlotBtn.Text = "Click a slot first";
                _equipToSlotBtn.Disabled = true;
            }

            RefreshCurrent();
        }

        private void OnEquipToSlot()
        {
            if (_selectedSkill == null || _pendingSlotIndex < 0 || _currentPlayer == null) return;
            TryEquipSkill(_selectedSkill.Id, _pendingSlotIndex, _equippingPassive);
        }

        private void ClearDetail()
        {
            _selectedSkill = null;
            _detailName.Text = "Select a skill";
            _detailDesc.Text = "";
            _detailType.Text = "";
            _detailCost.Text = "";
            _detailCooldown.Text = "";
            ClearChildren(_effectsContainer);
            _equipToSlotBtn.Text = "Click a slot first";
            _equipToSlotBtn.Disabled = true;
        }

        #endregion

        #region Class Switching

        private void OnSwitchClassPressed()
        {
            if (_currentPlayer == null) return;

            var unlockedIds = _currentPlayer.GetUnlockedClassIds();
            if (unlockedIds.Count <= 1)
            {
                ShowToast("No other classes unlocked.");
                return;
            }

            // Show class selection dialog
            var popup = new AcceptDialog
            {
                Title = "Switch Class",
                MinSize = new Vector2I(400, 300)
            };
            AddChild(popup);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            popup.AddChild(vbox);

            var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
            vbox.AddChild(scroll);

            var list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(list);

            foreach (var cid in unlockedIds)
            {
                var bc = BattleClass.Get(cid);
                if (bc == null) continue;

                bool isCurrent = cid == _currentPlayer.CurrentProfessionId;
                string label = isCurrent ? $"✓ {bc.ClassName} (Current)" : bc.ClassName;

                var btn = CreateSecondaryButton(label);
                btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                btn.Disabled = isCurrent;

                string capturedId = cid;
                btn.Pressed += () =>
                {
                    if (_currentPlayer.SwitchClass(capturedId))
                    {
                        popup.QueueFree();
                        RefreshCurrent();
                        ShowToast($"Switched to {bc.ClassName}");
                    }
                };
                list.AddChild(btn);

                // Class info
                var info = CreateInfoLabel($"  HP:{bc.HealthGrowth:F1} ATK:{bc.AttackGrowth:F1} DEF:{bc.DefenseGrowth:F1} SPD:{bc.SpeedGrowth:F1}");
                info.AddThemeFontSizeOverride("font_size", 11);
                list.AddChild(info);

                // Proficiency
                if (_currentPlayer.ClassProficiencies.TryGetValue(capturedId, out var prof))
                {
                    var profLabel = CreateInfoLabel($"  Proficiency: {prof.ProficiencyLevel}/100");
                    profLabel.AddThemeFontSizeOverride("font_size", 11);
                    profLabel.AddThemeColorOverride("font_color", ColorSuccess);
                    list.AddChild(profLabel);
                }
            }

            popup.Popup();
        }

        #endregion

        #region Utilities

        private static string GetPassiveId(PassiveSkill ps)
        {
            // Use passive name as a simple identifier
            return ps?.PassiveName ?? "";
        }

        private static Color GetSkillTypeColor(Skill skill)
        {
            if (skill.SkillDefs?.Count > 0)
            {
                return skill.SkillDefs[0].Type switch
                {
                    Skill.SkillType.Attack => ColorAttack,
                    Skill.SkillType.Defense => ColorDefense,
                    Skill.SkillType.Healing => ColorHealing,
                    Skill.SkillType.Support => ColorSupport,
                    _ => ColorTextPrimary
                };
            }
            return ColorTextPrimary;
        }

        private static Color GetSkillTypeDefColor(Skill.SkillType type) => type switch
        {
            Skill.SkillType.Attack => ColorAttack,
            Skill.SkillType.Defense => ColorDefense,
            Skill.SkillType.Healing => ColorHealing,
            Skill.SkillType.Support => ColorSupport,
            _ => ColorTextPrimary
        };

        private void ShowToast(string message)
        {
            Log.Info($"[SkillsPanel] {message}");
        }

        #endregion
    }
}
