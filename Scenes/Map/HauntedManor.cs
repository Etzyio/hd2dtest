using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 鬼庄 - 荒废多年的庄园，夜半常有诡异声响，村民不敢靠近
    /// </summary>
    public partial class HauntedManor : Node2D
    {
        public override void _Ready()
        {
            Log.Info("HauntedManor scene loaded");
        }
    }
}
