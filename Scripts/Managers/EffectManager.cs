using Godot;
using hd2dtest.Scripts.Utilities;
using System;

namespace hd2dtest.Scripts.Managers
{
    public partial class EffectManager : Node
    {
        public static EffectManager Instance { get; private set; }

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                Log.Info("EffectManager initialized");
            }
            else
            {
                Log.Warning("EffectManager instance already exists");
                QueueFree();
            }
        }

        public void SpawnEffect(string effectName, Vector3 position)
        {
            Log.Info($"Spawning effect: {effectName} at {position}");
        }

        public void PlayScreenEffect(string effectName)
        {
            Log.Info($"Playing screen effect: {effectName}");
        }
    }
}