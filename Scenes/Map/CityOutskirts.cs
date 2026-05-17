using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 城郊官道 - 连接破庙与墨羽镖局的城郊道路
    /// </summary>
    public partial class CityOutskirts : Node2D
    {
        public override void _Ready()
        {
            Log.Info("CityOutskirts transition scene loaded");
        }
    }
}
