using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 山寨堡垒 - 盘踞山中的盗匪老巢，依山而建，易守难攻
    /// </summary>
    public partial class BanditFortress : Node2D
    {
        public override void _Ready()
        {
            Log.Info("BanditFortress scene loaded");
        }
    }
}
