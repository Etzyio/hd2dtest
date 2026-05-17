using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 毒瘴谷 - 终年被毒雾笼罩的山谷，谷中生长着罕见的毒草与药草
    /// </summary>
    public partial class PoisonValley : Node2D
    {
        public override void _Ready()
        {
            Log.Info("PoisonValley scene loaded");
        }
    }
}
