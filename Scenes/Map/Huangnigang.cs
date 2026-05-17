using Godot;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scenes.NPC;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 黄泥岗 - 序章伏击战场，沈剑锋牺牲之地
    /// </summary>
    public partial class Huangnigang : Node2D
    {
        public override void _Ready()
        {
            Log.Info("Huangnigang scene loaded");
            SpawnNPCs();
        }

        private void SpawnNPCs()
        {
            var npcScene = GD.Load<PackedScene>("res://Scenes/NPC/MapNPC.tscn");

            // Spawn Shen Jianfeng for pre-battle dialogue
            var shen = npcScene.Instantiate<MapNPC>();
            shen.NpcDataId = "shen_jianfeng";
            shen.DefaultDialogueGraph = "res://Resources/Dialogues/chapter1.json";
            shen.Position = new Vector3(5, 0, 0);
            AddChild(shen);
            Log.Info("Spawned NPC: Shen Jianfeng at Huangnigang");
        }
    }
}
