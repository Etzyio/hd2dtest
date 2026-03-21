using Godot;
using System;
using System.Diagnostics;
using System.IO;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 版本管理器，负责管理游戏的版本信息
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为自动加载的单例使用，负责管理游戏中的版本信息。
    /// 它提供了版本号的生成、Git提交哈希的获取、版本信息的保存和加载等功能，支持四位数字的版本号格式。
    /// </remarks>
    public partial class VersionManager : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static VersionManager _instance;

        /// <summary>
        /// 版本管理器的单例实例
        /// </summary>
        /// <value>版本管理器的单例实例</value>
        public static VersionManager Instance => _instance;

        /// <summary>
        /// 默认版本号
        /// </summary>
        private const string DEFAULT_VERSION = "0.1.0";

        /// <summary>
        /// 游戏版本号
        /// </summary>
        public string GameVersion { get; private set; }

        /// <summary>
        /// 构建日期
        /// </summary>
        public string BuildDate { get; private set; }

        /// <summary>
        /// Git提交哈希
        /// </summary>
        public string GitCommit { get; private set; }

        /// <summary>
        /// 节点准备就绪时调用的方法
        /// </summary>
        /// <remarks>
        /// 该方法在节点进入场景树时自动调用，用于设置单例实例并初始化版本信息。
        /// 它会获取构建日期、Git提交哈希，并确保版本号格式为四位数字。
        /// </remarks>
        public override void _Ready()
        {
            // 设置单例实例
            _instance = this;

            // 尝试加载版本信息
            LoadVersionInfo();
        }

        /// <summary>
        /// 加载版本信息
        /// </summary>
        /// <returns>版本信息字典</returns>
        /// <remarks>
        /// 该方法从version.json文件中加载版本信息，如果文件不存在或加载失败，会返回当前的版本信息。
        /// </remarks>
        public Godot.Collections.Dictionary LoadVersionInfo()
        {
            try
            {
                string versionPath = "res://version.json";
                if (Godot.FileAccess.FileExists(versionPath))
                {
                    // 使用Godot的FileAccess正确方法读取文件
                    using var file = Godot.FileAccess.Open(versionPath, Godot.FileAccess.ModeFlags.Read);
                    if (file != null)
                    {
                        string json = file.GetAsText();
                        file.Close();
                        var versionData = Godot.Json.ParseString(json).AsGodotDictionary();
                        
                        // 更新当前版本信息
                        if (versionData.ContainsKey("version"))
                            GameVersion = versionData["version"].AsString();
                        if (versionData.ContainsKey("build_date"))
                            BuildDate = versionData["build_date"].AsString();
                        if (versionData.ContainsKey("git_commit"))
                            GitCommit = versionData["git_commit"].AsString();
                        
                        Log.Info("Version info loaded from file");
                        Log.Info($"Loaded version: {GameVersion}");
                        return versionData;
                    }
                }
                else
                {
                    Log.Info("Version.json file not found, using generated version info");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load version info: {e.Message}");
            }

            return new Godot.Collections.Dictionary
        {
            { "version", GameVersion },
            { "build_date", BuildDate },
            { "git_commit", GitCommit }
        };
        }

        /// <summary>
        /// 获取版本字符串（用于显示）
        /// </summary>
        /// <returns>格式化的版本字符串</returns>
        /// <remarks>
        /// 该方法返回一个格式化的版本字符串，包含版本号、Git提交哈希和构建日期，用于在游戏中显示。
        /// </remarks>
        public string GetVersionString()
        {
            return $"v{GameVersion} ({GitCommit}) - {BuildDate}";
        }
    }
}
