/*
 * File: PopupInventoryPanel.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 物品背包面板，显示玩家背包中的物品列表和详情。
 * 支持物品分类筛选、使用、丢弃等操作。
 *
 * Key Features:
 * - 物品列表（左侧滚动列表 + 右侧详情面板）
 * - 分类筛选：全部/消耗品/材料
 * - 物品使用和丢弃功能
 * - 背包容量显示
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
    /// <summary>
    /// 物品背包面板
    /// </summary>
    public partial class PopupInventoryPanel : PopupPanelBase
    {
        private VBoxContainer _itemListContainer;
        private Label _detailName;
        private Label _detailDesc;
        private Label _detailInfo;
        private Label _capacityLabel;
        private string _selectedItemId;
        private Item.ItemType _filterType = Item.ItemType.Consumable;
        private bool _showAll = true;
        private Button _useButton;
        private Button _dropButton;

        /// <summary>
        /// 返回主菜单的回调
        /// </summary>
        public Action OnBackPressed { get; set; }

        public override void _Ready()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Name = "InventoryPanel";
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

            var hbox = new HBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.02f, AnchorTop = 0.02f,
                AnchorRight = 0.98f, AnchorBottom = 0.98f
            };
            AddChild(hbox);

            // Left: item list
            var leftVbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            leftVbox.CustomMinimumSize = new Vector2(350, 0);
            hbox.AddChild(leftVbox);

            leftVbox.AddChild(CreateTitleLabel("Items"));
            leftVbox.AddChild(CreateSeparator());

            var capHbox = new HBoxContainer();
            _capacityLabel = CreateInfoLabel("0/50");
            capHbox.AddChild(_capacityLabel);
            capHbox.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

            var allBtn = CreateSmallButton("All");
            allBtn.Pressed += () => { _showAll = true; Refresh(); };
            capHbox.AddChild(allBtn);

            var consumableBtn = CreateSmallButton("Consumables");
            consumableBtn.Pressed += () => { _showAll = false; _filterType = Item.ItemType.Consumable; Refresh(); };
            capHbox.AddChild(consumableBtn);

            var materialBtn = CreateSmallButton("Materials");
            materialBtn.Pressed += () => { _showAll = false; _filterType = Item.ItemType.Material; Refresh(); };
            capHbox.AddChild(materialBtn);

            leftVbox.AddChild(capHbox);

            var scroll = new ScrollContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            leftVbox.AddChild(scroll);

            _itemListContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(_itemListContainer);

            // Right: detail panel
            var rightPanel = CreateSurfacePanel();
            rightPanel.CustomMinimumSize = new Vector2(280, 0);
            hbox.AddChild(rightPanel);

            var rightVbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.05f, AnchorTop = 0.05f,
                AnchorRight = 0.95f, AnchorBottom = 0.95f
            };
            rightPanel.AddChild(rightVbox);

            _detailName = CreateTitleLabel("");
            rightVbox.AddChild(_detailName);
            rightVbox.AddChild(CreateSeparator());
            _detailDesc = CreateInfoLabel("Select an item to view details");
            _detailDesc.AutowrapMode = TextServer.AutowrapMode.Word;
            rightVbox.AddChild(_detailDesc);
            rightVbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
            _detailInfo = CreateInfoLabel("");
            rightVbox.AddChild(_detailInfo);

            rightVbox.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

            _useButton = CreatePrimaryButton("Use");
            _useButton.Disabled = true;
            _useButton.Pressed += OnUse;
            rightVbox.AddChild(_useButton);

            rightVbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });

            _dropButton = CreateSecondaryButton("Drop");
            _dropButton.Disabled = true;
            _dropButton.Pressed += OnDrop;
            rightVbox.AddChild(_dropButton);

            rightVbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 12) });

            var backBtn = CreateSecondaryButton("Back");
            backBtn.Pressed += () => OnBackPressed?.Invoke();
            rightVbox.AddChild(backBtn);
        }

        public void Refresh()
        {
            ClearChildren(_itemListContainer);
            _selectedItemId = null;
            ClearDetail();

            var allItems = InventoryManager.Instance?.GetAllItems() ?? new List<(string, Item, int)>();
            var filtered = _showAll
                ? allItems
                : allItems.Where(x => x.Item.ItemTypeValue == _filterType).ToList();

            if (filtered.Count == 0)
                _itemListContainer.AddChild(CreateInfoLabel("No items found."));

            foreach (var (itemId, item, qty) in filtered)
            {
                _itemListContainer.AddChild(CreateItemSlot(itemId, item, qty));
            }

            UpdateCapacity();
        }

        private Control CreateItemSlot(string itemId, Item item, int quantity)
        {
            var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddThemeStyleboxOverride("panel", CreateSlotStyle(
                _selectedItemId == itemId ? ColorPrimary : ColorSurface));

            var hbox = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddChild(hbox);

            var iconRect = new ColorRect
            {
                CustomMinimumSize = new Vector2(40, 40),
                Color = GetItemTypeColor(item.ItemTypeValue)
            };
            hbox.AddChild(iconRect);

            var infoVbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var nameLabel = new Label { Text = item.ItemName ?? itemId };
            nameLabel.AddThemeFontOverride("font", GetFont());
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            nameLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
            infoVbox.AddChild(nameLabel);

            var typeLabel = new Label { Text = $"{item.ItemTypeValue} | {item.ConsumableTypeValue}" };
            typeLabel.AddThemeFontSizeOverride("font_size", 11);
            typeLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
            infoVbox.AddChild(typeLabel);
            hbox.AddChild(infoVbox);

            var qtyLabel = new Label { Text = $"x{quantity}" };
            qtyLabel.AddThemeFontOverride("font", GetFont());
            qtyLabel.AddThemeFontSizeOverride("font_size", 16);
            qtyLabel.AddThemeColorOverride("font_color", ColorWarning);
            qtyLabel.VerticalAlignment = VerticalAlignment.Center;
            hbox.AddChild(qtyLabel);

            var btn = new Button { Text = "" };
            btn.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
            btn.Flat = true;
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            var cId = itemId; var cItem = item; var cQty = quantity;
            btn.Pressed += () => SelectItem(cId, cItem, cQty);
            btn.MouseFilter = MouseFilterEnum.Stop;
            panel.AddChild(btn);
            btn.SetAnchorsPreset(LayoutPreset.FullRect);

            return panel;
        }

        private void SelectItem(string itemId, Item item, int quantity)
        {
            _selectedItemId = itemId;
            Refresh();

            _detailName.Text = item.ItemName ?? itemId;
            _detailDesc.Text = item.Description ?? "";
            _detailInfo.Text = $"Type: {item.ItemTypeValue}\n"
                + $"SubType: {item.ConsumableTypeValue}\n"
                + $"Price: {item.Price} Gold\n"
                + $"Stack: {quantity}/{item.MaxStackCount}\n"
                + $"Effect: {item.EffectValue}";

            _useButton.Disabled = !item.IsUsable || item.ItemTypeValue != Item.ItemType.Consumable;
            _dropButton.Disabled = !item.IsDroppable;
        }

        private void ClearDetail()
        {
            _detailName.Text = "";
            _detailDesc.Text = "Select an item to view details";
            _detailInfo.Text = "";
            _useButton.Disabled = true;
            _dropButton.Disabled = true;
        }

        private void UpdateCapacity()
        {
            if (InventoryManager.Instance != null)
                _capacityLabel.Text = $"{InventoryManager.Instance.CurrentCount}/{InventoryManager.Instance.MaxCapacity}";
        }

        private void OnUse()
        {
            if (string.IsNullOrEmpty(_selectedItemId)) return;
            var player = GetPlayer();
            if (player == null) return;
            InventoryManager.Instance?.UseItem(_selectedItemId, player);
            Refresh();
            Log.Info($"Used item: {_selectedItemId}");
        }

        private void OnDrop()
        {
            if (string.IsNullOrEmpty(_selectedItemId)) return;
            InventoryManager.Instance?.RemoveItem(_selectedItemId, 1);
            _selectedItemId = null;
            Refresh();
            Log.Info($"Dropped item: {_selectedItemId}");
        }
    }
}
