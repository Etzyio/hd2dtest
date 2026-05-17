using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 铁匠铺 - 名匠铸剑之所，可锻造与强化兵器装备
    /// </summary>
    public partial class BlacksmithWorkshop : Node2D
    {
        public override void _Ready()
        {
            Log.Info("BlacksmithWorkshop scene loaded");
        }
    }
}
