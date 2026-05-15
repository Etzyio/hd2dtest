/*
 * File: PopupMapPanel.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 地图面板，展示剧情地图的2D节点图。
 * 地图按区域分组显示，节点之间绘制连接线，支持点击查看详情。
 *
 * Key Features:
 * - 2D 画布展示剧情地图坐标节点
 * - 节点间连接线（反映剧情流向）
 * - 按区域颜色区分（江南/西域/京城）
 * - 点击节点查看详情
 * - 当前所在地高亮
 * - 区域筛选切换
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Popup
{
    public partial class PopupMapPanel : PopupPanelBase
    {
        // Map canvas dimensions
        private const int CanvasWidth = 600;
        private const int CanvasHeight = 600;
        private const int NodeRadius = 18;
        private const float MapScale = 0.85f;

        private static readonly Color ColorJiangnan = new(0.2f, 0.7f, 0.4f, 1f);
        private static readonly Color ColorWestern = new(0.9f, 0.6f, 0.15f, 1f);
        private static readonly Color ColorCapital = new(0.8f, 0.2f, 0.2f, 1f);
        private static readonly Color ColorCurrent = new(1f, 0.9f, 0.2f, 1f);
        private static readonly Color ColorLine = new(0.4f, 0.5f, 0.6f, 0.4f);
        private static readonly Color ColorLineCurrent = new(0.6f, 0.8f, 1f, 0.6f);
        private static readonly Color ColorNodeBg = new(0.1f, 0.12f, 0.16f, 0.95f);
        private static readonly Color ColorNodeHover = new(0.2f, 0.3f, 0.45f, 0.9f);

        private MapCanvas _canvasDraw;
        private Control _mapContainer;
        private Control _detailPanel;
        private Label _detailName;
        private Label _detailRegion;
        private Label _detailClimate;
        private Label _detailDescription;
        private Label _detailNpcs;
        private Label _detailConnections;
        private Control _detailContent;
        private HBoxContainer _regionFilter;
        private Label _titleLabel;

        private MapData _selectedMap;
        private string _currentMapId;
        private List<MapData> _allMaps = new();
        private string _activeRegionFilter = ""; // empty = show all
        private List<(Button Btn, MapData Map)> _mapButtons = new();

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
            AnchorLeft = 0.02f; AnchorTop = 0.02f;
            AnchorRight = 0.98f; AnchorBottom = 0.98f;
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
                AnchorLeft = 0.01f, AnchorTop = 0.01f,
                AnchorRight = 0.99f, AnchorBottom = 0.99f
            };
            AddChild(hbox);

            // Left: map canvas
            var leftPanel = CreateSurfacePanel();
            leftPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            leftPanel.CustomMinimumSize = new Vector2(500, 0);
            hbox.AddChild(leftPanel);

            var leftVbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.02f, AnchorTop = 0.02f,
                AnchorRight = 0.98f, AnchorBottom = 0.98f
            };
            leftPanel.AddChild(leftVbox);

            _titleLabel = CreateTitleLabel("江湖地图");
            leftVbox.AddChild(_titleLabel);

            // Region filter
            _regionFilter = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            leftVbox.AddChild(_regionFilter);

            var scroll = new ScrollContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
                VerticalScrollMode = ScrollContainer.ScrollMode.Auto
            };
            leftVbox.AddChild(scroll);

            _canvasDraw = new MapCanvas();
            _canvasDraw.CustomMinimumSize = new Vector2(CanvasWidth, CanvasHeight);
            scroll.AddChild(_canvasDraw);

            _mapContainer = new Control
            {
                MouseFilter = MouseFilterEnum.Ignore,
                CustomMinimumSize = new Vector2(CanvasWidth, CanvasHeight)
            };
            _canvasDraw.AddChild(_mapContainer);

            // Right: detail panel
            _detailPanel = CreateSurfacePanel();
            _detailPanel.CustomMinimumSize = new Vector2(280, 0);
            hbox.AddChild(_detailPanel);

            _detailContent = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorLeft = 0.05f, AnchorTop = 0.02f,
                AnchorRight = 0.95f, AnchorBottom = 0.95f
            };
            _detailPanel.AddChild(_detailContent);

            _detailName = CreateTitleLabel("选择地点");
            _detailContent.AddChild(_detailName);
            _detailContent.AddChild(CreateSeparator());

            _detailRegion = CreateInfoLabel("");
            _detailContent.AddChild(_detailRegion);
            _detailClimate = CreateInfoLabel("");
            _detailContent.AddChild(_detailClimate);
            _detailContent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });

            var descTitle = CreateSectionLabel("描述");
            _detailContent.AddChild(descTitle);
            _detailDescription = CreateInfoLabel("");
            _detailDescription.AutowrapMode = TextServer.AutowrapMode.Word;
            _detailDescription.CustomMinimumSize = new Vector2(0, 60);
            _detailContent.AddChild(_detailDescription);

            _detailContent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });
            var npcTitle = CreateSectionLabel("相关人物");
            _detailContent.AddChild(npcTitle);
            _detailNpcs = CreateInfoLabel("");
            _detailContent.AddChild(_detailNpcs);

            _detailContent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });
            var connTitle2 = CreateSectionLabel("连接地点");
            _detailContent.AddChild(connTitle2);
            _detailConnections = CreateInfoLabel("");
            _detailContent.AddChild(_detailConnections);

            _detailContent.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

            var backBtn = CreateSecondaryButton("返回");
            backBtn.Pressed += () => OnBackPressed?.Invoke();
            _detailContent.AddChild(backBtn);
        }

        public void Refresh()
        {
            var maps = MapManager.Instance?.GetAllMaps() ?? new List<MapData>();
            var currentMap = MapManager.Instance?.GetCurrentMap();
            _currentMapId = currentMap?.Id ?? "";
            _allMaps = maps;

            // Build region filter
            BuildRegionFilter();

            // Apply filter
            ApplyFilterAndRebuild();
        }

        private void BuildRegionFilter()
        {
            ClearChildren(_regionFilter);

            var allBtn = CreateSmallButton("全部");
            ApplyFilterButtonStyle(allBtn, _activeRegionFilter == "");
            allBtn.Pressed += () => { _activeRegionFilter = ""; ApplyFilterAndRebuild(); };
            _regionFilter.AddChild(allBtn);

            var regions = _allMaps.Select(m => m.Region).Distinct().OrderBy(r => r).ToList();
            foreach (var region in regions)
            {
                var btn = CreateSmallButton(region);
                ApplyFilterButtonStyle(btn, _activeRegionFilter == region);
                var captured = region;
                btn.Pressed += () => { _activeRegionFilter = captured; ApplyFilterAndRebuild(); };
                _regionFilter.AddChild(btn);
            }
        }

        private void ApplyFilterAndRebuild()
        {
            ClearChildren(_mapContainer);
            _mapButtons.Clear();
            _selectedMap = null;

            var filtered = string.IsNullOrEmpty(_activeRegionFilter)
                ? _allMaps
                : _allMaps.Where(m => m.Region == _activeRegionFilter).ToList();

            if (filtered.Count == 0)
            {
                var noData = new Label
                {
                    Text = "该区域暂无地点",
                    Position = new Vector2(CanvasWidth / 2 - 80, CanvasHeight / 2),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                noData.AddThemeFontOverride("font", GetFont());
                noData.AddThemeFontSizeOverride("font_size", 18);
                noData.AddThemeColorOverride("font_color", ColorTextSecondary);
                _mapContainer.AddChild(noData);
                _canvasDraw.SetMapData(null, null);
                return;
            }

            // Compute bounds for scaling
            float minX = filtered.Min(m => m.MapX);
            float maxX = filtered.Max(m => m.MapX);
            float minY = filtered.Min(m => m.MapY);
            float maxY = filtered.Max(m => m.MapY);
            float rangeX = Math.Max(maxX - minX, 1);
            float rangeY = Math.Max(maxY - minY, 1);

            float scale = Math.Min(
                (CanvasWidth - 80) / rangeX,
                (CanvasHeight - 80) / rangeY
            ) * MapScale;

            float offsetX = (CanvasWidth - rangeX * scale) / 2 - minX * scale;
            float offsetY = (CanvasHeight - rangeY * scale) / 2 - minY * scale;

            // Create map node buttons
            foreach (var map in filtered)
            {
                float px = map.MapX * scale + offsetX;
                float py = map.MapY * scale + offsetY;

                bool isCurrent = map.Id == _currentMapId;
                Color nodeColor = GetRegionColor(map.Region);
                if (isCurrent) nodeColor = ColorCurrent;

                var btn = new Button
                {
                    Text = "",
                    Position = new Vector2(px - NodeRadius, py - NodeRadius),
                    CustomMinimumSize = new Vector2(NodeRadius * 2, NodeRadius * 2),
                    MouseFilter = MouseFilterEnum.Stop,
                    TooltipText = map.Name
                };
                btn.AddThemeStyleboxOverride("normal", MakeNodeStyle(nodeColor, false, isCurrent));
                btn.AddThemeStyleboxOverride("hover", MakeNodeStyle(nodeColor, true, isCurrent));
                btn.AddThemeStyleboxOverride("pressed", MakeNodeStyle(nodeColor * 0.7f, false, isCurrent));

                var captured = map;
                btn.Pressed += () => SelectMap(captured);
                _mapContainer.AddChild(btn);
                _mapButtons.Add((btn, map));

                // Name label
                var label = new Label
                {
                    Text = map.Name,
                    Position = new Vector2(px - 50, py + NodeRadius + 2),
                    CustomMinimumSize = new Vector2(100, 20),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                label.AddThemeFontOverride("font", GetFont());
                label.AddThemeFontSizeOverride("font_size", 10);
                label.AddThemeColorOverride("font_color", isCurrent ? ColorCurrent : ColorTextSecondary);
                _mapContainer.AddChild(label);
            }

            // Pass data to canvas for connection lines
            var mapPositions = new Dictionary<string, Vector2>();
            foreach (var map in _allMaps)
            {
                float px = map.MapX * scale + offsetX;
                float py = map.MapY * scale + offsetY;
                mapPositions[map.Id] = new Vector2(px, py);
            }
            _canvasDraw.SetMapData(_allMaps, mapPositions);

            // Auto-select first map if none selected
            if (filtered.Count > 0)
                SelectMap(filtered[0]);
        }

        private void SelectMap(MapData map)
        {
            _selectedMap = map;

            _detailName.Text = map.Name;

            string regionStr = $"■ {map.Region}";
            _detailRegion.Text = regionStr;
            _detailRegion.AddThemeColorOverride("font_color", GetRegionColor(map.Region));

            _detailClimate.Text = $"气候：{map.Climate}";

            _detailDescription.Text = map.Description;

            var npcNames = map.Npcs?.Count > 0
                ? string.Join(", ", map.Npcs)
                : "无";
            _detailNpcs.Text = npcNames;

            var connNames = new List<string>();
            if (map.Connections != null)
            {
                foreach (var connId in map.Connections)
                {
                    var connMap = _allMaps.Find(m => m.Id == connId);
                    if (connMap != null)
                        connNames.Add(connMap.Name);
                }
            }
            _detailConnections.Text = connNames.Count > 0
                ? string.Join(" → ", connNames)
                : "无";
        }

        #region Styling

        private static StyleBoxFlat MakeNodeStyle(Color color, bool hover, bool isCurrent)
        {
            var style = new StyleBoxFlat
            {
                BgColor = isCurrent ? color : (hover ? ColorNodeHover : ColorNodeBg),
                BorderWidthLeft = 3, BorderWidthTop = 3,
                BorderWidthRight = 3, BorderWidthBottom = 3,
                BorderColor = color,
                CornerRadiusTopLeft = (int)NodeRadius,
                CornerRadiusTopRight = (int)NodeRadius,
                CornerRadiusBottomLeft = (int)NodeRadius,
                CornerRadiusBottomRight = (int)NodeRadius,
                ContentMarginLeft = 0, ContentMarginTop = 0,
                ContentMarginRight = 0, ContentMarginBottom = 0
            };
            return style;
        }

        private static Color GetRegionColor(string region)
        {
            return region switch
            {
                "江南" => ColorJiangnan,
                "西域" => ColorWestern,
                "京城" => ColorCapital,
                _ => ColorTextSecondary
            };
        }

        private void ApplyFilterButtonStyle(Button btn, bool active)
        {
            var normal = new StyleBoxFlat
            {
                BgColor = active ? GetRegionColor(_activeRegionFilter) * new Color(0.5f, 0.5f, 0.5f, 1f) : ColorSurface,
                BorderWidthLeft = 1, BorderWidthTop = 1,
                BorderWidthRight = 1, BorderWidthBottom = 1,
                BorderColor = active ? GetRegionColor(_activeRegionFilter) : new Color(0.3f, 0.4f, 0.5f, 0.5f),
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 8, ContentMarginTop = 2,
                ContentMarginRight = 8, ContentMarginBottom = 2
            };
            btn.AddThemeStyleboxOverride("normal", normal);
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 11);
            btn.AddThemeColorOverride("font_color", active ? new Color(1, 1, 1, 1) : ColorTextSecondary);
        }

        #endregion

        /// <summary>
        /// Custom Control that draws connection lines between map nodes via _Draw().
        /// </summary>
        private partial class MapCanvas : Control
        {
            private List<MapData> _maps;
            private Dictionary<string, Vector2> _positions;

            public void SetMapData(List<MapData> maps, Dictionary<string, Vector2> positions)
            {
                _maps = maps;
                _positions = positions;
                QueueRedraw();
            }

            public override void _Draw()
            {
                if (_maps == null || _positions == null) return;

                foreach (var map in _maps)
                {
                    if (!_positions.TryGetValue(map.Id, out Vector2 from)) continue;
                    if (map.Connections == null) continue;

                    foreach (var connId in map.Connections)
                    {
                        if (!_positions.TryGetValue(connId, out Vector2 to)) continue;

                        Color lineColor = ColorLine;
                        DrawLine(from, to, lineColor, 1.5f, true);
                    }
                }

                // Draw a subtle grid/border
                DrawRect(new Rect2(new Vector2(0, 0), Size), new Color(0.15f, 0.2f, 0.3f, 0.3f), false, 1f);
            }
        }
    }
}
