using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 焦土路 - 通往龙门客栈废墟的荒凉之路，沿途焦土遍地
    /// </summary>
    public partial class BurnedRoad : Node2D
    {
        public override void _Ready()
        {
            Log.Info("BurnedRoad transition scene loaded");
        }
    }
}
