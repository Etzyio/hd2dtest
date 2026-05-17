using Godot;
using System;
using System.Threading.Tasks;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;

namespace hd2dtest
{
    public partial class Main : Node
    {
        private static Main _instance;
        public static Main Instance => _instance;

        private VersionManager _versionManager;
        private Control _sceneLayer;
        private ColorRect _shade;
        private hd2dtest.Scenes.Popup.PopupMenu _popupLayer;
        private Node _managersNode;
        private TextureRect _startScreen;
        private Timer _startScreenTimer;
        private bool _isLoading = false;
        private float _loadProgress = 0f;

        public bool PopupStatus => _popupLayer.Visible;
        public Node NowScene => _sceneLayer.GetChild(0);

        public override void _Ready()
        {
            _instance = this;

            _sceneLayer = GetNode<Control>("sceneLayer");
            _shade = GetNode<ColorRect>("shade");
            _popupLayer = GetNode<hd2dtest.Scenes.Popup.PopupMenu>("popupLayer/PopupMenu");

            SetupExceptionHandling();
            CreateManagersNode();
            InitializeAllManagers();
            InitializeStartScreen();
            LoadPreferences();
            StartPreloadProcess();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                Log.Error($"Unhandled exception: {ex.Message}\n{ex.StackTrace}");
                ShowErrorDialog(ex.Message);
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Error($"Unobserved task exception: {args.Exception.Message}\n{args.Exception.StackTrace}");
                args.SetObserved();
            };

            Log.Info("Exception handling setup completed");
        }

        private void CreateManagersNode()
        {
            if (GetNodeOrNull("Managers") == null)
            {
                _managersNode = new Node();
                _managersNode.Name = "Managers";
                AddChild(_managersNode);
                Log.Info("Managers container node created");
            }
            else
            {
                _managersNode = GetNode<Node>("Managers");
                Log.Info("Managers container node already exists, using existing node");
            }
        }

        private void InitializeAllManagers()
        {
            InitializeVersionManager();
            InitializeSaveManager();
            InitializeConfigManager();
            InitializeGameStateManager();
            InitializeAudioManager();
            InitializeInventoryManager();
            InitializeCharacterStatsManager();
            InitializeShopManager();
            InitializeMapManager();
            InitializeAchievementManager();
            InitializeEffectManager();
            Log.Info("All managers initialized");
        }

        private void InitializeStartScreen()
        {
            _startScreen = new TextureRect();
            _startScreen.Name = "StartScreen";
            _startScreen.AnchorLeft = 0f;
            _startScreen.AnchorRight = 1f;
            _startScreen.AnchorTop = 0f;
            _startScreen.AnchorBottom = 1f;
            _startScreen.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
            _startScreen.Visible = true;
            AddChild(_startScreen);

            _startScreenTimer = new Timer();
            _startScreenTimer.Name = "StartScreenTimer";
            _startScreenTimer.WaitTime = 3.0f;
            _startScreenTimer.OneShot = true;
            _startScreenTimer.Timeout += OnStartScreenComplete;
            AddChild(_startScreenTimer);

            Log.Info("Start screen initialized");
        }

        private void LoadPreferences()
        {
            try
            {
                ConfigManager configManager = ConfigManager.GetInstance();
                float bgmVolume = configManager.GetFloat("audio", "bgm_volume", 0.7f);
                float sfxVolume = configManager.GetFloat("audio", "sfx_volume", 1.0f);
                string language = configManager.GetString("game", "language", "zh-CN");
                
                Log.Info($"Loaded preferences - BGM Volume: {bgmVolume}, SFX Volume: {sfxVolume}, Language: {language}");
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to load preferences: {ex.Message}, using defaults");
            }
        }

        private async void StartPreloadProcess()
        {
            _isLoading = true;
            _loadProgress = 0f;
            Log.Info("Starting preload process...");
            await PreloadCoroutine();
        }

        private async Task PreloadCoroutine()
        {
            await Task.Delay(500);
            
            Log.Info("Preloading critical resources...");
            _loadProgress = 0.2f;
            await Task.Delay(300);

            Log.Info("Preloading game data...");
            _loadProgress = 0.5f;
            await Task.Delay(300);

            Log.Info("Preloading assets...");
            _loadProgress = 0.8f;
            await Task.Delay(300);

            Log.Info("Preloading complete");
            _loadProgress = 1.0f;
            _isLoading = false;

            if (_startScreenTimer != null && _startScreenTimer.IsStopped())
            {
                _startScreenTimer.Start();
            }
        }

        private void OnStartScreenComplete()
        {
            _startScreen?.QueueFree();
            _startScreen = null;
            Log.Info("HD2D Game Initialized - Loading Start Scene");
            SwitchScene("start");
        }

        private void ShowErrorDialog(string message)
        {
            Log.Error($"Fatal error: {message}");
        }

        public void TriggerSceneReady()
        {
            Log.Info("Scene ready triggered, showing scene layer");
            ShowSceneLayer();
        }

        public override void _Process(double delta)
        {
        }

        public override void _ExitTree()
        {
            HandleGameExit();
        }

        private void HandleGameExit()
        {
            Log.Info("Game is exiting, performing cleanup...");
            
            try
            {
                SaveManager.GetInstance().AutoSave();
                Log.Info("Auto-save completed");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to auto-save: {ex.Message}");
            }

            try
            {
                ResourcesManager.GetInstance().Cleanup();
                Log.Info("Resources cleaned up");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to cleanup resources: {ex.Message}");
            }

            Log.Info("Game exit complete");
        }

        private static void InitializeVersionManager()
        {
            Log.Info("VersionManager initialized");
        }

        public void SwitchScene(String sceneName)
        {
            Log.Info($"Starting to switch scene: {sceneName}");

            HideSceneLayer();
            try
            {
                PackedScene scene = GameViewRegister.GetScene(sceneName);

                if (scene != null)
                {
                    foreach (var a in _sceneLayer.GetChildren())
                    {
                        _sceneLayer.RemoveChild(a);
                    }
                    var newScene = scene.Instantiate<Node>();
                    _sceneLayer.AddChild(newScene);
                }
                Log.Info($"Scene {sceneName} loaded successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Exception during scene switch: {ex.Message}");
            }
            finally
            {
                ShowSceneLayer();
                ResumeSceneLayer();
            }
        }

        private void ShowSceneLayer()
        {
            if (_sceneLayer != null)
            {
                _sceneLayer.Visible = true;
            }
            if (_popupLayer != null)
            {
                _popupLayer.Visible = false;
            }
            Log.Info("Scene layer shown (CanvasItem)");
        }

        private void ResumeSceneLayer()
        {
            if (_sceneLayer != null)
            {
                _sceneLayer.ProcessMode = Node.ProcessModeEnum.Inherit;
                RemovePauseVisualEffect();
                HandleAudioResume();
                Log.Info("Scene layer resumed");
            }
        }

        private void RemovePauseVisualEffect()
        {
            _shade?.Visible = false;
        }

        private void PauseSceneLayer()
        {
            _shade?.Visible = true;
        }

        private void HandleAudioResume()
        {
            try
            {
                AudioManager.Instance?.ResumeBgm();
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to resume audio: {ex.Message}");
            }
            Log.Info("Audio resumed - background music volume restored");
        }

        private void HandleAudioPause()
        {
            try
            {
                AudioManager.Instance?.PauseBgm();
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to pause audio: {ex.Message}");
            }
            Log.Info("Audio paused - background music volume reduced");
        }

        private void HideSceneLayer()
        {
            if (_sceneLayer != null)
            {
                _sceneLayer.Visible = false;
            }
            if (_popupLayer != null)
            {
                _popupLayer.Visible = false;
            }
            Log.Info("Scene layer hidden (CanvasItem)");
        }

        private void InitializeSaveManager()
        {
            SaveManager saveManager = SaveManager.GetInstance();
            try
            {
                if (saveManager.GetParent() == null)
                {
                    AddChild(saveManager);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"SaveManager parent check failed: {ex.Message}, skipping AddChild");
            }
            Log.Info("SaveManager initialized successfully");
        }

        private void InitializeConfigManager()
        {
            ConfigManager configManager = ConfigManager.GetInstance();
            try
            {
                if (configManager.GetParent() == null)
                {
                    AddChild(configManager);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"ConfigManager parent check failed: {ex.Message}, skipping AddChild");
            }
            Log.Info("ConfigManager initialized");
        }

        private void InitializeGameStateManager()
        {
            try
            {
                var gameStateManager = new GameStateManager();
                gameStateManager.Name = "GameStateManager";
                _managersNode.AddChild(gameStateManager);
                Log.Info("GameStateManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to initialize GameStateManager: {ex.Message}");
            }
        }

        private void InitializeAudioManager()
        {
            try
            {
                var audioManager = new AudioManager();
                audioManager.Name = "AudioManager";
                _managersNode.AddChild(audioManager);
                Log.Info("AudioManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to initialize AudioManager: {ex.Message}");
            }
        }

        private void InitializeInventoryManager()
        {
            try
            {
                var inventoryManager = new InventoryManager();
                inventoryManager.Name = "InventoryManager";
                _managersNode.AddChild(inventoryManager);
                Log.Info("InventoryManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to initialize InventoryManager: {ex.Message}");
            }
        }

        private void InitializeCharacterStatsManager()
        {
            try
            {
                var statsManager = new CharacterStatsManager();
                statsManager.Name = "CharacterStatsManager";
                _managersNode.AddChild(statsManager);
                Log.Info("CharacterStatsManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to initialize CharacterStatsManager: {ex.Message}");
            }
        }

        private void InitializeShopManager()
        {
            try
            {
                var shopManager = new ShopManager();
                shopManager.Name = "ShopManager";
                _managersNode.AddChild(shopManager);
                Log.Info("ShopManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to initialize ShopManager: {ex.Message}");
            }
        }

        private void InitializeMapManager()
        {
            try
            {
                var mapManager = new MapManager();
                mapManager.Name = "MapManager";
                _managersNode.AddChild(mapManager);
                Log.Info("MapManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to initialize MapManager: {ex.Message}");
            }
        }

        private void InitializeAchievementManager()
        {
            try
            {
                var achievementManager = new AchievementManager();
                achievementManager.Name = "AchievementManager";
                _managersNode.AddChild(achievementManager);
                Log.Info("AchievementManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to initialize AchievementManager: {ex.Message}");
            }
        }

        private void InitializeEffectManager()
        {
            try
            {
                var effectManager = new EffectManager();
                effectManager.Name = "EffectManager";
                _managersNode.AddChild(effectManager);
                Log.Info("EffectManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to initialize EffectManager: {ex.Message}");
            }
        }

        public void OpenPopup()
        {
            _shade.Visible = true;
            _sceneLayer.ProcessMode = Node.ProcessModeEnum.Disabled;
            if (_popupLayer != null)
            {
                _popupLayer.Visible = true;
                _popupLayer.ShowMainMenu();
                PauseSceneLayer();
                HandleAudioPause();
            }
        }

        public void ClosePopup()
        {
            _shade?.Visible = false;
            _sceneLayer.ProcessMode = Node.ProcessModeEnum.Inherit;
            if (_popupLayer != null)
            {
                _popupLayer.Visible = false;
                _shade.Visible = false;
                ResumeSceneLayer();
                HandleAudioResume();
            }
        }
    }
}