using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 江南古道 - 从苏州镖局前往黄泥岗的官道，绿树成荫，偶尔有行人往来
    /// </summary>
    public partial class JiangnanRoad : Node2D
    {
        public override void _Ready()
        {
            Log.Info("JiangnanRoad transition scene loaded");
        }
    }
}
