using Godot;
using System;

namespace hd2dtest.Scripts.Player
{
    public partial class Player : CharacterBody3D
    {
        private float _speed = 5.0f;
        private bool _canMove = true;
        private bool _gravityEnabled = true;
        private const float GRAVITY_ACCELERATION = 40.0f;

        [Signal]
        public delegate void MovementPausedEventHandler();

        [Signal]
        public delegate void MovementResumedEventHandler();

        public override void _Ready()
        {
            // 连接信号
            InputMap.AddAction("pause_movement");
            InputMap.ActionAddEvent("pause_movement", new InputEventKey() { Keycode = Key.Space });
            InputMap.AddAction("resume_movement");
            InputMap.ActionAddEvent("resume_movement", new InputEventKey() { Keycode = Key.Enter });
            // 添加重力开关动作
            InputMap.AddAction("toggle_gravity");
            InputMap.ActionAddEvent("toggle_gravity", new InputEventKey() { Keycode = Key.G });
        }

        public override void _PhysicsProcess(double delta)
        {
            // 处理输入信号
            if (Input.IsActionJustPressed("pause_movement"))
            {
                PauseMovement();
            }
            else if (Input.IsActionJustPressed("resume_movement"))
            {
                ResumeMovement();
            }
            // 切换重力开关
            if (Input.IsActionJustPressed("toggle_gravity"))
            {
                _gravityEnabled = !_gravityEnabled;
                if (!_gravityEnabled)
                {
                    Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                }
            }

            // 手动应用重力
            if (_gravityEnabled)
            {
                Velocity = new Vector3(Velocity.X, Velocity.Y - GRAVITY_ACCELERATION * (float)delta, Velocity.Z);
            }

            // 如果可以移动，处理WASD移动
            if (_canMove)
            {
                HandleMovement(delta);
            }
            else
            {
                // 停止水平移动
                Velocity = new Vector3(0, Velocity.Y, 0);
            }

            // 应用移动和碰撞
            MoveAndSlide();
        }

        private void HandleMovement(double delta)
        {
            var direction = Vector3.Zero;

            // WASD移动 - 使用标准的ui_前缀动作名称
            if (Input.IsActionPressed("ui_up"))
                direction.X -= 1;
            if (Input.IsActionPressed("ui_down"))
                direction.X += 1;
            if (Input.IsActionPressed("ui_left"))
                direction.Z += 1;
            if (Input.IsActionPressed("ui_right"))
                direction.Z -= 1;

            // 归一化方向向量
            if (direction.Length() > 0)
            {
                direction = direction.Normalized();
                // 应用速度
                Velocity = new Vector3(direction.X * _speed, Velocity.Y, direction.Z * _speed);
            }
            else
            {
                // 停止水平移动
                Velocity = new Vector3(0, Velocity.Y, 0);
            }
        }

        public void PauseMovement()
        {
            _canMove = false;
            EmitSignal(SignalName.MovementPaused);
        }

        public void ResumeMovement()
        {
            _canMove = true;
            EmitSignal(SignalName.MovementResumed);
        }

        // 外部调用方法，用于控制移动状态
        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
        }

        public bool CanMove
        {
            get { return _canMove; }
            set { _canMove = value; }
        }
    }
}
