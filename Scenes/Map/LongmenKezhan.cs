using Godot;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scenes.NPC;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 龙门客栈 - 江湖消息集散地，各方势力交汇
    /// </summary>
    public partial class LongmenKezhan : Node2D
    {
        public override void _Ready()
        {
            Log.Info("LongmenKezhan scene loaded");
            SpawnNPCs();
        }

        private void SpawnNPCs()
        {
            var npcScene = GD.Load<PackedScene>("res://Scenes/NPC/MapNPC.tscn");

            // Spawn Su Wanqing (appears based on story progression)
            var wanqing = npcScene.Instantiate<MapNPC>();
            wanqing.NpcDataId = "su_wanqing";
            wanqing.DefaultDialogueGraph = "res://Resources/Dialogues/su_wanqing.json";
            wanqing.Position = new Vector3(3, 0, 2);
            AddChild(wanqing);
            Log.Info("Spawned NPC: Su Wanqing at LongmenKezhan");
        }
    }
}
