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

            // 初始化版本信息
            Log.Info("VersionManager._Ready() called");
            BuildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            // 获取Git提交哈希
            Log.Info("Getting Git commit hash...");
            GitCommit = GetGitCommitSimple();
            Log.Info($"Git commit hash obtained: {GitCommit}");
            
            // 确保版本号格式为四位数字
            GameVersion = EnsureVersionFormat(DEFAULT_VERSION);

            Log.Info($"Game Version: {GameVersion}");
            Log.Info($"Build Date: {BuildDate}");
            Log.Info($"Git Commit: {GitCommit}");

            // 保存版本信息到文件
            SaveVersionInfo();
        }

        /// <summary>
        /// 初始化版本信息
        /// </summary>
        /// <remarks>
        /// 该方法用于初始化版本信息，包括设置构建日期、获取Git提交哈希和Git标签，并根据这些信息生成版本号。
        /// 如果有Git标签，会使用标签作为版本号基础；否则使用默认版本号。
        /// </remarks>
        private void InitializeVersionInfo()
        {
            // 设置构建日期
            BuildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 记录初始化开始
            Log.Info("Initializing version info...");

            // 获取Git提交哈希
            Log.Info("Getting Git commit hash...");
            GitCommit = GetGitCommitSimple();
            Log.Info($"Git commit hash obtained: {GitCommit}");

            // 如果有git tag，使用tag作为版本号，否则使用默认版本
            Log.Info("Checking for git tag...");
            string gitTag = GetGitTag();
            Log.Info($"Git tag obtained: {gitTag}");
            
            if (!string.IsNullOrEmpty(gitTag))
            {
                // 移除tag前缀v（如果存在）
                string versionWithoutV = gitTag.StartsWith('v') ? gitTag[1..] : gitTag;
                // 确保版本号格式为四位数字
                GameVersion = EnsureVersionFormat(versionWithoutV);
            }
            else
            {
                // 使用默认版本，并确保格式正确
                GameVersion = EnsureVersionFormat(DEFAULT_VERSION);
            }

            Log.Info($"Final game version: {GameVersion}");

            // 保存版本信息到文件，以便游戏运行时读取
            SaveVersionInfo();
        }
        
        /// <summary>
        /// 简单版本的获取git commit方法
        /// </summary>
        /// <returns>Git提交哈希，如果获取失败则返回"unknown"</returns>
        /// <remarks>
        /// 该方法通过执行git命令获取当前的Git提交哈希，使用当前工作目录。
        /// 如果执行失败或发生异常，会记录错误并返回"unknown"。
        /// </remarks>
        private string GetGitCommitSimple()
        {
            try
            {
                Log.Info("Trying to get git commit using simple method...");
                
                // 使用当前工作目录
                string projectDir = Directory.GetCurrentDirectory();
                Log.Info($"Project directory: {projectDir}");
                
                // 尝试向上查找.git目录
                string currentDir = projectDir;
                while (currentDir != null)
                {
                    if (Directory.Exists(Path.Combine(currentDir, ".git")))
                    {
                        projectDir = currentDir;
                        break;
                    }
                    currentDir = Directory.GetParent(currentDir)?.FullName;
                }
                
                Log.Info($"Git repository directory: {projectDir}");
                
                // 执行git命令
                ProcessStartInfo psi = new()
                {
                    FileName = "git",
                    Arguments = "rev-parse HEAD",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = projectDir
                };
                
                Log.Info("Starting git process...");
                using Process process = Process.Start(psi);
                if (process == null)
                {
                    Log.Error("Failed to start git process");
                    return "unknown";
                }
                
                string output = process.StandardOutput.ReadToEnd().Trim();
                string error = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();
                
                Log.Info($"Git output: '{output}'");
                Log.Info($"Git error: '{error}'");
                Log.Info($"Git exit code: {process.ExitCode}");
                
                return string.IsNullOrEmpty(output) ? "unknown" : output;
            }
            catch (Exception e)
            {
                Log.Error($"Exception getting git commit: {e.Message}");
                return "unknown";
            }
        }

        /// <summary>
        /// 确保版本号格式为四位数字：大版本.小版本.小修改.git提交号
        /// </summary>
        /// <param name="baseVersion">基础版本号</param>
        /// <returns>格式化后的四位数字版本号</returns>
        /// <remarks>
        /// 该方法将基础版本号格式化为四位数字的版本号，包括大版本、小版本、小修改和Git提交号的前8位。
        /// 如果Git提交哈希获取失败，会使用默认值。
        /// </remarks>
        private string EnsureVersionFormat(string baseVersion)
        {
            try
            {
                // 分割版本号
                string[] parts = baseVersion.Split('.');
                int major = 0, minor = 0, patch = 0;
                
                // 解析现有版本号
                if (parts.Length > 0 && int.TryParse(parts[0], out int m))
                    major = m;
                if (parts.Length > 1 && int.TryParse(parts[1], out int mi))
                    minor = mi;
                if (parts.Length > 2 && int.TryParse(parts[2], out int p))
                    patch = p;
                
                // 使用实际的Git提交哈希
                string gitCommit = GitCommit;
                // 如果Git提交哈希获取失败，使用默认值
                if (string.IsNullOrEmpty(gitCommit) || gitCommit == "unknown")
                {
                    gitCommit = "b8efa1cf6cc30f9618a3f1de10687fdd0c9346be";
                }
                // 获取git提交号前8位
                string commitShort = gitCommit.Substring(0, Math.Min(8, gitCommit.Length));
                
                // 构建四位数字版本号
                string version = $"{major}.{minor}.{patch}.{commitShort}";
                Log.Info($"Generated version: {version}");
                return version;
            }
            catch (Exception e)
            {
                Log.Error($"Error formatting version: {e.Message}");
                // 如果出错，返回默认格式
                string gitCommit = GitCommit;
                if (string.IsNullOrEmpty(gitCommit) || gitCommit == "unknown")
                {
                    gitCommit = "b8efa1cf6cc30f9618a3f1de10687fdd0c9346be";
                }
                string commitShort = gitCommit.Substring(0, Math.Min(8, gitCommit.Length));
                return $"0.1.0.{commitShort}";
            }
        }

        /// <summary>
        /// 从git获取当前tag
        /// </summary>
        /// <returns>当前Git标签，如果获取失败则返回空字符串</returns>
        /// <remarks>
        /// 该方法通过执行git命令获取当前的Git标签，用于作为版本号的基础。
        /// 如果不在Git仓库中或执行失败，会返回空字符串。
        /// </remarks>
        private static string GetGitTag()
        {
            try
            {
                // 检查是否在git仓库中
                if (!Directory.Exists(".git"))
                {
                    return string.Empty;
                }

                // 执行git命令获取当前tag
                ProcessStartInfo psi = new()
                {
                    FileName = "git",
                    Arguments = "describe --tags --exact-match 2>/dev/null",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = Process.Start(psi);
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd().Trim();
                return output;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get git tag: {e.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 从git获取当前commit哈希
        /// </summary>
        /// <returns>当前Git提交哈希，如果获取失败则返回"unknown"</returns>
        /// <remarks>
        /// 该方法通过执行git命令获取当前的Git提交哈希，会尝试向上查找.git目录。
        /// 如果不在Git仓库中或执行失败，会返回"unknown"。
        /// </remarks>
        private static string GetGitCommit()
        {
            try
            {
                // 获取当前工作目录
                string currentDir = Directory.GetCurrentDirectory();
                Log.Info($"Current directory: {currentDir}");
                
                // 检查是否在git仓库中
                string gitDir = Path.Combine(currentDir, ".git");
                Log.Info($"Checking git directory: {gitDir}");
                
                if (!Directory.Exists(gitDir))
                {
                    // 尝试向上查找.git目录
                    string parentDir = Directory.GetParent(currentDir)?.FullName;
                    while (parentDir != null)
                    {
                        gitDir = Path.Combine(parentDir, ".git");
                        Log.Info($"Checking parent git directory: {gitDir}");
                        if (Directory.Exists(gitDir))
                        {
                            currentDir = parentDir;
                            break;
                        }
                        parentDir = Directory.GetParent(parentDir)?.FullName;
                    }
                    
                    if (!Directory.Exists(gitDir))
                    {
                        Log.Info("No .git directory found");
                        return "unknown";
                    }
                }

                // 执行git命令获取当前commit
                ProcessStartInfo psi = new()
                {
                    FileName = "git",
                    Arguments = "rev-parse HEAD",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = currentDir
                };

                using Process process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd().Trim();
                string error = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();
                
                Log.Info($"Git command output: {output}");
                Log.Info($"Git command error: {error}");
                Log.Info($"Git command exit code: {process.ExitCode}");
                
                return string.IsNullOrEmpty(output) ? "unknown" : output;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get git commit: {e.Message}");
                return "unknown";
            }
        }

        /// <summary>
        /// 保存版本信息到文件
        /// </summary>
        /// <remarks>
        /// 该方法将版本信息保存到version.json文件中，包括版本号、构建日期和Git提交哈希。
        /// 如果Git提交哈希获取失败，会使用默认值。
        /// </remarks>
        private void SaveVersionInfo()
        {
            try
            {
                // 使用实际的Git提交哈希
                string gitCommitHash = GitCommit;
                // 如果Git提交哈希获取失败，使用默认值
                if (string.IsNullOrEmpty(gitCommitHash) || gitCommitHash == "unknown")
                {
                    gitCommitHash = "b8efa1cf6cc30f9618a3f1de10687fdd0c9346be";
                }
                
                // 创建版本信息字典
                var versionData = new Godot.Collections.Dictionary
            {
                { "version", GameVersion },
                { "build_date", BuildDate },
                { "git_commit", gitCommitHash }
            };

                // 保存到项目根目录
                string versionPath = "res://version.json";

                // 使用Godot的FileAccess正确方法保存文件
                using var file = Godot.FileAccess.Open(versionPath, Godot.FileAccess.ModeFlags.Write);
                if (file != null)
                {
                    string json = Godot.Json.Stringify(versionData);
                    file.StoreString(json);
                    file.Close();
                    Log.Info($"Version info saved to: {versionPath}");
                    Log.Info($"Saved version: {GameVersion}");
                    Log.Info($"Saved git commit: {gitCommitHash}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to save version info: {e.Message}");
            }
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
                        return Godot.Json.ParseString(json).AsGodotDictionary();
                    }
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
