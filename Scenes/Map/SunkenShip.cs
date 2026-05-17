using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 沉船遗迹 - 搁浅在暗礁上的巨大商船残骸，舱中藏有航海者的遗物
    /// </summary>
    public partial class SunkenShip : Node2D
    {
        public override void _Ready()
        {
            Log.Info("SunkenShip scene loaded");
        }
    }
}
