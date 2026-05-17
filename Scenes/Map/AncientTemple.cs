using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 古寺遗迹 - 荒废已久的古寺，残垣断壁间隐约可见昔日香火之盛。传闻寺中藏有武学秘籍
    /// </summary>
    public partial class AncientTemple : Node2D
    {
        public override void _Ready()
        {
            Log.Info("AncientTemple scene loaded");
        }
    }
}
