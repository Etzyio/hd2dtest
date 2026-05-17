using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 赌坊 - 江湖豪客一掷千金之地，运气好可赢取稀有物品
    /// </summary>
    public partial class GamblingDen : Node2D
    {
        public override void _Ready()
        {
            Log.Info("GamblingDen scene loaded");
        }
    }
}
