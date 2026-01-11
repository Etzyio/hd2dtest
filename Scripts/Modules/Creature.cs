using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 弱点结构体，包含弱点类型和伤害倍率
    /// </summary>
    public struct Weakness
    {
        /// <summary>
        /// 弱点类型（如：火、水、雷、冰、物理、魔法等）
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// 弱点倍率，大于1表示弱点，小于1表示抗性，等于1表示正常
        /// </summary>
        public float Multiplier { get; set; }
        
        /// <summary>
        /// 构造弱点
        /// </summary>
        /// <param name="type">弱点类型</param>
        /// <param name="multiplier">伤害倍率</param>
        public Weakness(string type, float multiplier)
        {
            Type = type;
            Multiplier = multiplier;
        }
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
        
        // 弱点属性
        /// <summary>
        /// 弱点列表
        /// </summary>
        public List<Weakness> Weaknesses { get; private set; } = new List<Weakness>();
        
        // 状态属性
        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive { get; private set; } = true;
        
        /// <summary>
        /// 初始化生物
        /// </summary>
        public virtual void Initialize()
        {
            Health = MaxHealth;
            IsAlive = true;
            Weaknesses.Clear();
        }
        
        /// <summary>
        /// 添加弱点
        /// </summary>
        /// <param name="weakness">弱点</param>
        public void AddWeakness(Weakness weakness)
        {
            // 检查是否已存在该类型的弱点
            int index = Weaknesses.FindIndex(w => w.Type == weakness.Type);
            if (index >= 0)
            {
                // 更新现有弱点的倍率
                Weaknesses[index] = weakness;
            }
            else
            {
                // 添加新弱点
                Weaknesses.Add(weakness);
            }
        }
        
        /// <summary>
        /// 添加弱点
        /// </summary>
        /// <param name="type">弱点类型</param>
        /// <param name="multiplier">伤害倍率</param>
        public void AddWeakness(string type, float multiplier)
        {
            AddWeakness(new Weakness(type, multiplier));
        }
        
        /// <summary>
        /// 移除弱点
        /// </summary>
        /// <param name="type">弱点类型</param>
        public void RemoveWeakness(string type)
        {
            Weaknesses.RemoveAll(w => w.Type == type);
        }
        
        /// <summary>
        /// 清除所有弱点
        /// </summary>
        public void ClearWeaknesses()
        {
            Weaknesses.Clear();
        }
        
        /// <summary>
        /// 获取弱点倍率
        /// </summary>
        /// <param name="damageType">伤害类型</param>
        /// <returns>弱点倍率</returns>
        public float GetWeaknessMultiplier(string damageType)
        {
            // 查找对应的弱点
            Weakness? weakness = Weaknesses.Find(w => w.Type == damageType);
            return weakness.HasValue ? weakness.Value.Multiplier : 1.0f;
        }
        
        /// <summary>
        /// 受到伤害（不指定伤害类型，默认为物理伤害）
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <returns>实际受到的伤害值</returns>
        public virtual float TakeDamage(float damage)
        {
            return TakeDamage(damage, "物理");
        }
        
        /// <summary>
        /// 受到伤害（指定伤害类型）
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="damageType">伤害类型</param>
        /// <returns>实际受到的伤害值</returns>
        public virtual float TakeDamage(float damage, string damageType)
        {
            if (!IsAlive)
            {
                return 0f;
            }
            
            // 计算弱点倍率
            float weaknessMultiplier = GetWeaknessMultiplier(damageType);
            
            // 计算实际伤害（考虑防御和弱点）
            float actualDamage = Mathf.Max(1f, damage - Defense * 0.1f) * weaknessMultiplier;
            
            // 显示弱点伤害信息
            if (weaknessMultiplier > 1f)
            {
                Log.Info($"{CreatureName} is weak to {damageType}! Deals {actualDamage:F1} damage (x{weaknessMultiplier:F1} multiplier)");
            }
            else if (weaknessMultiplier < 1f)
            {
                Log.Info($"{CreatureName} resists {damageType}! Deals {actualDamage:F1} damage (x{weaknessMultiplier:F1} multiplier)");
            }
            
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
            // 构建弱点信息字符串
            string weaknessesInfo = "";
            if (Weaknesses.Count > 0)
            {
                weaknessesInfo = " | Weaknesses: ";
                for (int i = 0; i < Weaknesses.Count; i++)
                {
                    var weakness = Weaknesses[i];
                    weaknessesInfo += $"{weakness.Type}({weakness.Multiplier:F1}x)";
                    if (i < Weaknesses.Count - 1)
                    {
                        weaknessesInfo += ", ";
                    }
                }
            }
            
            return $"{CreatureName} - Level {Level} - HP: {Health:F0}/{MaxHealth:F0} - ATK: {Attack:F0} - DEF: {Defense:F0} - SPD: {Speed:F0}{weaknessesInfo}";
        }
    }
}