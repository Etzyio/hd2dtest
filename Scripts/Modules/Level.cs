
using System.Collections.Generic;
using Godot;

namespace hd2dtest.Scripts.Modules
{
    // 关卡数据类
    public class Level
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ScenePath { get; set; }
        public float[] SpawnPosition { get; set; }
        public int Difficulty { get; set; }
        public int TimeLimit { get; set; }
        public int Collectibles { get; set; }
        public int Enemies { get; set; }
        public bool IsLocked { get; set; }
        public List<string> RequiredItems { get; set; }

        // 获取Vector2格式的出生位置
        public Vector3 GetSpawnPosition()
        {
            if (SpawnPosition == null || SpawnPosition.Length < 2)
                return new Vector3(100, 0, 0);
            return new Vector3(SpawnPosition[0], SpawnPosition[1], SpawnPosition[2]);
        }
    }
}