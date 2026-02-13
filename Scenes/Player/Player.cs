using Godot;

namespace hd2dtest.Scripts.Player
{
	/// <summary>
	/// 玩家类
	/// 处理玩家移动、动画更新和方向控制
	/// </summary>
	public partial class Player : CharacterBody3D
	{
		/// <summary>
		/// 移动速度
		/// </summary>
		private float _speed = 10.0f;
		
		/// <summary>
		/// 是否启用重力
		/// </summary>
		private bool _gravityEnabled = true;
		
		/// <summary>
		/// 重力加速度
		/// </summary>
		private const float GRAVITY_ACCELERATION = 40.0f;
		
		/// <summary>
		/// 禁用的方向数组
		/// 0: up, 1: down, 2: left, 3: right
		/// </summary>
		private readonly bool[] _disabledDirections = new bool[4];
		
		/// <summary>
		/// 动画精灵
		/// </summary>
		private AnimatedSprite3D _animatedSprite;
		
		/// <summary>
		/// 最后移动方向
		/// </summary>
		private Vector3 _lastDirection = Vector3.Zero;

		/// <summary>
		/// 移动暂停信号
		/// </summary>
		[Signal]
		public delegate void MovementPausedEventHandler();

		/// <summary>
		/// 移动恢复信号
		/// </summary>
		[Signal]
		public delegate void MovementResumedEventHandler();

		/// <summary>
		/// 方向禁用信号
		/// </summary>
		/// <param name="directionIndex">方向索引</param>
		[Signal]
		public delegate void DirectionDisabledEventHandler(int directionIndex);

		/// <summary>
		/// 方向启用信号
		/// </summary>
		/// <param name="directionIndex">方向索引</param>
		[Signal]
		public delegate void DirectionEnabledEventHandler(int directionIndex);

		/// <summary>
		/// 是否可以移动
		/// </summary>
		public bool CanMove { get; set; } = true;

		/// <summary>
		/// 节点就绪时的回调
		/// 获取动画精灵引用并添加输入映射
		/// </summary>
		public override void _Ready()
		{
			// 获取动画精灵引用
			_animatedSprite = GetNode<AnimatedSprite3D>("AnimatedSprite3D");
			
			// 连接信号
			InputMap.AddAction("pause_movement");
			InputMap.ActionAddEvent("pause_movement", new InputEventKey() { Keycode = Key.Space });
			InputMap.AddAction("resume_movement");
			InputMap.ActionAddEvent("resume_movement", new InputEventKey() { Keycode = Key.Enter });
			// 添加重力开关动作
			InputMap.AddAction("toggle_gravity");
			InputMap.ActionAddEvent("toggle_gravity", new InputEventKey() { Keycode = Key.G });
		}

		/// <summary>
		/// 物理处理回调
		/// 处理重力、移动和碰撞
		/// </summary>
		/// <param name="delta">时间增量</param>
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
				// 播放 idle 动画
				UpdateAnimation(Vector3.Zero);
			}

			// 应用移动和碰撞
			_ = MoveAndSlide();
		}

		/// <summary>
		/// 处理移动
		/// 响应WASD输入并更新移动方向
		/// </summary>
		/// <param name="delta">时间增量</param>
		private void HandleMovement(double delta)
		{
			Vector3 direction = Vector3.Zero;
			// WASD移动 - 使用标准的ui_前缀动作名称
			if (!_disabledDirections[2] && Input.IsActionPressed("ui_up"))
			{
				direction.Z -= 1;
			}

			if (!_disabledDirections[3] && Input.IsActionPressed("ui_down"))
			{
				direction.Z += 1;
			}

			if (!_disabledDirections[0] && Input.IsActionPressed("ui_left"))
			{
				direction.X -= 1;
			}

			if (!_disabledDirections[1] && Input.IsActionPressed("ui_right"))
			{
				direction.X += 1;
			}

			// 归一化方向向量
			if (direction.Length() > 0)
			{
				direction = direction.Normalized();
				// 保存最后输入的方向
				_lastDirection = direction;
				// 应用速度
				Velocity = new Vector3(direction.X * _speed, Velocity.Y, direction.Z * _speed);
				// 更新动画
				UpdateAnimation(direction);
			}
			else
			{
				// 停止水平移动
				Velocity = new Vector3(0, Velocity.Y, 0);
				// 播放 idle 动画
				UpdateAnimation(Vector3.Zero);
			}
		}

		/// <summary>
		/// 更新动画
		/// 根据移动方向播放相应的动画
		/// </summary>
		/// <param name="direction">移动方向</param>
		private void UpdateAnimation(Vector3 direction)
		{
			if (_animatedSprite == null)
			{
				return;
			}

			if (direction == Vector3.Zero)
			{
				// 停止移动，根据最后输入的方向播放相应的 idle 动画
				_animatedSprite.Stop();
				
				if (_lastDirection == Vector3.Zero)
				{
					// 如果没有最后输入的方向，播放默认 idle 动画
					_animatedSprite.FlipH = false;
					_animatedSprite.Play("idle");
				}
				else
				{
					// 根据最后输入的方向播放相应的 idle 动画
					if (Mathf.Abs(_lastDirection.Z) > Mathf.Abs(_lastDirection.X))
					{
						if (_lastDirection.Z < 0)
						{
							// 向上移动（背面）
							_animatedSprite.FlipH = false;
							_animatedSprite.Play("back_idle");
						}
						else
						{
							// 向下移动（正面）
							_animatedSprite.FlipH = false;
							_animatedSprite.Play("idle");
						}
					}
					else
					{
						if (_lastDirection.X < 0)
						{
							// 向左移动，使用反转的 right_idle
							_animatedSprite.FlipH = true;
							_animatedSprite.Play("right_idle");
						}
						else
						{
							// 向右移动
							_animatedSprite.FlipH = false;
							_animatedSprite.Play("right_idle");
						}
					}
				}
			}
			else
			{
				// 根据移动方向播放相应的动画
				if (Mathf.Abs(direction.Z) > Mathf.Abs(direction.X))
				{
					if (direction.Z < 0)
					{
						// 向上移动（背面）
						_animatedSprite.FlipH = false;
						_animatedSprite.Play("back_run");
					}
					else
					{
						// 向下移动（正面）
						_animatedSprite.FlipH = false;
						_animatedSprite.Play("main_run");
					}
				}
				else
				{
					if (direction.X < 0)
					{
						// 向左移动，使用反转的 right_run
						_animatedSprite.FlipH = true;
						_animatedSprite.Play("right_run");
					}
					else
					{
						// 向右移动
						_animatedSprite.FlipH = false;
						_animatedSprite.Play("right_run");
					}
				}
			}
		}

		/// <summary>
		/// 禁用指定方向
		/// </summary>
		/// <param name="directionIndex">方向索引</param>
		public void DisableDirection(int directionIndex)
		{
			if (directionIndex is >= 0 and < 4)
			{
				_disabledDirections[directionIndex] = true;
				_ = EmitSignal(SignalName.DirectionDisabled, directionIndex);
			}
		}

		/// <summary>
		/// 启用指定方向
		/// </summary>
		/// <param name="directionIndex">方向索引</param>
		public void EnableDirection(int directionIndex)
		{
			if (directionIndex is >= 0 and < 4)
			{
				_disabledDirections[directionIndex] = false;
				_ = EmitSignal(SignalName.DirectionEnabled, directionIndex);
			}
		}

		/// <summary>
		/// 禁用所有方向
		/// </summary>
		public void DisableAllDirections()
		{
			for (int i = 0; i < 4; i++)
			{
				DisableDirection(i);
			}
		}

		/// <summary>
		/// 启用所有方向
		/// </summary>
		public void EnableAllDirections()
		{
			for (int i = 0; i < 4; i++)
			{
				EnableDirection(i);
			}
		}

		/// <summary>
		/// 检查方向是否被禁用
		/// </summary>
		/// <param name="directionIndex">方向索引</param>
		/// <returns>方向是否被禁用</returns>
		public bool IsDirectionDisabled(int directionIndex)
		{
			return directionIndex is >= 0 and < 4 && _disabledDirections[directionIndex];
		}

		/// <summary>
		/// 暂停移动
		/// </summary>
		public void PauseMovement()
		{
			CanMove = false;
			_ = EmitSignal(SignalName.MovementPaused);
		}

		/// <summary>
		/// 恢复移动
		/// </summary>
		public void ResumeMovement()
		{
			CanMove = true;
			_ = EmitSignal(SignalName.MovementResumed);
		}

		/// <summary>
		/// 外部调用方法，用于控制移动状态
		/// </summary>
		/// <param name="canMove">是否可以移动</param>
		public void SetCanMove(bool canMove)
		{
			CanMove = canMove;
		}
	}
}
