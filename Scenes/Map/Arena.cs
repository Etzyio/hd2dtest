using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 比武场 - 天下英雄切磋武艺之地，胜者可获丰厚奖励与江湖声望
    /// </summary>
    public partial class Arena : Node2D
    {
        public override void _Ready()
        {
            Log.Info("Arena scene loaded");
        }
    }
}
