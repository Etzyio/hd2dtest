using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Town
{
    /// <summary>
    /// 城镇中心
    /// </summary>
    public partial class Town : Node2D
    {
        public override void _Ready()
        {
            Log.Info("Town scene loaded");
        }
    }
}
