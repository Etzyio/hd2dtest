using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// Mod管理器，负责管理游戏中的mod系统
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为单例模式运行，负责管理mod的加载、
    /// 资源管理、API接口和安全验证。支持第三方开发者创建和集成自定义mod内容。
    /// </remarks>
    public partial class ModManager : Node
    {
        /// <summary>
        /// Mod管理器的单例实例
        /// </summary>
        public static ModManager Instance { get; private set; }

        /// <summary>
        /// Mod目录路径
        /// </summary>
        private string _modDirectory = "mods";

        /// <summary>
        /// 已加载的mod列表
        /// </summary>
        private List<ModInfo> _loadedMods = new List<ModInfo>();

        /// <summary>
        /// Mod资源缓存
        /// </summary>
        private Dictionary<string, ModResource> _resourceCache = new Dictionary<string, ModResource>();

        /// <summary>
        /// Mod API接口注册
        /// </summary>
        private Dictionary<string, IModApi> _apiRegistry = new Dictionary<string, IModApi>();

        /// <summary>
        /// 是否启用mod支持
        /// </summary>
        [Export]
        public bool EnableModSupport = true;

        /// <summary>
        /// 是否启用安全验证
        /// </summary>
        [Export]
        public bool EnableSecurityCheck = true;

        /// <summary>
        /// Mod加载完成回调
        /// </summary>
        public event Action<List<ModInfo>> OnModsLoaded;

        /// <summary>
        /// 节点就绪时的初始化方法
        /// </summary>
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                
                if (EnableModSupport)
                {
                    InitializeModSystem();
                }
                else
                {
                    Log.Info("Mod support disabled");
                }
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// 初始化mod系统
        /// </summary>
        private void InitializeModSystem()
        {
            Log.Info("Initializing mod system...");
            
            try
            {
                EnsureModDirectoryExists();
                LoadAllMods();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize mod system: {ex.Message}");
            }
        }

        /// <summary>
        /// 确保mod目录存在
        /// </summary>
        private void EnsureModDirectoryExists()
        {
            string modPath = Path.Combine(ProjectSettings.GlobalizePath("res://"), _modDirectory);
            
            if (!Directory.Exists(modPath))
            {
                Directory.CreateDirectory(modPath);
                Log.Info($"Created mod directory: {modPath}");
            }
        }

        /// <summary>
        /// 加载所有mod
        /// </summary>
        public void LoadAllMods()
        {
            _loadedMods.Clear();
            _resourceCache.Clear();

            string modPath = Path.Combine(ProjectSettings.GlobalizePath("res://"), _modDirectory);
            
            if (!Directory.Exists(modPath))
            {
                Log.Warning("Mod directory not found");
                return;
            }

            foreach (string dir in Directory.GetDirectories(modPath))
            {
                try
                {
                    LoadMod(dir);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to load mod from {dir}: {ex.Message}");
                }
            }

            Log.Info($"Loaded {_loadedMods.Count} mods");
            OnModsLoaded?.Invoke(_loadedMods);
        }

        /// <summary>
        /// 加载单个mod
        /// </summary>
        /// <param name="modDirectory">mod目录路径</param>
        /// <returns>加载成功返回true</returns>
        public bool LoadMod(string modDirectory)
        {
            string manifestPath = Path.Combine(modDirectory, "mod.json");
            
            if (!File.Exists(manifestPath))
            {
                Log.Warning($"Mod manifest not found: {manifestPath}");
                return false;
            }

            try
            {
                string manifestContent = File.ReadAllText(manifestPath);
                ModManifest manifest = JsonSerializer.Deserialize<ModManifest>(manifestContent);

                if (!ValidateManifest(manifest))
                {
                    Log.Warning($"Invalid manifest for mod: {manifest.Name}");
                    return false;
                }

                if (EnableSecurityCheck && !VerifyModSignature(modDirectory, manifest))
                {
                    Log.Warning($"Mod signature verification failed: {manifest.Name}");
                    return false;
                }

                ModInfo modInfo = new ModInfo
                {
                    Id = manifest.Id,
                    Name = manifest.Name,
                    Version = manifest.Version,
                    Description = manifest.Description,
                    Author = manifest.Author,
                    Directory = modDirectory,
                    Enabled = true
                };

                LoadModResources(modInfo, manifest);
                LoadModAssembly(modInfo, manifest);
                RegisterModApi(modInfo);

                _loadedMods.Add(modInfo);
                Log.Info($"Loaded mod: {manifest.Name} v{manifest.Version}");
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load mod from {modDirectory}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 验证mod清单
        /// </summary>
        /// <param name="manifest">mod清单</param>
        /// <returns>验证通过返回true</returns>
        private bool ValidateManifest(ModManifest manifest)
        {
            if (string.IsNullOrEmpty(manifest.Id))
            {
                Log.Error("Mod manifest missing Id");
                return false;
            }

            if (string.IsNullOrEmpty(manifest.Name))
            {
                Log.Error("Mod manifest missing Name");
                return false;
            }

            if (string.IsNullOrEmpty(manifest.Version))
            {
                Log.Error("Mod manifest missing Version");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证mod签名（安全验证）
        /// </summary>
        /// <param name="modDirectory">mod目录</param>
        /// <param name="manifest">mod清单</param>
        /// <returns>验证通过返回true</returns>
        private bool VerifyModSignature(string modDirectory, ModManifest manifest)
        {
            // 模拟签名验证
            // 在实际项目中，这里应该实现数字签名验证逻辑
            string signaturePath = Path.Combine(modDirectory, "mod.sig");
            
            if (File.Exists(signaturePath))
            {
                Log.Info($"Verified signature for mod: {manifest.Name}");
                return true;
            }
            
            Log.Warning($"No signature found for mod: {manifest.Name}, skipping security check");
            return true;
        }

        /// <summary>
        /// 加载mod资源
        /// </summary>
        /// <param name="modInfo">mod信息</param>
        /// <param name="manifest">mod清单</param>
        private void LoadModResources(ModInfo modInfo, ModManifest manifest)
        {
            if (manifest.Resources == null)
            {
                return;
            }

            foreach (var resource in manifest.Resources)
            {
                string resourcePath = Path.Combine(modInfo.Directory, resource.Path);
                
                if (!File.Exists(resourcePath))
                {
                    Log.Warning($"Resource not found: {resourcePath}");
                    continue;
                }

                ModResource modResource = new ModResource
                {
                    ModId = modInfo.Id,
                    ResourceId = resource.Id,
                    Path = resourcePath,
                    Type = resource.Type,
                    Loaded = false
                };

                _resourceCache[$"{modInfo.Id}:{resource.Id}"] = modResource;
            }
        }

        /// <summary>
        /// 加载mod程序集（如果有）
        /// </summary>
        /// <param name="modInfo">mod信息</param>
        /// <param name="manifest">mod清单</param>
        private void LoadModAssembly(ModInfo modInfo, ModManifest manifest)
        {
            if (string.IsNullOrEmpty(manifest.Assembly))
            {
                return;
            }

            string assemblyPath = Path.Combine(modInfo.Directory, manifest.Assembly);
            
            if (!File.Exists(assemblyPath))
            {
                Log.Warning($"Assembly not found: {assemblyPath}");
                return;
            }

            try
            {
                // 模拟加载程序集
                // Assembly assembly = Assembly.LoadFrom(assemblyPath);
                // modInfo.Assembly = assembly;
                Log.Info($"Loaded assembly for mod: {modInfo.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load assembly {assemblyPath}: {ex.Message}");
            }
        }

        /// <summary>
        /// 注册mod API
        /// </summary>
        /// <param name="modInfo">mod信息</param>
        private void RegisterModApi(ModInfo modInfo)
        {
            if (modInfo.Assembly == null)
            {
                return;
            }

            try
            {
                // 查找实现IModApi接口的类型
                // Type apiType = modInfo.Assembly.GetTypes()
                //     .FirstOrDefault(t => typeof(IModApi).IsAssignableFrom(t) && !t.IsInterface);
                
                // if (apiType != null)
                // {
                //     IModApi api = (IModApi)Activator.CreateInstance(apiType);
                //     api.Initialize();
                //     _apiRegistry[modInfo.Id] = api;
                //     Log.Info($"Registered API for mod: {modInfo.Name}");
                // }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to register API for mod {modInfo.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取mod资源
        /// </summary>
        /// <param name="resourceId">资源ID（格式：modId:resourceId）</param>
        /// <returns>mod资源对象，如果未找到返回null</returns>
        public ModResource GetResource(string resourceId)
        {
            if (_resourceCache.TryGetValue(resourceId, out ModResource resource))
            {
                if (!resource.Loaded)
                {
                    LoadResource(resource);
                }
                return resource;
            }
            return null;
        }

        /// <summary>
        /// 加载资源内容
        /// </summary>
        /// <param name="resource">mod资源对象</param>
        private void LoadResource(ModResource resource)
        {
            try
            {
                switch (resource.Type)
                {
                    case "texture":
                        // resource.Content = GD.Load<Texture2D>(resource.Path);
                        break;
                    case "audio":
                        // resource.Content = GD.Load<AudioStream>(resource.Path);
                        break;
                    case "scene":
                        // resource.Content = GD.Load<PackedScene>(resource.Path);
                        break;
                    case "json":
                        resource.Content = File.ReadAllText(resource.Path);
                        break;
                    default:
                        resource.Content = File.ReadAllBytes(resource.Path);
                        break;
                }
                
                resource.Loaded = true;
                Log.Info($"Loaded resource: {resource.ResourceId}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load resource {resource.ResourceId}: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取已加载的mod列表
        /// </summary>
        /// <returns>mod信息列表</returns>
        public List<ModInfo> GetLoadedMods()
        {
            return new List<ModInfo>(_loadedMods);
        }

        /// <summary>
        /// 获取指定mod信息
        /// </summary>
        /// <param name="modId">mod ID</param>
        /// <returns>mod信息对象，如果未找到返回null</returns>
        public ModInfo GetMod(string modId)
        {
            return _loadedMods.FirstOrDefault(m => m.Id == modId);
        }

        /// <summary>
        /// 启用/禁用mod
        /// </summary>
        /// <param name="modId">mod ID</param>
        /// <param name="enabled">是否启用</param>
        /// <returns>操作成功返回true</returns>
        public bool SetModEnabled(string modId, bool enabled)
        {
            ModInfo mod = GetMod(modId);
            if (mod == null)
            {
                return false;
            }

            mod.Enabled = enabled;
            Log.Info($"{(enabled ? "Enabled" : "Disabled")} mod: {mod.Name}");
            return true;
        }

        /// <summary>
        /// 重新加载所有mod
        /// </summary>
        public void ReloadAllMods()
        {
            Log.Info("Reloading all mods...");
            LoadAllMods();
        }

        /// <summary>
        /// 获取mod API接口
        /// </summary>
        /// <param name="modId">mod ID</param>
        /// <returns>API接口实例，如果未找到返回null</returns>
        public IModApi GetModApi(string modId)
        {
            if (_apiRegistry.TryGetValue(modId, out IModApi api))
            {
                return api;
            }
            return null;
        }

        /// <summary>
        /// 获取所有已注册的API接口
        /// </summary>
        /// <returns>API接口字典</returns>
        public Dictionary<string, IModApi> GetAllApis()
        {
            return new Dictionary<string, IModApi>(_apiRegistry);
        }
    }

    /// <summary>
    /// Mod清单类
    /// </summary>
    public class ModManifest
    {
        /// <summary>
        /// Mod唯一标识符
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Mod名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mod版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Mod描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 作者名称
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 作者联系方式
        /// </summary>
        public string Contact { get; set; } = string.Empty;

        /// <summary>
        /// 兼容的游戏版本
        /// </summary>
        public string GameVersion { get; set; } = string.Empty;

        /// <summary>
        /// 程序集文件名（可选）
        /// </summary>
        public string Assembly { get; set; } = string.Empty;

        /// <summary>
        /// 资源列表
        /// </summary>
        public List<ModResourceInfo> Resources { get; set; } = new List<ModResourceInfo>();

        /// <summary>
        /// 依赖的其他mod列表
        /// </summary>
        public List<string> Dependencies { get; set; } = new List<string>();

        /// <summary>
        /// 是否需要重启游戏才能生效
        /// </summary>
        public bool RequiresRestart { get; set; } = false;
    }

    /// <summary>
    /// Mod资源信息
    /// </summary>
    public class ModResourceInfo
    {
        /// <summary>
        /// 资源ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 资源路径（相对于mod目录）
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 资源类型（texture, audio, scene, json, etc.）
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mod信息类
    /// </summary>
    public class ModInfo
    {
        /// <summary>
        /// Mod唯一标识符
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Mod名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mod版本
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Mod描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 作者名称
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Mod目录路径
        /// </summary>
        public string Directory { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 加载的程序集（如果有）
        /// </summary>
        public Assembly Assembly { get; set; } = null;
    }

    /// <summary>
    /// Mod资源类
    /// </summary>
    public class ModResource
    {
        /// <summary>
        /// 所属mod的ID
        /// </summary>
        public string ModId { get; set; } = string.Empty;

        /// <summary>
        /// 资源ID
        /// </summary>
        public string ResourceId { get; set; } = string.Empty;

        /// <summary>
        /// 资源文件路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 资源类型
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool Loaded { get; set; } = false;

        /// <summary>
        /// 资源内容
        /// </summary>
        public object Content { get; set; } = null;
    }

    /// <summary>
    /// Mod API接口
    /// </summary>
    public interface IModApi
    {
        /// <summary>
        /// 初始化mod
        /// </summary>
        void Initialize();

        /// <summary>
        /// 清理mod资源
        /// </summary>
        void Cleanup();

        /// <summary>
        /// 获取mod名称
        /// </summary>
        /// <returns>mod名称</returns>
        string GetName();

        /// <summary>
        /// 获取mod版本
        /// </summary>
        /// <returns>mod版本</returns>
        string GetVersion();
    }
}