using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 练功场 - 镖局后院的练武场，可在此修炼武艺
    /// </summary>
    public partial class TrainingGround : Node2D
    {
        public override void _Ready()
        {
            Log.Info("TrainingGround scene loaded");
        }
    }
}
