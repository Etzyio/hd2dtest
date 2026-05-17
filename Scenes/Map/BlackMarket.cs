using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 黑市 - 地下交易之所，可买到寻常店铺不售的稀罕之物
    /// </summary>
    public partial class BlackMarket : Node2D
    {
        public override void _Ready()
        {
            Log.Info("BlackMarket scene loaded");
        }
    }
}
