/*
 * File: PopupMapPanel.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 地图面板，显示所有可用的地图列表和当前地图的详细信息。
 * 支持地图之间的传送功能。
 *
 * Key Features:
 * - 地图列表（左侧）与详情（右侧）分栏布局
 * - 当前地图高亮标识
 * - 地图类型、尺寸、出生点、连接地图显示
 * - 传送功能（通过MapManager.ChangeMap）
 */

using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
    /// <summary>
    /// 地图面板
    /// </summary>
    public partial class PopupMapPanel : PopupPanelBase
    {
        private VBoxContainer _mapListContainer;
        private Label _mapNameLabel;
        private Label _mapTypeLabel;
        private Label _mapSizeLabel;
        private Label _mapConnectedLabel;
        private Label _mapSpawnLabel;
        private Button _teleportButton;
        private MapData _selectedMapData;
        private string _currentMapId;

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
            Name = "MapPanel";
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

            // Left: map list
            var leftVbox = new VBoxContainer { CustomMinimumSize = new Vector2(280, 0) };
            hbox.AddChild(leftVbox);

            leftVbox.AddChild(CreateTitleLabel("Maps"));
            leftVbox.AddChild(CreateSeparator());

            var scroll = new ScrollContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            leftVbox.AddChild(scroll);

            _mapListContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(_mapListContainer);

            // Right: map detail
            var rightPanel = CreateSurfacePanel();
            rightPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(rightPanel);

            var rightVbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.05f, AnchorTop = 0.05f,
                AnchorRight = 0.95f, AnchorBottom = 0.95f
            };
            rightPanel.AddChild(rightVbox);

            _mapNameLabel = CreateTitleLabel("Select a map");
            rightVbox.AddChild(_mapNameLabel);
            rightVbox.AddChild(CreateSeparator());

            _mapTypeLabel = CreateInfoLabel("");
            rightVbox.AddChild(_mapTypeLabel);
            _mapSizeLabel = CreateInfoLabel("");
            rightVbox.AddChild(_mapSizeLabel);
            _mapSpawnLabel = CreateInfoLabel("");
            rightVbox.AddChild(_mapSpawnLabel);

            rightVbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });
            rightVbox.AddChild(CreateSeparator());

            var connTitle = CreateTitleLabel("Connected Maps");
            connTitle.AddThemeFontSizeOverride("font_size", 16);
            rightVbox.AddChild(connTitle);
            _mapConnectedLabel = CreateInfoLabel("");
            rightVbox.AddChild(_mapConnectedLabel);

            rightVbox.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

            _teleportButton = CreatePrimaryButton("Teleport");
            _teleportButton.Disabled = true;
            _teleportButton.Pressed += OnTeleport;
            rightVbox.AddChild(_teleportButton);

            rightVbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

            var backBtn = CreateSecondaryButton("Back");
            backBtn.Pressed += () => OnBackPressed?.Invoke();
            rightVbox.AddChild(backBtn);
        }

        public void Refresh()
        {
            ClearChildren(_mapListContainer);
            _selectedMapData = null;
            _teleportButton.Disabled = true;

            var maps = MapManager.Instance?.GetAllMaps() ?? new List<MapData>();
            var currentMap = MapManager.Instance?.GetCurrentMap();
            _currentMapId = currentMap?.Id;

            if (maps.Count == 0)
            {
                _mapListContainer.AddChild(CreateInfoLabel("No maps available."));
                return;
            }

            foreach (var map in maps)
            {
                var btn = new Button
                {
                    Text = map.Name ?? map.Id,
                    TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
                };
                ApplyButtonStyle(btn);
                if (map.Id == _currentMapId)
                {
                    btn.AddThemeColorOverride("font_color", ColorWarning);
                    btn.Text = $"★ {btn.Text}";
                }
                var capturedMap = map;
                btn.Pressed += () => SelectMap(capturedMap);
                _mapListContainer.AddChild(btn);
            }
        }

        private void SelectMap(MapData map)
        {
            _selectedMapData = map;

            _mapNameLabel.Text = map.Name ?? map.Id;
            _mapTypeLabel.Text = $"Type: {map.Type}";
            _mapSizeLabel.Text = $"Size: {map.Size.X:F0} x {map.Size.Y:F0}";
            _mapSpawnLabel.Text = $"Spawn: ({map.SpawnPosition.X:F0}, {map.SpawnPosition.Y:F0}, {map.SpawnPosition.Z:F0})";

            var connected = map.ConnectedMaps?.Count > 0
                ? string.Join(", ", map.ConnectedMaps)
                : "None";
            _mapConnectedLabel.Text = connected;

            _teleportButton.Disabled = map.Id == _currentMapId ||
                (map.ConnectedMaps == null || map.ConnectedMaps.Count == 0);
        }

        private void OnTeleport()
        {
            if (_selectedMapData == null) return;
            MapManager.Instance?.ChangeMap(_selectedMapData.Id);
            Log.Info($"Teleported to map: {_selectedMapData.Id}");
            _selectedMapData = null;
            Refresh();
        }
    }
}
