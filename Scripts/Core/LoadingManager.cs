using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 加载管理器，作为autoload使用，处理场景加载动画
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为自动加载的单例使用，负责管理游戏中的加载动画。
    /// 它提供了显示和隐藏加载动画的方法，以及检查加载动画是否可见的功能。
    /// </remarks>
    public partial class LoadingManager : Node
    {
        /// <summary>
        /// 加载动画节点
        /// </summary>
        private Control _loadingAnimation;

        /// <summary>
        /// 加载管理器的单例实例
        /// </summary>
        /// <value>加载管理器的单例实例</value>
        public static LoadingManager Instance { get; private set; }

        /// <summary>
        /// 节点准备就绪时调用的方法
        /// </summary>
        /// <remarks>
        /// 该方法在节点进入场景树时自动调用，用于初始化单例实例和加载动画。
        /// </remarks>
        public override void _Ready()
        {
            Instance = this;
            InitializeLoadingAnimation();
        }

        /// <summary>
        /// 初始化加载动画
        /// </summary>
        /// <remarks>
        /// 该方法创建加载动画节点，包括背景遮罩、加载文本和加载指示器，并将其添加到场景中。
        /// 默认情况下，加载动画是可见的。
        /// </remarks>
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

        /// <summary>
        /// 显示加载动画
        /// </summary>
        /// <remarks>
        /// 该方法将加载动画节点移到场景树的最顶层并设置为可见，用于在场景切换或资源加载时显示加载状态。
        /// </remarks>
        public void ShowLoading()
        {
            if (_loadingAnimation != null)
            {
                _loadingAnimation.GetParent().MoveChild(_loadingAnimation, 999);
                _loadingAnimation.Visible = true;
                Log.Info("Loading animation shown");
            }
        }

        /// <summary>
        /// 隐藏加载动画
        /// </summary>
        /// <remarks>
        /// 该方法将加载动画节点移到场景树的底层并设置为不可见，用于在加载完成后隐藏加载状态。
        /// </remarks>
        public void HideLoading()
        {
            if (_loadingAnimation != null)
            {
                _loadingAnimation.GetParent().MoveChild(_loadingAnimation, -1);
                _loadingAnimation.Visible = false;
                Log.Info("Loading animation hidden");
            }
        }

        /// <summary>
        /// 检查加载动画是否可见
        /// </summary>
        /// <returns>加载动画是否可见</returns>
        /// <remarks>
        /// 该方法检查加载动画节点是否存在且可见，返回布尔值表示加载动画的可见状态。
        /// </remarks>
        public bool IsLoadingVisible()
        {
            return _loadingAnimation != null && _loadingAnimation.Visible;
        }
    }
}