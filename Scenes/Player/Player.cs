using Godot;

namespace hd2dtest.Scripts.Player
{
    public partial class Player : CharacterBody3D
    {
        private float _speed = 5.0f;
        private bool _gravityEnabled = true;
        private const float GRAVITY_ACCELERATION = 40.0f;
        private readonly bool[] _disabledDirections = new bool[4]; // 0: up, 1: down, 2: left, 3: right

        [Signal]
        public delegate void MovementPausedEventHandler();

        [Signal]
        public delegate void MovementResumedEventHandler();

        [Signal]
        public delegate void DirectionDisabledEventHandler(int directionIndex);

        [Signal]
        public delegate void DirectionEnabledEventHandler(int directionIndex);

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
                Velocity = new Vector3(Velocity.X, Velocity.Y - (GRAVITY_ACCELERATION * (float)delta), Velocity.Z);
            }

            // 如果可以移动，处理WASD移动
            if (CanMove)
            {
                HandleMovement(delta);
            }
            else
            {
                // 停止水平移动
                Velocity = new Vector3(0, Velocity.Y, 0);
            }

            // 应用移动和碰撞
            _ = MoveAndSlide();
        }

        private void HandleMovement(double delta)
        {
            Vector3 direction = Vector3.Zero;
            // WASD移动 - 使用标准的ui_前缀动作名称
            if (!_disabledDirections[2] && Input.IsActionPressed("ui_up"))
            {
                direction.X -= 1;
            }

            if (!_disabledDirections[3] && Input.IsActionPressed("ui_down"))
            {
                direction.X += 1;
            }

            if (!_disabledDirections[0] && Input.IsActionPressed("ui_left"))
            {
                direction.Z += 1;
            }

            if (!_disabledDirections[1] && Input.IsActionPressed("ui_right"))
            {
                direction.Z -= 1;
            }

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

        // 禁用指定方向
        public void DisableDirection(int directionIndex)
        {
            if (directionIndex is >= 0 and < 4)
            {
                _disabledDirections[directionIndex] = true;
                _ = EmitSignal(SignalName.DirectionDisabled, directionIndex);
            }
        }

        // 启用指定方向
        public void EnableDirection(int directionIndex)
        {
            if (directionIndex is >= 0 and < 4)
            {
                _disabledDirections[directionIndex] = false;
                _ = EmitSignal(SignalName.DirectionEnabled, directionIndex);
            }
        }

        // 禁用所有方向
        public void DisableAllDirections()
        {
            for (int i = 0; i < 4; i++)
            {
                DisableDirection(i);
            }
        }

        // 启用所有方向
        public void EnableAllDirections()
        {
            for (int i = 0; i < 4; i++)
            {
                EnableDirection(i);
            }
        }

        // 检查方向是否被禁用
        public bool IsDirectionDisabled(int directionIndex)
        {
            return directionIndex is >= 0 and < 4 && _disabledDirections[directionIndex];
        }

        public void PauseMovement()
        {
            CanMove = false;
            _ = EmitSignal(SignalName.MovementPaused);
        }

        public void ResumeMovement()
        {
            CanMove = true;
            _ = EmitSignal(SignalName.MovementResumed);
        }

        // 外部调用方法，用于控制移动状态
        public void SetCanMove(bool canMove)
        {
            CanMove = canMove;
        }

        public bool CanMove { get; set; } = true;
    }
}
