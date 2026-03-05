using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Core.UI
{
    [GlobalClass]
    public partial class FocusablePanel : PanelContainer
    {
        public string BackgroundColorKey { get; set; } = "surface";
        public string BorderColorKey { get; set; } = "primary";
        public string CornerRadiusKey { get; set; } = "medium";
        public string AnimationDurationKey { get; set; } = "normal";

        private StyleBoxFlat _normalStyle;
        private StyleBoxFlat _focusStyle;
        private Tween _tween;

        public override void _Ready()
        {
            InitializeStyles();
            ConnectSignals();
            ApplyDesignTokens();
            
            FocusMode = FocusModeEnum.All; // Enable focus
        }

        private void InitializeStyles()
        {
            _normalStyle = new StyleBoxFlat();
            _focusStyle = new StyleBoxFlat();

            AddThemeStyleboxOverride("panel", _normalStyle);
        }

        private void ApplyDesignTokens()
        {
            Color baseColor = DesignSystem.GetColor(BackgroundColorKey);
            Color borderColor = DesignSystem.GetColor(BorderColorKey);
            int cornerRadius = DesignSystem.GetCornerRadius(CornerRadiusKey);

            // Normal Style
            _normalStyle.BgColor = baseColor;
            _normalStyle.SetCornerRadiusAll(cornerRadius);
            _normalStyle.BorderWidthBottom = 0;
            _normalStyle.BorderWidthLeft = 0;
            _normalStyle.BorderWidthRight = 0;
            _normalStyle.BorderWidthTop = 0;

            // Focus Style (same base but with border)
            _focusStyle.BgColor = baseColor;
            _focusStyle.SetCornerRadiusAll(cornerRadius);
            _focusStyle.BorderColor = borderColor;
            _focusStyle.BorderWidthBottom = 2;
            _focusStyle.BorderWidthLeft = 2;
            _focusStyle.BorderWidthRight = 2;
            _focusStyle.BorderWidthTop = 2;
        }

        private void ConnectSignals()
        {
            FocusEntered += OnFocusEnter;
            FocusExited += OnFocusExit;
            MouseEntered += OnHoverEnter;
            MouseExited += OnHoverExit;
        }

        private void OnFocusEnter()
        {
            AddThemeStyleboxOverride("panel", _focusStyle);
            AnimateScale(new Vector2(1.02f, 1.02f));
        }

        private void OnFocusExit()
        {
            AddThemeStyleboxOverride("panel", _normalStyle);
            AnimateScale(Vector2.One);
        }

        private void OnHoverEnter()
        {
            if (!HasFocus())
            {
                AnimateScale(new Vector2(1.01f, 1.01f));
            }
        }

        private void OnHoverExit()
        {
            if (!HasFocus())
            {
                AnimateScale(Vector2.One);
            }
        }

        private void AnimateScale(Vector2 targetScale)
        {
            if (_tween != null && _tween.IsRunning())
            {
                _tween.Kill();
            }

            _tween = CreateTween();
            float duration = DesignSystem.GetAnimationDuration(AnimationDurationKey);
            
            PivotOffset = Size / 2;
            
            _tween.TweenProperty(this, "scale", targetScale, duration)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
        }
    }
}
