using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 渡口 - 河畔渡口，可乘船过河
    /// </summary>
    public partial class RiverCrossing : Node2D
    {
        public override void _Ready()
        {
            Log.Info("RiverCrossing transition scene loaded");
        }
    }
}
