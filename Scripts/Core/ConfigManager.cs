using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 配置管理器，用于管理游戏的所有设置
    /// 实现了单例模式，自动绑定到root节点
    /// 负责配置的加载、保存和应用
    /// </summary>
    public partial class ConfigManager : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static ConfigManager _instance;
        
        /// <summary>
        /// 缓存的Json序列化选项，用于提高序列化性能
        /// 通过预先获取TypeInfo，避免每次序列化/反序列化时重复生成
        /// </summary>
        private readonly JsonTypeInfo _cachedJsonOptions = JsonSerializerOptions.Default.GetTypeInfo(typeof(ConfigData));

        public static ConfigManager Instance => _instance;

        // 配置文件路径
        private const string CONFIG_FILE_PATH = "user://config.json";

        /// <summary>
        /// 配置数据结构，包含游戏的所有可配置项
        /// </summary>
        public class ConfigData
        {
            #region 音量设置
            /// <summary>
            /// 主音量，范围0.0-1.0
            /// </summary>
            public float MasterVolume { get; set; } = 1.0f;
            
            /// <summary>
            /// 音乐音量，范围0.0-1.0
            /// </summary>
            public float MusicVolume { get; set; } = 0.8f;
            
            /// <summary>
            /// 音效音量，范围0.0-1.0
            /// </summary>
            public float SoundEffectVolume { get; set; } = 1.0f;
            
            /// <summary>
            /// 语音音量，范围0.0-1.0
            /// </summary>
            public float VoiceVolume { get; set; } = 1.0f;
            #endregion

            #region 图形设置
            /// <summary>
            /// 亮度，范围0.0-2.0
            /// </summary>
            public float Brightness { get; set; } = 1.0f;
            
            /// <summary>
            /// 对比度，范围0.0-2.0
            /// </summary>
            public float Contrast { get; set; } = 1.0f;
            
            /// <summary>
            /// 饱和度，范围0.0-2.0
            /// </summary>
            public float Saturation { get; set; } = 1.0f;
            
            /// <summary>
            /// 分辨率宽度，最小值800
            /// </summary>
            public int ResolutionWidth { get; set; } = 1280;
            
            /// <summary>
            /// 分辨率高度，最小值600
            /// </summary>
            public int ResolutionHeight { get; set; } = 720;
            
            /// <summary>
            /// 是否全屏
            /// </summary>
            public bool Fullscreen { get; set; } = false;
            #endregion

            #region 游戏设置
            /// <summary>
            /// 是否自动保存
            /// </summary>
            public bool AutoSave { get; set; } = true;
            
            /// <summary>
            /// 文本显示速度，范围0.1-2.0
            /// </summary>
            public float TextSpeed { get; set; } = 0.5f;
            
            /// <summary>
            /// 是否显示FPS
            /// </summary>
            public bool ShowFPS { get; set; } = false;
            
            /// <summary>
            /// 是否开启垂直同步
            /// </summary>
            public bool VSync { get; set; } = true;
            #endregion

            #region 控制设置
            /// <summary>
            /// 按键绑定字典，键为动作名称，值为输入事件名称
            /// </summary>
            public Dictionary<string, string> KeyBindings { get; set; } = new Dictionary<string, string>
            {
                { "move_left", "ui_left" },
                { "move_right", "ui_right" },
                { "move_up", "ui_up" },
                { "move_down", "ui_down" },
                { "jump", "ui_accept" },
                { "attack", "ui_focus_next" },
                { "skill1", "ui_focus_prev" },
                { "skill2", "ui_page_up" },
                { "skill3", "ui_page_down" },
                { "inventory", "ui_cancel" },
                { "menu", "ui_home" }
            };
            #endregion
        }

        /// <summary>
        /// 当前配置数据，包含游戏的所有可配置项
        /// 只读属性，通过SetXXX方法修改配置值
        /// </summary>
        public ConfigData CurrentConfig { get; private set; } = new ConfigData();

        /// <summary>
        /// 配置变更信号，当配置发生变化时发出
        /// </summary>
        [Signal]
        public delegate void ConfigChangedEventHandler();

        /// <summary>
        /// 私有构造函数，防止外部实例化
        /// </summary>
        private ConfigManager() { }

        /// <summary>
        /// 公共静态方法，用于获取或创建ConfigManager实例
        /// 使用空合并赋值运算符实现懒加载单例模式
        /// </summary>
        /// <returns>ConfigManager单例实例</returns>
        public static ConfigManager GetInstance()
        {
            _instance ??= new ConfigManager();
            return _instance;
        }

        /// <summary>
        /// 节点就绪时调用
        /// 设置单例实例，加载并应用配置
        /// </summary>
        public override void _Ready()
        {
            // 设置单例实例
            _instance = this;

            // 加载配置
            LoadConfig();

            // 应用配置
            ApplyConfig();
        }

        /// <summary>
        /// 从文件加载配置
        /// 如果文件不存在，则创建默认配置
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                if (Godot.FileAccess.FileExists(CONFIG_FILE_PATH))
                {
                    using (var file = Godot.FileAccess.Open(CONFIG_FILE_PATH, Godot.FileAccess.ModeFlags.Read))
                    {
                        string json = file.GetAsText();
                        CurrentConfig = JsonSerializer.Deserialize<ConfigData>(json) ?? new ConfigData();
                    }
                    Log.Info("Config loaded successfully");
                }
                else
                {
                    // 如果配置文件不存在，使用默认配置并保存
                    CurrentConfig = new ConfigData();
                    SaveConfig();
                    Log.Info("Default config created");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load config: {e.Message}");
                // 加载失败时使用默认配置
                CurrentConfig = new ConfigData();
            }
        }

        /// <summary>
        /// 将当前配置保存到文件
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                // 使用缓存的JsonTypeInfo提高序列化性能
                string json = JsonSerializer.Serialize(CurrentConfig, _cachedJsonOptions);
                using (var file = Godot.FileAccess.Open(CONFIG_FILE_PATH, Godot.FileAccess.ModeFlags.Write))
                {
                    file.StoreString(json);
                }
                Log.Info("Config saved successfully");
            }
            catch (Exception e)
            {
                Log.Error($"Failed to save config: {e.Message}");
            }
        }

        /// <summary>
        /// 应用当前配置到游戏
        /// </summary>
        public void ApplyConfig()
        {
            #region 应用音量设置
            // 只设置Master总线，其他总线可能不存在
            int masterBusIndex = AudioServer.GetBusIndex("Master");
            if (masterBusIndex != -1)
            {
                AudioServer.SetBusVolumeDb(masterBusIndex, LinearToDb(CurrentConfig.MasterVolume));
            }

            // 检查其他总线是否存在，存在则设置
            int musicBusIndex = AudioServer.GetBusIndex("Music");
            if (musicBusIndex != -1)
            {
                AudioServer.SetBusVolumeDb(musicBusIndex, LinearToDb(CurrentConfig.MusicVolume));
            }

            int sfxBusIndex = AudioServer.GetBusIndex("SFX");
            if (sfxBusIndex != -1)
            {
                AudioServer.SetBusVolumeDb(sfxBusIndex, LinearToDb(CurrentConfig.SoundEffectVolume));
            }

            int voiceBusIndex = AudioServer.GetBusIndex("Voice");
            if (voiceBusIndex != -1)
            {
                AudioServer.SetBusVolumeDb(voiceBusIndex, LinearToDb(CurrentConfig.VoiceVolume));
            }
            #endregion

            #region 应用图形设置
            // 亮度、对比度、饱和度设置需要通过着色器实现，这里只做记录
            #endregion

            #region 应用分辨率和全屏设置
            DisplayServer.WindowSetSize(new Vector2I(CurrentConfig.ResolutionWidth, CurrentConfig.ResolutionHeight));
            DisplayServer.WindowSetMode(CurrentConfig.Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
            #endregion

            #region 应用垂直同步设置
            DisplayServer.WindowSetVsyncMode(CurrentConfig.VSync ? Godot.DisplayServer.VSyncMode.Enabled : Godot.DisplayServer.VSyncMode.Disabled);
            #endregion

            // 发出配置变更信号
            EmitSignal(SignalName.ConfigChanged);

            Log.Info("Config applied");
        }

        /// <summary>
        /// 将线性音量转换为分贝值
        /// 线性音量与分贝的转换公式：dB = 20 * log10(linear)
        /// Godot音频系统使用分贝值进行音量控制
        /// </summary>
        /// <param name="linear">线性音量（0-1）</param>
        /// <returns>分贝值（范围：-80dB 到 0dB）</returns>
        private static float LinearToDb(float linear)
        {
            return Mathf.LinearToDb(linear);
        }

        /// <summary>
        /// 重置配置为默认值
        /// 执行流程：
        /// 1. 创建新的ConfigData实例（使用默认值）
        /// 2. 保存配置到文件
        /// 3. 应用配置到游戏
        /// 4. 记录日志
        /// </summary>
        public void ResetToDefaults()
        {
            CurrentConfig = new ConfigData();
            SaveConfig();
            ApplyConfig();
            Log.Info("Config reset to defaults");
        }

        #region 音量设置方法
        /// <summary>
        /// 设置主音量
        /// </summary>
        /// <param name="volume">音量值（0.0-1.0）</param>
        public void SetMasterVolume(float volume)
        {
            CurrentConfig.MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            SaveConfig();
            ApplyConfig();
        }

        /// <summary>
        /// 设置音乐音量
        /// </summary>
        /// <param name="volume">音量值（0.0-1.0）</param>
        public void SetMusicVolume(float volume)
        {
            CurrentConfig.MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            SaveConfig();
            ApplyConfig();
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="volume">音量值（0.0-1.0）</param>
        public void SetSoundEffectVolume(float volume)
        {
            CurrentConfig.SoundEffectVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            SaveConfig();
            ApplyConfig();
        }

        /// <summary>
        /// 设置语音音量
        /// </summary>
        /// <param name="volume">音量值（0.0-1.0）</param>
        public void SetVoiceVolume(float volume)
        {
            CurrentConfig.VoiceVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            SaveConfig();
            ApplyConfig();
        }
        #endregion

        #region 图形设置方法
        /// <summary>
        /// 设置亮度
        /// </summary>
        /// <param name="brightness">亮度值（0.0-2.0）</param>
        public void SetBrightness(float brightness)
        {
            CurrentConfig.Brightness = Mathf.Clamp(brightness, 0.0f, 2.0f);
            SaveConfig();
            ApplyConfig();
        }

        /// <summary>
        /// 设置对比度
        /// </summary>
        /// <param name="contrast">对比度值（0.0-2.0）</param>
        public void SetContrast(float contrast)
        {
            CurrentConfig.Contrast = Mathf.Clamp(contrast, 0.0f, 2.0f);
            SaveConfig();
            ApplyConfig();
        }

        /// <summary>
        /// 设置饱和度
        /// </summary>
        /// <param name="saturation">饱和度值（0.0-2.0）</param>
        public void SetSaturation(float saturation)
        {
            CurrentConfig.Saturation = Mathf.Clamp(saturation, 0.0f, 2.0f);
            SaveConfig();
            ApplyConfig();
        }

        /// <summary>
        /// 设置分辨率
        /// </summary>
        /// <param name="width">宽度（最小值800）</param>
        /// <param name="height">高度（最小值600）</param>
        public void SetResolution(int width, int height)
        {
            CurrentConfig.ResolutionWidth = Mathf.Max(800, width);
            CurrentConfig.ResolutionHeight = Mathf.Max(600, height);
            SaveConfig();
            ApplyConfig();
        }

        /// <summary>
        /// 设置是否全屏
        /// </summary>
        /// <param name="fullscreen">是否全屏</param>
        public void SetFullscreen(bool fullscreen)
        {
            CurrentConfig.Fullscreen = fullscreen;
            SaveConfig();
            ApplyConfig();
        }
        #endregion

        #region 游戏设置方法
        /// <summary>
        /// 设置是否自动保存
        /// </summary>
        /// <param name="autoSave">是否自动保存</param>
        public void SetAutoSave(bool autoSave)
        {
            CurrentConfig.AutoSave = autoSave;
            SaveConfig();
        }

        /// <summary>
        /// 设置文本显示速度
        /// </summary>
        /// <param name="textSpeed">文本速度（0.1-2.0）</param>
        public void SetTextSpeed(float textSpeed)
        {
            CurrentConfig.TextSpeed = Mathf.Clamp(textSpeed, 0.1f, 2.0f);
            SaveConfig();
        }

        /// <summary>
        /// 设置是否显示FPS
        /// </summary>
        /// <param name="showFPS">是否显示FPS</param>
        public void SetShowFPS(bool showFPS)
        {
            CurrentConfig.ShowFPS = showFPS;
            SaveConfig();
        }

        /// <summary>
        /// 设置是否开启垂直同步
        /// </summary>
        /// <param name="vSync">是否开启垂直同步</param>
        public void SetVSync(bool vSync)
        {
            CurrentConfig.VSync = vSync;
            SaveConfig();
            ApplyConfig();
        }
        #endregion

        #region 控制设置方法
        /// <summary>
        /// 设置按键绑定
        /// </summary>
        /// <param name="action">动作名称</param>
        /// <param name="key">输入事件名称</param>
        public void SetKeyBinding(string action, string key)
        {
            if (!CurrentConfig.KeyBindings.TryAdd(action, key))
            {
                CurrentConfig.KeyBindings[action] = key;
            }

            SaveConfig();
        }

        /// <summary>
        /// 获取按键绑定
        /// </summary>
        /// <param name="action">动作名称</param>
        /// <returns>输入事件名称</returns>
        public string GetKeyBinding(string action)
        {
            return CurrentConfig.KeyBindings.TryGetValue(action, out string key) ? key : "";
        }

        /// <summary>
        /// 重置按键绑定为默认值
        /// </summary>
        public void ResetKeyBindings()
        {
            CurrentConfig.KeyBindings = new Dictionary<string, string>
            {
                { "move_left", "ui_left" },
                { "move_right", "ui_right" },
                { "move_up", "ui_up" },
                { "move_down", "ui_down" },
                { "jump", "ui_accept" },
                { "attack", "ui_focus_next" },
                { "skill1", "ui_focus_prev" },
                { "skill2", "ui_page_up" },
                { "skill3", "ui_page_down" },
                { "inventory", "ui_cancel" },
                { "menu", "ui_home" }
            };
            SaveConfig();
        }
        #endregion
    }
}