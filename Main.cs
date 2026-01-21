using Godot;
using System;

namespace hd2dtest.Scripts.Core
{
    public partial class Main : Node
    {
        private VersionManager _versionManager;
        private Node _sceneLayer;
        private Node _popupLayer;

        public override void _Ready()
        {
            // 初始化版本管理器
            InitializeVersionManager();

            Log.Info("HD2D Game Initialized - Loading Start Scene");

            // 初始化存档管理器（单例模式）
            InitializeSaveManager();

            // 初始化配置管理器（单例模式）
            InitializeConfigManager();

            // 输出当前存档信息
            PrintCurrentSaveInfo();

            // 获取子节点
            _sceneLayer = GetNode<Node>("sceneLayer");
            _popupLayer = GetNode<Node>("popupLayer");
            // 初始化视图管理器
            GameViewManager.Init(_sceneLayer, _popupLayer);
            // 切换初始场景
            _ = GameViewManager.SwitchScene("start");
        }

        public override void _Process(double delta)
        {
        }

        private static void InitializeVersionManager()
        {
        }

        private void InitializeSaveManager()
        {
            // 使用单例模式获取SaveManager实例
            SaveManager saveManager = SaveManager.GetInstance();

            // 检查实例是否已添加到场景树
            if (saveManager.GetParent() == null)
            {
                AddChild(saveManager);
            }
        }

        private void InitializeConfigManager()
        {
            // 使用单例模式获取ConfigManager实例
            ConfigManager configManager = ConfigManager.GetInstance();

            // 检查实例是否已添加到场景树
            if (configManager.GetParent() == null)
            {
                AddChild(configManager);
            }

            Log.Info("ConfigManager initialized");
        }

        private static void PrintCurrentSaveInfo()
        {
            Log.Info("=== Current Save Info ===");

            // 使用单例模式获取SaveManager实例
            SaveManager saveManager = SaveManager.Instance;

            if (saveManager != null)
            {
                // 获取所有存档信息
                System.Collections.Generic.List<Modules.SaveInfo> saveInfos = saveManager.GetAllSaveInfos();

                if (saveInfos.Count == 0)
                {
                    Log.Info("No saves found.");
                }
                else
                {
                    Log.Info($"Found {saveInfos.Count} save(s):");
                    foreach (Modules.SaveInfo info in saveInfos)
                    {
                        Log.Info($"- Slot {info.SaveId}: {info.SaveName}");
                        Log.Info($"  Version: {info.GameVersion}, Players: {info.PlayerCount}, Avg Level: {info.AveragePlayerLevel}, Game Score: {info.GameScore}");
                        Log.Info($"  Play Time: {info.PlayTime} seconds, Scene: {info.CurrentScene}");
                        Log.Info($"  Saved: {info.SaveTime}");
                    }
                }
            }
            else
            {
                Log.Error("SaveManager instance not available.");
            }
        }

        // 场景切换方法 - 只切换sceneLayer中的场景内容
        private void SwitchScene(string scenePath)
        {
            Log.Info($"Switching scene content in sceneLayer to: {scenePath}");

            try
            {
                // 获取sceneLayer
                Node sceneLayer = GetNode<Node>("sceneLayer");

                // 清除sceneLayer中的所有子节点
                foreach (Node child in sceneLayer.GetChildren())
                {
                    child.QueueFree();
                }

                // 加载新场景
                PackedScene scene = GD.Load<PackedScene>(scenePath);
                if (scene != null)
                {
                    // 实例化场景并添加到sceneLayer
                    Node sceneInstance = scene.Instantiate();
                    sceneLayer.AddChild(sceneInstance);
                    Log.Info($"Successfully loaded scene: {scenePath} into sceneLayer");
                }
                else
                {
                    Log.Error($"Failed to load scene: {scenePath}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error switching scene: {e.Message}");
            }
        }
    }
}
