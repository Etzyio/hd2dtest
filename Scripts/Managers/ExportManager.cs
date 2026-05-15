using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 跨平台导出管理器，负责管理游戏在不同平台上的导出流程
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为单例模式运行，负责管理Windows、macOS、Linux、
    /// 移动设备等不同平台的导出配置和流程自动化。
    /// </remarks>
    public partial class ExportManager : Node
    {
        /// <summary>
        /// 导出管理器的单例实例
        /// </summary>
        public static ExportManager Instance { get; private set; }

        /// <summary>
        /// 导出配置字典
        /// </summary>
        private Dictionary<string, ExportConfig> _exportConfigs = new Dictionary<string, ExportConfig>();

        /// <summary>
        /// 当前选中的平台
        /// </summary>
        private string _currentPlatform = string.Empty;

        /// <summary>
        /// 节点就绪时的初始化方法
        /// </summary>
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeExportConfigs();
                Log.Info("ExportManager initialized");
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// 初始化导出配置
        /// </summary>
        private void InitializeExportConfigs()
        {
            _exportConfigs.Add("windows", new ExportConfig
            {
                Platform = "windows",
                PlatformName = "Windows",
                PlatformType = PlatformType.Desktop,
                ExecutableName = "HD2DGame.exe",
                OutputDirectory = "build/windows",
                Architecture = "x86_64",
                BuildTarget = "release",
                Flags = new Dictionary<string, string>
                {
                    { "debug", "false" },
                    { "symbols", "false" },
                    { "optimize", "true" },
                    { "embed_pck", "true" }
                }
            });

            _exportConfigs.Add("macos", new ExportConfig
            {
                Platform = "macos",
                PlatformName = "macOS",
                PlatformType = PlatformType.Desktop,
                ExecutableName = "HD2DGame.app",
                OutputDirectory = "build/macos",
                Architecture = "universal",
                BuildTarget = "release",
                Flags = new Dictionary<string, string>
                {
                    { "debug", "false" },
                    { "symbols", "false" },
                    { "optimize", "true" },
                    { "embed_pck", "true" }
                }
            });

            _exportConfigs.Add("linux", new ExportConfig
            {
                Platform = "linux",
                PlatformName = "Linux",
                PlatformType = PlatformType.Desktop,
                ExecutableName = "HD2DGame.x86_64",
                OutputDirectory = "build/linux",
                Architecture = "x86_64",
                BuildTarget = "release",
                Flags = new Dictionary<string, string>
                {
                    { "debug", "false" },
                    { "symbols", "false" },
                    { "optimize", "true" },
                    { "embed_pck", "true" }
                }
            });

            _exportConfigs.Add("android", new ExportConfig
            {
                Platform = "android",
                PlatformName = "Android",
                PlatformType = PlatformType.Mobile,
                ExecutableName = "HD2DGame.apk",
                OutputDirectory = "build/android",
                Architecture = "arm64-v8a",
                BuildTarget = "release",
                Flags = new Dictionary<string, string>
                {
                    { "debug", "false" },
                    { "symbols", "false" },
                    { "optimize", "true" },
                    { "embed_pck", "true" },
                    { "android_manifest", "res://android/AndroidManifest.xml" },
                    { "keystore_path", "res://android/debug.keystore" },
                    { "keystore_password", "android" },
                    { "key_alias", "debug" },
                    { "key_password", "android" }
                }
            });

            _exportConfigs.Add("ios", new ExportConfig
            {
                Platform = "ios",
                PlatformName = "iOS",
                PlatformType = PlatformType.Mobile,
                ExecutableName = "HD2DGame.ipa",
                OutputDirectory = "build/ios",
                Architecture = "arm64",
                BuildTarget = "release",
                Flags = new Dictionary<string, string>
                {
                    { "debug", "false" },
                    { "symbols", "false" },
                    { "optimize", "true" },
                    { "embed_pck", "true" },
                    { "provisioning_profile", "" },
                    { "signing_identity", "" },
                    { "bundle_identifier", "com.example.hd2dgame" }
                }
            });
        }

        /// <summary>
        /// 获取所有可用平台
        /// </summary>
        /// <returns>平台ID列表</returns>
        public List<string> GetAvailablePlatforms()
        {
            return _exportConfigs.Keys.ToList();
        }

        /// <summary>
        /// 获取平台配置
        /// </summary>
        /// <param name="platform">平台ID</param>
        /// <returns>导出配置，如果未找到返回null</returns>
        public ExportConfig GetPlatformConfig(string platform)
        {
            if (_exportConfigs.TryGetValue(platform, out ExportConfig config))
            {
                return config;
            }
            return null;
        }

        /// <summary>
        /// 设置当前平台
        /// </summary>
        /// <param name="platform">平台ID</param>
        /// <returns>设置成功返回true</returns>
        public bool SetCurrentPlatform(string platform)
        {
            if (_exportConfigs.ContainsKey(platform))
            {
                _currentPlatform = platform;
                Log.Info($"Current platform set to: {platform}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取当前平台配置
        /// </summary>
        /// <returns>当前平台的导出配置，如果未设置返回null</returns>
        public ExportConfig GetCurrentPlatformConfig()
        {
            return GetPlatformConfig(_currentPlatform);
        }

        /// <summary>
        /// 导出指定平台
        /// </summary>
        /// <param name="platform">平台ID</param>
        /// <returns>导出成功返回true</returns>
        public bool ExportPlatform(string platform)
        {
            ExportConfig config = GetPlatformConfig(platform);
            if (config == null)
            {
                Log.Error($"Platform not found: {platform}");
                return false;
            }

            Log.Info($"Starting export for platform: {config.PlatformName}");

            try
            {
                EnsureOutputDirectory(config);
                
                if (!ValidateConfig(config))
                {
                    Log.Error($"Invalid configuration for platform: {platform}");
                    return false;
                }

                BuildProject(config);
                
                if (config.PlatformType == PlatformType.Desktop)
                {
                    PackageDesktop(config);
                }
                else if (config.PlatformType == PlatformType.Mobile)
                {
                    PackageMobile(config);
                }

                Log.Info($"Export completed successfully for {config.PlatformName}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to export platform {platform}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 导出所有平台
        /// </summary>
        /// <returns>成功导出的平台数量</returns>
        public int ExportAllPlatforms()
        {
            int successCount = 0;

            foreach (string platform in GetAvailablePlatforms())
            {
                if (ExportPlatform(platform))
                {
                    successCount++;
                }
            }

            Log.Info($"Exported {successCount}/{_exportConfigs.Count} platforms");
            return successCount;
        }

        /// <summary>
        /// 确保输出目录存在
        /// </summary>
        /// <param name="config">导出配置</param>
        private void EnsureOutputDirectory(ExportConfig config)
        {
            string outputPath = ProjectSettings.GlobalizePath($"res://{config.OutputDirectory}");
            
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                Log.Info($"Created output directory: {outputPath}");
            }
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        /// <param name="config">导出配置</param>
        /// <returns>验证通过返回true</returns>
        private bool ValidateConfig(ExportConfig config)
        {
            bool valid = true;

            if (string.IsNullOrEmpty(config.ExecutableName))
            {
                Log.Error("Executable name is empty");
                valid = false;
            }

            if (string.IsNullOrEmpty(config.OutputDirectory))
            {
                Log.Error("Output directory is empty");
                valid = false;
            }

            if (config.Platform == "android")
            {
                string manifestPath = ProjectSettings.GlobalizePath(config.Flags["android_manifest"]);
                if (!File.Exists(manifestPath))
                {
                    Log.Error($"Android manifest not found: {manifestPath}");
                    valid = false;
                }

                string keystorePath = ProjectSettings.GlobalizePath(config.Flags["keystore_path"]);
                if (!File.Exists(keystorePath))
                {
                    Log.Warning($"Keystore not found: {keystorePath} - will use default");
                }
            }

            if (config.Platform == "ios")
            {
                if (string.IsNullOrEmpty(config.Flags["provisioning_profile"]))
                {
                    Log.Warning("Provisioning profile not set for iOS");
                }

                if (string.IsNullOrEmpty(config.Flags["signing_identity"]))
                {
                    Log.Warning("Signing identity not set for iOS");
                }
            }

            return valid;
        }

        /// <summary>
        /// 构建项目
        /// </summary>
        /// <param name="config">导出配置</param>
        private void BuildProject(ExportConfig config)
        {
            Log.Info($"Building project for {config.PlatformName} ({config.Architecture})");

            // 模拟构建过程
            // 在实际项目中，这里应该调用Godot的导出命令或使用命令行工具
            
            string buildLog = $"Building {config.ExecutableName}...\n";
            buildLog += $"Target: {config.BuildTarget}\n";
            buildLog += $"Architecture: {config.Architecture}\n";
            buildLog += "Build completed successfully";

            Log.Info(buildLog);
        }

        /// <summary>
        /// 打包桌面平台
        /// </summary>
        /// <param name="config">导出配置</param>
        private void PackageDesktop(ExportConfig config)
        {
            Log.Info($"Packaging desktop build for {config.PlatformName}");

            string outputPath = ProjectSettings.GlobalizePath($"res://{config.OutputDirectory}");
            string executablePath = Path.Combine(outputPath, config.ExecutableName);

            // 模拟打包过程
            // 在实际项目中，这里应该复制必要的文件并创建压缩包

            if (config.Platform == "macos")
            {
                // macOS需要创建.app包结构
                string appPath = Path.Combine(outputPath, config.ExecutableName);
                string contentsPath = Path.Combine(appPath, "Contents");
                string macosPath = Path.Combine(contentsPath, "MacOS");
                
                Directory.CreateDirectory(macosPath);
                // 复制可执行文件到MacOS目录
                Log.Info($"Created macOS app bundle at: {appPath}");
            }
            else
            {
                // Windows/Linux直接复制可执行文件
                Log.Info($"Copied executable to: {executablePath}");
            }
        }

        /// <summary>
        /// 打包移动平台
        /// </summary>
        /// <param name="config">导出配置</param>
        private void PackageMobile(ExportConfig config)
        {
            Log.Info($"Packaging mobile build for {config.PlatformName}");

            string outputPath = ProjectSettings.GlobalizePath($"res://{config.OutputDirectory}");
            string packagePath = Path.Combine(outputPath, config.ExecutableName);

            // 模拟打包过程
            // 在实际项目中，这里应该调用相应的打包工具

            if (config.Platform == "android")
            {
                Log.Info($"Generating APK at: {packagePath}");
            }
            else if (config.Platform == "ios")
            {
                Log.Info($"Generating IPA at: {packagePath}");
            }
        }

        /// <summary>
        /// 更新平台配置
        /// </summary>
        /// <param name="platform">平台ID</param>
        /// <param name="config">新的配置</param>
        /// <returns>更新成功返回true</returns>
        public bool UpdatePlatformConfig(string platform, ExportConfig config)
        {
            if (_exportConfigs.ContainsKey(platform))
            {
                _exportConfigs[platform] = config;
                Log.Info($"Updated config for platform: {platform}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取平台类型
        /// </summary>
        /// <param name="platform">平台ID</param>
        /// <returns>平台类型</returns>
        public PlatformType GetPlatformType(string platform)
        {
            ExportConfig config = GetPlatformConfig(platform);
            return config?.PlatformType ?? PlatformType.Desktop;
        }

        /// <summary>
        /// 检查平台是否可用
        /// </summary>
        /// <param name="platform">平台ID</param>
        /// <returns>可用返回true</returns>
        public bool IsPlatformAvailable(string platform)
        {
            return _exportConfigs.ContainsKey(platform);
        }

        /// <summary>
        /// 获取平台类型列表
        /// </summary>
        /// <returns>平台类型列表</returns>
        public List<PlatformType> GetPlatformTypes()
        {
            return _exportConfigs.Values.Select(c => c.PlatformType).Distinct().ToList();
        }

        /// <summary>
        /// 获取指定类型的平台列表
        /// </summary>
        /// <param name="type">平台类型</param>
        /// <returns>平台ID列表</returns>
        public List<string> GetPlatformsByType(PlatformType type)
        {
            return _exportConfigs.Where(kv => kv.Value.PlatformType == type)
                               .Select(kv => kv.Key)
                               .ToList();
        }
    }

    /// <summary>
    /// 导出配置类
    /// </summary>
    public class ExportConfig
    {
        /// <summary>
        /// 平台标识符
        /// </summary>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// 平台名称（显示用）
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// 平台类型
        /// </summary>
        public PlatformType PlatformType { get; set; } = PlatformType.Desktop;

        /// <summary>
        /// 可执行文件名称
        /// </summary>
        public string ExecutableName { get; set; } = string.Empty;

        /// <summary>
        /// 输出目录（相对于项目根目录）
        /// </summary>
        public string OutputDirectory { get; set; } = string.Empty;

        /// <summary>
        /// 目标架构
        /// </summary>
        public string Architecture { get; set; } = string.Empty;

        /// <summary>
        /// 构建目标（debug/release）
        /// </summary>
        public string BuildTarget { get; set; } = "release";

        /// <summary>
        /// 额外的构建标志
        /// </summary>
        public Dictionary<string, string> Flags { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// 平台类型枚举
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// 桌面平台（Windows、macOS、Linux）
        /// </summary>
        Desktop,

        /// <summary>
        /// 移动平台（Android、iOS）
        /// </summary>
        Mobile,

        /// <summary>
        /// 控制台平台
        /// </summary>
        Console,

        /// <summary>
        /// Web平台
        /// </summary>
        Web
    }
}