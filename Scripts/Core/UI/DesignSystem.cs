using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Core.UI
{
    /// <summary>
    /// Design System Manager
    /// Manages Design Tokens like colors, fonts, spacing, etc.
    /// Supports runtime theme switching and JSON configuration.
    /// </summary>
    public static class DesignSystem
    {
        public static DesignTokens Tokens { get; private set; } = new DesignTokens();

        // Default Tokens
        static DesignSystem()
        {
            // Initialize with defaults
            Tokens.Colors.Add("primary", new Color("3498db"));
            Tokens.Colors.Add("secondary", new Color("2ecc71"));
            Tokens.Colors.Add("danger", new Color("e74c3c"));
            Tokens.Colors.Add("warning", new Color("f1c40f"));
            Tokens.Colors.Add("text_primary", new Color("ecf0f1"));
            Tokens.Colors.Add("text_secondary", new Color("bdc3c7"));
            Tokens.Colors.Add("background", new Color("2c3e50"));
            Tokens.Colors.Add("surface", new Color("34495e"));

            Tokens.Spacing.Add("small", 8f);
            Tokens.Spacing.Add("medium", 16f);
            Tokens.Spacing.Add("large", 24f);

            Tokens.CornerRadius.Add("small", 4);
            Tokens.CornerRadius.Add("medium", 8);
            Tokens.CornerRadius.Add("large", 12);

            Tokens.AnimationDuration.Add("fast", 0.1f);
            Tokens.AnimationDuration.Add("normal", 0.2f);
            Tokens.AnimationDuration.Add("slow", 0.3f);
        }

        public static void LoadTokens(string jsonPath)
        {
            if (!FileAccess.FileExists(jsonPath))
            {
                Log.Warning($"Design Tokens file not found at {jsonPath}, using defaults.");
                return;
            }

            try
            {
                var loadedTokens = JsonHelper.LoadJsonFile<DesignTokens>(jsonPath);
                if (loadedTokens != null)
                {
                    Tokens = loadedTokens;
                    Log.Info($"Loaded Design Tokens from {jsonPath}");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"Failed to load Design Tokens: {e.Message}");
            }
        }

        public static Color GetColor(string key) => Tokens.Colors.ContainsKey(key) ? Tokens.Colors[key] : Colors.Magenta;
        public static float GetSpacing(string key) => Tokens.Spacing.ContainsKey(key) ? Tokens.Spacing[key] : 0f;
        public static int GetCornerRadius(string key) => Tokens.CornerRadius.ContainsKey(key) ? Tokens.CornerRadius[key] : 0;
        public static float GetAnimationDuration(string key) => Tokens.AnimationDuration.ContainsKey(key) ? Tokens.AnimationDuration[key] : 0.2f;
    }

    public class DesignTokens
    {
        public Dictionary<string, Color> Colors { get; set; } = new Dictionary<string, Color>();
        public Dictionary<string, float> Spacing { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> CornerRadius { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, float> AnimationDuration { get; set; } = new Dictionary<string, float>();
        // Fonts would need Resource loading logic
    }
}
