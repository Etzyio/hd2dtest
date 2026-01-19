using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace hd2dtest.Scripts.Core
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
        private List<Level> _levels = new List<Level>();
        private Level _currentLevel;
        private const string LEVELS_JSON_PATH = "res://Resources/Static/Levels.json";

        public override void _Ready()
        {
            LoadLevelsData();
            InitializeLevel();
        }

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
            public Vector2 GetSpawnPosition()
            {
                if (SpawnPosition == null || SpawnPosition.Length < 2)
                    return new Vector2(100, 0);
                return new Vector2(SpawnPosition[0], SpawnPosition[1]);
            }
        }

        // 加载关卡数据
        private void LoadLevelsData()
        {
            try
            {
                // 读取JSON文件
                var fileAccess = Godot.FileAccess.Open(LEVELS_JSON_PATH, Godot.FileAccess.ModeFlags.Read);
                if (fileAccess == null)
                {
                    Log.Error("Failed to open level data file: " + LEVELS_JSON_PATH);
                    return;
                }

                string jsonContent = fileAccess.GetAsText();
                fileAccess.Close();

                // 解析JSON数据
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                // 获取关卡数组
                if (root.TryGetProperty("Levels", out var levelsArray))
                {
                    foreach (var levelElement in levelsArray.EnumerateArray())
                    {
                        Level level = new Level
                        {
                            Id = levelElement.GetProperty("Id").GetString(),
                            Name = levelElement.GetProperty("Name").GetString(),
                            Description = levelElement.GetProperty("Description").GetString(),
                            ScenePath = levelElement.GetProperty("ScenePath").GetString(),
                            SpawnPosition = JsonSerializer.Deserialize<float[]>(levelElement.GetProperty("SpawnPosition").GetRawText()),
                            Difficulty = levelElement.GetProperty("Difficulty").GetInt32(),
                            TimeLimit = levelElement.GetProperty("TimeLimit").GetInt32(),
                            Collectibles = levelElement.GetProperty("Collectibles").GetInt32(),
                            Enemies = levelElement.GetProperty("Enemies").GetInt32(),
                            IsLocked = levelElement.GetProperty("IsLocked").GetBoolean(),
                            RequiredItems = JsonSerializer.Deserialize<List<string>>(levelElement.GetProperty("RequiredItems").GetRawText())
                        };
                        _levels.Add(level);
                    }

                    Log.Info("Successfully loaded " + _levels.Count + " levels data");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error loading level data: " + ex.Message);
            }
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