using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 废弃矿坑 - 曾经盛产铁矿的矿洞，废弃后被山贼占据为窝点
    /// </summary>
    public partial class AbandonedMine : Node2D
    {
        public override void _Ready()
        {
            Log.Info("AbandonedMine scene loaded");
        }
    }
}
