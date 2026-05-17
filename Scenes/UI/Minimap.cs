using Godot;
using System;
using System.Collections.Generic;

namespace hd2dtest.Scenes.UI
{
	[GlobalClass]
	public partial class Minimap : Control
	{
		[Export] public Node2D Player { get; set; }
		[Export] public float MapSize { get; set; } = 1000f;
		[Export] public float DefaultZoom { get; set; } = 1.0f;
		[Export] public float MinZoom { get; set; } = 0.5f;
		[Export] public float MaxZoom { get; set; } = 3.0f;

		private TextureRect _mapBackground;
		private TextureRect _playerMarker;
		private Control _markersContainer;
		private Label _zoomLabel;
		private Panel _mapPanel;
		private Button _toggleButton;

		private float _currentZoom = 1.0f;
		private Vector2 _dragStartPos;
		private bool _isDragging = false;
		private bool _isExpanded = false;
		private Vector2 _mapOffset = Vector2.Zero;

		public enum MarkerType
		{
			Quest,
			NPC,
			Enemy,
			Treasure,
			SavePoint,
			Custom
		}

		public struct MapMarker
		{
			public string Id;
			public Vector2 WorldPosition;
			public MarkerType Type;
			public string Label;
			public bool IsVisible;

			public MapMarker(string id, Vector2 pos, MarkerType type, string label = "", bool isVisible = true)
			{
				Id = id;
				WorldPosition = pos;
				Type = type;
				Label = label;
				IsVisible = isVisible;
			}
		}

		private List<MapMarker> _markers = [];
		private Dictionary<string, TextureRect> _markerNodes = [];

		private Color _colorExplored = new(0.2f, 0.6f, 0.86f, 0.3f);
		private Color _colorUnexplored = new(0.1f, 0.15f, 0.2f, 0.8f);
		private Color _colorPlayer = new(1f, 0.3f, 0.3f, 1f);

		public override void _Ready()
		{
			InitializeUI();
			SetupInputHandlers();
			_currentZoom = DefaultZoom;
		}

		private void InitializeUI()
		{
			CustomMinimumSize = new Vector2(180, 180);
			AnchorLeft = 1.0f;
			AnchorRight = 1.0f;
			AnchorTop = 0.0f;
			AnchorBottom = 0.0f;
			OffsetLeft = -200;
			OffsetRight = -20;
			OffsetTop = 20;
			OffsetBottom = 200;

			_mapPanel = new Panel
			{
				Name = "MapPanel",
				AnchorLeft = 0,
				AnchorRight = 1,
				AnchorTop = 0,
				AnchorBottom = 1
			};

			var styleBox = new StyleBoxFlat
			{
				BgColor = new Color(0.13f, 0.19f, 0.25f, 0.95f),
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderWidthBottom = 2,
				BorderColor = new Color(0.3f, 0.45f, 0.6f, 0.8f),
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8
			};
			_mapPanel.AddThemeStyleboxOverride("panel", styleBox);

			_mapBackground = new TextureRect
			{
				Name = "MapBackground",
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
				ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional
			};
			UpdateMapBackground();

			_markersContainer = new Control
			{
				Name = "MarkersContainer",
				MouseFilter = Control.MouseFilterEnum.Ignore
			};

			_playerMarker = new TextureRect
			{
				Name = "PlayerMarker",
				Size = new Vector2(12, 12),
				PivotOffset = new Vector2(6, 6),
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			UpdatePlayerMarkerStyle();

			_zoomLabel = new Label
			{
				Text = $"Zoom: {_currentZoom:F1}x",
				HorizontalAlignment = HorizontalAlignment.Right
			};
			_zoomLabel.AddThemeFontSizeOverride("font_size", 10);
			_zoomLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 0.7f));
			_zoomLabel.ZIndex = 10;

			_toggleButton = new Button
			{
				Text = "⛶",
				TooltipText = "Toggle minimap size",
				CustomMinimumSize = new Vector2(24, 24)
			};
			_toggleButton.AnchorLeft = 1.0f;
			_toggleButton.AnchorRight = 1.0f;
			_toggleButton.AnchorTop = 0.0f;
			_toggleButton.AnchorBottom = 0.0f;
			_toggleButton.OffsetLeft = -28;
			_toggleButton.OffsetRight = -4;
			_toggleButton.OffsetTop = 4;
			_toggleButton.OffsetBottom = 28;
			_toggleButton.ZIndex = 20;
			_toggleButton.Pressed += OnToggleButtonPressed;

			_mapPanel.AddChild(_mapBackground);
			_mapPanel.AddChild(_markersContainer);
			_mapPanel.AddChild(_playerMarker);
			_mapPanel.AddChild(_zoomLabel);
			_mapPanel.AddChild(_toggleButton);

			AddChild(_mapPanel);
		}

		private void UpdateMapBackground()
		{
			var img = Image.CreateEmpty(256, 256, false, Image.Format.Rgba8);
			img.Fill(_colorUnexplored);

			for (int x = 0; x < 256; x++)
			{
				for (int y = 0; y < 256; y++)
				{
					float noise = (float)((GD.Randf() - 0.5f) * 0.05f);
					var col = _colorExplored + new Color(noise, noise, noise, 0);
					if ((x + y) % 3 == 0)
					{
						img.SetPixel(x, y, col);
					}
				}
			}

			var texture = ImageTexture.CreateFromImage(img);
			_mapBackground.Texture = texture;
		}

		private void UpdatePlayerMarkerStyle()
		{
			var img = Image.CreateEmpty(12, 12, false, Image.Format.Rgba8);
			img.Fill(new Color(0, 0, 0, 0));

			for (int x = 2; x < 10; x++)
			{
				for (int y = 2; y < 10; y++)
				{
					float dx = x - 6;
					float dy = y - 6;
					if (dx * dx + dy * dy <= 16)
					{
						img.SetPixel(x, y, _colorPlayer);
					}
				}
			}

			img.SetPixel(6, 0, new Color(1, 0.5f, 0.5f, 1));
			img.SetPixel(5, 1, _colorPlayer);
			img.SetPixel(7, 1, _colorPlayer);

			var texture = ImageTexture.CreateFromImage(img);
			_playerMarker.Texture = texture;
		}

		private void SetupInputHandlers()
		{
			GuiInput += OnGuiInput;
			MouseEntered += () => { MouseDefaultCursorShape = Control.CursorShape.PointingHand; };
			MouseExited += () => { MouseDefaultCursorShape = Control.CursorShape.Arrow; };
		}

		public override void _Process(double delta)
		{
			UpdatePlayerPosition();
			UpdateMarkers();
		}

		private void UpdatePlayerPosition()
		{
			if (Player == null || _mapPanel == null) return;

			Vector2 playerWorldPos = new(Player.Position.X, Player.Position.Y);
			Vector2 mapSize = _mapPanel.Size;

			Vector2 normalizedPos = new(
				(playerWorldPos.X / MapSize + 0.5f + _mapOffset.X / MapSize) * _currentZoom,
				(playerWorldPos.Y / MapSize + 0.5f + _mapOffset.Y / MapSize) * _currentZoom
			);

			Vector2 screenPos = new(
				(normalizedPos.X * 0.5f + 0.5f) * mapSize.X,
				(normalizedPos.Y * 0.5f + 0.5f) * mapSize.Y
			);

			_playerMarker.Position = screenPos - _playerMarker.Size / 2f;
			_playerMarker.Rotation = -Player.Rotation;
		}

		private void UpdateMarkers()
		{
			foreach (var marker in _markers)
			{
				if (!marker.IsVisible) continue;

				if (!_markerNodes.ContainsKey(marker.Id))
				{
					CreateMarkerNode(marker);
				}

				if (_markerNodes.TryGetValue(marker.Id, out var node))
				{
					Vector2 mapSize = _mapPanel.Size;
					Vector2 normalizedPos = new(
						(marker.WorldPosition.X / MapSize + 0.5f + _mapOffset.X / MapSize) * _currentZoom,
						(marker.WorldPosition.Y / MapSize + 0.5f + _mapOffset.Y / MapSize) * _currentZoom
					);

					Vector2 screenPos = new(
						(normalizedPos.X * 0.5f + 0.5f) * mapSize.X,
						(normalizedPos.Y * 0.5f + 0.5f) * mapSize.Y
					);

					node.Position = screenPos - node.Size / 2f;
				}
			}
		}

		private void CreateMarkerNode(MapMarker marker)
		{
			Color markerColor = marker.Type switch
			{
				MarkerType.Quest => new Color(1f, 0.85f, 0f, 1f),
				MarkerType.NPC => new Color(0.3f, 0.8f, 0.4f, 1f),
				MarkerType.Enemy => new Color(1f, 0.3f, 0.3f, 1f),
				MarkerType.Treasure => new Color(1f, 0.65f, 0f, 1f),
				MarkerType.SavePoint => new Color(0.3f, 0.6f, 1f, 1f),
				_ => new Color(1f, 1f, 1f, 1f)
			};

			var img = Image.CreateEmpty(10, 10, false, Image.Format.Rgba8);
			img.Fill(new Color(0, 0, 0, 0));

			switch (marker.Type)
			{
				case MarkerType.Quest:
					DrawDiamond(img, markerColor);
					break;
				case MarkerType.NPC:
					DrawCircle(img, markerColor, 4);
					break;
				case MarkerType.Enemy:
					DrawSquare(img, markerColor);
					break;
				case MarkerType.Treasure:
					DrawStar(img, markerColor);
					break;
				case MarkerType.SavePoint:
					DrawFlag(img, markerColor);
					break;
				default:
					DrawCircle(img, markerColor, 3);
					break;
			}

			var texture = ImageTexture.CreateFromImage(img);
			var markerNode = new TextureRect
			{
				Texture = texture,
				Size = new Vector2(10, 10),
				MouseFilter = Control.MouseFilterEnum.Ignore,
				TooltipText = !string.IsNullOrEmpty(marker.Label) ? marker.Label : marker.Type.ToString()
			};

			_markersContainer.AddChild(markerNode);
			_markerNodes[marker.Id] = markerNode;
		}

		private static void DrawCircle(Image img, Color color, int radius)
		{
			int cx = 5, cy = 5;
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
					{
						img.SetPixel(x, y, color);
					}
				}
			}
		}

		private static void DrawSquare(Image img, Color color)
		{
			for (int x = 2; x < 8; x++)
			{
				for (int y = 2; y < 8; y++)
				{
					img.SetPixel(x, y, color);
				}
			}
		}

		private static void DrawDiamond(Image img, Color color)
		{
			int cx = 5, cy = 5;
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					if (Math.Abs(x - cx) + Math.Abs(y - cy) <= 4)
					{
						img.SetPixel(x, y, color);
					}
				}
			}
		}

		private static void DrawStar(Image img, Color color)
		{
			int cx = 5, cy = 5;
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					float dx = x - cx, dy = y - cy;
					float dist = (float)Math.Sqrt(dx * dx + dy * dy);
					if (dist <= 4 && dist >= 1.5f)
					{
						img.SetPixel(x, y, color);
					}
				}
			}
		}

		private static void DrawFlag(Image img, Color color)
		{
			for (int i = 2; i < 8; i++) img.SetPixel(i, 7, color);
			for (int i = 2; i < 6; i++) img.SetPixel(5, i, color);
			for (int x = 3; x < 6; x++)
			{
				for (int y = 2; y < 5; y++)
				{
					img.SetPixel(x, y, color);
				}
			}
		}

		private void OnGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButton)
			{
				if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
				{
					_isDragging = true;
					_dragStartPos = mouseButton.Position;
					MouseDefaultCursorShape = Control.CursorShape.Drag;
				}
				else if (mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
				{
					_isDragging = false;
					MouseDefaultCursorShape = Control.CursorShape.PointingHand;
				}

				if (mouseButton.ButtonIndex == MouseButton.WheelUp)
				{
					ZoomIn();
				}
				else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
				{
					ZoomOut();
				}
			}

			if (@event is InputEventMouseMotion motion && _isDragging)
			{
				Vector2 delta = motion.Position - _dragStartPos;
				_mapOffset -= delta / _currentZoom;
				_dragStartPos = motion.Position;
			}
		}

		public void ZoomIn()
		{
			_currentZoom = Mathf.Min(_currentZoom + 0.2f, MaxZoom);
			UpdateZoomDisplay();
		}

		public void ZoomOut()
		{
			_currentZoom = Mathf.Max(_currentZoom - 0.2f, MinZoom);
			UpdateZoomDisplay();
		}

		private void UpdateZoomDisplay()
		{
			_zoomLabel.Text = $"Zoom: {_currentZoom:F1}x";
		}

		private void OnToggleButtonPressed()
		{
			_isExpanded = !_isExpanded;

			var tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Quart);

			if (_isExpanded)
			{
				tween.TweenProperty(this, "offset_left", -400, 0.3);
				tween.TweenProperty(this, "offset_bottom", 420, 0.3);
				_toggleButton.Text = "✕";
			}
			else
			{
				tween.TweenProperty(this, "offset_left", -200, 0.3);
				tween.TweenProperty(this, "offset_bottom", 200, 0.3);
				_toggleButton.Text = "⛶";
			}
		}

		public void AddMarker(string id, Vector2 worldPosition, MarkerType type, string label = "", bool isVisible = true)
		{
			_markers.Add(new MapMarker(id, worldPosition, type, label, isVisible));
		}

		public void RemoveMarker(string id)
		{
			_markers.RemoveAll(m => m.Id == id);
			if (_markerNodes.TryGetValue(id, out var node))
			{
				node.QueueFree();
				_markerNodes.Remove(id);
			}
		}

		public void SetMarkerVisibility(string id, bool visible)
		{
			int idx = _markers.FindIndex(m => m.Id == id);
			if (idx >= 0)
			{
				var marker = _markers[idx];
				marker.IsVisible = visible;
				_markers[idx] = marker;
				if (_markerNodes.TryGetValue(id, out var node))
				{
					node.Visible = visible;
				}
			}
		}

		public void ClearMarkers()
		{
			foreach (var node in _markerNodes.Values)
			{
				node.QueueFree();
			}
			_markerNodes.Clear();
			_markers.Clear();
		}

		public void SetMapSize(float size)
		{
			MapSize = size;
		}

		public void ResetView()
		{
			_currentZoom = DefaultZoom;
			_mapOffset = Vector2.Zero;
			UpdateZoomDisplay();
		}

		public void SetBattleMode(bool inBattle)
		{
			var tween = CreateTween();
			if (inBattle)
			{
				tween.TweenProperty(_mapPanel, "modulate:a", 0.5f, 0.2);
				tween.TweenProperty(_mapPanel, "position", new Vector2(-10, 0), 0.2);
			}
			else
			{
				tween.TweenProperty(_mapPanel, "modulate:a", 1.0f, 0.2);
				tween.TweenProperty(_mapPanel, "position", Vector2.Zero, 0.2);
			}
		}
	}
}
