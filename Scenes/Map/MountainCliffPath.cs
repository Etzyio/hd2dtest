using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 山崖险径 - 连接黄泥岗与山崖洞穴的陡峭山路，云雾缭绕
    /// </summary>
    public partial class MountainCliffPath : Node2D
    {
        public override void _Ready()
        {
            Log.Info("MountainCliffPath transition scene loaded");
        }
    }
}
