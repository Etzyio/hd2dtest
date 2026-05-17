/*
 * File: MapManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 地图管理器，作为全局单例负责管理游戏中的地图系统。
 * 从 Maps.json 加载所有剧情地图数据，支持地图查询、切换和位置显示。
 *
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 从 Maps.json 加载剧情地图数据（含区域、气候、背景、坐标等）
 * - 按区域分组查询地图
 * - 地图节点位置系统（用于地图面板展示）
 * - 完整的异常处理和日志记录
 */

using Godot;
using hd2dtest.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hd2dtest.Scripts.Managers
{
    public class MapData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Region { get; set; } = "";
        public string Climate { get; set; } = "";
        public string Background { get; set; } = "";
        public string Music { get; set; } = "";
        public List<string> Npcs { get; set; } = new List<string>();
        public List<string> Connections { get; set; } = new List<string>();
        public int MapX { get; set; } = 0;
        public int MapY { get; set; } = 0;
    }

    public partial class MapManager : Node
    {
        public static MapManager Instance { get; private set; }

        private Dictionary<string, MapData> _maps = new Dictionary<string, MapData>();
        private string _currentMapId = "";

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;

                // Try to load from cache if resources already loaded
                if (ResourcesManager.MapsCache.Count > 0)
                {
                    LoadMapsFromCache();
                }

                Log.Info("MapManager initialized");
            }
            else
            {
                Log.Warning("MapManager instance already exists");
                QueueFree();
            }
        }

        public void LoadMapsFromCache()
        {
            _maps.Clear();
            var maps = ResourcesManager.GetAllMaps();
            foreach (var map in maps)
            {
                if (!string.IsNullOrEmpty(map.Id))
                {
                    _maps[map.Id] = map;
                }
            }
            Log.Info($"MapManager loaded {_maps.Count} maps from cache");
        }

        public void SetCurrentMap(string mapId)
        {
            if (_maps.ContainsKey(mapId))
            {
                _currentMapId = mapId;
            }
        }

        public MapData GetCurrentMap()
        {
            if (!string.IsNullOrEmpty(_currentMapId) && _maps.TryGetValue(_currentMapId, out var map))
                return map;
            return null;
        }

        public MapData GetMap(string mapId)
        {
            if (_maps.TryGetValue(mapId, out MapData mapData))
                return mapData;
            return null;
        }

        public List<MapData> GetAllMaps()
        {
            return new List<MapData>(_maps.Values);
        }

        public List<MapData> GetMapsByRegion(string region)
        {
            return _maps.Values.Where(m => m.Region == region).ToList();
        }

        public List<string> GetAllRegions()
        {
            return _maps.Values.Select(m => m.Region).Distinct().ToList();
        }

        public List<MapData> GetConnectedMaps(string mapId)
        {
            var result = new List<MapData>();
            if (_maps.TryGetValue(mapId, out var map) && map.Connections != null)
            {
                foreach (var connId in map.Connections)
                {
                    if (_maps.TryGetValue(connId, out var conn))
                        result.Add(conn);
                }
            }
            return result;
        }
    }
}
