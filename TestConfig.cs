using Godot;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Core
{
    public partial class TestConfig : Node
{
    public override void _Ready()
    {
        // 获取ConfigManager实例
        ConfigManager configManager = ConfigManager.Instance;
        
        // 输出当前配置信息
        Log.Info("=== Current Config Info ===");
        Log.Info($"Master Volume: {configManager.CurrentConfig.MasterVolume}");
        Log.Info($"Music Volume: {configManager.CurrentConfig.MusicVolume}");
        Log.Info($"Sound Effect Volume: {configManager.CurrentConfig.SoundEffectVolume}");
        Log.Info($"Voice Volume: {configManager.CurrentConfig.VoiceVolume}");
        Log.Info($"Brightness: {configManager.CurrentConfig.Brightness}");
        Log.Info($"Resolution: {configManager.CurrentConfig.ResolutionWidth}x{configManager.CurrentConfig.ResolutionHeight}");
        Log.Info($"Fullscreen: {configManager.CurrentConfig.Fullscreen}");
        Log.Info($"VSync: {configManager.CurrentConfig.VSync}");
        
        // 修改一些配置
        configManager.SetMasterVolume(0.8f);
        configManager.SetMusicVolume(0.6f);
        configManager.SetBrightness(1.2f);
        
        Log.Info("\n=== Config Updated ===");
        Log.Info($"Master Volume: {configManager.CurrentConfig.MasterVolume}");
        Log.Info($"Music Volume: {configManager.CurrentConfig.MusicVolume}");
        Log.Info($"Brightness: {configManager.CurrentConfig.Brightness}");
        
        Log.Info("\nConfigManager test completed successfully!");
    }
}
}