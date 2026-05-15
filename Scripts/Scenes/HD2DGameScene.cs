using Godot;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scenes
{
    public partial class HD2DGameScene : Node3D
    {
        [Export] public PackedScene PlayerScene;
        [Export] public PackedScene TerrainScene;

        private HD2DRenderManager _renderManager;
        private HD2DCharacter _playerNode;
        private HD2DTerrain _terrain;

        public override void _Ready()
        {
            InitializeRenderManager();
            InitializeTerrain();
            InitializePlayer();
            SetupLighting();
        }

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
                _terrain = new HD2DTerrain();
                _terrain.Name = "HD2DTerrain";
                _terrain.Width = 30;
                _terrain.Height = 30;
                _terrain.TileSize = 1.0f;
                _terrain.HeightScale = 3.0f;
                AddChild(_terrain);
            }
        }

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

        public HD2DRenderManager GetRenderManager()
        {
            return _renderManager;
        }

        public HD2DTerrain GetTerrain()
        {
            return _terrain;
        }

        public HD2DCharacter GetPlayer()
        {
            return _playerNode;
        }
    }
}