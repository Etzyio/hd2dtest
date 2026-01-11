using Godot;
using System;

namespace hd2dtest.Scripts.Player
{
    public partial class PlayerController : CharacterBody2D
    {
        [Export]
        public float Speed = 100.0f;

        [Export]
        public float JumpVelocity = -300.0f;

        private AnimatedSprite2D _animatedSprite;
        private Vector2 _direction;

        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        public override void _Process(double delta)
        {
            _direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

            if (_direction != Vector2.Zero)
            {
                _animatedSprite.Play("run");
                _animatedSprite.FlipH = _direction.X < 0;
            }
            else
            {
                _animatedSprite.Play("idle");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 velocity = Velocity;

            // 处理重力
            if (!IsOnFloor())
            {
                velocity.Y += GetGravity().Y * (float)delta;
            }

            // 处理跳跃
            if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            {
                velocity.Y = JumpVelocity;
            }

            // 处理移动
            velocity.X = _direction.X * Speed;

            Velocity = velocity;
            MoveAndSlide();
        }
    }
}