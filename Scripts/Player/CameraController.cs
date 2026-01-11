using Godot;
using System;

namespace hd2dtest.Scripts.Player
{
    public partial class CameraController : Camera2D
    {
        [Export]
        public Node2D Target;

        [Export]
        public float SmoothSpeed = 0.1f;

        [Export]
        public new Vector2 Offset = new(0, -50);

        private Vector2 _targetPosition;

        public override void _Ready()
        {
            if (Target != null)
            {
                Position = Target.Position + Offset;
            }
        }

        public override void _Process(double delta)
        {
            if (Target == null)
                return;

            _targetPosition = Target.Position + Offset;
            Position = Position.Lerp(_targetPosition, SmoothSpeed);
        }
    }
}