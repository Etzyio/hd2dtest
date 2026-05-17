using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 山隘 - 群山之间的狭窄隘口，易守难攻
    /// </summary>
    public partial class MountainPass : Node2D
    {
        public override void _Ready()
        {
            Log.Info("MountainPass transition scene loaded");
        }
    }
}
