using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 配置管理器，用于管理游戏设置
    /// </summary>
    public partial class ConfigManager : Node
    {
        // 单例实例
        private static ConfigManager _instance;
        private readonly JsonTypeInfo _cachedJsonOptions = JsonSerializerOptions.Default.GetTypeInfo(typeof(ConfigData));

        public static ConfigManager Instance => _instance;

        // 配置文件路径
        private const string CONFIG_FILE_PATH = "user://config.json";

        // 配置数据结构
        public class ConfigData
        {
            // 音量设置
            public float MasterVolume { get; set; } = 1.0f;
            public float MusicVolume { get; set; } = 0.8f;
            public float SoundEffectVolume { get; set; } = 1.0f;
            public float VoiceVolume { get; set; } = 1.0f;

            // 图形设置
            public float Brightness { get; set; } = 1.0f;
            public float Contrast { get; set; } = 1.0f;
            public float Saturation { get; set; } = 1.0f;
            public int ResolutionWidth { get; set; } = 1280;
            public int ResolutionHeight { get; set; } = 720;
            public bool Fullscreen { get; set; } = false;

            // 游戏设置
            public bool AutoSave { get; set; } = true;
            public float TextSpeed { get; set; } = 0.5f;
            public bool ShowFPS { get; set; } = false;
            public bool VSync { get; set; } = true;

            // 控制设置
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
        }

        // 当前配置数据
        public ConfigData CurrentConfig { get; private set; } = new ConfigData();

        // 配置变更事件
        [Signal]
        public delegate void ConfigChangedEventHandler();

        // 私有构造函数，防止外部实例化
        private ConfigManager() { }

        /// <summary>
        /// 公共静态方法，用于获取或创建实例
        /// </summary>
        /// <returns>ConfigManager实例</returns>
        public static ConfigManager GetInstance()
        {
            _instance ??= new ConfigManager();
            return _instance;
        }

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
        /// 加载配置
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
                    // 如果配置文件不存在，使用默认配置
                    CurrentConfig = new ConfigData();
                    SaveConfig();
                    Log.Info("Default config created");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load config: {e.Message}");
                // 使用默认配置
                CurrentConfig = new ConfigData();
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void SaveConfig()
        {
            try
            {
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
        /// 应用配置
        /// </summary>
        public void ApplyConfig()
        {
            // 应用音量设置
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

            // 应用图形设置
            // 亮度、对比度、饱和度设置需要通过着色器实现，这里只做记录

            // 应用分辨率和全屏设置
            DisplayServer.WindowSetSize(new Vector2I(CurrentConfig.ResolutionWidth, CurrentConfig.ResolutionHeight));
            DisplayServer.WindowSetMode(CurrentConfig.Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);

            // 应用垂直同步设置
            DisplayServer.WindowSetVsyncMode(CurrentConfig.VSync ? Godot.DisplayServer.VSyncMode.Enabled : Godot.DisplayServer.VSyncMode.Disabled);

            // 发出配置变更信号
            EmitSignal(SignalName.ConfigChanged);

            Log.Info("Config applied");
        }

        /// <summary>
        /// 线性音量转分贝
        /// </summary>
        /// <param name="linear">线性音量（0-1）</param>
        /// <returns>分贝值</returns>
        private static float LinearToDb(float linear)
        {
            return Mathf.LinearToDb(linear);
        }

        /// <summary>
        /// 重置配置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            CurrentConfig = new ConfigData();
            SaveConfig();
            ApplyConfig();
            Log.Info("Config reset to defaults");
        }

        // 音量设置方法
        public void SetMasterVolume(float volume)
        {
            CurrentConfig.MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            SaveConfig();
            ApplyConfig();
        }

        public void SetMusicVolume(float volume)
        {
            CurrentConfig.MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            SaveConfig();
            ApplyConfig();
        }

        public void SetSoundEffectVolume(float volume)
        {
            CurrentConfig.SoundEffectVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            SaveConfig();
            ApplyConfig();
        }

        public void SetVoiceVolume(float volume)
        {
            CurrentConfig.VoiceVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            SaveConfig();
            ApplyConfig();
        }

        // 图形设置方法
        public void SetBrightness(float brightness)
        {
            CurrentConfig.Brightness = Mathf.Clamp(brightness, 0.0f, 2.0f);
            SaveConfig();
            ApplyConfig();
        }

        public void SetContrast(float contrast)
        {
            CurrentConfig.Contrast = Mathf.Clamp(contrast, 0.0f, 2.0f);
            SaveConfig();
            ApplyConfig();
        }

        public void SetSaturation(float saturation)
        {
            CurrentConfig.Saturation = Mathf.Clamp(saturation, 0.0f, 2.0f);
            SaveConfig();
            ApplyConfig();
        }

        public void SetResolution(int width, int height)
        {
            CurrentConfig.ResolutionWidth = Mathf.Max(800, width);
            CurrentConfig.ResolutionHeight = Mathf.Max(600, height);
            SaveConfig();
            ApplyConfig();
        }

        public void SetFullscreen(bool fullscreen)
        {
            CurrentConfig.Fullscreen = fullscreen;
            SaveConfig();
            ApplyConfig();
        }

        // 游戏设置方法
        public void SetAutoSave(bool autoSave)
        {
            CurrentConfig.AutoSave = autoSave;
            SaveConfig();
        }

        public void SetTextSpeed(float textSpeed)
        {
            CurrentConfig.TextSpeed = Mathf.Clamp(textSpeed, 0.1f, 2.0f);
            SaveConfig();
        }

        public void SetShowFPS(bool showFPS)
        {
            CurrentConfig.ShowFPS = showFPS;
            SaveConfig();
        }

        public void SetVSync(bool vSync)
        {
            CurrentConfig.VSync = vSync;
            SaveConfig();
            ApplyConfig();
        }

        // 控制设置方法
        public void SetKeyBinding(string action, string key)
        {
            if (!CurrentConfig.KeyBindings.TryAdd(action, key))
            {
                CurrentConfig.KeyBindings[action] = key;
            }

            SaveConfig();
        }

        public string GetKeyBinding(string action)
        {
            return CurrentConfig.KeyBindings.TryGetValue(action, out string key) ? key : "";
        }

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
    }
}