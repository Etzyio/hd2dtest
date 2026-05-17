using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 药庐 - 药师采药炼丹之所，可炼制各类丹药与疗伤圣药
    /// </summary>
    public partial class Apothecary : Node2D
    {
        public override void _Ready()
        {
            Log.Info("Apothecary scene loaded");
        }
    }
}
