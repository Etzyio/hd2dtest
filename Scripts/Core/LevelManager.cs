using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 关卡管理器，负责关卡的初始化、加载和管理
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，负责管理游戏中的关卡信息，包括关卡的加载、重置、玩家出生位置设置等功能。
    /// 它通过TileMapLayer来管理地图的地面层和碰撞层，并提供了获取瓦片信息的方法。
    /// </remarks>
    public partial class LevelManager : Node
    {
        /// <summary>
        /// 地面图层，用于显示游戏地图
        /// </summary>
        [Export]
        public TileMapLayer GroundLayer;

        /// <summary>
        /// 碰撞图层，用于处理游戏中的碰撞检测
        /// </summary>
        [Export]
        public TileMapLayer CollisionLayer;

        /// <summary>
        /// 玩家节点引用，用于设置玩家的位置
        /// </summary>
        [Export]
        public Node3D Player;

        /// <summary>
        /// 玩家出生位置
        /// </summary>
        private Vector3 _spawnPosition = new(100, 0, 0);

        /// <summary>
        /// 关卡列表，存储所有可用的关卡信息
        /// </summary>
        private List<Level> _levels = new List<Level>();

        /// <summary>
        /// 当前关卡
        /// </summary>
        private Level _currentLevel;

        /// <summary>
        /// 节点准备就绪时调用的方法
        /// </summary>
        /// <remarks>
        /// 该方法在节点进入场景树时自动调用，用于初始化关卡。
        /// </remarks>
        public override void _Ready()
        {
            InitializeLevel();
        }

        /// <summary>
        /// 初始化关卡
        /// </summary>
        /// <remarks>
        /// 该方法设置玩家的出生位置，并处理关卡的初始化逻辑。
        /// 注意：碰撞初始化已移至TileMapLayer节点属性，此处不再需要代码设置。
        /// </remarks>
        private void InitializeLevel()
        {
            // 设置玩家出生位置
            if (Player != null)
            {
                Player.Position = _spawnPosition;
            }
            // 初始化碰撞已移至TileMapLayer节点属性，此处不再需要代码设置
        }

        /// <summary>
        /// 重置关卡
        /// </summary>
        /// <remarks>
        /// 该方法调用InitializeLevel()来重置关卡状态，包括玩家位置等。
        /// </remarks>
        public void ResetLevel()
        {
            InitializeLevel();
        }

        /// <summary>
        /// 设置出生位置
        /// </summary>
        /// <param name="position">新的出生位置</param>
        /// <remarks>
        /// 该方法用于设置玩家的出生位置，会在关卡重置或加载时应用。
        /// </remarks>
        public void SetSpawnPosition(Vector3 position)
        {
            _spawnPosition = position;
        }

        /// <summary>
        /// 获取指定图层中的瓦片信息
        /// </summary>
        /// <param name="position">世界坐标位置</param>
        /// <param name="layer">指定的瓦片图层，默认为null（使用地面图层）</param>
        /// <returns>瓦片的源ID，-1表示获取失败</returns>
        /// <remarks>
        /// 该方法根据世界坐标计算瓦片坐标，并返回指定图层中对应瓦片的源ID。
        /// 如果没有指定图层，默认使用地面图层；如果图层为null，则返回-1。
        /// </remarks>
        public int GetTileAtPosition(Vector2 position, TileMapLayer layer = null)
        {
            // 如果没有指定图层，默认使用地面图层
            layer ??= GroundLayer;

            if (layer == null)
                return -1;

            Vector2I tileCoords = layer.LocalToMap(position);
            return layer.GetCellSourceId(tileCoords);
        }

        /// <summary>
        /// 根据ID获取关卡信息
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        /// <returns>找到的关卡对象，未找到则返回null</returns>
        /// <remarks>
        /// 该方法通过关卡ID在关卡列表中查找对应的关卡对象。
        /// </remarks>
        public Level GetLevelById(string levelId)
        {
            return _levels.Find(level => level.Id == levelId);
        }

        /// <summary>
        /// 获取所有关卡信息
        /// </summary>
        /// <returns>所有关卡的列表</returns>
        /// <remarks>
        /// 该方法返回存储在_levels列表中的所有关卡对象。
        /// </remarks>
        public List<Level> GetAllLevels()
        {
            return _levels;
        }

        /// <summary>
        /// 获取当前关卡
        /// </summary>
        /// <returns>当前关卡对象</returns>
        /// <remarks>
        /// 该方法返回当前正在加载的关卡对象。
        /// </remarks>
        public Level GetCurrentLevel()
        {
            return _currentLevel;
        }

        /// <summary>
        /// 根据场景路径获取关卡信息
        /// </summary>
        /// <param name="scenePath">场景路径</param>
        /// <returns>找到的关卡对象，未找到则返回null</returns>
        /// <remarks>
        /// 该方法通过场景路径在关卡列表中查找对应的关卡对象。
        /// </remarks>
        public Level GetLevelByScenePath(string scenePath)
        {
            return _levels.Find(level => level.ScenePath == scenePath);
        }

        /// <summary>
        /// 加载指定ID的关卡
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        /// <returns>加载成功返回true，失败返回false</returns>
        /// <remarks>
        /// 该方法根据关卡ID查找关卡对象，然后调用LoadLevel方法加载关卡。
        /// 如果关卡不存在，会记录错误日志并返回false。
        /// </remarks>
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

        /// <summary>
        /// 加载指定场景路径的关卡
        /// </summary>
        /// <param name="scenePath">场景路径</param>
        /// <returns>加载成功返回true，失败返回false</returns>
        /// <remarks>
        /// 该方法根据场景路径查找关卡对象，然后调用LoadLevel方法加载关卡。
        /// 如果关卡不存在，会记录错误日志并返回false。
        /// </remarks>
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

        /// <summary>
        /// 加载关卡
        /// </summary>
        /// <param name="level">要加载的关卡对象</param>
        /// <returns>加载成功返回true，失败返回false</returns>
        /// <remarks>
        /// 该方法负责加载指定的关卡，包括保存当前关卡、设置出生位置、记录日志等操作。
        /// 加载完成后会重置关卡状态。
        /// </remarks>
        /// <exception cref="System.Exception">加载关卡过程中可能发生的异常</exception>
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