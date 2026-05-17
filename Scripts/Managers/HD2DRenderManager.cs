using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    public enum HD2DRenderQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    public partial class HD2DRenderManager : Node
    {
        public static HD2DRenderManager Instance { get; private set; }

        [Export] public HD2DRenderQuality RenderQuality = HD2DRenderQuality.High;
        [Export] public Color AmbientColor = new Color(0.2f, 0.2f, 0.25f);

        private WorldEnvironment _worldEnvironment;
        private Camera3D _mainCamera;
        private Node3D _renderRoot;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else
            {
                QueueFree();
            }
        }

        private void Initialize()
        {
            CreateWorldEnvironment();
            CreateRenderRoot();
            ConfigureCamera();
            
            Log.Info("HD2D Render Manager initialized");
        }

        private void CreateWorldEnvironment()
        {
            _worldEnvironment = new WorldEnvironment();
            _worldEnvironment.Name = "HD2DWorldEnvironment";
            AddChild(_worldEnvironment);

            Godot.Environment environment = new Godot.Environment();
            environment.AmbientLightColor = AmbientColor;
            environment.AmbientLightEnergy = 0.5f;

            _worldEnvironment.Environment = environment;
        }

        private void CreateRenderRoot()
        {
            _renderRoot = new Node3D();
            _renderRoot.Name = "HD2DRenderRoot";
            AddChild(_renderRoot);
        }

        private void ConfigureCamera()
        {
            _mainCamera = new Camera3D();
            _mainCamera.Name = "HD2DCamera";
            _mainCamera.Position = new Vector3(0, 10, 15);
            _mainCamera.Fov = 45.0f;
            _mainCamera.Near = 0.1f;
            _mainCamera.Far = 100.0f;

            AddChild(_mainCamera);
            _mainCamera.LookAt(Vector3.Zero);
            _mainCamera.MakeCurrent();
        }

        public void SetCameraPosition(Vector3 position)
        {
            if (_mainCamera != null)
            {
                _mainCamera.Position = position;
            }
        }

        public void SetCameraLookAt(Vector3 target)
        {
            if (_mainCamera != null)
            {
                _mainCamera.LookAt(target);
            }
        }

        public void SetAmbientColor(Color color)
        {
            AmbientColor = color;
            if (_worldEnvironment?.Environment != null)
            {
                _worldEnvironment.Environment.AmbientLightColor = color;
            }
        }

        public void SetRenderQuality(HD2DRenderQuality quality)
        {
            RenderQuality = quality;
        }

        public Node3D GetRenderRoot()
        {
            return _renderRoot;
        }

        public Camera3D GetMainCamera()
        {
            return _mainCamera;
        }
    }
}