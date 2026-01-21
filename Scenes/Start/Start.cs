using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts
{
    public partial class Start : Node2D
    {
        private CanvasLayer _canvasLayer;
        private Button _startButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _exitButton;

        // 存档列表相关节点
        private ColorRect _saveListPanel;
        private Button _backButton;
        private VBoxContainer _saveListContainer;

        public override void _Ready()
        {
            _canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
            // 获取按钮节点
            _startButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/VBoxContainer/StartButton");
            _continueButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/VBoxContainer/ContinueButton");
            _settingsButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/VBoxContainer/SettingsButton");
            _exitButton = GetNode<Button>("CanvasLayer/CenterContainer/Panel/VBoxContainer/ExitButton");

            // 连接信号
            _startButton.Pressed += OnStartButtonPressed;
            _continueButton.Pressed += OnContinueButtonPressed;
            _settingsButton.Pressed += OnSettingsButtonPressed;
            _exitButton.Pressed += OnExitButtonPressed;

            // 初始化存档列表UI
            // CreateSaveListUI();
            GameViewManager.TriggerSceneReady();
        }

        public override void _Process(double delta)
        {
            if (!Visible)
            {
                _canvasLayer.Visible = false;
            }
        }

        // 开始游戏按钮点击事件
        private void OnStartButtonPressed()
        {
            Log.Info("Start button pressed");
            // 切换到First场景
            _ = GameViewManager.SwitchScene("test");
        }

        // 继续游戏按钮点击事件
        private void OnContinueButtonPressed()
        {
            Log.Info("Continue button pressed");
            // 显示存档列表
            // ShowSaveList();

        }

        // 显示存档列表
        private void ShowSaveList()
        {
            // 获取所有存档信息
            List<SaveInfo> saveInfos = SaveManager.Instance.GetAllSaveInfos();
            Log.Info($"Found {saveInfos.Count} saves");
            if (saveInfos.Count == 0)
            {
                // 如果没有存档，显示提示
                Label noSaveLabel = new()
                {
                    Text = "没有找到存档",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                noSaveLabel.AddThemeFontSizeOverride("font_size", 20);
                noSaveLabel.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                noSaveLabel.AddThemeConstantOverride("margin_top", 20);
                noSaveLabel.AddThemeConstantOverride("margin_bottom", 20);
                _saveListContainer.AddChild(noSaveLabel);
            }
            else
            {
                // 动态创建存档项
                foreach (SaveInfo saveInfo in saveInfos)
                {
                    CreateSaveItem(saveInfo);
                }
            }

            // 显示存档列表面板
            _saveListPanel.Visible = true;
        }

        // 创建存档列表UI
        private void CreateSaveListUI()
        {
            // 获取CanvasLayer节点
            CanvasLayer canvasLayer = GetNode<CanvasLayer>("CanvasLayer");

            // 创建存档列表面板
            _saveListPanel = new ColorRect
            {
                Name = "SaveListPanel",
                Visible = false
            };
            _saveListPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _saveListPanel.SizeFlagsHorizontal = Control.SizeFlags.Fill;
            _saveListPanel.SizeFlagsVertical = Control.SizeFlags.Fill;
            _saveListPanel.Modulate = new Color(0, 0, 0, 0.8f);
            canvasLayer.AddChild(_saveListPanel);

            // 创建居中容器
            CenterContainer centerContainer = new()
            {
                Name = "CenterContainer"
            };
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            centerContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill;
            centerContainer.SizeFlagsVertical = Control.SizeFlags.Fill;
            _saveListPanel.AddChild(centerContainer);

            // 创建面板
            Panel panel = new()
            {
                Name = "Panel",
                CustomMinimumSize = new Vector2(600, 400)
            };
            centerContainer.AddChild(panel);

            // 创建垂直容器
            VBoxContainer vBoxContainer = new()
            {
                Name = "VBoxContainer"
            };
            vBoxContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vBoxContainer.AddThemeConstantOverride("margin_left", 10);
            vBoxContainer.AddThemeConstantOverride("margin_top", 10);
            vBoxContainer.AddThemeConstantOverride("margin_right", 10);
            vBoxContainer.AddThemeConstantOverride("margin_bottom", 10);
            vBoxContainer.Alignment = BoxContainer.AlignmentMode.Center;
            panel.AddChild(vBoxContainer);

            // 创建标题标签
            Label titleLabel = new()
            {
                Name = "TitleLabel",
                Text = "选择存档",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 32);
            vBoxContainer.AddChild(titleLabel);

            // 创建间隔
            MarginContainer marginContainer = new()
            {
                Name = "MarginContainer",
                CustomMinimumSize = new Vector2(0, 20)
            };
            vBoxContainer.AddChild(marginContainer);

            // 创建滚动容器
            ScrollContainer scrollContainer = new()
            {
                Name = "ScrollContainer",
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                SizeFlagsVertical = Control.SizeFlags.Fill
            };
            vBoxContainer.AddChild(scrollContainer);

            // 创建存档列表容器
            _saveListContainer = new VBoxContainer
            {
                Name = "SaveListContainer",
                SizeFlagsHorizontal = Control.SizeFlags.Fill
            };
            scrollContainer.AddChild(_saveListContainer);

            // 创建返回按钮
            _backButton = new Button
            {
                Name = "BackButton",
                Text = "返回",
                CustomMinimumSize = new Vector2(0, 50)
            };
            _backButton.AddThemeFontSizeOverride("font_size", 20);
            _backButton.Pressed += OnBackButtonPressed;
            vBoxContainer.AddChild(_backButton);
        }

        // 设置按钮点击事件
        private void OnSettingsButtonPressed()
        {
            Log.Info("Settings button pressed");
            // TODO: 实现设置功能
            ShowToast("设置功能尚未实现");
        }

        // 退出按钮点击事件
        private void OnExitButtonPressed()
        {
            Log.Info("Exit button pressed");
            // 退出游戏
            GetTree().Quit();
        }

        // 返回按钮点击事件
        private void OnBackButtonPressed()
        {
            // 隐藏存档列表
            _saveListPanel.Visible = false;
        }

        // 清空存档列表
        private void ClearSaveList()
        {
            foreach (Node child in _saveListContainer.GetChildren())
            {
                child.QueueFree();
            }
        }

        // 创建存档项
        private void CreateSaveItem(SaveInfo saveInfo)
        {
            // 创建存档项容器
            Panel saveItemPanel = new()
            {
                CustomMinimumSize = new Vector2(0, 120)
            };
            saveItemPanel.AddThemeConstantOverride("margin_bottom", 10);
            _saveListContainer.AddChild(saveItemPanel);

            // 创建垂直容器
            VBoxContainer saveItemVBox = new();
            saveItemVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            saveItemVBox.AddThemeConstantOverride("margin_left", 10);
            saveItemVBox.AddThemeConstantOverride("margin_top", 10);
            saveItemVBox.AddThemeConstantOverride("margin_right", 10);
            saveItemVBox.AddThemeConstantOverride("margin_bottom", 10);
            saveItemPanel.AddChild(saveItemVBox);

            // 存档名称
            Label nameLabel = new()
            {
                Text = saveInfo.SaveName
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 24);
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            saveItemVBox.AddChild(nameLabel);

            // 存档信息
            Label infoLabel = new();
            string playTime = FormatPlayTime(saveInfo.PlayTime);
            infoLabel.Text = $"等级: {saveInfo.AveragePlayerLevel} | 游戏时间: {playTime} | 保存时间: {saveInfo.SaveTime:yyyy-MM-dd HH:mm}";
            infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            infoLabel.AddThemeFontSizeOverride("font_size", 16);
            saveItemVBox.AddChild(infoLabel);

            // 操作按钮容器
            HBoxContainer buttonContainer = new()
            {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            buttonContainer.AddThemeConstantOverride("margin_top", 10);
            saveItemVBox.AddChild(buttonContainer);

            // 加载按钮
            Button loadButton = new()
            {
                Text = "加载",
                CustomMinimumSize = new Vector2(100, 30)
            };
            loadButton.AddThemeFontSizeOverride("font_size", 16);
            loadButton.Pressed += () => OnLoadSavePressed(saveInfo.SaveId);
            buttonContainer.AddChild(loadButton);

            // 间隔
            Control spacer = new()
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            buttonContainer.AddChild(spacer);

            // 删除按钮
            Button deleteButton = new()
            {
                Text = "删除",
                CustomMinimumSize = new Vector2(100, 30)
            };
            deleteButton.AddThemeFontSizeOverride("font_size", 16);
            deleteButton.Modulate = Colors.Red;
            deleteButton.Pressed += () => OnDeleteSavePressed(saveInfo.SaveId);
            buttonContainer.AddChild(deleteButton);

            // 添加点击事件
            saveItemPanel.MouseEntered += () =>
            {
                saveItemPanel.Modulate = new Color(1f, 1f, 1f, 0.8f);
            };
            saveItemPanel.MouseExited += () =>
            {
                saveItemPanel.Modulate = new Color(1f, 1f, 1f, 1f);
            };
        }

        // 格式化游戏时间
        private static string FormatPlayTime(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = seconds % 3600 / 60;
            int secs = seconds % 60;

            return hours > 0 ? $"{hours}h {minutes}m" : minutes > 0 ? $"{minutes}m {secs}s" : $"{secs}s";
        }

        // 加载存档按钮点击事件
        private static void OnLoadSavePressed(string saveId)
        {
            Log.Info($"Load save pressed for slot {saveId}");
            // TODO: 实现加载存档功能
            ShowToast($"加载存档 {saveId} 功能尚未实现");
        }

        // 删除存档按钮点击事件
        private static void OnDeleteSavePressed(string saveId)
        {
            Log.Info($"Delete save pressed for slot {saveId}");
            // TODO: 实现删除存档功能
            ShowToast($"删除存档 {saveId} 功能尚未实现");
        }

        // 显示提示信息
        private static void ShowToast(string message)
        {
            // 这里可以实现一个简单的提示信息显示
            Log.Info($"Toast: {message}");
        }
    }
}
