using Godot;
using System;
using System.Diagnostics;
using System.IO;

namespace hd2dtest.Scripts.Core
{
    public partial class VersionManager : Node
{
    // 单例实例
    private static VersionManager _instance;
    public static VersionManager Instance => _instance;
    
    // 默认版本号
    private const string DEFAULT_VERSION = "0.0.1";
    
    // 版本信息
    public string GameVersion { get; private set; }
    public string BuildDate { get; private set; }
    public string GitCommit { get; private set; }
    
    public override void _Ready()
    {
        // 设置单例实例
        _instance = this;
        
        // 初始化版本信息
        InitializeVersionInfo();
        
        Log.Info($"Game Version: {GameVersion}");
        Log.Info($"Build Date: {BuildDate}");
        Log.Info($"Git Commit: {GitCommit}");
    }
    
    private void InitializeVersionInfo()
    {
        // 设置构建日期
        BuildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 尝试从git获取版本信息
        string gitTag = GetGitTag();
        GitCommit = GetGitCommit();
        
        // 如果有git tag，使用tag作为版本号，否则使用默认版本
        if (!string.IsNullOrEmpty(gitTag))
        {
            // 移除tag前缀v（如果存在）
            GameVersion = gitTag.StartsWith("v") ? gitTag.Substring(1) : gitTag;
        }
        else
        {
            GameVersion = DEFAULT_VERSION;
        }
        
        // 保存版本信息到文件，以便游戏运行时读取
        SaveVersionInfo();
    }
    
    // 从git获取当前tag
    private string GetGitTag()
    {
        try
        {
            // 检查是否在git仓库中
            if (!Directory.Exists(".git"))
            {
                return string.Empty;
            }
            
            // 执行git命令获取当前tag
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "describe --tags --exact-match 2>/dev/null",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd().Trim();
                return output;
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to get git tag: {e.Message}");
            return string.Empty;
        }
    }
    
    // 从git获取当前commit哈希
    private string GetGitCommit()
    {
        try
        {
            // 检查是否在git仓库中
            if (!Directory.Exists(".git"))
            {
                return "unknown";
            }
            
            // 执行git命令获取当前commit
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --short HEAD 2>/dev/null",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd().Trim();
                return string.IsNullOrEmpty(output) ? "unknown" : output;
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to get git commit: {e.Message}");
            return "unknown";
        }
    }
    
    // 保存版本信息到文件
    private void SaveVersionInfo()
    {
        try
        {
            // 创建版本信息字典
            var versionData = new Godot.Collections.Dictionary
            {
                { "version", GameVersion },
                { "build_date", BuildDate },
                { "git_commit", GitCommit }
            };
            
            // 保存到用户目录
            string versionPath = "user://version.json";
            
            // 使用Godot的FileAccess正确方法保存文件
            using var file = Godot.FileAccess.Open(versionPath, Godot.FileAccess.ModeFlags.Write);
            if (file != null)
            {
                string json = Godot.Json.Stringify(versionData);
                file.StoreString(json);
                file.Close();
                Log.Info($"Version info saved to: {versionPath}");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to save version info: {e.Message}");
        }
    }
    
    // 加载版本信息
    public Godot.Collections.Dictionary LoadVersionInfo()
    {
        try
        {
            string versionPath = "user://version.json";
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
    
    // 获取版本字符串（用于显示）
    public string GetVersionString()
    {
        return $"v{GameVersion} ({GitCommit}) - {BuildDate}";
    }
    }
}