using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 渔村 - 太湖之畔的渔村，码头停泊着数艘渔船，鱼鲜味美
    /// </summary>
    public partial class FishingVillage : Node2D
    {
        public override void _Ready()
        {
            Log.Info("FishingVillage scene loaded");
        }
    }
}
