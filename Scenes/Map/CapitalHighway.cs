using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 京城大道 - 前往皇宫的宽阔官道，车水马龙
    /// </summary>
    public partial class CapitalHighway : Node2D
    {
        public override void _Ready()
        {
            Log.Info("CapitalHighway transition scene loaded");
        }
    }
}
