using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 悬空寺 - 依峭壁而建的奇险寺院，寺中僧人修习独门轻功
    /// </summary>
    public partial class CliffTemple : Node2D
    {
        public override void _Ready()
        {
            Log.Info("CliffTemple scene loaded");
        }
    }
}
