using Godot;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 加载管理器，作为autoload使用，处理场景加载动画
    /// </summary>
    public partial class LoadingManager : Node
    {
        // 加载动画节点
        private Control _loadingAnimation;

        public static LoadingManager Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            InitializeLoadingAnimation();
        }

        // 初始化加载动画
        private void InitializeLoadingAnimation()
        {
            // 创建加载动画节点
            ColorRect colorRect = new()
            {
                Name = "LoadingAnimation",
                Color = new Color(0, 0, 0, 0.8f),
                Size = new Vector2(1800, 900)
            };
            colorRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _loadingAnimation = colorRect;

            // 添加加载文本
            Label label = new()
            {
                Name = "LoadingLabel",
                Text = "Loading...",
                Modulate = Colors.White,
                CustomMinimumSize = new Vector2(200, 50)
            };
            label.SetAnchorsPreset(Control.LayoutPreset.Center);
            _loadingAnimation.AddChild(label);

            // 添加加载指示器
            Label loadingIndicator = new()
            {
                Name = "LoadingIndicator",
                Text = "●",
                Modulate = Colors.White,
                CustomMinimumSize = new Vector2(50, 50)
            };
            loadingIndicator.SetAnchorsPreset(Control.LayoutPreset.Center);
            loadingIndicator.Position -= new Vector2(0, 100);
            _loadingAnimation.AddChild(loadingIndicator);

            // 默认隐藏加载动画
            _loadingAnimation.Visible = true;
            AddChild(_loadingAnimation);
        }

        // 显示加载动画
        public void ShowLoading()
        {
            if (_loadingAnimation != null)
            {
                _loadingAnimation.GetParent().MoveChild(_loadingAnimation, 999);
                _loadingAnimation.Visible = true;
                Log.Info("Loading animation shown");
            }
        }

        // 隐藏加载动画
        public void HideLoading()
        {
            if (_loadingAnimation != null)
            {
                _loadingAnimation.GetParent().MoveChild(_loadingAnimation, -1);
                _loadingAnimation.Visible = false;
                Log.Info("Loading animation hidden");
            }
        }

        // 检查加载动画是否可见
        public bool IsLoadingVisible()
        {
            return _loadingAnimation != null && _loadingAnimation.Visible;
        }
    }
}