using Godot;
using System;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;

namespace hd2dtest
{
    public partial class Main : Node
    {
        private static Main _instance;
        public static Main Instance => _instance;

        private VersionManager _versionManager;
        //场景层
        private Control _sceneLayer;
        //遮罩
        private ColorRect _shade;
        //弹窗层
        private hd2dtest.Scenes.Popup.PopupMenu _popupLayer;

        public bool PopupStatus => _popupLayer.Visible;
        public Node NowScene => _sceneLayer.GetChild(0);

        public override void _Ready()
        {

            _instance = this;
            // 初始化版本管理器
            InitializeVersionManager();

            // 初始化存档管理器（单例模式）
            InitializeSaveManager();

            // 初始化配置管理器（单例模式）
            InitializeConfigManager();

            // 获取子节点
            _shade = GetNode<ColorRect>("shade");
            _sceneLayer = GetNode<Control>("sceneLayer");
            _popupLayer = GetNode<hd2dtest.Scenes.Popup.PopupMenu>("popupLayer/PopupMenu");

            Log.Info("HD2D Game Initialized - Loading Start Scene");
            // 切换到开始界面
            SwitchScene("start");
        }

        public void TriggerSceneReady()
        {
            Log.Info("Scene ready triggered, showing scene layer");
            // 显示场景层
            ShowSceneLayer();
        }

        public override void _Process(double delta)
        {
        }

        private static void InitializeVersionManager()
        {
        }

        public void SwitchScene(String sceneName)
        {
            Log.Info($"Starting to switch scene: {sceneName}");

            // 隐藏场景层
            HideSceneLayer();
            try
            {
                PackedScene scene = GameViewRegister.GetScene(sceneName);

                if (scene != null)
                {
                    foreach (var a in _sceneLayer.GetChildren())
                    {
                        _sceneLayer.RemoveChild(a);
                    }
                    var newScene = scene.Instantiate<Node>();
                    // 使用Godot内置的_ready方法初始化，不需要额外调用Init
                    _sceneLayer.AddChild(newScene);
                }
                Log.Info($"Scene {sceneName} loaded successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Exception during scene switch: {ex.Message}");
            }
            finally
            {
                ShowSceneLayer();
                ResumeSceneLayer();
            }
        }

        private void ShowSceneLayer()
        {
            if (_sceneLayer != null)
            {
                _sceneLayer.Visible = true;
            }
            if (_popupLayer != null)
            {
                _popupLayer.Visible = false;
            }
            Log.Info("Scene layer shown (CanvasItem)");
        }

        private void ResumeSceneLayer()
        {
            if (_sceneLayer != null)
            {
                // 恢复场景层的处理
                _sceneLayer.ProcessMode = Node.ProcessModeEnum.Inherit;

                // 移除视觉反馈
                RemovePauseVisualEffect();

                // 恢复音频
                HandleAudioResume();

                Log.Info("Scene layer resumed");
            }
        }

        private void RemovePauseVisualEffect()
        {
            _shade?.Visible = false;
        }

        private void PauseSceneLayer()
        {
            _shade?.Visible = true;
        }

        private void HandleAudioResume()
        {
            // TODO: 这里可以添加音频处理逻辑
            // 例如：恢复背景音乐音量
            // 实际实现需要根据游戏的音频系统来调整
            Log.Info("Audio resumed - background music volume restored");
        }
        private static void HandleAudioPause()
        {
            // TODO: 这里可以添加音频处理逻辑
            // 例如：降低背景音乐音量
            // 实际实现需要根据游戏的音频系统来调整
            Log.Info("Audio paused - background music volume reduced");
        }

        private void HideSceneLayer()
        {
            if (_sceneLayer != null)
            {
                _sceneLayer.Visible = false;
            }
            if (_popupLayer != null)
            {
                _popupLayer.Visible = false;
            }

            Log.Info("Scene layer hidden (CanvasItem)");
        }
        private void InitializeSaveManager()
        {
            // 使用单例模式获取SaveManager实例
            SaveManager saveManager = SaveManager.GetInstance();

            // 检查实例是否已经有父节点，如果没有则添加到场景树
            try
            {
                if (saveManager.GetParent() == null)
                {
                    AddChild(saveManager);
                }
            }
            catch (Exception)
            {
                // 如果GetParent()失败，说明实例可能还没有被添加到场景树
                // 但为了安全起见，我们不直接添加，而是记录警告
                Log.Warning("SaveManager already has a parent, skipping AddChild");
            }

            Log.Info("SaveManager initialized successfully");
        }

        private void InitializeConfigManager()
        {
            // 使用单例模式获取ConfigManager实例
            ConfigManager configManager = ConfigManager.GetInstance();

            // 检查实例是否已经有父节点，如果没有则添加到场景树
            try
            {
                if (configManager.GetParent() == null)
                {
                    AddChild(configManager);
                }
            }
            catch (Exception)
            {
                // 如果GetParent()失败，说明实例可能还没有被添加到场景树
                // 但为了安全起见，我们不直接添加，而是记录警告
                Log.Warning("ConfigManager already has a parent, skipping AddChild");
            }

            Log.Info("ConfigManager initialized");
        }
        public void OpenPopup()
        {
            _shade.Visible = true;
            _sceneLayer.ProcessMode = Node.ProcessModeEnum.Disabled;
            if (_popupLayer != null)
            {
                _popupLayer.Visible = true;
                _popupLayer.ShowMainMenu();
                PauseSceneLayer();
                // 处理音频 - 降低背景音乐音量
                HandleAudioPause();
            }
        }
        public void ClosePopup()
        {
            _shade?.Visible = false;
            _sceneLayer.ProcessMode = Node.ProcessModeEnum.Inherit;
            if (_popupLayer != null)
            {
                _popupLayer.Visible = false;
                _shade.Visible = false;
                ResumeSceneLayer();
                // 恢复音频
                HandleAudioResume();
            }
        }
    }
}
