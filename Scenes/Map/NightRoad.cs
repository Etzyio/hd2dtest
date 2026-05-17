using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 夜路 - 月光下的夜行道路，危机四伏
    /// </summary>
    public partial class NightRoad : Node2D
    {
        public override void _Ready()
        {
            Log.Info("NightRoad transition scene loaded");
        }
    }
}
