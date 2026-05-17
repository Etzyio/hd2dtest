using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 温泉 - 山中天然温泉，浸泡可恢复体力与内伤
    /// </summary>
    public partial class HotSpring : Node2D
    {
        public override void _Ready()
        {
            Log.Info("HotSpring scene loaded");
        }
    }
}
