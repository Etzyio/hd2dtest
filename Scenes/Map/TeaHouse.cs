using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 茶馆 - 江湖消息集散地，三教九流在此饮茶论事，可探听各路情报
    /// </summary>
    public partial class TeaHouse : Node2D
    {
        public override void _Ready()
        {
            Log.Info("TeaHouse scene loaded");
        }
    }
}
