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
    /// 存档选择UI组件，显示所有存档槽位供用户选择或删除
    /// </summary>
    public partial class SaveSlotSelector : Control
    {
        private VBoxContainer _slotsContainer;
        private List<SaveInfo> _saveInfos = new();
        private string _selectedSlotId;
        private const int MAX_SLOTS = 10;
        private Button _deleteButton;
        private Button _confirmButton;

        /// <summary>
        /// 存档选择信号
        /// </summary>
        [Signal] public delegate void SaveSlotSelectedEventHandler(string slotId);
        [Signal] public delegate void SaveSlotDeletedEventHandler(string slotId);

        public override void _Ready()
        {
            CreateUI();
            RefreshSaveSlots();
        }

        private void CreateUI()
        {
            var mainContainer = new VBoxContainer
            {
                Name = "SaveSlotsContainer",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            AddChild(mainContainer);

            var titleLabel = new Label
            {
                Text = "Save Slots",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainContainer.AddChild(titleLabel);

            _slotsContainer = new VBoxContainer
            {
                Name = "SlotsContainer",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            mainContainer.AddChild(_slotsContainer);

            var buttonContainer = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            mainContainer.AddChild(buttonContainer);

            var refreshButton = new Button
            {
                Text = "Refresh",
                CustomMinimumSize = new Vector2(100, 40)
            };
            refreshButton.Pressed += () => RefreshSaveSlots();
            buttonContainer.AddChild(refreshButton);

            _deleteButton = new Button
            {
                Text = "Delete Selected",
                CustomMinimumSize = new Vector2(150, 40),
                Disabled = true
            };
            _deleteButton.Pressed += () => DeleteSelectedSlot();
            buttonContainer.AddChild(_deleteButton);

            _confirmButton = new Button
            {
                Text = "Confirm",
                CustomMinimumSize = new Vector2(120, 40),
                Disabled = true
            };
            _confirmButton.Pressed += () => ConfirmSelection();
            buttonContainer.AddChild(_confirmButton);
        }

        /// <summary>
        /// 刷新存档槽位列表，从SaveManager获取所有存档信息
        /// </summary>
        public void RefreshSaveSlots()
        {
            foreach (Node child in _slotsContainer.GetChildren())
                child.QueueFree();

            _saveInfos = SaveManager.Instance?.GetAllSaveInfos() ?? new List<SaveInfo>();

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                var slotId = (i + 1).ToString();
                var info = _saveInfos.FirstOrDefault(s => s.SaveId == slotId);
                CreateSlotUI(slotId, info);
            }
        }

        private void CreateSlotUI(string slotId, SaveInfo info)
        {
            bool isEmpty = info == null;

            var slotPanel = new PanelContainer
            {
                Name = $"Slot_{slotId}",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 80)
            };
            slotPanel.AddThemeStyleboxOverride("panel", CreateSlotStylebox(isEmpty));

            var slotContent = new HBoxContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            slotPanel.AddChild(slotContent);

            var infoContainer = new VBoxContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsStretchRatio = 3
            };
            slotContent.AddChild(infoContainer);

            var nameText = isEmpty
                ? $"Slot {slotId}  —  Empty"
                : $"Slot {slotId}  —  {info.SaveName ?? "Untitled"}";
            var nameLabel = new Label { Text = nameText };
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.AddThemeColorOverride("font_color", isEmpty
                ? new Color(0.7f, 0.7f, 0.7f) : new Color(1, 1, 1));
            infoContainer.AddChild(nameLabel);

            if (!isEmpty)
            {
                var detailText = $"Lv.{info.AveragePlayerLevel}  |  {info.CurrentScene ?? "Unknown"}  |  "
                    + $"{TimeSpan.FromSeconds(info.PlayTime).TotalHours:F1}h  |  {info.SaveTime:yyyy-MM-dd HH:mm}";
                var detailLabel = new Label { Text = detailText };
                detailLabel.AddThemeFontSizeOverride("font_size", 12);
                detailLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
                infoContainer.AddChild(detailLabel);
            }
            else
            {
                var emptyLabel = new Label { Text = "Empty" };
                emptyLabel.AddThemeFontSizeOverride("font_size", 12);
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                infoContainer.AddChild(emptyLabel);
            }

            var selectButton = new Button
            {
                Text = isEmpty ? "Save Here" : "Select",
                CustomMinimumSize = new Vector2(100, 60),
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };
            selectButton.Pressed += () => SelectSlot(slotId);
            slotContent.AddChild(selectButton);

            _slotsContainer.AddChild(slotPanel);
        }

        private static StyleBoxFlat CreateSlotStylebox(bool isEmpty)
        {
            return new StyleBoxFlat
            {
                BgColor = isEmpty ? new Color(0.2f, 0.2f, 0.2f, 0.8f) : new Color(0.15f, 0.15f, 0.2f, 0.9f),
                BorderColor = new Color(0.3f, 0.3f, 0.3f),
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
                CornerRadiusBottomRight = 8, CornerRadiusBottomLeft = 8,
                ContentMarginLeft = 10, ContentMarginTop = 5,
                ContentMarginRight = 10, ContentMarginBottom = 5
            };
        }

        private void SelectSlot(string slotId)
        {
            _selectedSlotId = slotId;
            UpdateButtonStates();
            HighlightSelectedSlot();
            Log.Info($"Selected save slot: {slotId}");
        }

        private void HighlightSelectedSlot()
        {
            foreach (PanelContainer slotPanel in _slotsContainer.GetChildren().Cast<PanelContainer>())
            {
                bool isSelected = slotPanel.Name == $"Slot_{_selectedSlotId}";
                if (slotPanel.GetThemeStylebox("panel") is StyleBoxFlat stylebox)
                {
                    stylebox.BorderColor = isSelected
                        ? new Color(0.8f, 0.6f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
                    stylebox.BorderWidthLeft = isSelected ? 3 : 2;
                    stylebox.BorderWidthTop = isSelected ? 3 : 2;
                    stylebox.BorderWidthRight = isSelected ? 3 : 2;
                    stylebox.BorderWidthBottom = isSelected ? 3 : 2;
                }
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = !string.IsNullOrEmpty(_selectedSlotId);
            var selectedInfo = _saveInfos.FirstOrDefault(s => s.SaveId == _selectedSlotId);
            bool isEmpty = selectedInfo == null;

            _deleteButton.Disabled = !hasSelection || isEmpty;
            _confirmButton.Disabled = !hasSelection;
        }

        private void DeleteSelectedSlot()
        {
            if (string.IsNullOrEmpty(_selectedSlotId)) return;

            try
            {
                var filePath = ProjectSettings.GlobalizePath(
                    $"{SaveManager.Instance.SaveDirectory}/save_{_selectedSlotId}.json");

                if (Godot.FileAccess.FileExists(filePath))
                {
                    using var dir = DirAccess.Open(ProjectSettings.GlobalizePath(SaveManager.Instance.SaveDirectory));
                    if (dir != null)
                        dir.Remove($"save_{_selectedSlotId}.json");
                }

                Log.Info($"Deleted save slot: {_selectedSlotId}");
                EmitSignal(SignalName.SaveSlotDeleted, _selectedSlotId);

                _selectedSlotId = null;
                UpdateButtonStates();
                RefreshSaveSlots();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to delete save slot {_selectedSlotId}: {ex.Message}");
            }
        }

        private void ConfirmSelection()
        {
            if (string.IsNullOrEmpty(_selectedSlotId)) return;
            EmitSignal(SignalName.SaveSlotSelected, _selectedSlotId);
            Visible = false;
        }

        public string GetSelectedSlotId() => _selectedSlotId;

        public void ShowSelector()
        {
            Visible = true;
            RefreshSaveSlots();
        }

        public void HideSelector() => Visible = false;
    }
}
