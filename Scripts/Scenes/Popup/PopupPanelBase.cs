/*
 * File: PopupPanelBase.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 弹出菜单面板的共享基类，提供统一的配色方案、字体加载和UI工厂方法。
 * 所有具体的功能面板（物品、地图、人物、技能、设置、存档）均继承此类。
 *
 * Key Features:
 * - 统一的深色主题配色常量
 * - 懒加载字体（QiushuiShotaiBright.ttf）
 * - 15+ UI工厂方法（标题、标签、按钮、进度条、滑块等）
 * - ClearChildren 和 GetPlayer 工具方法
 */

using Godot;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scenes.Popup
{
    /// <summary>
    /// 弹出菜单面板基类，提供共享的UI样式和工厂方法
    /// </summary>
    public partial class PopupPanelBase : Control
    {
        #region Color Palette

        protected static readonly Color ColorBg = new(0, 0, 0, 0.9f);
        protected static readonly Color ColorSurface = new(0.13f, 0.19f, 0.25f, 0.95f);
        protected static readonly Color ColorPrimary = new(0.2f, 0.6f, 0.86f, 1f);
        protected static readonly Color ColorDanger = new(0.9f, 0.25f, 0.25f, 1f);
        protected static readonly Color ColorSuccess = new(0.25f, 0.8f, 0.35f, 1f);
        protected static readonly Color ColorWarning = new(1f, 0.75f, 0.2f, 1f);
        protected static readonly Color ColorTextPrimary = new(0.93f, 0.94f, 0.95f, 1f);
        protected static readonly Color ColorTextSecondary = new(0.6f, 0.65f, 0.7f, 1f);
        protected static readonly Color ColorAttack = new(0.9f, 0.3f, 0.25f, 1f);
        protected static readonly Color ColorDefense = new(0.3f, 0.55f, 0.9f, 1f);
        protected static readonly Color ColorHealing = new(0.25f, 0.8f, 0.35f, 1f);
        protected static readonly Color ColorSupport = new(0.9f, 0.75f, 0.2f, 1f);

        #endregion

        #region Font

        private static Font _font;

        protected static Font GetFont()
        {
            return _font ??= GD.Load<Font>("res://Resources/Font/QiushuiShotai Bright/QiushuiShotaiBright.ttf");
        }

        #endregion

        #region Panel Base

        protected Control CreatePanelBase(string name)
        {
            var panel = new Control
            {
                Name = name,
                Visible = false,
                LayoutMode = 1,
                AnchorLeft = 0.05f, AnchorTop = 0.05f,
                AnchorRight = 0.95f, AnchorBottom = 0.95f,
                MouseFilter = MouseFilterEnum.Stop
            };
            var bg = new ColorRect
            {
                LayoutMode = 1,
                AnchorLeft = 0, AnchorTop = 0,
                AnchorRight = 1, AnchorBottom = 1,
                Color = ColorBg
            };
            panel.AddChild(bg);
            return panel;
        }

        protected Control CreateSurfacePanel()
        {
            var panel = new PanelContainer();
            panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = ColorSurface,
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.5f),
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 10, ContentMarginTop = 8,
                ContentMarginRight = 10, ContentMarginBottom = 8
            });
            return panel;
        }

        protected StyleBoxFlat CreateSlotStyle(Color bgColor)
        {
            return new StyleBoxFlat
            {
                BgColor = bgColor,
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.5f),
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ContentMarginLeft = 8, ContentMarginTop = 4,
                ContentMarginRight = 8, ContentMarginBottom = 4
            };
        }

        #endregion

        #region Labels

        protected Label CreateTitleLabel(string text)
        {
            var label = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center };
            label.AddThemeFontOverride("font", GetFont());
            label.AddThemeFontSizeOverride("font_size", 22);
            label.AddThemeColorOverride("font_color", ColorTextPrimary);
            return label;
        }

        protected Label CreateInfoLabel(string text)
        {
            var label = new Label { Text = text };
            label.AddThemeFontOverride("font", GetFont());
            label.AddThemeFontSizeOverride("font_size", 14);
            label.AddThemeColorOverride("font_color", ColorTextSecondary);
            return label;
        }

        protected Label CreateSectionLabel(string text)
        {
            var label = new Label { Text = text };
            label.AddThemeFontOverride("font", GetFont());
            label.AddThemeFontSizeOverride("font_size", 18);
            label.AddThemeColorOverride("font_color", ColorPrimary);
            return label;
        }

        protected HSeparator CreateSeparator()
        {
            return new HSeparator();
        }

        #endregion

        #region Buttons

        protected Button CreateMenuButton(string text)
        {
            var btn = new Button { Text = text, CustomMinimumSize = new Vector2(220, 56) };
            ApplyButtonStyle(btn);
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 18);
            return btn;
        }

        protected Button CreatePrimaryButton(string text)
        {
            var btn = new Button { Text = text, CustomMinimumSize = new Vector2(160, 44) };
            ApplyPrimaryButtonStyle(btn);
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 15);
            return btn;
        }

        protected Button CreateSecondaryButton(string text)
        {
            var btn = new Button { Text = text, CustomMinimumSize = new Vector2(140, 40) };
            ApplyButtonStyle(btn);
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 14);
            return btn;
        }

        protected Button CreateSmallButton(string text)
        {
            var btn = new Button { Text = text, CustomMinimumSize = new Vector2(80, 30) };
            ApplyButtonStyle(btn);
            btn.AddThemeFontOverride("font", GetFont());
            btn.AddThemeFontSizeOverride("font_size", 12);
            return btn;
        }

        protected void ApplyButtonStyle(Button btn)
        {
            var normal = new StyleBoxFlat
            {
                BgColor = ColorSurface,
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.4f, 0.5f, 0.5f),
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 12, ContentMarginTop = 6,
                ContentMarginRight = 12, ContentMarginBottom = 6
            };
            btn.AddThemeStyleboxOverride("normal", normal);

            var hover = (StyleBoxFlat)normal.Duplicate();
            hover.BgColor = new Color(0.2f, 0.28f, 0.36f, 0.95f);
            hover.BorderColor = new Color(0.4f, 0.55f, 0.7f, 0.7f);
            btn.AddThemeStyleboxOverride("hover", hover);

            var pressed = (StyleBoxFlat)normal.Duplicate();
            pressed.BgColor = new Color(0.1f, 0.15f, 0.2f, 0.95f);
            btn.AddThemeStyleboxOverride("pressed", pressed);

            var disabled = (StyleBoxFlat)normal.Duplicate();
            disabled.BgColor = new Color(0.08f, 0.08f, 0.08f, 0.6f);
            btn.AddThemeStyleboxOverride("disabled", disabled);
        }

        protected void ApplyPrimaryButtonStyle(Button btn)
        {
            var normal = new StyleBoxFlat
            {
                BgColor = ColorPrimary * new Color(0.7f, 0.7f, 0.7f, 1f),
                BorderWidthLeft = 2, BorderWidthTop = 2,
                BorderWidthRight = 2, BorderWidthBottom = 2,
                BorderColor = ColorPrimary,
                CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 16, ContentMarginTop = 6,
                ContentMarginRight = 16, ContentMarginBottom = 6
            };
            btn.AddThemeStyleboxOverride("normal", normal);

            var hover = (StyleBoxFlat)normal.Duplicate();
            hover.BgColor = ColorPrimary;
            btn.AddThemeStyleboxOverride("hover", hover);

            var pressed = (StyleBoxFlat)normal.Duplicate();
            pressed.BgColor = ColorPrimary * new Color(0.5f, 0.5f, 0.5f, 1f);
            btn.AddThemeStyleboxOverride("pressed", pressed);

            var disabled = (StyleBoxFlat)normal.Duplicate();
            disabled.BgColor = new Color(0.08f, 0.08f, 0.08f, 0.6f);
            disabled.BorderColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            btn.AddThemeStyleboxOverride("disabled", disabled);
        }

        #endregion

        #region Progress Bars

        protected ProgressBar CreateProgressBar(Color fillColor)
        {
            var bar = new ProgressBar
            {
                Value = 100, MaxValue = 100,
                ShowPercentage = false,
                CustomMinimumSize = new Vector2(0, 22),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            var bgStyle = new StyleBoxFlat
            {
                BgColor = fillColor * new Color(0.2f, 0.2f, 0.2f, 1f),
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6
            };
            bar.AddThemeStyleboxOverride("background", bgStyle);
            var fillStyle = new StyleBoxFlat
            {
                BgColor = fillColor,
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6
            };
            bar.AddThemeStyleboxOverride("fill", fillStyle);
            return bar;
        }

        protected ProgressBar CreateSmallBar(Color fillColor)
        {
            var bar = new ProgressBar { Value = 100, MaxValue = 100, ShowPercentage = false };
            var bgStyle = new StyleBoxFlat
            {
                BgColor = fillColor * new Color(0.15f, 0.15f, 0.15f, 1f),
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4
            };
            bar.AddThemeStyleboxOverride("background", bgStyle);
            var fillStyle = new StyleBoxFlat
            {
                BgColor = fillColor,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4
            };
            bar.AddThemeStyleboxOverride("fill", fillStyle);
            return bar;
        }

        #endregion

        #region Settings Controls

        protected HSlider CreateSettingSliderFloat(string labelText, double maxValue, double step, System.Func<double, string> formatValue, VBoxContainer parent)
        {
            var hbox = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var label = new Label { Text = labelText, CustomMinimumSize = new Vector2(140, 0) };
            label.AddThemeFontOverride("font", GetFont());
            label.AddThemeFontSizeOverride("font_size", 14);
            label.AddThemeColorOverride("font_color", ColorTextSecondary);
            hbox.AddChild(label);

            var slider = new HSlider
            {
                MinValue = 0, MaxValue = maxValue, Step = step,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            hbox.AddChild(slider);

            var valueLabel = new Label { Text = formatValue(slider.Value), CustomMinimumSize = new Vector2(50, 0) };
            valueLabel.AddThemeFontOverride("font", GetFont());
            valueLabel.AddThemeFontSizeOverride("font_size", 13);
            valueLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
            slider.ValueChanged += (v) => valueLabel.Text = formatValue(v);
            hbox.AddChild(valueLabel);

            parent.AddChild(hbox);
            return slider;
        }

        protected HSlider CreateSettingSlider(string labelText, double maxValue, VBoxContainer parent)
        {
            var hbox = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var label = new Label { Text = labelText, CustomMinimumSize = new Vector2(140, 0) };
            label.AddThemeFontOverride("font", GetFont());
            label.AddThemeFontSizeOverride("font_size", 14);
            label.AddThemeColorOverride("font_color", ColorTextSecondary);
            hbox.AddChild(label);

            var slider = new HSlider
            {
                MinValue = 0, MaxValue = maxValue, Step = 1,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            hbox.AddChild(slider);

            var valueLabel = new Label { Text = $"{(int)slider.Value}", CustomMinimumSize = new Vector2(40, 0) };
            valueLabel.AddThemeFontOverride("font", GetFont());
            valueLabel.AddThemeFontSizeOverride("font_size", 13);
            valueLabel.AddThemeColorOverride("font_color", ColorTextPrimary);
            slider.ValueChanged += (v) => valueLabel.Text = $"{(int)v}";
            hbox.AddChild(valueLabel);

            parent.AddChild(hbox);
            return slider;
        }

        protected CheckButton CreateSettingCheck(string labelText, VBoxContainer parent)
        {
            var hbox = new HBoxContainer();
            var check = new CheckButton { Text = labelText };
            check.AddThemeFontOverride("font", GetFont());
            check.AddThemeFontSizeOverride("font_size", 14);
            check.AddThemeColorOverride("font_color", ColorTextSecondary);
            hbox.AddChild(check);
            parent.AddChild(hbox);
            return check;
        }

        protected OptionButton CreateSettingOption(string labelText, string[] options, VBoxContainer parent)
        {
            var hbox = new HBoxContainer();
            var label = new Label { Text = labelText, CustomMinimumSize = new Vector2(140, 0) };
            label.AddThemeFontOverride("font", GetFont());
            label.AddThemeFontSizeOverride("font_size", 14);
            label.AddThemeColorOverride("font_color", ColorTextSecondary);
            hbox.AddChild(label);

            var optionBtn = new OptionButton();
            foreach (var opt in options)
                optionBtn.AddItem(opt);
            hbox.AddChild(optionBtn);

            parent.AddChild(hbox);
            return optionBtn;
        }

        #endregion

        #region Inventory Colors

        protected Color GetItemTypeColor(Item.ItemType type) => type switch
        {
            Item.ItemType.Consumable => ColorSuccess,
            Item.ItemType.Material => new Color(0.6f, 0.6f, 0.6f, 1f),
            Item.ItemType.KeyItem => ColorWarning,
            Item.ItemType.QuestItem => new Color(0.7f, 0.4f, 0.9f, 1f),
            _ => ColorTextSecondary
        };

        #endregion

        #region Utilities

        protected static void ClearChildren(Control parent)
        {
            foreach (Node child in parent.GetChildren())
                child.QueueFree();
        }

        protected Player GetPlayer()
        {
            return GameDataManager.Instance?.Teammates?.Player
                ?? GameManager.Instance?.Teammates?.Player;
        }

        #endregion
    }
}
