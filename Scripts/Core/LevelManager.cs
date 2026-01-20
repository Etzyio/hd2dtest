using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scripts.Core
{
    public partial class LevelManager : Node
    {
        [Export]
        public TileMapLayer GroundLayer;

        [Export]
        public TileMapLayer CollisionLayer;

        [Export]
        public Node3D Player;

        private Vector3 _spawnPosition = new(100, 0, 0);
        private List<Level> _levels = new List<Level>();
        private Level _currentLevel;
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
        public void SetSpawnPosition(Vector3 position)
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

        // 根据ID获取关卡信息
        public Level GetLevelById(string levelId)
        {
            return _levels.Find(level => level.Id == levelId);
        }

        // 获取所有关卡信息
        public List<Level> GetAllLevels()
        {
            return _levels;
        }

        // 获取当前关卡
        public Level GetCurrentLevel()
        {
            return _currentLevel;
        }

        // 根据场景路径获取关卡信息
        public Level GetLevelByScenePath(string scenePath)
        {
            return _levels.Find(level => level.ScenePath == scenePath);
        }

        // 加载指定ID的关卡
        public bool LoadLevelById(string levelId)
        {
            Level level = GetLevelById(levelId);
            if (level == null)
            {
                Log.Error("Level with ID " + levelId + " not found");
                return false;
            }

            return LoadLevel(level);
        }

        // 加载指定场景路径的关卡
        public bool LoadLevelByScenePath(string scenePath)
        {
            Level level = GetLevelByScenePath(scenePath);
            if (level == null)
            {
                Log.Error("Level with scene path " + scenePath + " not found");
                return false;
            }

            return LoadLevel(level);
        }

        // 加载关卡
        private bool LoadLevel(Level level)
        {
            try
            {
                // 保存当前关卡
                _currentLevel = level;

                // 设置出生位置
                _spawnPosition = level.GetSpawnPosition();

                // 可以在这里添加关卡加载逻辑，例如：
                // 1. 卸载当前场景
                // 2. 加载新场景
                // 3. 初始化玩家位置
                // 4. 触发关卡加载事件

                Log.Info("Loading level: " + level.Name + " (" + level.Id + ")");
                Log.Debug("Spawn position: " + _spawnPosition);
                Log.Debug("Difficulty: " + level.Difficulty);

                // 重置关卡
                InitializeLevel();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error loading level: " + ex.Message);
                return false;
            }
        }
    }
}