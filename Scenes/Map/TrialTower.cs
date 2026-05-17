using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 试炼塔 - 层层挑战的试炼之塔，每层有强敌把守，登顶者可获绝世武学
    /// </summary>
    public partial class TrialTower : Node2D
    {
        public override void _Ready()
        {
            Log.Info("TrialTower scene loaded");
        }
    }
}
