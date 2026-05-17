using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 废墟小径 - 从洞穴通往清风门废墟的山路，沿途可见战斗痕迹
    /// </summary>
    public partial class RuinsPath : Node2D
    {
        public override void _Ready()
        {
            Log.Info("RuinsPath transition scene loaded");
        }
    }
}
