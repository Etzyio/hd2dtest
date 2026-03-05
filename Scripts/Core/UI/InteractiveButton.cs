using Godot;
using System;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Core.UI
{
    [GlobalClass]
    public partial class InteractiveButton : Button
    {
         public string BackgroundColorKey { get; set; } = "primary";
         public string TextColorKey { get; set; } = "text_primary";
         public string CornerRadiusKey { get; set; } = "medium";
         public string AnimationDurationKey { get; set; } = "normal";

        private StyleBoxFlat _normalStyle;
        private StyleBoxFlat _hoverStyle;
        private StyleBoxFlat _pressedStyle;
        private StyleBoxFlat _disabledStyle;

        private Tween _tween;

        public override void _Ready()
        {
            InitializeStyles();
            ConnectSignals();
            ApplyDesignTokens();
        }

        private void InitializeStyles()
        {
            // Create StyleBoxFlat instances
            _normalStyle = new StyleBoxFlat();
            _hoverStyle = new StyleBoxFlat();
            _pressedStyle = new StyleBoxFlat();
            _disabledStyle = new StyleBoxFlat();

            // Set base properties
            AddThemeStyleboxOverride("normal", _normalStyle);
            AddThemeStyleboxOverride("hover", _hoverStyle);
            AddThemeStyleboxOverride("pressed", _pressedStyle);
            AddThemeStyleboxOverride("disabled", _disabledStyle);
            AddThemeStyleboxOverride("focus", _hoverStyle); // Use hover style for focus for now
        }

        private void ApplyDesignTokens()
        {
            // Apply Colors
            Color baseColor = DesignSystem.GetColor(BackgroundColorKey);
            Color textColor = DesignSystem.GetColor(TextColorKey);
            int cornerRadius = DesignSystem.GetCornerRadius(CornerRadiusKey);

            // Normal Style
            _normalStyle.BgColor = baseColor;
            _normalStyle.SetCornerRadiusAll(cornerRadius);

            // Hover Style (lighter)
            _hoverStyle.BgColor = baseColor.Lightened(0.1f);
            _hoverStyle.SetCornerRadiusAll(cornerRadius);

            // Pressed Style (darker)
            _pressedStyle.BgColor = baseColor.Darkened(0.1f);
            _pressedStyle.SetCornerRadiusAll(cornerRadius);

            // Disabled Style (gray)
            _disabledStyle.BgColor = new Color(0.5f, 0.5f, 0.5f);
            _disabledStyle.SetCornerRadiusAll(cornerRadius);

            // Text Color
            AddThemeColorOverride("font_color", textColor);
            AddThemeColorOverride("font_hover_color", textColor);
            AddThemeColorOverride("font_pressed_color", textColor);
            AddThemeColorOverride("font_disabled_color", new Color(0.7f, 0.7f, 0.7f));
        }

        private void ConnectSignals()
        {
            MouseEntered += OnHoverEnter;
            MouseExited += OnHoverExit;
            ButtonDown += OnPressed;
            ButtonUp += OnReleased;
            FocusEntered += OnFocusEnter;
            FocusExited += OnFocusExit;
        }

        private void OnHoverEnter()
        {
            if (Disabled) return;
            AnimateScale(new Vector2(1.05f, 1.05f));
            // Play Hover Sound
        }

        private void OnHoverExit()
        {
            if (Disabled) return;
            AnimateScale(Vector2.One);
        }

        private void OnPressed()
        {
            if (Disabled) return;
            AnimateScale(new Vector2(0.95f, 0.95f));
            // Play Click Sound
        }

        private void OnReleased()
        {
            if (Disabled) return;
            AnimateScale(new Vector2(1.05f, 1.05f)); // Return to hover scale
        }

        private void OnFocusEnter()
        {
            OnHoverEnter();
        }

        private void OnFocusExit()
        {
            OnHoverExit();
        }

        private void AnimateScale(Vector2 targetScale)
        {
            if (_tween != null && _tween.IsRunning())
            {
                _tween.Kill();
            }

            _tween = CreateTween();
            float duration = DesignSystem.GetAnimationDuration(AnimationDurationKey);
            
            // Pivot center for scaling
            PivotOffset = Size / 2;
            
            _tween.TweenProperty(this, "scale", targetScale, duration)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
        }
    }
}
