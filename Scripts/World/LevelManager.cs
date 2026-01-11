using Godot;
using System;

namespace hd2dtest.Scripts.World
{
    public partial class LevelManager : Node
    {
        [Export]
        public TileMapLayer GroundLayer;

        [Export]
        public TileMapLayer CollisionLayer;

        [Export]
        public Node2D Player;

        private Vector2 _spawnPosition = new(100, 0);

        public override void _Ready()
        {
            InitializeLevel();
        }

        private void InitializeLevel()
        {
            // 设置玩家出生位置
            if (Player != null)
            {
                Player.Position = _spawnPosition;
            }

            // 初始化碰撞已移至TileMapLayer节点属性，此处不再需要代码设置
        }

        // 重置关卡
        public void ResetLevel()
        {
            InitializeLevel();
        }

        // 设置出生位置
        public void SetSpawnPosition(Vector2 position)
        {
            _spawnPosition = position;
        }

        // 获取指定图层中的瓦片信息
        public int GetTileAtPosition(Vector2 position, TileMapLayer layer = null)
        {
            // 如果没有指定图层，默认使用地面图层
            layer ??= GroundLayer;

            if (layer == null)
                return -1;

            Vector2I tileCoords = layer.LocalToMap(position);
            return layer.GetCellSourceId(tileCoords);
        }
    }
}