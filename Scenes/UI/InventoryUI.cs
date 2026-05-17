using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scenes.UI
{
	[GlobalClass]
	public partial class InventoryUI : Control
	{
		[Export] public Player PlayerData { get; set; }
		[Export] public int BaseCapacity { get; set; } = 20;
		[Export] public int MaxCapacity { get; set; } = 100;

		private Panel _mainPanel;
		private PanelContainer _headerPanel;
		private Label _titleLabel;
		private Label _capacityLabel;
		private Button _closeButton;
		private Button _sortButton;
		private TabBar _categoryTabBar;
		private ScrollContainer _itemScrollContainer;
		private GridContainer _itemGridContainer;
		private PanelContainer _detailPanel;
		private Label _itemNameLabel;
		private RichTextLabel _itemDescriptionLabel;
		private Button _useButton;
		private Button _equipButton;
		private Button _dropButton;
		private HBoxContainer _actionButtons;
		private Panel _dragPreview;
		private TextureRect _dragIcon;
		private Label _dragCount;

		private int _currentCapacity;
		private int _usedSlots;
		private Item _selectedItem;
		private Item _draggedItem;
		private bool _isDragging = false;
		private bool _isVisible = false;

		public enum SortType
		{
			Type,
			Name,
			Level,
			Price,
			TimeAcquired
		}

		public enum CategoryTab
		{
			All = 0,
			Consumables = 1,
			Materials = 2,
			KeyItems = 3,
			QuestItems = 4,
			Miscellaneous = 5
		}

		private SortType _currentSortType = SortType.Type;
		private CategoryTab _currentCategory = CategoryTab.All;
		private List<Item> _inventoryItems = [];

		private readonly Color _colorBackground = new(0.13f, 0.19f, 0.25f, 0.98f);
		private readonly Color _colorSurface = new(0.18f, 0.26f, 0.34f, 1f);
		private readonly Color _colorPrimary = new(0.2f, 0.6f, 0.86f, 1f);
		private readonly Color _colorSecondary = new(0.18f, 0.8f, 0.44f, 1f);
		private readonly Color _colorDanger = new(0.9f, 0.3f, 0.23f, 1f);
		private readonly Color _colorWarning = new(0.94f, 0.77f, 0.06f, 1f);
		private readonly Color _colorTextPrimary = new(0.93f, 0.94f, 0.95f, 1f);
		private readonly Color _colorTextSecondary = new(0.74f, 0.76f, 0.78f, 1f);

		public override void _Ready()
		{
			_currentCapacity = BaseCapacity;
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
			OffsetLeft = -400;
			OffsetRight = 400;
			OffsetTop = -300;
			OffsetBottom = 300;

			CreateMainPanel();
			CreateHeader();
			CreateCategoryTabs();
			CreateItemGrid();
			CreateDetailPanel();
			CreateDragPreview();
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
				BorderWidthLeft = 3,
				BorderWidthRight = 3,
				BorderWidthTop = 3,
				BorderWidthBottom = 3,
				BorderColor = _colorPrimary,
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				ShadowSize = 8,
				ShadowColor = new Color(0, 0, 0, 0.4f)
			};
			_mainPanel.AddThemeStyleboxOverride("panel", styleBox);

			AddChild(_mainPanel);
		}

		private void CreateHeader()
		{
			_headerPanel = new PanelContainer
			{
				Name = "HeaderPanel"
			};
			var headerStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				BorderWidthBottom = 2,
				BorderColor = new Color(_colorPrimary.R, _colorPrimary.G, _colorPrimary.B, 0.3f),
				CornerRadiusTopLeft = 9,
				CornerRadiusTopRight = 9,
				ContentMarginLeft = 16,
				ContentMarginRight = 16,
				ContentMarginTop = 12,
				ContentMarginBottom = 12
			};
			_headerPanel.AddThemeStyleboxOverride("panel", headerStyle);

			var headerHBox = new HBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};

			_titleLabel = new Label
			{
				Text = "📦 Inventory",
				HorizontalAlignment = HorizontalAlignment.Left
			};
			_titleLabel.AddThemeFontSizeOverride("font_size", 18);
			_titleLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			_titleLabel.AddThemeColorOverride("font_color", _colorTextPrimary);

			_capacityLabel = new Label
			{
				Text = $"Slots: 0/{_currentCapacity}",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			_capacityLabel.AddThemeFontSizeOverride("font_size", 14);
			_capacityLabel.AddThemeColorOverride("font_color", _colorTextSecondary);

			_sortButton = new Button
			{
				Text = "↕ Sort",
				TooltipText = "Change sort order"
			};
			_sortButton.Pressed += OnSortButtonPressed;
			ApplyButtonStyle(_sortButton, false);

			_closeButton = new Button
			{
				Text = "✕",
				TooltipText = "Close inventory"
			};
			_closeButton.Pressed += ToggleVisibility;
			ApplyButtonStyle(_closeButton, true, _colorDanger);

			headerHBox.AddChild(_titleLabel);
			headerHBox.AddChild(_capacityLabel);
			headerHBox.AddChild(_sortButton);
			headerHBox.AddChild(_closeButton);

			_headerPanel.AddChild(headerHBox);
			_headerPanel.AnchorLeft = 0;
			_headerPanel.AnchorRight = 1;
			_headerPanel.AnchorTop = 0;
			_headerPanel.AnchorBottom = 0;
			_headerPanel.OffsetBottom = 50;

			_mainPanel.AddChild(_headerPanel);
		}

		private void CreateCategoryTabs()
		{
			_categoryTabBar = new TabBar
			{
				Name = "CategoryTabs",
				TabAlignment = TabBar.AlignmentMode.Center
			};

			string[] tabs = { "All", "❤️ Consumables", "📦 Materials", "🔑 Key Items", "📜 Quest Items", "✨ Misc" };
			foreach (var tab in tabs)
			{
				_categoryTabBar.AddTab(tab);
			}

			_categoryTabBar.TabChanged += OnCategoryChanged;

			var tabContainer = new MarginContainer
			{
				AnchorLeft = 0,
				AnchorRight = 1,
				AnchorTop = 0,
				AnchorBottom = 0,
				OffsetTop = 55,
				OffsetBottom = 90
			};
			tabContainer.AddChild(_categoryTabBar);
			_mainPanel.AddChild(tabContainer);
		}

		private void CreateItemGrid()
		{
			_itemScrollContainer = new ScrollContainer
			{
				Name = "ItemScroll",
				AnchorLeft = 0,
				AnchorRight = 0.65f,
				AnchorTop = 0,
				AnchorBottom = 1,
				OffsetTop = 95,
				OffsetLeft = 10,
				OffsetRight = -10,
				OffsetBottom = -10
			};

			_itemGridContainer = new GridContainer
			{
				Name = "ItemGrid",
				Columns = 6,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};

			_itemScrollContainer.AddChild(_itemGridContainer);
			_mainPanel.AddChild(_itemScrollContainer);
		}

		private void CreateDetailPanel()
		{
			_detailPanel = new PanelContainer
			{
				Name = "DetailPanel",
				AnchorLeft = 0.67f,
				AnchorRight = 1f,
				AnchorTop = 0,
				AnchorBottom = 1,
				OffsetTop = 95,
				OffsetLeft = 10,
				OffsetRight = -10,
				OffsetBottom = -10
			};

			var detailStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8,
				ContentMarginLeft = 16,
				ContentMarginRight = 16,
				ContentMarginTop = 16,
				ContentMarginBottom = 16
			};
			_detailPanel.AddThemeStyleboxOverride("panel", detailStyle);

			var detailVBox = new VBoxContainer
			{
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};

			_itemNameLabel = new Label
			{
				Text = "Select an item",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			_itemNameLabel.AddThemeFontSizeOverride("font_size", 16);
			_itemNameLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf"));
			_itemNameLabel.AddThemeColorOverride("font_color", _colorPrimary);

			var separator = new HSeparator();

			_itemDescriptionLabel = new RichTextLabel
			{
				FitContent = true,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			_itemDescriptionLabel.AddThemeConstantOverride("lines_skipped", 0);
			_itemDescriptionLabel.AddThemeColorOverride("default_color", _colorTextSecondary);

			_actionButtons = new HBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center
			};

			_useButton = new Button
			{
				Text = "Use",
				Disabled = true
			};
			_useButton.Pressed += OnUseButtonPressed;
			ApplyButtonStyle(_useButton, false, _colorSecondary);

			_equipButton = new Button
			{
				Text = "Equip",
				Disabled = true
			};
			_equipButton.Pressed += OnEquipButtonPressed;
			ApplyButtonStyle(_equipButton, false, _colorPrimary);

			_dropButton = new Button
			{
				Text = "Drop",
				Disabled = true
			};
			_dropButton.Pressed += OnDropButtonPressed;
			ApplyButtonStyle(_dropButton, false, _colorDanger);

			_actionButtons.AddChild(_useButton);
			_actionButtons.AddChild(_equipButton);
			_actionButtons.AddChild(_dropButton);

			detailVBox.AddChild(_itemNameLabel);
			detailVBox.AddChild(separator);
			detailVBox.AddChild(_itemDescriptionLabel);
			detailVBox.AddChild(_actionButtons);

			_detailPanel.AddChild(detailVBox);
			_mainPanel.AddChild(_detailPanel);
		}

		private void CreateDragPreview()
		{
			_dragPreview = new Panel
			{
				Name = "DragPreview",
				Visible = false,
				ZIndex = 100,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			var previewStyle = new StyleBoxFlat
			{
				BgColor = new Color(1, 1, 1, 0.9f),
				CornerRadiusTopLeft = 6,
				CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6,
				CornerRadiusBottomRight = 6,
				ShadowSize = 4,
				ShadowColor = new Color(0, 0, 0, 0.5f)
			};
			_dragPreview.AddThemeStyleboxOverride("panel", previewStyle);

			var dragVBox = new VBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center
			};

			_dragIcon = new TextureRect
			{
				Size = new Vector2(40, 40),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
			};

			_dragCount = new Label
			{
				HorizontalAlignment = HorizontalAlignment.Center
			};
			_dragCount.AddThemeFontSizeOverride("font_size", 12);
			_dragCount.AddThemeColorOverride("font_color", _colorTextPrimary);

			dragVBox.AddChild(_dragIcon);
			dragVBox.AddChild(_dragCount);
			_dragPreview.AddChild(dragVBox);

			AddChild(_dragPreview);
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

			var disabledStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.3f, 0.3f, 0.3f, 0.5f),
				CornerRadiusTopLeft = cr, CornerRadiusTopRight = cr,
				CornerRadiusBottomLeft = cr, CornerRadiusBottomRight = cr,
				ContentMarginLeft = ml, ContentMarginRight = mr,
				ContentMarginTop = mt, ContentMarginBottom = mb
			};
			button.AddThemeStyleboxOverride("disabled", disabledStyle);

			button.AddThemeColorOverride("font_color", _colorTextPrimary);
			button.AddThemeFontSizeOverride("font_size", isSmall ? 12 : 14);
		}

		private void SetupSignals()
		{
			GuiInput += OnGuiInput;
		}

		public override void _Process(double delta)
		{
			if (_isDragging && _dragPreview != null)
			{
				_dragPreview.Position = GetGlobalMousePosition() + new Vector2(15, 15);
			}
		}

		public void LoadInventory(Player player)
		{
			PlayerData = player;
			if (player == null) return;

			_inventoryItems.Clear();

			foreach (var entry in player.Inventory)
			{
				var item = new Item
				{
					Id = entry.Key,
					ItemName = entry.Key,
					Description = "",
					StackCount = entry.Value,
					MaxStackCount = 99,
					IsUsable = true,
					IsSellable = true,
					IsDroppable = true
				};
				_inventoryItems.Add(item);
			}

			RefreshItemGrid();
			UpdateCapacityDisplay();
		}

		private void RefreshItemGrid()
		{
			foreach (var child in _itemGridContainer.GetChildren())
			{
				child.QueueFree();
			}

			List<Item> filteredItems = FilterItemsByCategory(_inventoryItems, _currentCategory);
			List<Item> sortedItems = SortItems(filteredItems, _currentSortType);

			_usedSlots = sortedItems.Count;

			foreach (var item in sortedItems)
			{
				CreateItemSlot(item);
			}

			int emptySlots = _currentCapacity - _usedSlots;
			for (int i = 0; i < Math.Min(emptySlots, 12); i++)
			{
				CreateEmptySlot();
			}
		}

		private List<Item> FilterItemsByCategory(List<Item> items, CategoryTab category)
		{
			if (category == CategoryTab.All) return items.ToList();

			return items.Where(item => category switch
			{
				CategoryTab.Consumables => item.ItemTypeValue == Item.ItemType.Consumable,
				CategoryTab.Materials => item.ItemTypeValue == Item.ItemType.Material,
				CategoryTab.KeyItems => item.ItemTypeValue == Item.ItemType.KeyItem,
				CategoryTab.QuestItems => item.ItemTypeValue == Item.ItemType.QuestItem,
				CategoryTab.Miscellaneous => item.ItemTypeValue == Item.ItemType.Miscellaneous,
				_ => true
			}).ToList();
		}

		private List<Item> SortItems(List<Item> items, SortType sortType)
		{
			return sortType switch
			{
				SortType.Type => items.OrderBy(i => i.ItemTypeValue).ThenBy(i => i.ItemName).ToList(),
				SortType.Name => items.OrderBy(i => i.ItemName).ToList(),
				SortType.Level => items.OrderByDescending(i => i.Price).ToList(),
				SortType.Price => items.OrderByDescending(i => i.Price).ToList(),
				SortType.TimeAcquired => items.ToList(),
				_ => items.ToList()
			};
		}

		private void CreateItemSlot(Item item)
		{
			var slot = new PanelContainer
			{
				CustomMinimumSize = new Vector2(64, 64),
				TooltipText = $"{item.ItemName} x{item.StackCount}"
			};

			var slotStyle = new StyleBoxFlat
			{
				BgColor = _colorSurface,
				CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
				BorderWidthLeft = 1, BorderWidthRight = 1,
				BorderWidthTop = 1, BorderWidthBottom = 1,
				BorderColor = new Color(_colorPrimary.R, _colorPrimary.G, _colorPrimary.B, 0.2f),
				ContentMarginLeft = 4, ContentMarginRight = 4,
				ContentMarginTop = 4, ContentMarginBottom = 4
			};
			slot.AddThemeStyleboxOverride("panel", slotStyle);

			var slotContent = new VBoxContainer
			{
				Alignment = BoxContainer.AlignmentMode.Center
			};

			var iconTexture = CreateItemIcon(item);
			var icon = new TextureRect
			{
				Texture = iconTexture,
				Size = new Vector2(36, 36),
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional
			};

			if (item.StackCount > 1)
			{
				var countLabel = new Label
				{
					Text = item.StackCount.ToString(),
					HorizontalAlignment = HorizontalAlignment.Right
				};
				countLabel.AddThemeFontSizeOverride("font_size", 10);
				countLabel.AddThemeColorOverride("font_color", _colorTextSecondary);
				slotContent.AddChild(countLabel);
			}

			slotContent.AddChild(icon);
			slot.AddChild(slotContent);

			slot.GuiInput += (InputEvent ev) => OnSlotGuiInput(ev, item, slot);
			slot.MouseEntered += () => OnSlotHover(slot, true);
			slot.MouseExited += () => OnSlotHover(slot, false);

			_itemGridContainer.AddChild(slot);
		}

		private Texture2D CreateItemIcon(Item item)
		{
			Color iconColor = item.ItemTypeValue switch
			{
				Item.ItemType.Consumable => new Color(1f, 0.4f, 0.4f, 1f),
				Item.ItemType.Material => new Color(0.8f, 0.8f, 0.4f, 1f),
				Item.ItemType.KeyItem => new Color(1f, 0.85f, 0f, 1f),
				Item.ItemType.QuestItem => new Color(0.6f, 0.4f, 1f, 1f),
				_ => new Color(0.7f, 0.7f, 0.7f, 1f)
			};

			var img = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
			img.Fill(new Color(0.15f, 0.22f, 0.28f, 1f));

			switch (item.ItemTypeValue)
			{
				case Item.ItemType.Consumable:
					DrawPotionIcon(img, iconColor);
					break;
				case Item.ItemType.Material:
					DrawMaterialIcon(img, iconColor);
					break;
				case Item.ItemType.KeyItem:
					DrawKeyIcon(img, iconColor);
					break;
				default:
					DrawDefaultIcon(img, iconColor);
					break;
			}

			return ImageTexture.CreateFromImage(img);
		}

		private static void DrawPotionIcon(Image img, Color color)
		{
			for (int x = 10; x < 22; x++) img.SetPixel(x, 24, color);
			for (int x = 11; x < 21; x++) img.SetPixel(x, 23, color);
			for (int x = 12; x < 20; x++) img.SetPixel(x, 22, color);
			for (int y = 14; y < 22; y++)
			{
				img.SetPixel(12, y, color);
				img.SetPixel(19, y, color);
			}
			img.SetPixel(14, 14, color);
			img.SetPixel(17, 14, color);
			img.SetPixel(15, 13, color);
			img.SetPixel(16, 13, color);
			for (int x = 14; x < 18; x++) img.SetPixel(x, 12, new Color(color.R, color.G, color.B, 0.7f));
		}

		private static void DrawMaterialIcon(Image img, Color color)
		{
			for (int x = 8; x < 24; x++)
			{
				for (int y = 8; y < 24; y++)
				{
					float dx = x - 16, dy = y - 16;
					float dist = (float)Math.Sqrt(dx * dx + dy * dy);
					if (dist < 7 && dist > 4) img.SetPixel(x, y, color);
				}
			}
		}

		private static void DrawKeyIcon(Image img, Color color)
		{
			for (int y = 12; y < 24; y++) img.SetPixel(20, y, color);
			for (int x = 14; x < 21; x++) img.SetPixel(x, 14, color);
			img.SetPixel(21, 13, color);
			img.SetPixel(21, 15, color);
			for (int x = 17; x < 20; x++) img.SetPixel(x, 12, color);
		}

		private static void DrawDefaultIcon(Image img, Color color)
		{
			for (int x = 10; x < 22; x++)
			{
				for (int y = 10; y < 22; y++)
				{
					img.SetPixel(x, y, color);
				}
			}
		}

		private void CreateEmptySlot()
		{
			var slot = new PanelContainer
			{
				CustomMinimumSize = new Vector2(64, 64)
			};

			var slotStyle = new StyleBoxFlat
			{
				BgColor = new Color(_colorSurface.R, _colorSurface.G, _colorSurface.B, 0.3f),
				CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
				BorderWidthLeft = 1, BorderWidthRight = 1,
				BorderWidthTop = 1, BorderWidthBottom = 1,
				BorderColor = new Color(1, 1, 1, 0.05f),
				ContentMarginLeft = 4, ContentMarginRight = 4,
				ContentMarginTop = 4, ContentMarginBottom = 4
			};
			slot.AddThemeStyleboxOverride("panel", slotStyle);

			_itemGridContainer.AddChild(slot);
		}

		private void OnSlotGuiInput(InputEvent @event, Item item, PanelContainer slot)
		{
			if (@event is InputEventMouseButton mouseButton)
			{
				if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
				{
					SelectItem(item);
				}

				if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.DoubleClick)
				{
					if (item.IsUsable)
					{
						UseSelectedItem();
					}
				}

				if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
				{
					StartDrag(item);
				}
			}
		}

		private void OnSlotHover(PanelContainer slot, bool hovering)
		{
			var style = (StyleBoxFlat)slot.GetThemeStylebox("panel");
			if (hovering)
			{
				style.BorderColor = new Color(_colorPrimary.R, _colorPrimary.G, _colorPrimary.B, 0.8f);
				style.BgColor = _colorPrimary * new Color(0.2f, 0.2f, 0.2f, 1f);
			}
			else
			{
				style.BorderColor = new Color(_colorPrimary.R, _colorPrimary.G, _colorPrimary.B, 0.2f);
				style.BgColor = _colorSurface;
			}
		}

		private void SelectItem(Item item)
		{
			_selectedItem = item;
			UpdateDetailPanel(item);
		}

		private void UpdateDetailPanel(Item item)
		{
			if (item == null)
			{
				_itemNameLabel.Text = "Select an item";
				_itemDescriptionLabel.Text = "";
				_useButton.Disabled = true;
				_equipButton.Disabled = true;
				_dropButton.Disabled = true;
				return;
			}

			_itemNameLabel.Text = item.ItemName;
			_itemDescriptionLabel.Text = FormatItemDescription(item);

			_useButton.Disabled = !item.IsUsable;
			_equipButton.Disabled = true;
			_dropButton.Disabled = !item.IsDroppable;
		}

		private string FormatItemDescription(Item item)
		{
			string text = $"[color=#{_colorPrimary.ToHtml()}][b]{item.ItemName}[/b][/color]\n\n";
			text += $"[color=#{_colorTextSecondary.ToHtml()}]{item.Description}\n\n[/color]";
			text += $"[b]Type:[/b] {item.GetTypeName()}\n";

			if (item.ItemTypeValue == Item.ItemType.Consumable)
			{
				text += $"[b]Subtype:[/b] {item.GetConsumableTypeName()}\n";
			}

			text += $"[b]Price:[/b] {item.Price} Gold\n";
			text += $"[b]Quantity:[/b] {item.StackCount}/{item.MaxStackCount}\n\n";

			if (item.EffectValue > 0)
			{
				text += $"[color=#{_colorSecondary.ToHtml()}][b]Effect:[/b] +{item.EffectValue:F0}";
				if (item.ConsumableTypeValue == Item.ConsumableType.HealthPotion) text += " HP";
				else if (item.ConsumableTypeValue == Item.ConsumableType.ManaPotion) text += " MP";
				text += "[/color]\n";
			}

			string statusText = "";
			if (item.IsUsable) statusText += $"[color=#{_colorSecondary.ToHtml()}]✓ Usable [/color]";
			if (item.IsSellable) statusText += $"[color=#{_colorPrimary.ToHtml()}]✓ Sellable [/color]";
			if (item.IsDroppable) statusText += $"[color=#{_colorDanger.ToHtml()}]✓ Droppable[/color]";
			if (!string.IsNullOrEmpty(statusText)) text += $"\n{statusText}";

			return text;
		}

		private void StartDrag(Item item)
		{
			if (!item.IsDroppable) return;

			_isDragging = true;
			_draggedItem = item;

			_dragIcon.Texture = CreateItemIcon(item);
			_dragCount.Text = item.StackCount > 1 ? $"x{item.StackCount}" : "";

			_dragPreview.Visible = true;
			_dragPreview.Position = GetGlobalMousePosition() + new Vector2(15, 15);
		}

		private void EndDrag()
		{
			_isDragging = false;
			_draggedItem = null;
			_dragPreview.Visible = false;
		}

		private void OnSortButtonPressed()
		{
			_currentSortType = (SortType)(((int)_currentSortType + 1) % Enum.GetValues(typeof(SortType)).Length);

			string[] sortNames = { "By Type", "By Name", "By Level", "By Price", "By Time" };
			_sortButton.Text = $"↕ {sortNames[(int)_currentSortType]}";

			RefreshItemGrid();
		}

		private void OnCategoryChanged(long tab)
		{
			_currentCategory = (CategoryTab)tab;
			RefreshItemGrid();
		}

		private void OnUseButtonPressed()
		{
			UseSelectedItem();
		}

		private void OnEquipButtonPressed()
		{
			GD.Print($"Equip functionality for: {_selectedItem?.ItemName}");
		}

		private void OnDropButtonPressed()
		{
			DropSelectedItem();
		}

		private void UseSelectedItem()
		{
			if (_selectedItem == null || PlayerData == null) return;

			bool success = _selectedItem.Use(PlayerData);
			if (success)
			{
				GD.Print($"Used: {_selectedItem.ItemName}");

				if (_selectedItem.StackCount <= 0)
				{
					RemoveItemFromInventory(_selectedItem.Id);
				}

				LoadInventory(PlayerData);
				UpdateDetailPanel(_selectedItem.StackCount > 0 ? _selectedItem : null);
			}
		}

		private void DropSelectedItem()
		{
			if (_selectedItem == null || !_selectedItem.IsDroppable) return;

			var dialog = new ConfirmDropDialog(_selectedItem);
			AddChild(dialog);
			dialog.PopupCentered();
			dialog.Confirmed += () =>
			{
				RemoveItemFromInventory(_selectedItem.Id);
				LoadInventory(PlayerData);
				_selectedItem = null;
				UpdateDetailPanel(null);
			};
		}

		private void RemoveItemFromInventory(string itemId)
		{
			if (PlayerData?.Inventory.ContainsKey(itemId) == true)
			{
				PlayerData.Inventory[itemId]--;
				if (PlayerData.Inventory[itemId] <= 0)
				{
					PlayerData.Inventory.Remove(itemId);
				}
			}
		}

		private void OnGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButton)
			{
				if (mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed && _isDragging)
				{
					EndDrag();
				}

				if (mouseButton.ButtonIndex == MouseButton.Right && !mouseButton.Pressed && _isDragging)
				{
					EndDrag();
				}
			}

			if (@event is InputEventKey key && key.Pressed)
			{
				if (key.Keycode == Key.Escape)
				{
					if (_isDragging)
					{
						EndDrag();
					}
					else if (_isVisible)
					{
						ToggleVisibility();
					}
				}
			}
		}

		public void ToggleVisibility()
		{
			_isVisible = !_isVisible;

			if (_isVisible)
			{
				Visible = true;
				if (PlayerData != null)
				{
					LoadInventory(PlayerData);
				}

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

		private void UpdateCapacityDisplay()
		{
			float percentage = (float)_usedSlots / _currentCapacity * 100f;
			Color capacityColor = percentage switch
			{
				< 50 => _colorSecondary,
				< 80 => _colorWarning,
				_ => _colorDanger
			};

			_capacityLabel.Text = $"Slots: {_usedSlots}/{_currentCapacity}";
			_capacityLabel.AddThemeColorOverride("font_color", capacityColor);
		}

		public bool AddItemToInventory(Item item, int quantity = 1)
		{
			if (_usedSlots >= _currentCapacity && !PlayerData.Inventory.ContainsKey(item.Id))
			{
				GD.Print("Inventory is full!");
				return false;
			}

			if (PlayerData.Inventory.ContainsKey(item.Id))
			{
				int currentAmount = PlayerData.Inventory[item.Id];
				int maxAdd = item.MaxStackCount - currentAmount;
				int toAdd = Mathf.Min(quantity, maxAdd);

				PlayerData.Inventory[item.Id] += toAdd;
				return toAdd == quantity;
			}
			else
			{
				PlayerData.Inventory[item.Id] = quantity;
				return true;
			}
		}

		public bool ExpandCapacity(int slots)
		{
			if (_currentCapacity + slots > MaxCapacity)
			{
				GD.Print($"Cannot expand beyond maximum capacity of {MaxCapacity}");
				return false;
			}

			_currentCapacity += slots;
			GD.Print($"Inventory expanded by {slots} slots. New capacity: {_currentCapacity}");

			RefreshItemGrid();
			UpdateCapacityDisplay();
			return true;
		}

		public int GetCurrentCapacity() => _currentCapacity;
		public int GetUsedSlots() => _usedSlots;
		public float GetCapacityPercentage() => (float)_usedSlots / _currentCapacity * 100f;

		private partial class ConfirmDropDialog : ConfirmationDialog
		{
			public ConfirmDropDialog(Item item)
			{
				Title = "Drop Item";
				DialogText = $"Are you sure you want to drop [b]{item.ItemName}[/b]?";
			}
		}
	}
}
