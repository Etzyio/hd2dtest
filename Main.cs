using Godot;
using System;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Core
{
    public partial class Main : Node
    {
        private VersionManager _versionManager;

        public override void _Ready()
        {
            // 初始化版本管理器
            InitializeVersionManager();

            Log.Info("HD2D Game Initialized - No Nodes Added");

            // 初始化存档管理器（单例模式）
            InitializeSaveManager();

            // 初始化配置管理器（单例模式）
            InitializeConfigManager();

            // 输出当前存档信息
            PrintCurrentSaveInfo();
        }

        private void InitializeVersionManager()
        {
            // 创建并添加VersionManager节点
            _versionManager = new VersionManager();
            AddChild(_versionManager);

            // 显示版本信息
            Log.Info("=== Version Info ===");
            Log.Info(_versionManager.GetVersionString());
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
                var saveInfos = saveManager.GetAllSaveInfos();

                if (saveInfos.Count == 0)
                {
                    Log.Info("No saves found.");
                }
                else
                {
                    Log.Info($"Found {saveInfos.Count} save(s):");
                    foreach (var info in saveInfos)
                    {
                        Log.Info($"- Slot {info.SaveId}: {info.SaveName}");
                        Log.Info($"  Players: {info.PlayerCount}, Avg Level: {info.AveragePlayerLevel}, Game Score: {info.GameScore}");
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
    }
}
