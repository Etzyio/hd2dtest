using System;
using System.Collections.Generic;
using Godot;

namespace hd2dtest.Scripts.Core
{
    public class GameViewRegister
    {
        // 场景字典（
        private static Dictionary<string, String> Scenes => ResourcesManager.Instance.ViewRegister;

        public static PackedScene GetScene(string sceneName)
        {
            Log.Info($"load[{sceneName}]");
            PackedScene packedScene = GD.Load<PackedScene>(Scenes[sceneName]);
            return packedScene;
        }
    }
}