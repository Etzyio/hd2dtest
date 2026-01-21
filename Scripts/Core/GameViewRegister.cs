using System.Collections.Generic;
using Godot;

namespace hd2dtest.Scripts.Core
{
    public class GameViewRegister
    {
        // 场景字典
        private static Dictionary<string, string> Scenes
        {
            get
            {
                if (ResourcesManager.Instance == null)
                {
                    Log.Error("ResourcesManager instance is null!");
                    return [];
                }
                return ResourcesManager.Instance.ViewRegister;
            }
        }

        public static PackedScene GetScene(string sceneName)
        {
            Log.Info($"load[{sceneName}]");

            if (!Scenes.TryGetValue(sceneName, out string scenePath))
            {
                Log.Error($"Scene '{sceneName}' not found in ViewRegister!");
                return null;
            }

            PackedScene packedScene = GD.Load<PackedScene>(scenePath);

            if (packedScene == null)
            {
                Log.Error($"Failed to load scene: {scenePath}");
                return null;
            }

            return packedScene;
        }
    }
}