using System;
using System.Collections.Generic;
using System.Numerics;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 弱点结构体，定义生物对特定类型伤害的弱点
    /// </summary>
    /// <remarks>
    /// 用于标识生物对某种伤害类型的敏感性，可用于伤害计算中的弱点加成
    /// </remarks>
    /// <param name="type">弱点类型（如：火、水、雷、冰、物理、魔法等）</param>
    public struct Weakness(string type)
    {
        /// <summary>
        /// 弱点类型
        /// </summary>
        /// <value>弱点类型的字符串表示，如："火"、"水"、"雷"、"冰"、"物理"、"魔法"等</value>
        public string Type { get; set; } = type;
    }

    /// <summary>
    /// 生物基类，定义所有生物应该具有的属性和方法
    /// </summary>
    /// <remarks>
    /// 作为游戏中所有生物（包括玩家、怪物、NPC等）的基础类，提供通用的属性和行为
    /// </remarks>
    public class Creature
    {
        /// <summary>
        /// 生物名称
        /// </summary>
        /// <value>生物的显示名称</value>
        public string CreatureName { get; set; } = "Creature";
        
        /// <summary>
        /// 当前生命值
        /// </summary>
        /// <value>当前生命值，范围：0 到 MaxHealth</value>
        public float Health { get; set; } = 100f;
        
        /// <summary>
        /// 最大生命值
        /// </summary>
        /// <value>生物的最大生命值上限</value>
        public float MaxHealth { get; set; } = 100f;
        
        /// <summary>
        /// 攻击力
        /// </summary>
        /// <value>生物的基础攻击力</value>
        public float Attack { get; set; } = 10f;
        
        /// <summary>
        /// 防御力
        /// </summary>
        /// <value>生物的基础防御力</value>
        public float Defense { get; set; } = 5f;
        
        /// <summary>
        /// 速度
        /// </summary>
        /// <value>生物的移动速度和行动顺序相关属性</value>
        public float Speed { get; set; } = 50f;
        
        /// <summary>
        /// 等级
        /// </summary>
        /// <value>生物的当前等级</value>
        public int Level { get; set; } = 1;
        
        /// <summary>
        /// 经验值
        /// </summary>
        /// <value>生物当前积累的经验值</value>
        public int Experience { get; set; } = 0;
        
        /// <summary>
        /// 技能ID列表
        /// </summary>
        /// <value>生物拥有的技能ID集合</value>
        public List<string> SkillIDs { get; set; } = [];

        /// <summary>
        /// 是否存活
        /// </summary>
        /// <value>true 表示生物存活，false 表示生物死亡</value>
        public bool IsAlive { get; private set; } = true;
        
        /// <summary>
        /// 弱点列表
        /// </summary>
        /// <value>生物的弱点类型集合</value>
        public List<Weakness> Weaknesses { get; set; } = [];
        
        /// <summary>
        /// 位置信息
        /// </summary>
        /// <value>生物在游戏世界中的坐标位置</value>
        public Vector2 Position { get; set; } = Vector2.Zero;

        /// <summary>
        /// 初始化生物
        /// </summary>
        /// <remarks>
        /// 重置生物状态，设置生命值为最大值，标记为存活状态
        /// </remarks>
        public virtual void Initialize()
        {
            Health = MaxHealth;
            IsAlive = true;
        }

        /// <summary>
        /// 受到伤害（指定伤害类型）
        /// </summary>
        /// <param name="creature">伤害来源生物</param>
        /// <param name="skill">使用的技能</param>
        /// <returns>实际受到的伤害值</returns>
        /// <remarks>
        /// 计算并应用伤害，检查生物是否死亡
        /// 对于基础生物，不考虑弱点，只考虑防御值
        /// </remarks>
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
        /// <param name="creature">治疗来源生物</param>
        /// <param name="skill">使用的治疗技能</param>
        /// <returns>实际恢复的生命值</returns>
        /// <remarks>
        /// 计算并应用治疗效果，确保生命值不超过最大值
        /// </remarks>
        public virtual float Heal(Creature creature, Skill skill)
        {
            if (!IsAlive)
            {
                return 0;
            }

            float healAmount = DamageCalculator.CalculateDamage(creature, this, skill);
            Health = Math.Min(Health + healAmount, MaxHealth);
            return healAmount;
        }

        /// <summary>
        /// 生物死亡
        /// </summary>
        /// <remarks>
        /// 标记生物为死亡状态，可在子类中重写以实现特定死亡逻辑
        /// </remarks>
        protected virtual void Die()
        {
            IsAlive = false;
        }

        /// <summary>
        /// 获取生物信息字符串
        /// </summary>
        /// <returns>包含生物基本属性的信息字符串</returns>
        /// <remarks>
        /// 格式化输出生物的名称、等级、生命值、攻击力、防御力和速度信息
        /// </remarks>
        public virtual string GetCreatureInfo()
        {
            return $"{CreatureName} - Level {Level} - HP: {Health:F0}/{MaxHealth:F0} - ATK: {Attack:F0} - DEF: {Defense:F0} - SPD: {Speed:F0}";
        }
    }
}