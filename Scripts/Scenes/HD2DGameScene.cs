/*
 * File: HD2DGameScene.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * HD2D游戏场景主节点，负责初始化游戏场景的核心组件。
 * 包括渲染管理器、地形、玩家角色和光照系统的初始化。
 * 
 * Key Features:
 * - 自动初始化HD2DRenderManager（如果不存在）
 * - 支持从场景文件或代码创建地形
 * - 程序化创建玩家角色
 * - 设置默认光照系统
 */
using Godot;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scenes
{
    /// <summary>
    /// HD2D游戏场景主节点
    /// 负责初始化游戏场景的核心组件
    /// </summary>
    public partial class HD2DGameScene : Node3D
    {
        /// <summary>
        /// 玩家场景资源（可选）
        /// </summary>
        [Export] public PackedScene PlayerScene;

        /// <summary>
        /// 地形场景资源（可选）
        /// </summary>
        [Export] public PackedScene TerrainScene;

        /// <summary>
        /// 渲染管理器引用
        /// </summary>
        private HD2DRenderManager _renderManager;

        /// <summary>
        /// 玩家角色节点
        /// </summary>
        private HD2DCharacter _playerNode;

        /// <summary>
        /// 地形节点
        /// </summary>
        private HD2DTerrain _terrain;

        /// <summary>
        /// 节点就绪时的回调
        /// 初始化所有核心组件
        /// </summary>
        public override void _Ready()
        {
            InitializeRenderManager();
            InitializeTerrain();
            InitializePlayer();
            SetupLighting();
        }

        /// <summary>
        /// 初始化渲染管理器
        /// 如果场景中不存在，则创建一个新的实例
        /// </summary>
        private void InitializeRenderManager()
        {
            _renderManager = GetNodeOrNull<HD2DRenderManager>("HD2DRenderManager");
            if (_renderManager == null)
            {
                _renderManager = new HD2DRenderManager();
                _renderManager.Name = "HD2DRenderManager";
                AddChild(_renderManager);
            }
        }

        /// <summary>
        /// 初始化地形
        /// 如果提供了地形场景资源则从场景实例化，否则创建默认地形
        /// </summary>
        private void InitializeTerrain()
        {
            if (TerrainScene != null)
            {
                _terrain = TerrainScene.Instantiate<HD2DTerrain>();
                _terrain.Name = "HD2DTerrain";
                AddChild(_terrain);
            }
            else
            {
                // 创建默认地形
                _terrain = new HD2DTerrain();
                _terrain.Name = "HD2DTerrain";
                _terrain.Width = 30;
                _terrain.Height = 30;
                _terrain.TileSize = 1.0f;
                _terrain.HeightScale = 3.0f;
                AddChild(_terrain);
            }
        }

        /// <summary>
        /// 初始化玩家角色
        /// 创建玩家节点并设置初始属性
        /// </summary>
        private void InitializePlayer()
        {
            _playerNode = new HD2DCharacter();
            _playerNode.Name = "Player";
            _playerNode.CharacterScale = 1.2f;
            _playerNode.CollisionHeight = 1.8f;
            _playerNode.CollisionRadius = 0.25f;
            _playerNode.Position = new Vector3(15, 1, 15);
            AddChild(_playerNode);
        }

        /// <summary>
        /// 设置光照系统
        /// 创建主光源（太阳光）
        /// </summary>
        private void SetupLighting()
        {
            DirectionalLight3D sunLight = new DirectionalLight3D();
            sunLight.Name = "SunLight";
            sunLight.Position = new Vector3(20, 30, 20);
            sunLight.LookAt(Vector3.Zero);
            sunLight.LightEnergy = 1.0f;
            sunLight.LightColor = new Color(1.0f, 0.95f, 0.85f);
            AddChild(sunLight);
        }

        /// <summary>
        /// 获取渲染管理器
        /// </summary>
        /// <returns>渲染管理器引用</returns>
        public HD2DRenderManager GetRenderManager()
        {
            return _renderManager;
        }

        /// <summary>
        /// 获取地形节点
        /// </summary>
        /// <returns>地形节点引用</returns>
        public HD2DTerrain GetTerrain()
        {
            return _terrain;
        }

        /// <summary>
        /// 获取玩家角色节点
        /// </summary>
        /// <returns>玩家角色节点引用</returns>
        public HD2DCharacter GetPlayer()
        {
            return _playerNode;
        }
    }
}