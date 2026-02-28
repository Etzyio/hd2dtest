using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
    /// <summary>
    /// 存档槽位数据模型
    /// </summary>
    public class SaveSlotData
    {
        /// <summary>
        /// 存档槽位ID
        /// </summary>
        public string SlotId { get; set; }

        /// <summary>
        /// 存档名称
        /// </summary>
        public string SaveName { get; set; }

        /// <summary>
        /// 存档时间
        /// </summary>
        public DateTime SaveTime { get; set; }

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int PlayerLevel { get; set; }

        /// <summary>
        /// 游戏时间（秒）
        /// </summary>
        public float GameTime { get; set; }

        /// <summary>
        /// 是否为空槽位
        /// </summary>
        public bool IsEmpty { get; set; } = true;

        /// <summary>
        /// 存档文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 获取格式化的时间字符串
        /// </summary>
        public string GetFormattedTime()
        {
            if (IsEmpty) return TranslationServer.Translate("save_slot_empty");
            
            var timeSpan = DateTime.Now - SaveTime;
            if (timeSpan.TotalDays >= 1)
                return string.Format(TranslationServer.Translate("time_days_ago"), Math.Floor(timeSpan.TotalDays));
            else if (timeSpan.TotalHours >= 1)
                return string.Format(TranslationServer.Translate("time_hours_ago"), Math.Floor(timeSpan.TotalHours));
            else if (timeSpan.TotalMinutes >= 1)
                return string.Format(TranslationServer.Translate("time_minutes_ago"), Math.Floor(timeSpan.TotalMinutes));
            else
                return TranslationServer.Translate("time_just_now");
        }

        /// <summary>
        /// 获取游戏时间字符串
        /// </summary>
        public string GetGameTimeString()
        {
            if (IsEmpty) return "";
            
            var time = TimeSpan.FromSeconds(GameTime);
            if (time.TotalHours >= 1)
                return $"{Math.Floor(time.TotalHours)}h {time.Minutes}m";
            else
                return $"{time.Minutes}m {time.Seconds}s";
        }
    }

    /// <summary>
    /// 存档选择UI组件
    /// </summary>
    public partial class SaveSlotSelector : Control
    {
        private VBoxContainer _slotsContainer;
        private List<SaveSlotData> _saveSlots = new();
        private string _selectedSlotId;
        private const int MAX_SLOTS = 10;
        private Button _deleteButton;
        private Button _confirmButton;

        /// <summary>
        /// 存档选择事件
        /// </summary>
        [Signal] public delegate void SaveSlotSelectedEventHandler(string slotId);
        [Signal] public delegate void SaveSlotDeletedEventHandler(string slotId);

        /// <summary>
        /// 初始化存档选择器
        /// </summary>
        public override void _Ready()
        {
            CreateUI();
            RefreshSaveSlots();
        }

        /// <summary>
        /// 创建UI界面
        /// </summary>
        private void CreateUI()
        {
            // 创建主容器
            var mainContainer = new VBoxContainer
            {
                Name = "SaveSlotsContainer",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            AddChild(mainContainer);

            // 创建标题
            var titleLabel = new Label
            {
                Text = TranslationServer.Translate("save_slot_title"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainContainer.AddChild(titleLabel);

            // 创建槽位容器
            _slotsContainer = new VBoxContainer
            {
                Name = "SlotsContainer",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            mainContainer.AddChild(_slotsContainer);

            // 创建按钮容器
            var buttonContainer = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            mainContainer.AddChild(buttonContainer);

            // 创建刷新按钮
            var refreshButton = new Button
            {
                Text = TranslationServer.Translate("ui_refresh"),
                CustomMinimumSize = new Vector2(100, 40)
            };
            refreshButton.Pressed += () => RefreshSaveSlots();
            buttonContainer.AddChild(refreshButton);

            // 创建删除按钮
            _deleteButton = new Button
            {
                Text = TranslationServer.Translate("ui_delete_selected"),
                CustomMinimumSize = new Vector2(150, 40),
                Disabled = true
            };
            _deleteButton.Pressed += () => DeleteSelectedSlot();
            buttonContainer.AddChild(_deleteButton);

            // 创建确认按钮
            _confirmButton = new Button
            {
                Text = TranslationServer.Translate("ui_confirm_selection"),
                CustomMinimumSize = new Vector2(120, 40),
                Disabled = true
            };
            _confirmButton.Pressed += () => ConfirmSelection();
            buttonContainer.AddChild(_confirmButton);
        }

        /// <summary>
        /// 刷新存档槽位列表
        /// </summary>
        public void RefreshSaveSlots()
        {
            // 清空现有槽位
            foreach (Node child in _slotsContainer.GetChildren())
            {
                child.QueueFree();
            }
            _saveSlots.Clear();

            // 扫描存档文件
            ScanSaveFiles();

            // 创建槽位UI
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                var slotId = $"save_slot_{i + 1}";
                var slotData = _saveSlots.FirstOrDefault(s => s.SlotId == slotId);
                
                if (slotData == null)
                {
                    slotData = new SaveSlotData
                    {
                        SlotId = slotId,
                        SaveName = string.Format(TranslationServer.Translate("save_slot_default_name"), i + 1),
                        IsEmpty = true
                    };
                }

                CreateSlotUI(slotData);
            }
        }

        /// <summary>
        /// 扫描存档文件
        /// </summary>
        private void ScanSaveFiles()
        {
            try
            {
                var saveDir = "user://saves";
                if (!DirAccess.DirExistsAbsolute(saveDir))
                {
                    return;
                }

                using var dir = DirAccess.Open(saveDir);
                if (dir == null) return;

                dir.ListDirBegin();
                string fileName = dir.GetNext();
                
                while (fileName != "")
                {
                    if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
                    {
                        var filePath = $"{saveDir}/{fileName}";
                        var slotId = fileName.Replace(".json", "");
                        
                        try
                        {
                            var saveData = SaveManager.Instance.LoadGame(slotId);
                            if (saveData != null && saveData.Players.Count > 0)
                            {
                                var playerData = saveData.Players[0];
                                var slotData = new SaveSlotData
                                {
                                    SlotId = slotId,
                                    SaveName = string.Format(TranslationServer.Translate("save_slot_default_name"), slotId),
                                    SaveTime = DateTime.Now, // 这里应该从文件获取实际时间
                                    PlayerName = playerData.PlayerName ?? TranslationServer.Translate("player_unknown"),
                                    PlayerLevel = playerData.Level,
                                    GameTime = 0, // 这里应该从存档数据获取
                                    IsEmpty = false,
                                    FilePath = filePath
                                };
                                _saveSlots.Add(slotData);
                            }
                        }
                        catch (Exception ex)
                        {
                            // 使用Godot的日志系统
                            GD.PrintErr($"Failed to load save file {filePath}: {ex.Message}");
                        }
                    }
                    fileName = dir.GetNext();
                }
            }
            catch (Exception ex)
            {
                // 使用Godot的日志系统
                GD.PrintErr($"Failed to scan save files: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建槽位UI
        /// </summary>
        private void CreateSlotUI(SaveSlotData slotData)
        {
            var slotPanel = new PanelContainer
            {
                Name = $"Slot_{slotData.SlotId}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 80)
            };

            // 添加样式
            slotPanel.AddThemeStyleboxOverride("panel", CreateSlotStylebox(slotData.IsEmpty));

            var slotContent = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            slotPanel.AddChild(slotContent);

            // 左侧信息
            var infoContainer = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsStretchRatio = 3
            };
            slotContent.AddChild(infoContainer);

            // 存档名称
            var nameLabel = new Label
            {
                Text = slotData.SaveName
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.AddThemeColorOverride("font_color", slotData.IsEmpty ? new Color(0.7f, 0.7f, 0.7f) : new Color(1, 1, 1));
            infoContainer.AddChild(nameLabel);

            // 存档信息
            if (!slotData.IsEmpty)
            {
                var detailsLabel = new Label
                {
                    Text = string.Format(TranslationServer.Translate("save_slot_details_format"), slotData.PlayerName, slotData.PlayerLevel, slotData.GetFormattedTime())
                };
                detailsLabel.AddThemeFontSizeOverride("font_size", 12);
                detailsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
                infoContainer.AddChild(detailsLabel);

                var gameTimeLabel = new Label
                {
                    Text = string.Format(TranslationServer.Translate("save_slot_game_time"), slotData.GetGameTimeString())
                };
                gameTimeLabel.AddThemeFontSizeOverride("font_size", 10);
                gameTimeLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
                infoContainer.AddChild(gameTimeLabel);
            }
            else
            {
                var emptyLabel = new Label
                {
                    Text = TranslationServer.Translate("save_slot_empty")
                };
                emptyLabel.AddThemeFontSizeOverride("font_size", 12);
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                infoContainer.AddChild(emptyLabel);
            }

            // 右侧选择按钮
            var selectButton = new Button
            {
                Text = slotData.IsEmpty ? TranslationServer.Translate("save_slot_new") : TranslationServer.Translate("save_slot_select"),
                CustomMinimumSize = new Vector2(100, 60),
                SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
            };
            selectButton.Pressed += () => SelectSlot(slotData.SlotId);
            slotContent.AddChild(selectButton);

            _slotsContainer.AddChild(slotPanel);
        }

        /// <summary>
        /// 创建槽位样式框
        /// </summary>
        private StyleBoxFlat CreateSlotStylebox(bool isEmpty)
        {
            var stylebox = new StyleBoxFlat
            {
                BgColor = isEmpty ? new Color(0.2f, 0.2f, 0.2f, 0.8f) : new Color(0.15f, 0.15f, 0.2f, 0.9f),
                BorderColor = new Color(0.3f, 0.3f, 0.3f),
                BorderWidthLeft = 2,
                BorderWidthTop = 2,
                BorderWidthRight = 2,
                BorderWidthBottom = 2,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomRight = 8,
                CornerRadiusBottomLeft = 8,
                ContentMarginLeft = 10,
                ContentMarginTop = 5,
                ContentMarginRight = 10,
                ContentMarginBottom = 5
            };
            return stylebox;
        }

        /// <summary>
        /// 选择槽位
        /// </summary>
        private void SelectSlot(string slotId)
        {
            _selectedSlotId = slotId;
            
            // 更新UI状态
            UpdateButtonStates();
            
            // 高亮选中的槽位
            HighlightSelectedSlot();
            
            // 使用Godot的日志系统
            GD.Print($"Selected save slot: {slotId}");
        }

        /// <summary>
        /// 高亮选中的槽位
        /// </summary>
        private void HighlightSelectedSlot()
        {
            foreach (PanelContainer slotPanel in _slotsContainer.GetChildren())
            {
                var isSelected = slotPanel.Name == $"Slot_{_selectedSlotId}";
                var stylebox = slotPanel.GetThemeStylebox("panel") as StyleBoxFlat;
                if (stylebox != null)
                {
                    stylebox.BorderColor = isSelected ? new Color(0.8f, 0.6f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
                    stylebox.BorderWidthLeft = isSelected ? 3 : 2;
                    stylebox.BorderWidthTop = isSelected ? 3 : 2;
                    stylebox.BorderWidthRight = isSelected ? 3 : 2;
                    stylebox.BorderWidthBottom = isSelected ? 3 : 2;
                }
            }
        }

        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            var hasSelection = !string.IsNullOrEmpty(_selectedSlotId);
            var selectedSlot = _saveSlots.FirstOrDefault(s => s.SlotId == _selectedSlotId);
            var isEmpty = selectedSlot?.IsEmpty ?? true;

            _deleteButton.Disabled = !hasSelection || isEmpty;
            _confirmButton.Disabled = !hasSelection;
        }

        /// <summary>
        /// 删除选中的存档
        /// </summary>
        private void DeleteSelectedSlot()
        {
            if (string.IsNullOrEmpty(_selectedSlotId)) return;

            var slotData = _saveSlots.FirstOrDefault(s => s.SlotId == _selectedSlotId);
            if (slotData == null || slotData.IsEmpty) return;

            // 这里应该显示确认对话框
            try
            {
                // 使用SaveManager删除存档
                var saveManager = SaveManager.Instance;
                if (saveManager != null)
                {
                    // 这里需要调用SaveManager的删除方法
                    // 暂时使用文件系统删除
                    var filePath = $"user://saves/{_selectedSlotId}.json";
                    if (FileAccess.FileExists(filePath))
                    {
                        // 注意：Godot中删除文件需要使用DirAccess
                        using var dir = DirAccess.Open("user://saves");
                        if (dir != null)
                        {
                            dir.Remove($"{_selectedSlotId}.json");
                        }
                    }
                }
                
                // 使用Godot的日志系统
                GD.Print($"Deleted save slot: {_selectedSlotId}");
                
                // 刷新列表
                RefreshSaveSlots();
                
                // 清除选择
                _selectedSlotId = null;
                UpdateButtonStates();
                
                EmitSignal(SignalName.SaveSlotDeleted, _selectedSlotId);
            }
            catch (Exception ex)
            {
                // 使用Godot的日志系统
                GD.PrintErr($"Failed to delete save slot {_selectedSlotId}: {ex.Message}");
            }
        }

        /// <summary>
        /// 确认选择
        /// </summary>
        private void ConfirmSelection()
        {
            if (string.IsNullOrEmpty(_selectedSlotId)) return;

            EmitSignal(SignalName.SaveSlotSelected, _selectedSlotId);
            
            // 隐藏选择器
            Visible = false;
        }

        /// <summary>
        /// 获取选中的槽位ID
        /// </summary>
        public string GetSelectedSlotId()
        {
            return _selectedSlotId;
        }

        /// <summary>
        /// 显示选择器
        /// </summary>
        public void ShowSelector()
        {
            Visible = true;
            RefreshSaveSlots();
        }

        /// <summary>
        /// 隐藏选择器
        /// </summary>
        public void HideSelector()
        {
            Visible = false;
        }
    }
}