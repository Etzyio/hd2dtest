using Godot;
using System;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 人物基类
    /// </summary>
    public partial class Character : Creature
    {
        // 基本属性
        /// <summary>
        /// 角色名称
        /// </summary>
        [Export]
        public string CharacterName { get; set; } = "Character";

        // 状态属性
        /// <summary>
        /// 是否正在移动
        /// </summary>
        public bool IsMoving { get; set; } = false;

        /// <summary>
        /// 是否正在攻击
        /// </summary>
        public bool IsAttacking { get; set; } = false;

        /// <summary>
        /// 是否正在防御
        /// </summary>
        public bool IsDefending { get; set; } = false;

        // 位置和方向
        /// <summary>
        /// 移动方向
        /// </summary>
        public Vector2 Direction { get; set; } = Vector2.Zero;

        // 动画状态
        /// <summary>
        /// 动画精灵组件
        /// </summary>
        [Export]
        public AnimatedSprite2D AnimatedSprite { get; set; }

        // 碰撞形状
        /// <summary>
        /// 碰撞形状组件
        /// </summary>
        [Export]
        public CollisionShape2D CollisionShape { get; set; }

        /// <summary>
        /// 初始化人物
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            CharacterName = "Character";
        }

        /// <summary>
        /// 角色死亡
        /// </summary>
        protected override void Die()
        {
            base.Die();
            IsMoving = false;
            IsAttacking = false;
            IsDefending = false;

            // 播放死亡动画
            AnimatedSprite?.Play("die");
        }

        /// <summary>
        /// 移动角色
        /// </summary>
        /// <param name="direction">移动方向</param>
        /// <param name="delta">时间增量</param>
        public virtual void Move(Vector2 direction, float delta)
        {
            if (!IsAlive || IsAttacking)
            {
                return;
            }

            Direction = direction.Normalized();
            Position += Direction * Speed * delta;
            IsMoving = direction.Length() > 0;

            // 更新动画
            UpdateAnimation();
        }

        /// <summary>
        /// 攻击目标
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否攻击成功</returns>
        public virtual bool AttackTarget(Character target)
        {
            if (!IsAlive || IsAttacking || target == null || !target.IsAlive)
            {
                return false;
            }

            IsAttacking = true;

            // 计算伤害
            float damage = Attack * (0.8f + GD.Randf() * 0.4f); // 80%-120%的攻击伤害
            target.TakeDamage(damage);

            // 播放攻击动画
            AnimatedSprite?.Play("attack");

            return true;
        }

        /// <summary>
        /// 防御姿态
        /// </summary>
        public virtual void Defend()
        {
            if (!IsAlive || IsAttacking)
            {
                return;
            }

            IsDefending = true;

            // 播放防御动画
            AnimatedSprite?.Play("defend");
        }

        /// <summary>
        /// 取消防御姿态
        /// </summary>
        public virtual void StopDefending()
        {
            IsDefending = false;
        }

        /// <summary>
        /// 更新动画状态
        /// </summary>
        protected virtual void UpdateAnimation()
        {
            if (AnimatedSprite == null)
            {
                return;
            }

            if (IsMoving)
            {
                AnimatedSprite.Play("move");

                // 设置动画方向
                if (Mathf.Abs(Direction.X) > Mathf.Abs(Direction.Y))
                {
                    // 水平移动
                    AnimatedSprite.FlipH = Direction.X < 0;
                }
            }
            else
            {
                AnimatedSprite.Play("idle");
            }
        }

        /// <summary>
        /// 获取角色信息字符串
        /// </summary>
        /// <returns>角色信息</returns>
        public virtual string GetCharacterInfo()
        {
            // 将CharacterName映射到父类的CreatureName
            CreatureName = CharacterName;
            return GetCreatureInfo();
        }
    }
}