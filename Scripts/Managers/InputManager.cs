/*
 * File: InputManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-17
 *
 * Purpose:
 * 输入管理器，作为全局单例负责统一管理所有游戏输入动作。
 * 支持键盘和手柄双输入，允许自定义键盘按键绑定。
 *
 * Key Features:
 * - 注册 15 个游戏动作到 InputMap，含默认键盘+手柄绑定
 * - 读取 ConfigManager 的自定义按键绑定并应用到 InputMap
 * - 改键功能：清除指定动作的键盘事件，添加新按键（手柄事件不变）
 * - 提供 GetActionLabel() 获取当前按键显示名
 */

using Godot;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    public partial class InputManager : Node
    {
        public static InputManager Instance { get; private set; }

        /// <summary>所有游戏动作名称列表</summary>
        public static readonly string[] Actions =
        {
            "move_left", "move_right", "move_up", "move_down",
            "sprint", "interact", "cancel", "menu", "map",
            "quick_action_1", "quick_action_2", "quick_action_3", "quick_action_4",
            "camera_zoom_in", "camera_zoom_out"
        };

        /// <summary>动作 → 默认键盘按键</summary>
        private static readonly Dictionary<string, Key> DefaultKeyboardBindings = new()
        {
            { "move_left", Key.A },
            { "move_right", Key.D },
            { "move_up", Key.W },
            { "move_down", Key.S },
            { "sprint", Key.Shift },
            { "interact", Key.E },
            { "cancel", Key.Escape },
            { "menu", Key.Tab },
            { "map", Key.M },
            { "quick_action_1", Key.Key1 },
            { "quick_action_2", Key.Key2 },
            { "quick_action_3", Key.Key3 },
            { "quick_action_4", Key.Key4 },
        };

        /// <summary>存储当前自定义按键绑定（键盘事件被修改过的动作）</summary>
        private Dictionary<string, Key> _customKeyBindings = new();

        /// <summary>所有动作的默认键盘事件缓存（用于重置）</summary>
        private Dictionary<string, List<InputEventKey>> _defaultKeyboardEvents = new();

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                RegisterAllDefaultActions();
                CacheDefaultKeyboardEvents();
                ApplyCustomKeyBindings();
                Log.Info("InputManager initialized");
            }
            else
            {
                QueueFree();
            }
        }

        #region Initialization

        /// <summary>注册全部 15 个动作的默认键盘+手柄绑定到 InputMap</summary>
        private void RegisterAllDefaultActions()
        {
            RegisterMovementActions();
            RegisterSprintAction();
            RegisterInteractAction();
            RegisterCancelAction();
            RegisterMenuAction();
            RegisterMapAction();
            RegisterQuickActions();
            RegisterCameraZoomActions();
        }

        private void RegisterMovementActions()
        {
            EnsureAction("move_left");
            AddKeyboardEvent("move_left", Key.A);
            AddJoypadAxisEvent("move_left", JoyAxis.LeftX, -1.0f);  // Left stick left
            AddJoypadButtonEvent("move_left", JoyButton.DpadLeft);

            EnsureAction("move_right");
            AddKeyboardEvent("move_right", Key.D);
            AddJoypadAxisEvent("move_right", JoyAxis.LeftX, 1.0f);   // Left stick right
            AddJoypadButtonEvent("move_right", JoyButton.DpadRight);

            EnsureAction("move_up");
            AddKeyboardEvent("move_up", Key.W);
            AddJoypadAxisEvent("move_up", JoyAxis.LeftY, -1.0f);     // Left stick up
            AddJoypadButtonEvent("move_up", JoyButton.DpadUp);

            EnsureAction("move_down");
            AddKeyboardEvent("move_down", Key.S);
            AddJoypadAxisEvent("move_down", JoyAxis.LeftY, 1.0f);    // Left stick down
            AddJoypadButtonEvent("move_down", JoyButton.DpadDown);
        }

        private void RegisterSprintAction()
        {
            EnsureAction("sprint");
            AddKeyboardEvent("sprint", Key.Shift);
            AddJoypadButtonEvent("sprint", JoyButton.LeftShoulder);
        }

        private void RegisterInteractAction()
        {
            EnsureAction("interact");
            AddKeyboardEvent("interact", Key.E);
            AddJoypadButtonEvent("interact", JoyButton.A);
        }

        private void RegisterCancelAction()
        {
            EnsureAction("cancel");
            AddKeyboardEvent("cancel", Key.Escape);
            AddJoypadButtonEvent("cancel", JoyButton.B);
        }

        private void RegisterMenuAction()
        {
            EnsureAction("menu");
            AddKeyboardEvent("menu", Key.Tab);
            AddJoypadButtonEvent("menu", JoyButton.Start);
        }

        private void RegisterMapAction()
        {
            EnsureAction("map");
            AddKeyboardEvent("map", Key.M);
            AddJoypadButtonEvent("map", JoyButton.Back);
        }

        private void RegisterQuickActions()
        {
            EnsureAction("quick_action_1");
            AddKeyboardEvent("quick_action_1", Key.Key1);
            AddJoypadButtonEvent("quick_action_1", JoyButton.DpadUp);

            EnsureAction("quick_action_2");
            AddKeyboardEvent("quick_action_2", Key.Key2);
            AddJoypadButtonEvent("quick_action_2", JoyButton.DpadDown);

            EnsureAction("quick_action_3");
            AddKeyboardEvent("quick_action_3", Key.Key3);
            AddJoypadButtonEvent("quick_action_3", JoyButton.DpadLeft);

            EnsureAction("quick_action_4");
            AddKeyboardEvent("quick_action_4", Key.Key4);
            AddJoypadButtonEvent("quick_action_4", JoyButton.DpadRight);
        }

        private void RegisterCameraZoomActions()
        {
            EnsureAction("camera_zoom_in");
            AddMouseWheelEvent("camera_zoom_in", MouseButton.WheelUp);
            AddJoypadButtonEvent("camera_zoom_in", JoyButton.RightShoulder);

            EnsureAction("camera_zoom_out");
            AddMouseWheelEvent("camera_zoom_out", MouseButton.WheelDown);
            AddJoypadButtonEvent("camera_zoom_out", JoyButton.LeftShoulder);
        }

        #endregion

        #region Event Helpers

        private static void EnsureAction(string action)
        {
            if (!InputMap.HasAction(action))
                InputMap.AddAction(action);
        }

        private static void AddKeyboardEvent(string action, Key key)
        {
            var ev = new InputEventKey { Keycode = key };
            InputMap.ActionAddEvent(action, ev);
        }

        private static void AddJoypadButtonEvent(string action, JoyButton button)
        {
            var ev = new InputEventJoypadButton { ButtonIndex = button };
            InputMap.ActionAddEvent(action, ev);
        }

        private static void AddJoypadAxisEvent(string action, JoyAxis axis, float axisValue)
        {
            var ev = new InputEventJoypadMotion { Axis = axis, AxisValue = axisValue };
            InputMap.ActionAddEvent(action, ev);
        }

        private static void AddMouseWheelEvent(string action, MouseButton button)
        {
            var ev = new InputEventMouseButton
            {
                ButtonIndex = button,
                Pressed = true
            };
            InputMap.ActionAddEvent(action, ev);
        }

        #endregion

        #region Cache & Apply

        /// <summary>缓存所有动作的默认键盘事件，供重置使用</summary>
        private void CacheDefaultKeyboardEvents()
        {
            foreach (var action in Actions)
            {
                var keyEvents = new List<InputEventKey>();
                foreach (var ev in InputMap.ActionGetEvents(action))
                {
                    if (ev is InputEventKey keyEvent)
                        keyEvents.Add((InputEventKey)keyEvent.Duplicate());
                }
                _defaultKeyboardEvents[action] = keyEvents;
            }
        }

        /// <summary>从 ConfigManager 读取自定义按键绑定并应用到 InputMap</summary>
        private void ApplyCustomKeyBindings()
        {
            var cm = ConfigManager.Instance;
            if (cm?.CurrentConfig?.KeyBindings == null) return;

            foreach (var kv in cm.CurrentConfig.KeyBindings)
            {
                string action = kv.Key;
                string keyText = kv.Value;

                if (string.IsNullOrEmpty(keyText)) continue;

                // Try to parse stored key text back to a Key enum
                Key key = KeyFromString(keyText);
                if (key != Key.Unknown && DefaultKeyboardBindings.ContainsKey(action))
                {
                    // Only apply if different from default
                    if (key != DefaultKeyboardBindings[action])
                    {
                        ApplyRebindInternal(action, key);
                    }
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>为指定动作重新绑定键盘按键（保留手柄/MouseWheel 事件）</summary>
        public void RebindAction(string action, Key key)
        {
            if (!InputMap.HasAction(action)) return;

            ApplyRebindInternal(action, key);
            _customKeyBindings[action] = key;

            // Persist to ConfigManager
            string keyText = OS.GetKeycodeString(key);
            ConfigManager.Instance?.SetKeyBinding(action, keyText);

            Log.Info($"Key rebound: {action} → {keyText}");
        }

        /// <summary>重置指定动作为默认键盘按键</summary>
        public void ResetActionBinding(string action)
        {
            if (!InputMap.HasAction(action)) return;

            // Remove current keyboard events
            RemoveAllKeyboardEvents(action);

            // Restore default keyboard events
            if (_defaultKeyboardEvents.TryGetValue(action, out var defaultEvents))
            {
                foreach (var ev in defaultEvents)
                    InputMap.ActionAddEvent(action, ev);
            }

            _customKeyBindings.Remove(action);

            // Update ConfigManager with default key text
            if (DefaultKeyboardBindings.TryGetValue(action, out Key defaultKey))
            {
                string keyText = OS.GetKeycodeString(defaultKey);
                ConfigManager.Instance?.SetKeyBinding(action, keyText);
            }

            Log.Info($"Key binding reset: {action}");
        }

        /// <summary>重置所有动作为默认按键</summary>
        public void ResetAllBindings()
        {
            foreach (var action in Actions)
            {
                ResetActionBinding(action);
            }
            _customKeyBindings.Clear();
            Log.Info("All key bindings reset to defaults");
        }

        /// <summary>获取指定动作的当前按键显示名（用于 UI 显示）</summary>
        public string GetActionLabel(string action)
        {
            if (_customKeyBindings.TryGetValue(action, out Key customKey))
                return OS.GetKeycodeString(customKey);

            if (DefaultKeyboardBindings.TryGetValue(action, out Key defaultKey))
                return OS.GetKeycodeString(defaultKey);

            return "---";
        }

        /// <summary>检查指定动作当前是否使用自定义绑定</summary>
        public bool IsActionRebound(string action)
        {
            return _customKeyBindings.ContainsKey(action);
        }

        /// <summary>获取所有可改键的动作列表（排除滚轮类动作）</summary>
        public static List<string> GetRebindableActions()
        {
            return Actions.Where(a => DefaultKeyboardBindings.ContainsKey(a)).ToList();
        }

        #endregion

        #region Internal

        /// <summary>清除指定动作的所有键盘事件，添加新按键</summary>
        private static void ApplyRebindInternal(string action, Key key)
        {
            RemoveAllKeyboardEvents(action);

            var ev = new InputEventKey { Keycode = key };
            InputMap.ActionAddEvent(action, ev);
        }

        /// <summary>移除指定动作的所有 InputEventKey 事件</summary>
        private static void RemoveAllKeyboardEvents(string action)
        {
            var events = InputMap.ActionGetEvents(action);
            foreach (var ev in events)
            {
                if (ev is InputEventKey)
                    InputMap.ActionEraseEvent(action, ev);
            }
        }

        /// <summary>从字符串解析 Key 枚举（OS.GetKeycodeString 的逆操作）</summary>
        private static Key KeyFromString(string keyText)
        {
            if (string.IsNullOrEmpty(keyText)) return Key.Unknown;

            // Try common key names
            foreach (Key key in System.Enum.GetValues(typeof(Key)))
            {
                if (OS.GetKeycodeString(key) == keyText)
                    return key;
            }

            return Key.Unknown;
        }

        #endregion
    }
}
