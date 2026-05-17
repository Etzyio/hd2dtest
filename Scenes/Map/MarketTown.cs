using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 集市镇 - 南北商旅交汇的商贸重镇，可购得各地奇珍异宝
    /// </summary>
    public partial class MarketTown : Node2D
    {
        public override void _Ready()
        {
            Log.Info("MarketTown scene loaded");
        }
    }
}
