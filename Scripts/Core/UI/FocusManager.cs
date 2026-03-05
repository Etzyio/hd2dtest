using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Core.UI
{
    /// <summary>
    /// Manages UI Focus navigation for Gamepad/Keyboard support.
    /// Handles focus history, layer stacking, and input redirection.
    /// </summary>
    public partial class FocusManager : Node
    {
        public static FocusManager Instance { get; private set; }

        private Stack<Control> _focusStack = new Stack<Control>();
        private Control _lastFocusedControl;

        public override void _Ready()
        {
            Instance = this;
            ProcessMode = ProcessModeEnum.Always; // Ensure it runs even when game is paused
        }

        public void PushFocusLayer(Control root)
        {
            if (_lastFocusedControl != null && IsInstanceValid(_lastFocusedControl))
            {
                _focusStack.Push(_lastFocusedControl);
            }

            // Find first focusable child in new root
            Control firstFocusable = FindFirstFocusable(root);
            if (firstFocusable != null)
            {
                firstFocusable.GrabFocus();
                _lastFocusedControl = firstFocusable;
                Log.Info($"Focus pushed to: {firstFocusable.Name}");
            }
            else
            {
                Log.Warning($"No focusable element found in {root.Name}");
            }
        }

        public void PopFocusLayer()
        {
            if (_focusStack.Count > 0)
            {
                Control previousFocus = _focusStack.Pop();
                if (IsInstanceValid(previousFocus) && previousFocus.IsVisibleInTree())
                {
                    previousFocus.GrabFocus();
                    _lastFocusedControl = previousFocus;
                    Log.Info($"Focus popped back to: {previousFocus.Name}");
                }
                else
                {
                    // If previous focus is invalid, try to find a valid one or just release
                    Log.Warning("Previous focus target is invalid or hidden.");
                    // Fallback logic could go here
                }
            }
        }

        public void RegisterFocus(Control control)
        {
            _lastFocusedControl = control;
        }

        private Control FindFirstFocusable(Control parent)
        {
            if (parent.FocusMode != Control.FocusModeEnum.None && parent.IsVisibleInTree())
            {
                return parent;
            }

            foreach (Node child in parent.GetChildren())
            {
                if (child is Control childControl)
                {
                    Control result = FindFirstFocusable(childControl);
                    if (result != null) return result;
                }
            }
            return null;
        }

        public override void _Input(InputEvent @event)
        {
            // Optional: Handle custom navigation if Godot's built-in isn't enough
            // e.g. Tab cycling or specific shortcuts
        }
    }
}
