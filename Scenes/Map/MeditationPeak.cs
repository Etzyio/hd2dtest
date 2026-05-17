using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 悟道峰 - 高山之巅的修炼圣地，在此打坐可提升内力修为
    /// </summary>
    public partial class MeditationPeak : Node2D
    {
        public override void _Ready()
        {
            Log.Info("MeditationPeak scene loaded");
        }
    }
}
