using Godot;
using System;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts
{
	public partial class First : Node
	{
		private ColorRect _popupDialog;
		private Label _popupTitle;
		private Label _popupContent;
		private Button _yesButton;
		private Button _noButton;
		private Action<bool> _popupCallback;
		private Camera3D _mainCamera;

		public override void _Ready()
		{
			// 初始化UI组件
			InitializeUI();
			
			// 设置3D像素风格
			SetupPixelArtStyle();
			
			Log.Info("First scene loaded");
		}

		// 设置3D像素风格
		private void SetupPixelArtStyle()
		{
			// 获取主相机
			_mainCamera = GetNode<Camera3D>("player/Camera3D");
			
			// 确保相机使用正交投影
			_mainCamera.Projection = Camera3D.ProjectionType.Orthogonal;
			_mainCamera.Size = 10.0f; // Godot 4中使用Size属性设置正交大小
			
			// 设置相机倾斜30度
			//_mainCamera.Rotation = new Vector3(Mathf.DegToRad(-30.0f), 0, 0);
			
			// 禁用抗锯齿
			GetViewport().Msaa3D = Viewport.Msaa.Disabled;
			
			// 暂时禁用后处理效果，解决屏幕全白问题
			// CreatePixelationEffect();
		}
		
			// 玩家移动速度
		private float _playerSpeed = 5.0f;
		
		// 处理玩家移动
		private void HandlePlayerMovement(double delta)
		{
			// 获取玩家节点（CharacterBody3D）
			var player = _mainCamera.GetParent() as CharacterBody3D;
			if (player == null)
				return;
			
			Vector3 movement = Vector3.Zero;
			
			// 使用WASD控制玩家移动
			if (Input.IsActionPressed("ui_up"))
				movement.Z -= 1;
			if (Input.IsActionPressed("ui_down"))
				movement.Z += 1;
			if (Input.IsActionPressed("ui_left"))
				movement.X -= 1;
			if (Input.IsActionPressed("ui_right"))
				movement.X += 1;
			
			// 归一化并应用速度
			if (movement.LengthSquared() > 0)
			{
				movement = movement.Normalized() * _playerSpeed;
				player.Velocity = movement;
			}
			else
			{
				// 停止移动
				player.Velocity = Vector3.Zero;
			}
			
			// 移动玩家并应用碰撞
			player.MoveAndSlide();
		}
		
		// 创建像素化后处理效果
		private void CreatePixelationEffect()
		{
			// 暂时注释掉后处理效果，解决屏幕全白问题
			// 后续可以调试着色器代码修复
		}

		private void InitializeUI()
		{
			// 检查是否已经存在CanvasLayer和弹窗
			if (!HasNode("CanvasLayer"))
			{
				// 创建CanvasLayer
				CanvasLayer canvasLayer = new CanvasLayer();
				AddChild(canvasLayer);

				// 创建弹窗背景
				_popupDialog = new ColorRect();
				_popupDialog.Name = "PopupDialog";
				_popupDialog.Visible = false;
				_popupDialog.Modulate = new Color(0, 0, 0, 0.5f);
				_popupDialog.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				_popupDialog.SizeFlagsHorizontal = Control.SizeFlags.Fill;
				_popupDialog.SizeFlagsVertical = Control.SizeFlags.Fill;
				canvasLayer.AddChild(_popupDialog);

				// 创建居中容器
				CenterContainer centerContainer = new CenterContainer();
				centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				centerContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill;
				centerContainer.SizeFlagsVertical = Control.SizeFlags.Fill;
				_popupDialog.AddChild(centerContainer);

				// 创建面板
				Panel panel = new Panel();
				panel.CustomMinimumSize = new Vector2(300, 200);
				centerContainer.AddChild(panel);

				// 创建垂直容器
				VBoxContainer vBoxContainer = new VBoxContainer();
				vBoxContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				vBoxContainer.AddThemeConstantOverride("margin_left", 10);
				vBoxContainer.AddThemeConstantOverride("margin_top", 10);
				vBoxContainer.AddThemeConstantOverride("margin_right", 10);
				vBoxContainer.AddThemeConstantOverride("margin_bottom", 10);
				vBoxContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill;
				vBoxContainer.SizeFlagsVertical = Control.SizeFlags.Fill;
				panel.AddChild(vBoxContainer);

				// 创建标题标签
				_popupTitle = new Label();
				_popupTitle.Name = "TitleLabel";
				_popupTitle.Text = "弹窗标题";
				_popupTitle.HorizontalAlignment = HorizontalAlignment.Center;
				vBoxContainer.AddChild(_popupTitle);

				// 创建内容标签
				_popupContent = new Label();
				_popupContent.Name = "ContentLabel";
				_popupContent.Text = "弹窗内容";
				vBoxContainer.AddChild(_popupContent);

				// 创建水平容器
				HBoxContainer hBoxContainer = new HBoxContainer();
				hBoxContainer.Name = "HBoxContainer";
				hBoxContainer.Alignment = BoxContainer.AlignmentMode.Center;
				vBoxContainer.AddChild(hBoxContainer);

				// 创建确定按钮
				_yesButton = new Button();
				_yesButton.Name = "YesButton";
				_yesButton.Text = "确定";
				hBoxContainer.AddChild(_yesButton);

				// 创建取消按钮
				_noButton = new Button();
				_noButton.Name = "NoButton";
				_noButton.Text = "取消";
				hBoxContainer.AddChild(_noButton);

				// 连接信号
				_yesButton.Pressed += OnYesButtonPressed;
				_noButton.Pressed += OnNoButtonPressed;
			}
			else
			{
				// 获取已存在的UI组件
				_popupDialog = GetNode<ColorRect>("CanvasLayer/PopupDialog");
				_popupTitle = _popupDialog.GetNode<Label>("CenterContainer/Panel/VBoxContainer/TitleLabel");
				_popupContent = _popupDialog.GetNode<Label>("CenterContainer/Panel/VBoxContainer/ContentLabel");
				_yesButton = _popupDialog.GetNode<Button>("CenterContainer/Panel/VBoxContainer/HBoxContainer/YesButton");
				_noButton = _popupDialog.GetNode<Button>("CenterContainer/Panel/VBoxContainer/HBoxContainer/NoButton");

				// 连接信号
				_yesButton.Pressed += OnYesButtonPressed;
				_noButton.Pressed += OnNoButtonPressed;
			}

			// 不需要重复注册输入映射，因为Main.cs已经在初始化时注册了
			// InputMap.AddAction("open_first_scene");
			// InputMap.ActionAddEvent("open_first_scene", new InputEventKey() { Keycode = Key.F1 });
			// 
			// InputMap.AddAction("open_main_scene");
			// InputMap.ActionAddEvent("open_main_scene", new InputEventKey() { Keycode = Key.F2 });
		}

		public override void _Process(double delta)
		{
			// 处理场景切换快捷键
			HandleSceneSwitching();
			
			// 处理玩家移动
			HandlePlayerMovement(delta);
		}

		private void HandleSceneSwitching()
		{
			if (Input.IsActionJustPressed("open_first_scene"))
			{
				ShowPopup("切换场景", "确定要刷新当前场景吗？", (result) =>
				{
					if (result)
					{
						SwitchScene("res://Scenes/First/First.tscn");
					}
				});
			}
			else if (Input.IsActionJustPressed("open_main_scene"))
			{
				ShowPopup("切换场景", "确定要切换回主场景吗？", (result) =>
				{
					if (result)
					{
						SwitchScene("res://main.tscn");
					}
				});
			}
		}

		// 场景切换方法
		private void SwitchScene(string scenePath)
		{
			Log.Info($"Switching to scene: {scenePath}");
			GetTree().ChangeSceneToFile(scenePath);
		}

		// 弹窗显示方法
		public void ShowPopup(string title, string content, Action<bool> callback)
		{
			_popupTitle.Text = title;
			_popupContent.Text = content;
			_popupCallback = callback;
			_popupDialog.Visible = true;
		}

		// 隐藏弹窗
		private void HidePopup()
		{
			_popupDialog.Visible = false;
			_popupCallback = null;
		}

		// 确定按钮点击事件
		private void OnYesButtonPressed()
		{
			Log.Info("Yes button pressed");
			_popupCallback?.Invoke(true);
			HidePopup();
		}

		// 取消按钮点击事件
		private void OnNoButtonPressed()
		{
			Log.Info("No button pressed");
			_popupCallback?.Invoke(false);
			HidePopup();
		}
	}
}
