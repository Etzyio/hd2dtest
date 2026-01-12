using Godot;
using System;
using hd2dtest.Scripts.Core;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 技能类
    /// </summary>
    public class Skill
    {
        // 技能类型枚举
        public enum SkillType
        {
            Attack,
            Defense,
            Support,
            Healing
        }

        /// <summary>
        /// 技能ID
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 技能名称Key（用于国际化）
        /// </summary>
        public string NameKey { get; set; } = "";

        /// <summary>
        /// 技能描述Key（用于国际化）
        /// </summary>
        public string DescriptionKey { get; set; } = "";

        /// <summary>
        /// 技能效果Key（用于国际化）
        /// </summary>
        public string EffectKey { get; set; } = "";

        // 技能属性
        /// <summary>
        /// 技能名称
        /// </summary>
        public string SkillName { get; set; } = "New Skill";

        /// <summary>
        /// 技能描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 技能类型
        /// </summary>
        public SkillType SkillTypeValue { get; set; } = SkillType.Attack;

        /// <summary>
        /// 技能类型字符串（用于JSON序列化）
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// 技能伤害
        /// </summary>
        public float Damage { get; set; } = 0f;

        /// <summary>
        /// 技能治疗量
        /// </summary>
        public float Healing { get; set; } = 0f;

        /// <summary>
        /// 技能防御加成
        /// </summary>
        public float DefenseBoost { get; set; } = 0f;

        /// <summary>
        /// 技能速度加成
        /// </summary>
        public float SpeedBoost { get; set; } = 0f;

        /// <summary>
        /// 技能持续时间
        /// </summary>
        public float Duration { get; set; } = 0f;

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        public float Cooldown { get; set; } = 1f;

        /// <summary>
        /// 技能魔法消耗
        /// </summary>
        public int ManaCost { get; set; } = 0;

        /// <summary>
        /// 技能所需等级
        /// </summary>
        public int RequiredLevel { get; set; } = 1;

        /// <summary>
        /// 技能范围
        /// </summary>
        public float Range { get; set; } = 0f;

        /// <summary>
        /// 技能作用范围
        /// </summary>
        public float AreaOfEffect { get; set; } = 0f;

        /// <summary>
        /// 技能伤害类型
        /// </summary>
        public string DamageType { get; set; } = "";

        /// <summary>
        /// 技能是否已解锁
        /// </summary>
        public bool IsUnlocked { get; set; } = false;

        // 技能状态
        /// <summary>
        /// 当前冷却时间
        /// </summary>
        private float _currentCooldown = 0f;

        /// <summary>
        /// 技能是否可用
        /// </summary>
        public bool IsAvailable => _currentCooldown <= 0f;

        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="target">目标</param>
        public void Use(Creature caster, Creature target)
        {
            if (!IsAvailable || !caster.IsAlive || (target != null && !target.IsAlive))
            {
                return;
            }

            // 消耗法力值（如果需要）
            if (ManaCost > 0)
            {
                // 这里简化处理，实际应该检查并消耗法力值
            }

            // 根据技能类型执行不同的效果
            switch (SkillTypeValue)
            {
                case SkillType.Attack:
                    ExecuteAttackEffect(caster, target);
                    break;
                case SkillType.Defense:
                    ExecuteDefenseEffect(caster);
                    break;
                case SkillType.Support:
                    ExecuteSupportEffect(caster);
                    break;
                case SkillType.Healing:
                    ExecuteHealingEffect(caster, target);
                    break;
            }

            // 开始冷却
            _currentCooldown = Cooldown;
        }

        /// <summary>
        /// 执行攻击效果
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="target">目标</param>
        private void ExecuteAttackEffect(Creature caster, Creature target)
        {
            if (target == null)
            {
                return;
            }

            // 目标受到伤害
            target.TakeDamage(caster, this);

            Log.Info($"{caster.CreatureName} used {SkillName} on {target.CreatureName}");
        }

        /// <summary>
        /// 执行防御效果
        /// </summary>
        /// <param name="caster">施法者</param>
        private void ExecuteDefenseEffect(Creature caster)
        {
            // 提升防御力   
            caster.Defense += DefenseBoost;

            Log.Info($"{caster.CreatureName} used {SkillName}, defense increased by {DefenseBoost:F1}!");

            // 如果有持续时间，设置定时器恢复
            if (Duration > 0)
            {
                // 这里简化处理，实际应该使用定时器
                Log.Info($"Defense boost will last for {Duration:F1} seconds");
            }
        }

        /// <summary>
        /// 执行辅助效果
        /// </summary>
        /// <param name="caster">施法者</param>
        private void ExecuteSupportEffect(Creature caster)
        {
            // 提升速度
            caster.Speed += SpeedBoost;

            Log.Info($"{caster.CreatureName} used {SkillName}, speed increased by {SpeedBoost:F1}!");

            // 如果有持续时间，设置定时器恢复
            if (Duration > 0)
            {
                // 这里简化处理，实际应该使用定时器
                Log.Info($"Speed boost will last for {Duration:F1} seconds");
            }
        }

        /// <summary>
        /// 执行治疗效果
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="target">目标</param>
        private void ExecuteHealingEffect(Creature caster, Creature target)
        {
            // 如果目标为null，治疗自己
            Creature healingTarget = target ?? caster;

            // 计算最终治疗量
            float finalHealing = Healing + caster.Level * 2f;

            // 目标恢复生命值
            healingTarget.Heal(finalHealing);

            Log.Info($"{caster.CreatureName} used {SkillName} on {healingTarget.CreatureName}, healing {finalHealing:F1} HP!");
        }

        /// <summary>
        /// 更新技能冷却
        /// </summary>
        /// <param name="delta">时间增量</param>
        public void Update(float delta)
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown -= delta;
            }
        }

        /// <summary>
        /// 解锁技能
        /// </summary>
        public void Unlock()
        {
            IsUnlocked = true;
            Log.Info($"Skill {SkillName} has been unlocked!");
        }

        /// <summary>
        /// 获取技能状态描述
        /// </summary>
        /// <returns>技能状态描述</returns>
        public string GetStatus()
        {
            if (!IsUnlocked)
            {
                return "Locked";
            }

            if (!IsAvailable)
            {
                return $"Cooldown: {_currentCooldown:F1}s";
            }

            return "Ready";
        }

        /// <summary>
        /// 获取技能信息
        /// </summary>
        /// <returns>技能信息</returns>
        public string GetInfo()
        {
            string typeStr = SkillTypeValue switch
            {
                SkillType.Attack => "Attack",
                SkillType.Defense => "Defense",
                SkillType.Support => "Support",
                SkillType.Healing => "Healing",
                _ => "Unknown"
            };

            return $"{SkillName} ({typeStr})\n" +
                   $"Description: {Description}\n" +
                   $"Damage: {Damage:F1} | Healing: {Healing:F1}\n" +
                   $"Cooldown: {Cooldown:F1}s | Mana Cost: {ManaCost}\n" +
                   $"Required Level: {RequiredLevel} | Status: {GetStatus()}";
        }
    }
}