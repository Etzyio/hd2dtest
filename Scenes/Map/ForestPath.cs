using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 林间小径 - 江南常见的林间小路，阳光透过树叶洒落
    /// </summary>
    public partial class ForestPath : Node2D
    {
        public override void _Ready()
        {
            Log.Info("ForestPath transition scene loaded");
        }
    }
}
