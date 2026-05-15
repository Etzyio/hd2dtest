/*
 * File: MapManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 地图管理器，作为全局单例负责管理游戏中的地图系统。
 * 支持地图的加载、卸载、切换和传送，管理地图间的连接关系和战斗遭遇率。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 地图数据结构包含：ID、名称、类型、尺寸、场景路径、出生点、连接地图等
 * - 支持多种地图类型（Overworld、Dungeon、Town、Cave、Arena）
 * - 地图连接验证和切换
 * - 战斗遭遇率计算
 * - 玩家传送功能
 * - 完整的异常处理和日志记录
 */

using Godot;
using hd2dtest.Scripts.Utilities;
using System;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Managers
{
    public enum MapType
    {
        Overworld,
        Dungeon,
        Town,
        Cave,
        Arena
    }

    public class MapData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public MapType Type { get; set; }
        public Vector2 Size { get; set; } = Vector2.Zero;
        public string ScenePath { get; set; } = "";
        public Vector3 SpawnPosition { get; set; } = Vector3.Zero;
        public List<string> ConnectedMaps { get; set; } = new List<string>();
        public bool HasBattleEncounters { get; set; } = true;
        public float EncounterRate { get; set; } = 0.05f;
    }

    public partial class MapManager : Node
    {
        public static MapManager Instance { get; private set; }

        private Dictionary<string, MapData> _maps = new Dictionary<string, MapData>();
        private MapData _currentMap = null;
        private Node3D _currentMapNode = null;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeMaps();
                Log.Info("MapManager initialized");
            }
            else
            {
                Log.Warning("MapManager instance already exists");
                QueueFree();
            }
        }

        private void InitializeMaps()
        {
            _maps.Add("world_map", new MapData
            {
                Id = "world_map",
                Name = "World Map",
                Type = MapType.Overworld,
                Size = new Vector2(100, 100),
                ScenePath = "res://Scenes/Maps/WorldMap.tscn",
                SpawnPosition = new Vector3(50, 1, 50),
                ConnectedMaps = ["forest", "mountain", "town_central"],
                HasBattleEncounters = true,
                EncounterRate = 0.03f
            });

            _maps.Add("forest", new MapData
            {
                Id = "forest",
                Name = "Green Forest",
                Type = MapType.Overworld,
                Size = new Vector2(50, 50),
                ScenePath = "res://Scenes/Maps/Forest.tscn",
                SpawnPosition = new Vector3(25, 0.5f, 25),
                ConnectedMaps = ["world_map", "cave_entrance"],
                HasBattleEncounters = true,
                EncounterRate = 0.08f
            });

            _maps.Add("town_central", new MapData
            {
                Id = "town_central",
                Name = "Central Town",
                Type = MapType.Town,
                Size = new Vector2(30, 30),
                ScenePath = "res://Scenes/Maps/CentralTown.tscn",
                SpawnPosition = new Vector3(15, 0.5f, 15),
                ConnectedMaps = ["world_map", "dungeon_entrance"],
                HasBattleEncounters = false,
                EncounterRate = 0f
            });

            _maps.Add("dungeon_entrance", new MapData
            {
                Id = "dungeon_entrance",
                Name = "Dark Dungeon",
                Type = MapType.Dungeon,
                Size = new Vector2(40, 40),
                ScenePath = "res://Scenes/Maps/Dungeon.tscn",
                SpawnPosition = new Vector3(20, 0.5f, 20),
                ConnectedMaps = ["town_central", "boss_room"],
                HasBattleEncounters = true,
                EncounterRate = 0.1f
            });

            _maps.Add("cave_entrance", new MapData
            {
                Id = "cave_entrance",
                Name = "Mysterious Cave",
                Type = MapType.Cave,
                Size = new Vector2(35, 35),
                ScenePath = "res://Scenes/Maps/Cave.tscn",
                SpawnPosition = new Vector3(17, 0.5f, 17),
                ConnectedMaps = ["forest"],
                HasBattleEncounters = true,
                EncounterRate = 0.07f
            });

            _maps.Add("mountain", new MapData
            {
                Id = "mountain",
                Name = "Snowy Mountains",
                Type = MapType.Overworld,
                Size = new Vector2(45, 45),
                ScenePath = "res://Scenes/Maps/Mountain.tscn",
                SpawnPosition = new Vector3(22, 2, 22),
                ConnectedMaps = ["world_map"],
                HasBattleEncounters = true,
                EncounterRate = 0.04f
            });

            _maps.Add("boss_room", new MapData
            {
                Id = "boss_room",
                Name = "Boss Room",
                Type = MapType.Arena,
                Size = new Vector2(20, 20),
                ScenePath = "res://Scenes/Maps/BossRoom.tscn",
                SpawnPosition = new Vector3(10, 0.5f, 10),
                ConnectedMaps = ["dungeon_entrance"],
                HasBattleEncounters = false,
                EncounterRate = 0f
            });
        }

        public void LoadMap(string mapId)
        {
            if (!_maps.TryGetValue(mapId, out MapData mapData))
            {
                Log.Error($"Map not found: {mapId}");
                return;
            }

            UnloadMap();

            _currentMap = mapData;
            Log.Info($"Loading map: {mapData.Name} ({mapId})");

            PackedScene scene = GD.Load<PackedScene>(mapData.ScenePath);
            if (scene != null)
            {
                _currentMapNode = scene.Instantiate<Node3D>();
                _currentMapNode.Name = "CurrentMap";
                
                if (GetTree().Root.HasNode("Main"))
                {
                    Node mainNode = GetTree().Root.GetNode("Main");
                    mainNode.AddChild(_currentMapNode);
                }

                Teleport(mapData.SpawnPosition);
                Log.Info($"Map loaded successfully: {mapData.Name}");
            }
            else
            {
                Log.Error($"Failed to load map scene: {mapData.ScenePath}");
            }
        }

        public void UnloadMap()
        {
            if (_currentMapNode != null)
            {
                _currentMapNode.QueueFree();
                _currentMapNode = null;
            }
            _currentMap = null;
            Log.Info("Current map unloaded");
        }

        public void Teleport(Vector3 position)
        {
            if (GameManager.Instance != null && GameManager.Instance.Teammates != null && GameManager.Instance.Teammates.Player != null)
            {
                GameManager.Instance.Teammates.Player.GlobalPosition = position;
                Log.Info($"Teleported to: {position}");
            }
        }

        public void ChangeMap(string targetMapId)
        {
            if (_currentMap == null)
            {
                LoadMap(targetMapId);
                return;
            }

            if (!_currentMap.ConnectedMaps.Contains(targetMapId))
            {
                Log.Error($"Cannot travel from {_currentMap.Id} to {targetMapId} - not connected");
                return;
            }

            LoadMap(targetMapId);
        }

        public MapData GetCurrentMap()
        {
            return _currentMap;
        }

        public MapData GetMap(string mapId)
        {
            if (_maps.TryGetValue(mapId, out MapData mapData))
            {
                return mapData;
            }
            return null;
        }

        public List<MapData> GetAllMaps()
        {
            return new List<MapData>(_maps.Values);
        }

        public List<MapData> GetConnectedMaps()
        {
            if (_currentMap == null)
            {
                return new List<MapData>();
            }

            List<MapData> connected = new List<MapData>();
            foreach (string mapId in _currentMap.ConnectedMaps)
            {
                if (_maps.TryGetValue(mapId, out MapData mapData))
                {
                    connected.Add(mapData);
                }
            }
            return connected;
        }

        public bool CanEncounterBattle()
        {
            if (_currentMap == null || !_currentMap.HasBattleEncounters)
            {
                return false;
            }

            return (float)GD.RandRange(0, 1) < _currentMap.EncounterRate;
        }
    }
}