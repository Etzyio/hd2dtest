using Godot;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scenes.NPC;

namespace hd2dtest.Scenes.Map
{
    /// <summary>
    /// 清风镖局 - 序章起点，周远师父所在之处
    /// </summary>
    public partial class QingfengBiaoju : Node2D
    {
        public override void _Ready()
        {
            Log.Info("QingfengBiaoju scene loaded");
            SpawnNPCs();
        }

        private void SpawnNPCs()
        {
            var npcScene = GD.Load<PackedScene>("res://Scenes/NPC/MapNPC.tscn");

            // Spawn Zhou Yuan (master)
            var zhouyuan = npcScene.Instantiate<MapNPC>();
            zhouyuan.NpcDataId = "zhou_yuan";
            zhouyuan.DefaultDialogueGraph = "res://Resources/Dialogues/prologue.json";
            zhouyuan.Position = new Vector3(0, 0, 0);
            AddChild(zhouyuan);
            Log.Info("Spawned NPC: Zhou Yuan at QingfengBiaoju");
        }
    }
}
