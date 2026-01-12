using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 弱点结构体，只包含弱点类型
    /// </summary>
    /// <remarks>
    /// 构造弱点
    /// </remarks>
    /// <param name="type">弱点类型</param>
    public struct Weakness(string type)
    {
        /// <summary>
        /// 弱点类型（如：火、水、雷、冰、物理、魔法等）
        /// </summary>
        public string Type { get; set; } = type;
    }

    /// <summary>
    /// 生物基类，包含所有生物共有的属性
    /// </summary>
    public partial class Creature : Node2D
    {
        // 基本属性
        /// <summary>
        /// 生物名称
        /// </summary>
        [Export]
        public string CreatureName { get; set; } = "Creature";

        /// <summary>
        /// 当前生命值
        /// </summary>
        [Export]
        public float Health { get; set; } = 100f;

        /// <summary>
        /// 最大生命值
        /// </summary>
        [Export]
        public float MaxHealth { get; set; } = 100f;

        /// <summary>
        /// 攻击力
        /// </summary>
        [Export]
        public float Attack { get; set; } = 10f;

        /// <summary>
        /// 防御力
        /// </summary>
        [Export]
        public float Defense { get; set; } = 5f;

        /// <summary>
        /// 移动速度
        /// </summary>
        [Export]
        public float Speed { get; set; } = 50f;

        /// <summary>
        /// 生物等级
        /// </summary>
        [Export]
        public int Level { get; set; } = 1;

        /// <summary>
        /// 经验值
        /// </summary>
        [Export]
        public int Experience { get; set; } = 0;

        // 状态属性
        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive { get; private set; } = true;

        public List<string> weaknesses = [];

        /// <summary>
        /// 初始化生物
        /// </summary>
        public virtual void Initialize()
        {
            Health = MaxHealth;
            IsAlive = true;
        }

        /// <summary>
        /// 受到伤害（指定伤害类型）
        /// </summary>
        /// <param name="creature">伤害来源</param>
        /// <param name="skill">伤害类型</param>
        /// <returns>实际受到的伤害值</returns>
        public virtual float TakeDamage(Creature creature, Skill skill)
        {
            if (!IsAlive)
            {
                return 0f;
            }

            // 对于基础生物，不考虑弱点，只考虑防御
            
            float actualDamage = DamageCalculator.CalculateDamage(creature, this, skill);

            Health -= actualDamage;

            // 检查是否死亡
            if (Health <= 0f)
            {
                Health = 0f;
                Die();
            }

            return actualDamage;
        }

        /// <summary>
        /// 恢复生命值
        /// </summary>
        /// <param name="amount">恢复量</param>
        public virtual void Heal(float amount)
        {
            if (!IsAlive)
            {
                return;
            }

            Health = Mathf.Min(MaxHealth, Health + amount);
        }

        /// <summary>
        /// 生物死亡
        /// </summary>
        protected virtual void Die()
        {
            IsAlive = false;
        }

        /// <summary>
        /// 获取生物信息字符串
        /// </summary>
        /// <returns>生物信息</returns>
        public virtual string GetCreatureInfo()
        {
            return $"{CreatureName} - Level {Level} - HP: {Health:F0}/{MaxHealth:F0} - ATK: {Attack:F0} - DEF: {Defense:F0} - SPD: {Speed:F0}";
        }
    }
}